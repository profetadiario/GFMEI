using GestaoFinanceiraMEI.Services;
using Moq;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Services;

[TestFixture]
public class CalculadoraPrecoServiceTests
{
    private Mock<IRelatorioCreditoService> _relatorio = null!;
    private CalculadoraPrecoService _service = null!;
    private static readonly DateTime Hoje = new(2026, 11, 20);

    [SetUp]
    public void SetUp()
    {
        _relatorio = new Mock<IRelatorioCreditoService>();
        _service = new CalculadoraPrecoService(_relatorio.Object);
    }

    private static EntradaCalculadoraPreco Entrada(decimal? custo = 50m, decimal? df = 15m, decimal? dv = 10m, decimal? ml = 20m) =>
        new() { CustoUnitario = custo, DespesasFixasPercentual = df, DespesasVariaveisPercentual = dv, LucroPercentual = ml };

    // ---------------- exemplo da orientadora (decisão 8) ----------------

    [Test]
    public void Calcular_ExemploDaAnnik_Divisor055_Multiplicador1818_Preco9091()
    {
        var r = _service.Calcular(Entrada());

        Assert.That(r.Valido, Is.True);
        Assert.That(r.MarkupDivisor, Is.EqualTo(0.55m));
        Assert.That(r.MarkupMultiplicador, Is.EqualTo(1.818m));
        Assert.That(r.PrecoSugerido, Is.EqualTo(90.91m));
    }

    [Test]
    public void Calcular_ExemploDaAnnik_ParaOndeVaiCadaVenda()
    {
        var r = _service.Calcular(Entrada());

        Assert.That(r.CustoPorUnidade, Is.EqualTo(50m));
        Assert.That(r.DespesasFixasPorUnidade, Is.EqualTo(13.64m));     // 15% de 90,91
        Assert.That(r.DespesasVariaveisPorUnidade, Is.EqualTo(9.09m));  // 10% de 90,91
        Assert.That(r.LucroPorUnidade, Is.EqualTo(18.18m));             // 20% de 90,91
    }

    [TestCase(50, 15, 10, 20)]
    [TestCase(12.37, 33.3, 4.9, 17.5)]
    [TestCase(7, 0, 0, 0)]
    public void Calcular_AsQuatroPartesSomamExatamenteOPreco(decimal custo, decimal df, decimal dv, decimal ml)
    {
        var r = _service.Calcular(Entrada(custo, df, dv, ml));

        Assert.That(r.CustoPorUnidade + r.DespesasFixasPorUnidade + r.DespesasVariaveisPorUnidade + r.LucroPorUnidade,
            Is.EqualTo(r.PrecoSugerido));
    }

    [Test]
    public void Calcular_SemPercentuais_PrecoIgualAoCusto_EMultiplicadorUm()
    {
        var r = _service.Calcular(Entrada(custo: 10m, df: 0m, dv: 0m, ml: 0m));

        Assert.That(r.PrecoSugerido, Is.EqualTo(10m));
        Assert.That(r.MarkupDivisor, Is.EqualTo(1m));
        Assert.That(r.MarkupMultiplicador, Is.EqualTo(1m));
    }

    [Test]
    public void Calcular_PercentuaisVazios_SaoTratadosComoZero()
    {
        var r = _service.Calcular(Entrada(custo: 10m, df: null, dv: null, ml: null));

        Assert.That(r.Valido, Is.True);
        Assert.That(r.PrecoSugerido, Is.EqualTo(10m));
    }

    // ---------------- limites ----------------

    [TestCase(60, 20, 20)]   // exatamente 100%
    [TestCase(50, 30, 30)]   // acima de 100%
    [TestCase(0, 0, 100)]
    public void Calcular_SomaDe100PorCentoOuMais_EhBloqueada(decimal df, decimal dv, decimal ml)
    {
        var r = _service.Calcular(Entrada(df: df, dv: dv, ml: ml));

        Assert.That(r.Valido, Is.False);
        Assert.That(r.Erros, Has.Some.Contains("menos de 100%"));
        Assert.That(r.PrecoSugerido, Is.EqualTo(0m));
    }

