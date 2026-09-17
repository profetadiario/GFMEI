using GestaoFinanceiraMEI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestaoFinanceiraMEI.Controllers;

/// <summary>
/// Demonstrativo de Resultado do Exercício (DRE) mensal, correspondendo à
/// competência de análise financeira/controle de custos orientada pela
/// banca: calcula o lucro (ou prejuízo) real do mês antes de comparar com
/// a meta de lucro desejado.
/// </summary>
public class DreController : AutenticadoController
{
    private readonly IDreService _dreService;

    public DreController(IDreService dreService)
    {
        _dreService = dreService;
    }

    public async Task<IActionResult> Index(int? mes, int? ano)
    {
        var mesFiltro = mes ?? DateTime.Today.Month;
        var anoFiltro = ano ?? DateTime.Today.Year;

        var dre = await _dreService.ObterDreAsync(UsuarioId, mesFiltro, anoFiltro);

        return View(dre);
    }
}
