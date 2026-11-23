using GestaoFinanceiraMEI.Data;
using GestaoFinanceiraMEI.Models;
using GestaoFinanceiraMEI.Services;
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
    private const string AcaoSalvarELancarOutro = "outro";
    public const string ConfirmacaoProLabore = "sim";

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
        ViewData["TotalDespesas"] = transacoes.Where(t => t.Tipo == TipoTransacao.Despesa).Sum(t => t.Valor);

        return View(transacoes);
    }

    public async Task<IActionResult> Create()
    {
        await CarregarCategoriasAsync(garantirProLabore: true);
        return View(new Transacao { Data = DateTime.Today });
    }

    /// <summary>
    /// Lançamento rápido. Toda saída é do negócio (decisão 10: não existe mais
    /// retirada pessoal; a empreendedora se remunera pelo pró-labore). Além
    /// dos campos do lançamento, recebe "acao" ("outro" salva e já abre um
    /// novo lançamento em branco) e "confirmarProLabore" ("sim" quando ela já
    /// viu o aviso de pró-labore repetido no mês e quer salvar mesmo assim).
    /// A descrição é opcional: se vier vazia, usa o nome da categoria.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Descricao,Valor,Tipo,Data,CategoriaId")] Transacao transacao, string? acao = null, string? confirmarProLabore = null)
    {
        var categoria = await _context.Categorias
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == transacao.CategoriaId && c.UsuarioId == UsuarioId && c.Tipo == transacao.Tipo);

        if (categoria is null)
            ModelState.AddModelError(nameof(transacao.CategoriaId), "Selecione uma categoria compatível com o tipo escolhido.");

        if (string.IsNullOrWhiteSpace(transacao.Descricao) && categoria is not null)
        {
            transacao.Descricao = categoria.Nome;
            ModelState.Remove(nameof(transacao.Descricao));
        }

        if (!ModelState.IsValid)
        {
            await CarregarCategoriasAsync();
            return View(transacao);
        }

        // Pró-labore repetido no mesmo mês: só avisa, não bloqueia.
        if (CategoriasPadrao.EhProLabore(categoria) && confirmarProLabore != ConfirmacaoProLabore)
        {
            var jaLancado = await ProLaboreLancadoNoMesAsync(categoria!.Id, transacao.Data);
            if (jaLancado > 0)
            {
                var pt = new System.Globalization.CultureInfo("pt-BR");
                ViewData["AvisoProLabore"] =
                    $"Você já lançou pró-labore em {transacao.Data.ToString("MMMM/yyyy", pt)} " +
                    $"(R$ {jaLancado.ToString("N2", pt)}). O pró-labore é lançado uma vez, no fechamento do mês. Quer salvar outro mesmo assim?";
                await CarregarCategoriasAsync();
                return View(transacao);
            }
        }

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

    /// <summary>Soma do pró-labore já lançado no mês/ano da data informada (zero se não houver).</summary>
    private async Task<decimal> ProLaboreLancadoNoMesAsync(int categoriaProLaboreId, DateTime data)
    {
        var valores = await _context.Transacoes
            .AsNoTracking()
            .Where(t => t.UsuarioId == UsuarioId
                        && t.CategoriaId == categoriaProLaboreId
                        && t.Data.Month == data.Month
                        && t.Data.Year == data.Year)
            .Select(t => t.Valor)
            .ToListAsync();
        return valores.Sum();
    }

    private async Task<bool> CategoriaValidaAsync(int categoriaId, TipoTransacao tipo)
    {
        return await _context.Categorias
            .AnyAsync(c => c.Id == categoriaId && c.UsuarioId == UsuarioId && c.Tipo == tipo);
    }

    private async Task CarregarCategoriasAsync(bool garantirProLabore = false)
    {
        // Quem já estava logada antes da decisão 10 também recebe o pró-labore.
        var proLaboreId = garantirProLabore
            ? await CategoriasPadrao.GarantirProLaboreAsync(_context, UsuarioId)
            : (int?)null;

        var categorias = await _context.Categorias
            .Where(c => c.UsuarioId == UsuarioId)
            .OrderBy(c => c.Tipo).ThenBy(c => c.Nome)
            .ToListAsync();

        ViewBag.Categorias = categorias.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = $"{c.Nome} ({(c.Tipo == TipoTransacao.Receita ? "Receita" : "Despesa")})"
        }).ToList();

        ViewBag.ProLaboreId = proLaboreId
            ?? categorias.FirstOrDefault(CategoriasPadrao.EhProLabore)?.Id;
    }
}
