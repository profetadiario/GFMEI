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
        var query = _context.Transacoes.Where(t => t.UsuarioId == usuarioId);

        if (mes.HasValue)
            query = query.Where(t => t.Data.Month == mes.Value);
        if (ano.HasValue)
            query = query.Where(t => t.Data.Year == ano.Value);

        var transacoesFiltradas = await query.ToListAsync();

        var resumo = new ResumoFinanceiro
        {
            TotalReceitas = transacoesFiltradas.Where(t => t.Tipo == TipoTransacao.Receita).Sum(t => t.Valor),
            TotalDespesas = transacoesFiltradas.Where(t => t.Tipo == TipoTransacao.Despesa).Sum(t => t.Valor)
        };

        var todasTransacoes = await _context.Transacoes
            .Where(t => t.UsuarioId == usuarioId)
            .ToListAsync();

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
