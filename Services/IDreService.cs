namespace GestaoFinanceiraMEI.Services;

/// <summary>
/// Contrato do serviço que calcula o Demonstrativo de Resultado do Exercício
/// (DRE) mensal de uma usuária, conforme a estrutura orientada pela banca:
/// Receita Bruta → (–) Deduções/Impostos → Receita Líquida → (–) CMV →
/// Lucro Bruto → (–) Despesas Variáveis → (–) Despesas Fixas → Lucro Líquido.
/// </summary>
public interface IDreService
{
    Task<DreMensal> ObterDreAsync(int usuarioId, int mes, int ano);
}

/// <summary>
/// Resultado do DRE de um mês específico, já com os totais intermediários
/// calculados e prontos para exibição na tabela e no gráfico de pizza.
/// </summary>
public class DreMensal
{
    public int Mes { get; set; }
    public int Ano { get; set; }

    public decimal ReceitaBrutaTotal { get; set; }
    public decimal DeducoesEImpostos { get; set; }
    public decimal ReceitaLiquida => ReceitaBrutaTotal - DeducoesEImpostos;
    public decimal CustoMercadoriaVendida { get; set; }
    public decimal LucroBruto => ReceitaLiquida - CustoMercadoriaVendida;
    public decimal DespesasVariaveis { get; set; }
    public decimal DespesasFixas { get; set; }
    public decimal LucroLiquido => LucroBruto - DespesasVariaveis - DespesasFixas;

    public bool HouveMovimentacao => ReceitaBrutaTotal > 0 || DeducoesEImpostos > 0
        || CustoMercadoriaVendida > 0 || DespesasVariaveis > 0 || DespesasFixas > 0;

    /// <summary>
    /// O gráfico de pizza (no estilo indicado pela orientadora) só faz
    /// sentido representando um resultado positivo: cada fatia é uma parcela
    /// da receita bruta, e uma fatia "negativa" (prejuízo) não é
    /// representável em pizza. Quando há prejuízo, a tela mostra um aviso
    /// textual no lugar do gráfico.
    /// </summary>
    public bool PodeExibirGraficoPizza => ReceitaBrutaTotal > 0 && LucroLiquido >= 0;
}
