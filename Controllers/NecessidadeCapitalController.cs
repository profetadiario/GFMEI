using GestaoFinanceiraMEI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestaoFinanceiraMEI.Controllers;

/// <summary>
/// Reserva de emergência e capital de giro: indicadores de necessidade de
/// capital (pós-TCC). Nada é gravado; a tela recalcula a cada envio (GET).
/// </summary>
public class NecessidadeCapitalController : AutenticadoController
{
    private readonly INecessidadeCapitalService _servico;

    public NecessidadeCapitalController(INecessidadeCapitalService servico)
    {
        _servico = servico;
    }

    public async Task<IActionResult> Index(int? meses, decimal? guardar)
    {
        var resultado = await _servico.CalcularAsync(
            UsuarioId, DateTime.Today, meses ?? NecessidadeCapital.MesesCoberturaPadrao, guardar);

        return View(resultado);
    }
}
