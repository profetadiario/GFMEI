using System.ComponentModel.DataAnnotations;

namespace GestaoFinanceiraMEI.Services;

/// <summary>
/// Calculadora de preço de venda (sem persistência: nada é gravado no
/// banco). Usa o método do mark-up divisor, o mais ensinado ao pequeno
/// negócio: o preço precisa cobrir o custo da unidade, a parte dos custos
/// fixos que cabe a cada unidade, as taxas sobre a venda e ainda deixar o
/// lucro desejado.
///
///   preço = (custo da unidade + custos fixos do mês ÷ unidades no mês)
///           ÷ (1 − (taxas % + lucro %) ÷ 100)
/// </summary>
public interface ICalculadoraPrecoService
{
    ResultadoCalculadoraPreco Calcular(EntradaCalculadoraPreco entrada);

    /// <summary>
    /// Sugestão para o campo "custos fixos do mês": as despesas fixas
    /// lançadas no mês anterior ao de <paramref name="hoje"/> (do DRE).
    /// </summary>
    Task<SugestaoCustosFixos> ObterCustosFixosSugeridosAsync(int usuarioId, DateTime hoje);
}

public class EntradaCalculadoraPreco
{
    [Display(Name = "Quanto custa fazer (ou comprar) uma unidade? (R$)")]
    public decimal? CustoUnitario { get; set; }

    [Display(Name = "Custos fixos do mês (R$)")]
    public decimal? CustosFixosMes { get; set; }

    [Display(Name = "Quantas unidades você vende por mês?")]
    public int? QuantidadeMes { get; set; }

    [Display(Name = "Taxas sobre a venda (%)")]
    public decimal? TaxasPercentual { get; set; }

    [Display(Name = "Lucro que você quer ter (%)")]
    public decimal? LucroPercentual { get; set; }

    /// <summary>True quando a usuária já preencheu o formulário (há algo para calcular).</summary>
    public bool FoiPreenchida => CustoUnitario.HasValue;
}

public class ResultadoCalculadoraPreco
{
    public bool Valido => Erros.Count == 0;
    public List<string> Erros { get; } = new();

    public decimal CustoFixoPorUnidade { get; set; }
    public decimal CustoTotalPorUnidade { get; set; }
    public decimal PrecoSugerido { get; set; }
    public decimal TaxasPorUnidade { get; set; }
    public decimal LucroPorUnidade { get; set; }
    public decimal FaturamentoNoMes { get; set; }
    public decimal LucroNoMes { get; set; }
}

public class SugestaoCustosFixos
{
    public int Mes { get; set; }
    public int Ano { get; set; }
    public decimal Valor { get; set; }
}
