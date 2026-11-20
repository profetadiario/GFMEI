using GestaoFinanceiraMEI.Services;

namespace GestaoFinanceiraMEI.ViewModels;

public class CalculadoraPrecoViewModel
{
    public EntradaCalculadoraPreco Entrada { get; set; } = new();

    /// <summary>Null enquanto a usuária ainda não preencheu o formulário.</summary>
    public ResultadoCalculadoraPreco? Resultado { get; set; }

    public SugestaoDespesasFixas Sugestao { get; set; } = new();
}
