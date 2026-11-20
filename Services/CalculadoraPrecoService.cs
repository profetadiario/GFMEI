namespace GestaoFinanceiraMEI.Services;

/// <inheritdoc cref="ICalculadoraPrecoService"/>
public class CalculadoraPrecoService : ICalculadoraPrecoService
{
    private readonly IRelatorioCreditoService _relatorio;

    public CalculadoraPrecoService(IRelatorioCreditoService relatorio)
    {
        _relatorio = relatorio;
    }

    public ResultadoCalculadoraPreco Calcular(EntradaCalculadoraPreco entrada)
    {
        var resultado = new ResultadoCalculadoraPreco();

        var custo = entrada.CustoUnitario ?? 0m;
        var df = entrada.DespesasFixasPercentual ?? 0m;
        var dv = entrada.DespesasVariaveisPercentual ?? 0m;
        var ml = entrada.LucroPercentual ?? 0m;

        if (custo <= 0) resultado.Erros.Add("Informe o custo direto de uma unidade (maior que zero).");
        if (df < 0 || dv < 0 || ml < 0) resultado.Erros.Add("Os percentuais não podem ser negativos.");
        if (df + dv + ml >= 100)
            resultado.Erros.Add("Despesas fixas + despesas variáveis + lucro precisam somar menos de 100%. Com 100% ou mais, nenhum preço cobre a conta.");

        if (!resultado.Valido) return resultado;

        var divisor = 1m - (df + dv + ml) / 100m;
        resultado.MarkupDivisor = Math.Round(divisor, 4);
        resultado.MarkupMultiplicador = Math.Round(1m / divisor, 3);
        resultado.PrecoSugerido = Math.Round(custo / divisor, 2);

        resultado.CustoPorUnidade = custo;
        resultado.DespesasFixasPorUnidade = Math.Round(resultado.PrecoSugerido * df / 100m, 2);
        resultado.DespesasVariaveisPorUnidade = Math.Round(resultado.PrecoSugerido * dv / 100m, 2);
        // O lucro fecha a conta, para as quatro partes somarem exatamente o preço.
        resultado.LucroPorUnidade = resultado.PrecoSugerido - custo
            - resultado.DespesasFixasPorUnidade - resultado.DespesasVariaveisPorUnidade;

        return resultado;
    }

    public async Task<SugestaoDespesasFixas> ObterDespesasFixasSugeridasAsync(int usuarioId, DateTime hoje)
    {
        var rel = await _relatorio.ObterRelatorioAsync(usuarioId, hoje);
        var sugestao = new SugestaoDespesasFixas { MesesConsiderados = rel.MesesConsiderados };

        if (rel.MesesConsiderados == 0 || rel.FaturamentoTotal <= 0) return sugestao;

        // Para o MEI, o DAS (deduções e impostos) é um valor fixo mensal: entra no DF.
        var fixas = rel.DespesasFixas + rel.DeducoesEImpostos;
        sugestao.DespesasFixasMedia = Math.Round(fixas / rel.MesesConsiderados, 2);
        sugestao.FaturamentoMedio = rel.FaturamentoMedio;
        sugestao.Percentual = Math.Round(fixas / rel.FaturamentoTotal * 100m, 1);
        return sugestao;
    }
}
