using GestaoFinanceiraMEI.Data;
using GestaoFinanceiraMEI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GestaoFinanceiraMEI.Controllers;

/// <summary>
/// CRUD de lançamentos financeiros (receitas e despesas), núcleo do
/// controle de fluxo de caixa e de custos do sistema.
/// </summary>
public class TransacoesController : AutenticadoController
{
    public const string OrigemNegocio = "negocio";
    public const string OrigemPessoal = "pessoal";
    public const string NomeCategoriaRetiradaPessoal = "Retirada pessoal";
    private const string AcaoSalvarELancarOutro = "outro";

    private readonly AppDbContext _context;

    public TransacoesController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? mes, int? ano)
    {
        var mesFiltro = mes ?? DateTime.Today.Month;
        var anoFiltro = ano ?? DateTime.Today.Year;

        var transacoes = await _context.Transacoes
            .Include(t => t.Categoria)
            .Where(t => t.UsuarioId == UsuarioId
                        && t.Data.Month == mesFiltro
                        && t.Data.Year == anoFiltro)
            .OrderByDescending(t => t.Data)
            .ToListAsync();

        ViewData["Mes"] = mesFiltro;
        ViewData["Ano"] = anoFiltro;
        ViewData["TotalReceitas"] = transacoes.Where(t => t.Tipo == TipoTransacao.Receita).Sum(t => t.Valor);
        // Despesas do negócio e retiradas pessoais aparecem em
        // linhas separadas; as duas reduzem o saldo do mês.
        ViewData["TotalDespesas"] = transacoes.Where(t => t.Tipo == TipoTransacao.Despesa && !EhRetiradaPessoal(t)).Sum(t => t.Valor);
        ViewData["TotalRetiradas"] = transacoes.Where(EhRetiradaPessoal).Sum(t => t.Valor);

        return View(transacoes);
    }

    public async Task<IActionResult> Create()
    {
        await CarregarCategoriasAsync();
        return View(new Transacao { Data = DateTime.Today });
    }

    /// <summary>
    /// Lançamento rápido. Além dos campos do lançamento, recebe:
    /// "origem" ("negocio" é o padrão; "pessoal" marca uma saída como retirada
    /// pessoal, que vai para uma categoria própria e fica fora do
    /// DRE) e "acao" ("outro" salva e já abre um novo lançamento em branco).
    /// A descrição é opcional: se vier vazia, usa o nome da categoria.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Descricao,Valor,Tipo,Data,CategoriaId")] Transacao transacao, string? origem = null, string? acao = null)
    {
        var retiradaPessoal = transacao.Tipo == TipoTransacao.Despesa && origem == OrigemPessoal;

        string? nomeCategoria = null;
        if (retiradaPessoal)
        {
            // A categoria é escolhida pelo sistema, então um CategoriaId vazio
            // não é erro neste caso.
            ModelState.Remove(nameof(transacao.CategoriaId));
            nomeCategoria = NomeCategoriaRetiradaPessoal;
        }
        else
        {
            var categoria = await _context.Categorias
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == transacao.CategoriaId && c.UsuarioId == UsuarioId && c.Tipo == transacao.Tipo);

            if (categoria is null)
                ModelState.AddModelError(nameof(transacao.CategoriaId), "Selecione uma categoria compatível com o tipo escolhido.");
            else
                nomeCategoria = categoria.Nome;
        }

        if (string.IsNullOrWhiteSpace(transacao.Descricao) && nomeCategoria is not null)
        {
            transacao.Descricao = nomeCategoria;
            ModelState.Remove(nameof(transacao.Descricao));
        }

        if (!ModelState.IsValid)
        {
            await CarregarCategoriasAsync();
            ViewData["Origem"] = origem;
            return View(transacao);
        }

        // A categoria "Retirada pessoal" só é criada (uma vez por usuária)
        // depois de o formulário ser validado, para não deixar categoria
        // sobrando quando o lançamento é rejeitado.
        if (retiradaPessoal)
            transacao.CategoriaId = await ObterOuCriarCategoriaRetiradaPessoalAsync();

        transacao.UsuarioId = UsuarioId;
        _context.Add(transacao);
        await _context.SaveChangesAsync();

        if (acao == AcaoSalvarELancarOutro)
        {
            TempData["Sucesso"] = "Lançamento salvo. Pode lançar o próximo.";
            return RedirectToAction(nameof(Create));
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var transacao = await _context.Transacoes
            .FirstOrDefaultAsync(t => t.Id == id && t.UsuarioId == UsuarioId);
        if (transacao is null) return NotFound();

        await CarregarCategoriasAsync();
        return View(transacao);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Descricao,Valor,Tipo,Data,CategoriaId")] Transacao transacao)
    {
        if (id != transacao.Id) return NotFound();

        var existente = await _context.Transacoes
            .FirstOrDefaultAsync(t => t.Id == id && t.UsuarioId == UsuarioId);
        if (existente is null) return NotFound();

        if (!await CategoriaValidaAsync(transacao.CategoriaId, transacao.Tipo))
            ModelState.AddModelError(nameof(transacao.CategoriaId), "Selecione uma categoria compatível com o tipo escolhido.");

        if (!ModelState.IsValid)
        {
            await CarregarCategoriasAsync();
            return View(transacao);
        }

        existente.Descricao = transacao.Descricao;
        existente.Valor = transacao.Valor;
        existente.Tipo = transacao.Tipo;
        existente.Data = transacao.Data;
        existente.CategoriaId = transacao.CategoriaId;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var transacao = await _context.Transacoes
            .Include(t => t.Categoria)
            .FirstOrDefaultAsync(t => t.Id == id && t.UsuarioId == UsuarioId);
        if (transacao is null) return NotFound();

        return View(transacao);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var transacao = await _context.Transacoes
            .FirstOrDefaultAsync(t => t.Id == id && t.UsuarioId == UsuarioId);
        if (transacao is null) return NotFound();

        _context.Transacoes.Remove(transacao);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private static bool EhRetiradaPessoal(Transacao transacao) =>
        transacao.Tipo == TipoTransacao.Despesa
        && transacao.Categoria?.NaturezaDespesa == NaturezaDespesa.RetiradaPessoal;

    private async Task<int> ObterOuCriarCategoriaRetiradaPessoalAsync()
    {
        var existente = await _context.Categorias.FirstOrDefaultAsync(c =>
            c.UsuarioId == UsuarioId
            && c.Tipo == TipoTransacao.Despesa
            && c.NaturezaDespesa == NaturezaDespesa.RetiradaPessoal);

        if (existente is not null)
            return existente.Id;

        var nova = new Categoria
        {
            Nome = NomeCategoriaRetiradaPessoal,
            Tipo = TipoTransacao.Despesa,
            NaturezaDespesa = NaturezaDespesa.RetiradaPessoal,
            UsuarioId = UsuarioId
        };
        _context.Categorias.Add(nova);
        await _context.SaveChangesAsync();

        return nova.Id;
    }

    private async Task<bool> CategoriaValidaAsync(int categoriaId, TipoTransacao tipo)
    {
        return await _context.Categorias
            .AnyAsync(c => c.Id == categoriaId && c.UsuarioId == UsuarioId && c.Tipo == tipo);
    }

    private async Task CarregarCategoriasAsync()
    {
        var categorias = await _context.Categorias
            .Where(c => c.UsuarioId == UsuarioId)
            .OrderBy(c => c.Tipo).ThenBy(c => c.Nome)
            .ToListAsync();

        ViewBag.Categorias = categorias.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = $"{c.Nome} ({(c.Tipo == TipoTransacao.Receita ? "Receita" : "Despesa")})"
        }).ToList();
    }
}
