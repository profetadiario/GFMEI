namespace GestaoFinanceiraMEI.Services;

/// <inheritdoc cref="ICalculadoraPrecoService"/>
public class CalculadoraPrecoService : ICalculadoraPrecoService
{
    private readonly IDreService _dreService;

    public CalculadoraPrecoService(IDreService dreService)
    {
        _dreService = dreService;
    }

    public ResultadoCalculadoraPreco Calcular(EntradaCalculadoraPreco entrada)
    {
        var resultado = new ResultadoCalculadoraPreco();

        var custoUnitario = entrada.CustoUnitario ?? 0m;
        var custosFixos = entrada.CustosFixosMes ?? 0m;
        var quantidade = entrada.QuantidadeMes ?? 0;
        var taxas = entrada.TaxasPercentual ?? 0m;
        var lucro = entrada.LucroPercentual ?? 0m;

        if (custoUnitario < 0) resultado.Erros.Add("O custo da unidade não pode ser negativo.");
        if (custosFixos < 0) resultado.Erros.Add("Os custos fixos não podem ser negativos.");
        if (quantidade < 1) resultado.Erros.Add("Informe quantas unidades você vende por mês (pelo menos 1).");
        if (taxas < 0 || lucro < 0) resultado.Erros.Add("Taxas e lucro não podem ser negativos.");
        if (taxas + lucro >= 100)
            resultado.Erros.Add("Taxas + lucro precisam somar menos de 100%. Com 100% ou mais, nenhum preço cobre os custos.");
        if (custoUnitario == 0 && custosFixos == 0)
            resultado.Erros.Add("Informe o custo da unidade ou os custos fixos do mês.");

        if (!resultado.Valido) return resultado;

        resultado.CustoFixoPorUnidade = Math.Round(custosFixos / quantidade, 2);
        resultado.CustoTotalPorUnidade = custoUnitario + resultado.CustoFixoPorUnidade;

        var divisor = 1m - (taxas + lucro) / 100m;
        resultado.PrecoSugerido = Math.Round(resultado.CustoTotalPorUnidade / divisor, 2);
        resultado.TaxasPorUnidade = Math.Round(resultado.PrecoSugerido * taxas / 100m, 2);
        resultado.LucroPorUnidade = resultado.PrecoSugerido - resultado.CustoTotalPorUnidade - resultado.TaxasPorUnidade;
        resultado.FaturamentoNoMes = resultado.PrecoSugerido * quantidade;
        resultado.LucroNoMes = resultado.LucroPorUnidade * quantidade;

        return resultado;
    }

    public async Task<SugestaoCustosFixos> ObterCustosFixosSugeridosAsync(int usuarioId, DateTime hoje)
    {
        var mesAnterior = new DateTime(hoje.Year, hoje.Month, 1).AddMonths(-1);
        var dre = await _dreService.ObterDreAsync(usuarioId, mesAnterior.Month, mesAnterior.Year);

        return new SugestaoCustosFixos
        {
            Mes = mesAnterior.Month,
            Ano = mesAnterior.Year,
            Valor = dre.DespesasFixas
        };
    }
}
