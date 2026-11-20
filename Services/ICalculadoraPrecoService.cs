using System.ComponentModel.DataAnnotations;

namespace GestaoFinanceiraMEI.Services;

/// <summary>
/// Calculadora de preço de venda pelo <b>markup divisor</b>, na forma ensinada
/// pela orientadora no Coworking Me Conecta (decisão 8 do DECISOES_TCC):
/// <code>
/// markup divisor      = 1 − (DF% + DV% + ML%) ÷ 100
/// preço de venda      = custo direto da unidade ÷ markup divisor
/// markup multiplicador = 1 ÷ markup divisor
/// </code>
/// DF% = despesas fixas ÷ faturamento (para o MEI, o DAS entra aqui);
/// DV% = despesas que variam com a venda (cartão, comissão, frete, embalagem);
/// ML% = margem de lucro desejada. Nada é gravado.
/// </summary>
public interface ICalculadoraPrecoService
{
    ResultadoCalculadoraPreco Calcular(EntradaCalculadoraPreco entrada);

    /// <summary>
    /// Sugere o DF% a partir dos lançamentos: (despesas fixas + DAS e impostos)
    /// ÷ faturamento, somados nos meses com lançamento dos últimos 12 meses
    /// (a mesma janela do relatório para o banco). Sem faturamento no período,
    /// não há sugestão e a usuária informa o percentual.
    /// </summary>
    Task<SugestaoDespesasFixas> ObterDespesasFixasSugeridasAsync(int usuarioId, DateTime hoje);
}

public class EntradaCalculadoraPreco
{
    [Display(Name = "Custo direto de uma unidade (R$)")]
    public decimal? CustoUnitario { get; set; }

    /// <summary>DF%: despesas fixas (aluguel, internet, DAS...) em % do faturamento.</summary>
    [Display(Name = "Despesas fixas (%)")]
    public decimal? DespesasFixasPercentual { get; set; }

    /// <summary>DV%: despesas que acompanham cada venda (cartão, comissão, frete, embalagem).</summary>
    [Display(Name = "Despesas variáveis (%)")]
    public decimal? DespesasVariaveisPercentual { get; set; }

    /// <summary>ML%: margem de lucro desejada, em % do preço.</summary>
    [Display(Name = "Lucro desejado (%)")]
    public decimal? LucroPercentual { get; set; }

    /// <summary>A conta só é feita depois que a usuária informa o custo.</summary>
    public bool FoiPreenchida => CustoUnitario.HasValue;
}

public class ResultadoCalculadoraPreco
{
    public bool Valido => Erros.Count == 0;
    public List<string> Erros { get; } = new();

    /// <summary>1 − (DF + DV + ML) ÷ 100, com 4 casas.</summary>
    public decimal MarkupDivisor { get; set; }

    /// <summary>1 ÷ markup divisor, com 3 casas.</summary>
    public decimal MarkupMultiplicador { get; set; }

    public decimal PrecoSugerido { get; set; }

    // Para onde vai cada venda (somam exatamente o preço).
    public decimal CustoPorUnidade { get; set; }
    public decimal DespesasFixasPorUnidade { get; set; }
    public decimal DespesasVariaveisPorUnidade { get; set; }
    public decimal LucroPorUnidade { get; set; }
}

public class SugestaoDespesasFixas
{
    /// <summary>Null quando não há faturamento no período para comparar.</summary>
    public decimal? Percentual { get; set; }
    public decimal DespesasFixasMedia { get; set; }
    public decimal FaturamentoMedio { get; set; }
    public int MesesConsiderados { get; set; }
    public bool TemSugestao => Percentual.HasValue;
}
