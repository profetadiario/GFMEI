using GestaoFinanceiraMEI.Data;
using GestaoFinanceiraMEI.Services;
using GestaoFinanceiraMEI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestaoFinanceiraMEI.Controllers;

/// <summary>
/// Painel de análise financeira: reúne saldo, histórico mensal, progresso
/// da meta do mês e total captado — correspondendo à competência de
/// análise financeira do referencial teórico do TCC.
/// </summary>
public class DashboardController : AutenticadoController
{
    private readonly AppDbContext _context;
    private readonly IFluxoCaixaService _fluxoCaixaService;

    public DashboardController(AppDbContext context, IFluxoCaixaService fluxoCaixaService)
    {
        _context = context;
        _fluxoCaixaService = fluxoCaixaService;
    }

    public async Task<IActionResult> Index()
    {
        var hoje = DateTime.Today;

        var resumoDoMes = await _fluxoCaixaService.ObterResumoAsync(UsuarioId, hoje.Month, hoje.Year);
        var resumoGeral = await _fluxoCaixaService.ObterResumoAsync(UsuarioId);

        var metaDoMes = await _context.Metas
            .Where(m => m.UsuarioId == UsuarioId
                        && m.MesReferencia.Month == hoje.Month
                        && m.MesReferencia.Year == hoje.Year)
            .FirstOrDefaultAsync();

        // SQLite (via EF Core) não traduz Sum/Average sobre "decimal" para SQL,
        // então materializamos a lista com ToListAsync() e somamos em memória
        // (LINQ to Objects) em vez de usar SumAsync diretamente na query.
        var totalCaptado = (await _context.Captacoes
            .Where(c => c.UsuarioId == UsuarioId)
            .ToListAsync())
            .Sum(c => c.Valor);

        var viewModel = new DashboardViewModel
        {
            NomeNegocio = User.FindFirst("NomeNegocio")?.Value ?? string.Empty,
            ResumoDoMes = resumoDoMes,
            SaldoGeral = resumoGeral.Saldo,
            HistoricoMensal = resumoGeral.HistoricoMensal,
            MetaDoMes = metaDoMes,
            ValorAlcancadoMeta = resumoDoMes.TotalReceitas,
            TotalCaptado = totalCaptado
        };

        return View(viewModel);
    }
}
