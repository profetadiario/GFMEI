using GestaoFinanceiraMEI.Services;
using GestaoFinanceiraMEI.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GestaoFinanceiraMEI.Controllers;

/// <summary>
/// Calculadora de preço de venda pelo markup divisor (decisão 8). Não grava
/// nada: o formulário é enviado por GET e o resultado é calculado na hora.
/// O DF% já vem sugerido a partir dos lançamentos, com o DAS dentro.
/// </summary>
public class CalculadoraController : AutenticadoController
{
    private readonly ICalculadoraPrecoService _calculadora;

    public CalculadoraController(ICalculadoraPrecoService calculadora)
    {
        _calculadora = calculadora;
    }

    public async Task<IActionResult> Index([FromQuery] EntradaCalculadoraPreco entrada)
    {
        var sugestao = await _calculadora.ObterDespesasFixasSugeridasAsync(UsuarioId, DateTime.Today);

        var modelo = new CalculadoraPrecoViewModel { Entrada = entrada, Sugestao = sugestao };

        if (entrada.FoiPreenchida)
        {
            modelo.Resultado = _calculadora.Calcular(entrada);
        }
        else
        {
            // Primeira visita: valores iniciais para a usuária só ajustar.
            entrada.DespesasFixasPercentual ??= sugestao.Percentual;
            entrada.DespesasVariaveisPercentual ??= 0m;
            entrada.LucroPercentual ??= 20m;
        }

        return View(modelo);
    }
}
