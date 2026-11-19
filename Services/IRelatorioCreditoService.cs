namespace GestaoFinanceiraMEI.Services;

/// <summary>
/// Relatório de apoio ao crédito: resume os últimos 12 meses do negócio
/// (faturamento, DRE resumida, retiradas pessoais e saldo em caixa) num
/// formato pronto para imprimir/salvar em PDF pelo navegador e levar ao
/// banco, e calcula o indicador de capacidade de pagamento de parcela.
/// </summary>
public interface IRelatorioCreditoService
{
    /// <param name="hoje">Data de referência: o relatório vai do 11º mês anterior até o mês de hoje.</param>
    Task<RelatorioCredito> ObterRelatorioAsync(int usuarioId, DateTime hoje);
}

public class MesRelatorio
{
    public int Mes { get; set; }
    public int Ano { get; set; }
    public string Nome { get; set; } = string.Empty;

    public decimal Receitas { get; set; }
    public decimal DeducoesEImpostos { get; set; }
    public decimal CustoMercadoriaVendida { get; set; }
    public decimal DespesasVariaveis { get; set; }
    public decimal DespesasFixas { get; set; }
    public decimal RetiradasPessoais { get; set; }

    /// <summary>Todas as despesas do negócio (sem as retiradas pessoais).</summary>
    public decimal DespesasDoNegocio => DeducoesEImpostos + CustoMercadoriaVendida + DespesasVariaveis + DespesasFixas;

    /// <summary>Lucro líquido do mês, como no DRE (retirada pessoal fica fora).</summary>
    public decimal LucroLiquido => Receitas - DespesasDoNegocio;

    /// <summary>O que sobrou de verdade no caixa no mês: lucro menos retiradas pessoais (decisão 1).</summary>
    public decimal Sobra => LucroLiquido - RetiradasPessoais;

    /// <summary>Saldo em caixa acumulado no fim do mês (todo o histórico até ali).</summary>
    public decimal SaldoFinal { get; set; }

    public bool TeveMovimento { get; set; }
}

public enum SituacaoParcela
{
    SemDados,
    Confortavel,
    Atencao,
    Arriscada
}

public class RelatorioCredito
{
    /// <summary>
    /// Parte da sobra média mensal que pode ir para a parcela com folga.
    /// Referência prudente usada no sistema (não é regra de banco): até 30%
    /// a parcela é "confortável"; até 60%, "atenção"; acima disso, "arriscada".
    /// </summary>
    public const decimal PercentualConfortavel = 30m;
    public const decimal PercentualAtencao = 60m;

    public DateTime EmitidoEm { get; set; }
    public List<MesRelatorio> Meses { get; set; } = new();

    /// <summary>
    /// Meses considerados nas médias: do primeiro mês com lançamento (dentro
    /// da janela de 12 meses) até o mês atual. Assim, quem começou a usar o
    /// sistema há 3 meses não tem a média "diluída" por 9 meses vazios.
    /// </summary>
    public int MesesConsiderados { get; set; }

    public int MesesComFaturamento => Meses.Count(m => m.Receitas > 0);

    public decimal FaturamentoTotal => Meses.Sum(m => m.Receitas);
    public decimal DeducoesEImpostos => Meses.Sum(m => m.DeducoesEImpostos);
    public decimal CustoMercadoriaVendida => Meses.Sum(m => m.CustoMercadoriaVendida);
    public decimal DespesasVariaveis => Meses.Sum(m => m.DespesasVariaveis);
    public decimal DespesasFixas => Meses.Sum(m => m.DespesasFixas);
    public decimal ReceitaLiquida => FaturamentoTotal - DeducoesEImpostos;
    public decimal LucroBruto => ReceitaLiquida - CustoMercadoriaVendida;
    public decimal LucroLiquido => LucroBruto - DespesasVariaveis - DespesasFixas;
    public decimal RetiradasPessoais => Meses.Sum(m => m.RetiradasPessoais);
    public decimal SaldoAtual => Meses.Count == 0 ? 0m : Meses[^1].SaldoFinal;

    public decimal FaturamentoMedio => MesesConsiderados == 0 ? 0m : Math.Round(FaturamentoTotal / MesesConsiderados, 2);
    public decimal SobraMedia => MesesConsiderados == 0 ? 0m : Math.Round(Meses.Sum(m => m.Sobra) / MesesConsiderados, 2);

    /// <summary>Maior parcela "confortável": 30% da sobra média (zero se a sobra média não é positiva).</summary>
    public decimal ParcelaConfortavel => SobraMedia <= 0 ? 0m : Math.Round(SobraMedia * PercentualConfortavel / 100m, 2);

    public decimal TotalCaptado { get; set; }
    public int QuantidadeCaptacoes { get; set; }

    public bool TemDados => MesesConsiderados > 0;

    /// <summary>Quanto da sobra média uma parcela comprometeria (%). Null quando não há sobra positiva.</summary>
    public decimal? PercentualDaSobra(decimal parcela) =>
        SobraMedia <= 0 ? null : Math.Round(parcela / SobraMedia * 100m, 1);

    public SituacaoParcela AvaliarParcela(decimal parcela)
    {
        if (!TemDados) return SituacaoParcela.SemDados;
        if (SobraMedia <= 0) return SituacaoParcela.Arriscada;

        var percentual = parcela / SobraMedia * 100m;
        if (percentual <= PercentualConfortavel) return SituacaoParcela.Confortavel;
        if (percentual <= PercentualAtencao) return SituacaoParcela.Atencao;
        return SituacaoParcela.Arriscada;
    }
}
