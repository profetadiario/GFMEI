using GestaoFinanceiraMEI.Services;
using GestaoFinanceiraMEI.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GestaoFinanceiraMEI.Controllers;

/// <summary>
/// Calculadora de preço pelo método da planilha da Adriana (decisão 9). Não
/// grava nada: o formulário é enviado por GET e o resultado é calculado na
/// hora. Os valores mensais (custos fixos com o pró-labore, despesas fixas
/// com o DAS e despesas variáveis) já vêm sugeridos a partir dos lançamentos.
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
        var sugestao = await _calculadora.ObterValoresSugeridosAsync(UsuarioId, DateTime.Today);

        var modelo = new CalculadoraPrecoViewModel { Entrada = entrada, Sugestao = sugestao };

        if (entrada.FoiPreenchida)
        {
            modelo.Resultado = _calculadora.Calcular(entrada);
        }
        else
        {
            // Primeira visita: valores mensais tirados dos lançamentos, para a
            // usuária só conferir e completar.
            entrada.CustosFixos = Itens(sugestao.CustosFixos);
            entrada.DespesasFixas = Itens(sugestao.DespesasFixas);
            entrada.DespesasVariaveis = Itens(sugestao.DespesasVariaveis);
            entrada.LucroPercentual ??= 20m;
        }

        return View(modelo);
    }

    private static List<ItemMensal> Itens(IEnumerable<ValorSugerido> sugeridos) =>
        sugeridos.Select(s => new ItemMensal
        {
            Nome = s.Nome,
            Valor = s.MediaMensal > 0 ? s.MediaMensal : null
        }).ToList();
}
