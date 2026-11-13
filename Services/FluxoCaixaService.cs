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
    private readonly AppDbContext _context;

    public FluxoCaixaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ResumoFinanceiro> ObterResumoAsync(int usuarioId, int? mes = null, int? ano = null)
    {
        // Uma única consulta, somente leitura (AsNoTracking), traz todos os
        // lançamentos da usuária. Os totais do período (filtrados por mês/ano)
        // e o histórico mensal saem da mesma lista em memória — antes eram
        // duas idas ao banco, e o Painel chama este método duas vezes.
        var todasTransacoes = await _context.Transacoes
            .AsNoTracking()
            .Where(t => t.UsuarioId == usuarioId)
            .ToListAsync();

        IEnumerable<Transacao> transacoesFiltradas = todasTransacoes;

        if (mes.HasValue)
            transacoesFiltradas = transacoesFiltradas.Where(t => t.Data.Month == mes.Value);
        if (ano.HasValue)
            transacoesFiltradas = transacoesFiltradas.Where(t => t.Data.Year == ano.Value);

        var resumo = new ResumoFinanceiro
        {
            TotalReceitas = transacoesFiltradas.Where(t => t.Tipo == TipoTransacao.Receita).Sum(t => t.Valor),
            TotalDespesas = transacoesFiltradas.Where(t => t.Tipo == TipoTransacao.Despesa).Sum(t => t.Valor)
        };

        resumo.HistoricoMensal = todasTransacoes
            .GroupBy(t => new { t.Data.Year, t.Data.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new ResumoMensal
            {
                Mes = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM/yyyy"),
                Receitas = g.Where(t => t.Tipo == TipoTransacao.Receita).Sum(t => t.Valor),
                Despesas = g.Where(t => t.Tipo == TipoTransacao.Despesa).Sum(t => t.Valor)
            })
            .TakeLast(6)
            .ToList();

        return resumo;
    }
}
