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
    private readonly IDreService _dreService;

    public DashboardController(AppDbContext context, IFluxoCaixaService fluxoCaixaService, IDreService dreService)
    {
        _context = context;
        _fluxoCaixaService = fluxoCaixaService;
        _dreService = dreService;
    }

    public async Task<IActionResult> Index()
    {
        var hoje = DateTime.Today;
        var cultura = new System.Globalization.CultureInfo("pt-BR");

        // Resumo acumulado do ano corrente (janeiro até o mês atual), a
        // pedido da orientadora: o painel deixa de mostrar só "o mês atual"
        // isolado e passa a mostrar o acumulado do ano, reiniciando a cada
        // janeiro.
        var resumoAcumuladoAno = await _fluxoCaixaService.ObterResumoAsync(UsuarioId, mes: null, ano: hoje.Year);
        var resumoGeral = await _fluxoCaixaService.ObterResumoAsync(UsuarioId);

        var nomeMesInicio = new DateTime(hoje.Year, 1, 1).ToString("MMMM", cultura).ToUpper(cultura);
        var nomeMesFim = new DateTime(hoje.Year, hoje.Month, 1).ToString("MMMM", cultura).ToUpper(cultura);
        var periodoResumo = hoje.Month == 1 ? nomeMesInicio : $"{nomeMesInicio} A {nomeMesFim}";

        var metaDoMes = await _context.Metas
            .Where(m => m.UsuarioId == UsuarioId
                        && m.MesReferencia.Month == hoje.Month
                        && m.MesReferencia.Year == hoje.Year)
            .FirstOrDefaultAsync();

        // O valor "alcançado" da meta passa a ser o Lucro Líquido do DRE do
        // mês (e não mais a receita bruta): a meta representa lucro
        // desejado, então precisa ser comparada com o lucro de fato apurado.
        var dreDoMes = await _dreService.ObterDreAsync(UsuarioId, hoje.Month, hoje.Year);

        // Materializamos a lista com ToListAsync() e somamos em memória (LINQ
        // to Objects) em vez de usar SumAsync diretamente na query. O volume
        // de captações por usuária é pequeno, então o custo extra é
        // desprezível, e isso evita depender de como cada provedor do EF
        // Core traduz Sum/Average sobre "decimal" para SQL (relevante no
        // passado, quando o projeto ainda rodava sobre SQLite; hoje o banco
        // é SQL Server, mas o padrão foi mantido por já estar em uso).
        var totalCaptado = (await _context.Captacoes
            .Where(c => c.UsuarioId == UsuarioId)
            .ToListAsync())
            .Sum(c => c.Valor);

        var viewModel = new DashboardViewModel
        {
            NomeNegocio = User.FindFirst("NomeNegocio")?.Value ?? string.Empty,
            PeriodoResumo = periodoResumo,
            ResumoDoMes = resumoAcumuladoAno,
            SaldoGeral = resumoGeral.Saldo,
            HistoricoMensal = resumoGeral.HistoricoMensal,
            MetaDoMes = metaDoMes,
            ValorAlcancadoMeta = dreDoMes.LucroLiquido,
            TotalCaptado = totalCaptado
        };

        return View(viewModel);
    }
}
