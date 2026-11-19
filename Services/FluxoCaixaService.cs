using GestaoFinanceiraMEI.Data;
using GestaoFinanceiraMEI.Models;
using Microsoft.EntityFrameworkCore;

namespace GestaoFinanceiraMEI.Services;

/// <summary>
/// Implementação do serviço de fluxo de caixa e análise financeira,
/// consultando os lançamentos (Transacao) do usuário no banco de dados.
/// </summary>
public class FluxoCaixaService : IFluxoCaixaService
{
    // Acima disso, as categorias menores viram "Outras categorias" no gráfico.
    private const int MaximoCategoriasNoGrafico = 6;

    private readonly AppDbContext _context;

    public FluxoCaixaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ResumoFinanceiro> ObterResumoAsync(int usuarioId, int? mes = null, int? ano = null)
    {
        // Uma única consulta, somente leitura (AsNoTracking), traz todos os
        // lançamentos da usuária. Os totais do período (filtrados por mês/ano)
        // e o histórico mensal saem da mesma lista em memória.
        var todasTransacoes = await _context.Transacoes
            .AsNoTracking()
            .Where(t => t.UsuarioId == usuarioId)
            .ToListAsync();

        var idsRetirada = await ObterIdsCategoriasRetiradaAsync(usuarioId);

        return MontarResumo(todasTransacoes, idsRetirada, mes, ano);
    }

    public async Task<PainelFinanceiro> ObterPainelAsync(int usuarioId, DateTime hoje)
    {
        // Duas consultas somente leitura (lançamentos e categorias da usuária)
        // alimentam o Painel inteiro. Antes, o Painel lia todos os lançamentos
        // do banco duas vezes por acesso.
        var todasTransacoes = await _context.Transacoes
            .AsNoTracking()
            .Where(t => t.UsuarioId == usuarioId)
            .ToListAsync();

        var categorias = await _context.Categorias
            .AsNoTracking()
            .Where(c => c.UsuarioId == usuarioId)
            .Select(c => new { c.Id, c.Nome, c.Tipo, c.NaturezaDespesa })
            .ToListAsync();

        var idsRetirada = categorias
            .Where(c => c.Tipo == TipoTransacao.Despesa && c.NaturezaDespesa == NaturezaDespesa.RetiradaPessoal)
            .Select(c => c.Id)
            .ToHashSet();

        var nomePorCategoria = categorias.ToDictionary(c => c.Id, c => c.Nome);

        var painel = new PainelFinanceiro
        {
            ResumoAnoCorrente = MontarResumo(todasTransacoes, idsRetirada, null, hoje.Year),
            ResumoGeral = MontarResumo(todasTransacoes, idsRetirada, null, null)
        };

        // Último lançamento até hoje: um lançamento com data futura (agendado)
        // não deve "zerar" o lembrete de quem está sem lançar há dias.
        var datasAteHoje = todasTransacoes
            .Where(t => t.Data.Date <= hoje.Date)
            .Select(t => t.Data.Date)
            .ToList();
        painel.UltimoLancamento = datasAteHoje.Count == 0 ? (DateTime?)null : datasAteHoje.Max();

        // Gastos do mês corrente, só do negócio (retirada pessoal fica de fora).
        var despesasDoMes = todasTransacoes
            .Where(t => EhDespesaDoNegocio(t, idsRetirada)
                        && t.Data.Month == hoje.Month
                        && t.Data.Year == hoje.Year)
            .ToList();
        var totalDespesasDoMes = despesasDoMes.Sum(t => t.Valor);

        var gastos = despesasDoMes
            .GroupBy(t => t.CategoriaId)
            .Select(g => new GastoPorCategoria
            {
                Categoria = nomePorCategoria.GetValueOrDefault(g.Key, "Sem categoria"),
                Valor = g.Sum(t => t.Valor)
            })
            .OrderByDescending(g => g.Valor)
            .ThenBy(g => g.Categoria)
            .ToList();

        foreach (var gasto in gastos)
        {
            gasto.Percentual = totalDespesasDoMes == 0
                ? 0
                : Math.Round(gasto.Valor / totalDespesasDoMes * 100, 1);
        }

        painel.MaiorGastoDoMes = gastos.FirstOrDefault();
        painel.GastosPorCategoriaDoMes = AgruparCategoriasMenores(gastos, totalDespesasDoMes);

        return painel;
    }

    private async Task<HashSet<int>> ObterIdsCategoriasRetiradaAsync(int usuarioId)
    {
        var ids = await _context.Categorias
            .AsNoTracking()
            .Where(c => c.UsuarioId == usuarioId
                        && c.Tipo == TipoTransacao.Despesa
                        && c.NaturezaDespesa == NaturezaDespesa.RetiradaPessoal)
            .Select(c => c.Id)
            .ToListAsync();

        return ids.ToHashSet();
    }

    private static bool EhRetiradaPessoal(Transacao transacao, HashSet<int> idsRetirada) =>
        transacao.Tipo == TipoTransacao.Despesa && idsRetirada.Contains(transacao.CategoriaId);

    private static bool EhDespesaDoNegocio(Transacao transacao, HashSet<int> idsRetirada) =>
        transacao.Tipo == TipoTransacao.Despesa && !idsRetirada.Contains(transacao.CategoriaId);

    private static ResumoFinanceiro MontarResumo(List<Transacao> todasTransacoes, HashSet<int> idsRetirada, int? mes, int? ano)
    {
        IEnumerable<Transacao> transacoesFiltradas = todasTransacoes;

        if (mes.HasValue)
            transacoesFiltradas = transacoesFiltradas.Where(t => t.Data.Month == mes.Value);
        if (ano.HasValue)
            transacoesFiltradas = transacoesFiltradas.Where(t => t.Data.Year == ano.Value);

        var filtradas = transacoesFiltradas.ToList();

        var resumo = new ResumoFinanceiro
        {
            TotalReceitas = filtradas.Where(t => t.Tipo == TipoTransacao.Receita).Sum(t => t.Valor),
            TotalDespesas = filtradas.Where(t => EhDespesaDoNegocio(t, idsRetirada)).Sum(t => t.Valor),
            TotalRetiradasPessoais = filtradas.Where(t => EhRetiradaPessoal(t, idsRetirada)).Sum(t => t.Valor)
        };

        resumo.HistoricoMensal = todasTransacoes
            .GroupBy(t => new { t.Data.Year, t.Data.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new ResumoMensal
            {
                Mes = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM/yyyy"),
                Receitas = g.Where(t => t.Tipo == TipoTransacao.Receita).Sum(t => t.Valor),
                Despesas = g.Where(t => EhDespesaDoNegocio(t, idsRetirada)).Sum(t => t.Valor),
                RetiradasPessoais = g.Where(t => EhRetiradaPessoal(t, idsRetirada)).Sum(t => t.Valor)
            })
            .TakeLast(6)
            .ToList();

        return resumo;
    }

    private static List<GastoPorCategoria> AgruparCategoriasMenores(List<GastoPorCategoria> ordenadas, decimal totalDespesas)
    {
        if (ordenadas.Count <= MaximoCategoriasNoGrafico)
            return ordenadas;

        var principais = ordenadas.Take(MaximoCategoriasNoGrafico - 1).ToList();
        var valorOutras = ordenadas.Skip(MaximoCategoriasNoGrafico - 1).Sum(g => g.Valor);

        principais.Add(new GastoPorCategoria
        {
            Categoria = "Outras categorias",
            Valor = valorOutras,
            Percentual = totalDespesas == 0 ? 0 : Math.Round(valorOutras / totalDespesas * 100, 1)
        });

        return principais;
    }
}
