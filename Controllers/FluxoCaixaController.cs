using GestaoFinanceiraMEI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestaoFinanceiraMEI.Controllers;

/// <summary>
/// Fluxo de caixa mensal (janeiro a dezembro), correspondendo à competência
/// de gestão de fluxo de caixa do referencial teórico do TCC — com saldo
/// carregado mês a mês, entradas e saídas detalhadas por categoria.
/// </summary>
public class FluxoCaixaController : AutenticadoController
{
    private readonly IFluxoCaixaMensalService _fluxoCaixaMensalService;

    public FluxoCaixaController(IFluxoCaixaMensalService fluxoCaixaMensalService)
    {
        _fluxoCaixaMensalService = fluxoCaixaMensalService;
    }

    public async Task<IActionResult> Index(int? ano)
    {
        var anoFiltro = ano ?? DateTime.Today.Year;
        var fluxo = await _fluxoCaixaMensalService.ObterFluxoAnualAsync(UsuarioId, anoFiltro);
        return View(fluxo);
    }
}
