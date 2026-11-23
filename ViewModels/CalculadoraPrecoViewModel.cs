using GestaoFinanceiraMEI.Services;

namespace GestaoFinanceiraMEI.ViewModels;

public class CalculadoraPrecoViewModel
{
    /// <summary>Linhas em branco extras em cada grupo de valores mensais.</summary>
    public const int LinhasEmBranco = 2;

    public EntradaCalculadoraPreco Entrada { get; set; } = new();

    /// <summary>Null enquanto a usuária ainda não preencheu o formulário.</summary>
    public ResultadoCalculadoraPreco? Resultado { get; set; }

    public SugestaoValoresMensais Sugestao { get; set; } = new();

    /// <summary>Itens digitados mais algumas linhas em branco, para o formulário.</summary>
    public static List<ItemMensal> ComLinhasEmBranco(List<ItemMensal> itens) =>
        itens.Where(i => !string.IsNullOrWhiteSpace(i.Nome) || i.Valor.HasValue)
             .Concat(Enumerable.Range(0, LinhasEmBranco).Select(_ => new ItemMensal()))
             .ToList();
}
