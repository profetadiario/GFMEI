using System.Security.Claims;
using GestaoFinanceiraMEI.Services;
using GestaoFinanceiraMEI.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GestaoFinanceiraMEI.Controllers;

/// <summary>
/// Relatório para o banco (apoio ao crédito): últimos 12 meses, DRE
/// resumida, fluxo mês a mês e indicador de capacidade de pagamento de
/// parcela. O PDF é gerado pela impressão do navegador (decisão 3).
/// </summary>
public class RelatorioController : AutenticadoController
{
    private readonly IRelatorioCreditoService _relatorioService;

    public RelatorioController(IRelatorioCreditoService relatorioService)
    {
        _relatorioService = relatorioService;
    }

    public async Task<IActionResult> Index(decimal? parcela)
    {
        var relatorio = await _relatorioService.ObterRelatorioAsync(UsuarioId, DateTime.Today);

        var modelo = new RelatorioCreditoViewModel
        {
            Relatorio = relatorio,
            NomeEmpreendedora = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
            NomeNegocio = User.FindFirstValue("NomeNegocio") ?? string.Empty,
            ParcelaSimulada = parcela is > 0 ? parcela : null
        };

        return View(modelo);
    }
}
