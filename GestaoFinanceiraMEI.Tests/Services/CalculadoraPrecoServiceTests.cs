using GestaoFinanceiraMEI.Services;
using Moq;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Services;

[TestFixture]
public class CalculadoraPrecoServiceTests
{
    private Mock<IDreService> _dre = null!;
    private CalculadoraPrecoService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _dre = new Mock<IDreService>();
        _service = new CalculadoraPrecoService(_dre.Object);
    }

    private static EntradaCalculadoraPreco Entrada(decimal custo = 25m, decimal fixos = 750m, int qtd = 50, decimal taxas = 5m, decimal lucro = 20m) =>
        new() { CustoUnitario = custo, CustosFixosMes = fixos, QuantidadeMes = qtd, TaxasPercentual = taxas, LucroPercentual = lucro };

    [Test]
    public void Calcular_AplicaMarkupDivisor()
    {
        // custo total = 25 + 750/50 = 40; preço = 40 / (1 - 0,25) = 53,33
        var r = _service.Calcular(Entrada());

        Assert.That(r.Valido, Is.True);
        Assert.That(r.CustoFixoPorUnidade, Is.EqualTo(15.00m));
        Assert.That(r.CustoTotalPorUnidade, Is.EqualTo(40.00m));
        Assert.That(r.PrecoSugerido, Is.EqualTo(53.33m));
    }

    [Test]
    public void Calcular_PrecoCobreCustoTaxasELucro()
    {
        var r = _service.Calcular(Entrada());

        Assert.That(r.TaxasPorUnidade, Is.EqualTo(2.67m)); // 5% de 53,33
        Assert.That(r.CustoTotalPorUnidade + r.TaxasPorUnidade + r.LucroPorUnidade, Is.EqualTo(r.PrecoSugerido));
        Assert.That(r.LucroPorUnidade, Is.EqualTo(10.66m)); // ~20% do preço
    }

    [Test]
    public void Calcular_TotaisDoMes_MultiplicamPelaQuantidade()
    {
        var r = _service.Calcular(Entrada());

        Assert.That(r.FaturamentoNoMes, Is.EqualTo(53.33m * 50));
        Assert.That(r.LucroNoMes, Is.EqualTo(r.LucroPorUnidade * 50));
    }

    [Test]
    public void Calcular_SemTaxasNemLucro_PrecoIgualAoCusto()
    {
        var r = _service.Calcular(Entrada(custo: 10m, fixos: 0m, qtd: 1, taxas: 0m, lucro: 0m));

        Assert.That(r.PrecoSugerido, Is.EqualTo(10m));
        Assert.That(r.LucroPorUnidade, Is.EqualTo(0m));
    }

    [TestCase(60, 40)]
    [TestCase(100, 0)]
    [TestCase(70, 50)]
    public void Calcular_TaxasMaisLucroAte100PorCento_EhInvalido(decimal taxas, decimal lucro)
    {
        var r = _service.Calcular(Entrada(taxas: taxas, lucro: lucro));

        Assert.That(r.Valido, Is.False);
        Assert.That(r.Erros, Has.Some.Contains("menos de 100%"));
    }

    [Test]
    public void Calcular_QuantidadeZero_EhInvalido()
    {
        var r = _service.Calcular(Entrada(qtd: 0));

        Assert.That(r.Valido, Is.False);
        Assert.That(r.Erros, Has.Some.Contains("unidades"));
    }

    [Test]
    public void Calcular_ValoresNegativos_SaoInvalidos()
    {
        Assert.That(_service.Calcular(Entrada(custo: -1m)).Valido, Is.False);
        Assert.That(_service.Calcular(Entrada(fixos: -1m)).Valido, Is.False);
        Assert.That(_service.Calcular(Entrada(lucro: -5m)).Valido, Is.False);
    }

    [Test]
    public void Calcular_SemCustoNenhum_EhInvalido()
    {
        var r = _service.Calcular(Entrada(custo: 0m, fixos: 0m));

        Assert.That(r.Valido, Is.False);
    }

    [Test]
    public void Calcular_CamposVazios_TratadosComoZero_EReportaErros()
    {
        var r = _service.Calcular(new EntradaCalculadoraPreco { CustoUnitario = 10m });

        Assert.That(r.Valido, Is.False); // quantidade vazia
    }

    [Test]
    public async Task ObterCustosFixosSugeridosAsync_UsaDespesasFixasDoMesAnterior()
    {
        _dre.Setup(d => d.ObterDreAsync(1, 10, 2026)).ReturnsAsync(new DreMensal { Mes = 10, Ano = 2026, DespesasFixas = 750m });

        var s = await _service.ObterCustosFixosSugeridosAsync(1, new DateTime(2026, 11, 19));

        Assert.That(s.Mes, Is.EqualTo(10));
        Assert.That(s.Ano, Is.EqualTo(2026));
        Assert.That(s.Valor, Is.EqualTo(750m));
    }

    [Test]
    public async Task ObterCustosFixosSugeridosAsync_EmJaneiro_UsaDezembroDoAnoAnterior()
    {
        _dre.Setup(d => d.ObterDreAsync(1, 12, 2025)).ReturnsAsync(new DreMensal { DespesasFixas = 300m });

        var s = await _service.ObterCustosFixosSugeridosAsync(1, new DateTime(2026, 1, 5));

        Assert.That((s.Mes, s.Ano, s.Valor), Is.EqualTo((12, 2025, 300m)));
    }
}