    [Test]
    public void Calcular_SomaDe99Virgula9PorCento_AindaCalcula()
    {
        var r = _service.Calcular(Entrada(custo: 1m, df: 40m, dv: 39.9m, ml: 20m));

        Assert.That(r.Valido, Is.True);
        Assert.That(r.PrecoSugerido, Is.EqualTo(1000m));
    }

    [TestCase(0)]
    [TestCase(-5)]
    public void Calcular_CustoZeroOuNegativo_EhInvalido(decimal custo)
    {
        var r = _service.Calcular(Entrada(custo: custo));

        Assert.That(r.Valido, Is.False);
        Assert.That(r.Erros, Has.Some.Contains("custo direto"));
    }

    [Test]
    public void Calcular_PercentualNegativo_EhInvalido()
    {
        Assert.That(_service.Calcular(Entrada(df: -1m)).Valido, Is.False);
        Assert.That(_service.Calcular(Entrada(dv: -1m)).Valido, Is.False);
        Assert.That(_service.Calcular(Entrada(ml: -1m)).Valido, Is.False);
    }

    // ---------------- DF% sugerido ----------------

    private void ComRelatorio(RelatorioCredito r) =>
        _relatorio.Setup(s => s.ObterRelatorioAsync(1, Hoje)).ReturnsAsync(r);

    private static RelatorioCredito Relatorio(int mesesConsiderados, params MesRelatorio[] meses)
    {
        var r = new RelatorioCredito { MesesConsiderados = mesesConsiderados };
        r.Meses.AddRange(meses);
        return r;
    }

    [Test]
    public async Task DfSugerido_SemHistorico_NaoSugere()
    {
        ComRelatorio(Relatorio(0));

        var s = await _service.ObterDespesasFixasSugeridasAsync(1, Hoje);

        Assert.That(s.TemSugestao, Is.False);
        Assert.That(s.Percentual, Is.Null);
    }

    [Test]
    public async Task DfSugerido_ComDespesasMasSemFaturamento_NaoSugere()
    {
        ComRelatorio(Relatorio(1, new MesRelatorio { DespesasFixas = 750m }));

        var s = await _service.ObterDespesasFixasSugeridasAsync(1, Hoje);

        Assert.That(s.TemSugestao, Is.False);
    }

    [Test]
    public async Task DfSugerido_ComHistorico_IncluiODasNoDf()
    {
        // Cenário da demo: faturamento 4.050, aluguel 750 e DAS 285 → (750 + 285) ÷ 4.050 = 25,6%.
        ComRelatorio(Relatorio(1, new MesRelatorio { Receitas = 4050m, DespesasFixas = 750m, DeducoesEImpostos = 285m, DespesasVariaveis = 520m }));

        var s = await _service.ObterDespesasFixasSugeridasAsync(1, Hoje);

        Assert.That(s.Percentual, Is.EqualTo(25.6m));
        Assert.That(s.DespesasFixasMedia, Is.EqualTo(1035m));
        Assert.That(s.FaturamentoMedio, Is.EqualTo(4050m));
    }

    [Test]
    public async Task DfSugerido_UsaAMediaDosMesesConsiderados()
    {
        // Mês fraco e mês forte: a média suaviza a oscilação das vendas.
        ComRelatorio(Relatorio(2,
            new MesRelatorio { Receitas = 2000m, DespesasFixas = 600m, DeducoesEImpostos = 80m },
            new MesRelatorio { Receitas = 6000m, DespesasFixas = 600m, DeducoesEImpostos = 80m }));

        var s = await _service.ObterDespesasFixasSugeridasAsync(1, Hoje);

        Assert.That(s.MesesConsiderados, Is.EqualTo(2));
        Assert.That(s.DespesasFixasMedia, Is.EqualTo(680m));
        Assert.That(s.FaturamentoMedio, Is.EqualTo(4000m));
        Assert.That(s.Percentual, Is.EqualTo(17m)); // 1.360 ÷ 8.000
    }
}
