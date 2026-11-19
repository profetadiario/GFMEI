using GestaoFinanceiraMEI.Controllers;
using GestaoFinanceiraMEI.Services;
using GestaoFinanceiraMEI.Tests.TestHelpers;
using GestaoFinanceiraMEI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Controllers;

[TestFixture]
public class CalculadoraControllerTests
{
    private Mock<ICalculadoraPrecoService> _calc = null!;
    private CalculadoraController _controller = null!;
    private const int UsuarioId = ControllerTestExtensions.UsuarioIdPadrao;

    [SetUp]
    public void SetUp()
    {
        _calc = new Mock<ICalculadoraPrecoService>();
        _calc.Setup(c => c.ObterCustosFixosSugeridosAsync(UsuarioId, It.IsAny<DateTime>()))
            .ReturnsAsync(new SugestaoCustosFixos { Mes = 10, Ano = 2026, Valor = 750m });
        _controller = new CalculadoraController(_calc.Object).ComoUsuarioAutenticado(UsuarioId);
    }

    [TearDown]
    public void TearDown() => _controller.Dispose();

    [Test]
    public async Task Index_PrimeiraVisita_PreencheCustosFixosSugeridos_ENaoCalcula()
    {
        var view = await _controller.Index(new EntradaCalculadoraPreco()) as ViewResult;
        var modelo = (CalculadoraPrecoViewModel)view!.Model!;

        Assert.That(modelo.Resultado, Is.Null);
        Assert.That(modelo.Entrada.CustosFixosMes, Is.EqualTo(750m));
        Assert.That(modelo.Entrada.LucroPercentual, Is.EqualTo(20m));
        _calc.Verify(c => c.Calcular(It.IsAny<EntradaCalculadoraPreco>()), Times.Never);
    }

    [Test]
    public async Task Index_ComCustoInformado_CalculaEMantemOsValoresDigitados()
    {
        var entrada = new EntradaCalculadoraPreco { CustoUnitario = 25m, CustosFixosMes = 100m, QuantidadeMes = 10 };
        var esperado = new ResultadoCalculadoraPreco { PrecoSugerido = 50m };
        _calc.Setup(c => c.Calcular(entrada)).Returns(esperado);

        var view = await _controller.Index(entrada) as ViewResult;
        var modelo = (CalculadoraPrecoViewModel)view!.Model!;

        Assert.That(modelo.Resultado, Is.SameAs(esperado));
        Assert.That(modelo.Entrada.CustosFixosMes, Is.EqualTo(100m)); // não sobrescreve com a sugestão
    }
}

[TestFixture]
public class RelatorioControllerTests
{
    private Mock<IRelatorioCreditoService> _servico = null!;
    private RelatorioController _controller = null!;
    private const int UsuarioId = ControllerTestExtensions.UsuarioIdPadrao;

    [SetUp]
    public void SetUp()
    {
        _servico = new Mock<IRelatorioCreditoService>();
        _servico.Setup(s => s.ObterRelatorioAsync(UsuarioId, It.IsAny<DateTime>())).ReturnsAsync(new RelatorioCredito());
        _controller = new RelatorioController(_servico.Object).ComoUsuarioAutenticado(UsuarioId);
    }

    [TearDown]
    public void TearDown() => _controller.Dispose();

    [Test]
    public async Task Index_PreencheIdentificacaoPelosClaims()
    {
        var modelo = (RelatorioCreditoViewModel)((ViewResult)await _controller.Index(null)).Model!;

        Assert.That(modelo.NomeEmpreendedora, Is.EqualTo(ControllerTestExtensions.NomePadrao));
        Assert.That(modelo.NomeNegocio, Is.EqualTo(ControllerTestExtensions.NomeNegocioPadrao));
        Assert.That(modelo.ParcelaSimulada, Is.Null);
        Assert.That(modelo.Situacao, Is.Null);
    }

    [Test]
    public async Task Index_ComParcela_GuardaASimulacao()
    {
        var modelo = (RelatorioCreditoViewModel)((ViewResult)await _controller.Index(300m)).Model!;

        Assert.That(modelo.ParcelaSimulada, Is.EqualTo(300m));
        Assert.That(modelo.Situacao, Is.EqualTo(SituacaoParcela.SemDados));
    }

    [TestCase(0)]
    [TestCase(-10)]
    public async Task Index_ParcelaZeroOuNegativa_EhIgnorada(decimal parcela)
    {
        var modelo = (RelatorioCreditoViewModel)((ViewResult)await _controller.Index(parcela)).Model!;

        Assert.That(modelo.ParcelaSimulada, Is.Null);
    }

    [Test]
    public async Task Index_UsaOUsuarioIdDaUsuariaAutenticada()
    {
        await _controller.Index(null);

        _servico.Verify(s => s.ObterRelatorioAsync(UsuarioId, It.IsAny<DateTime>()), Times.Once);
    }
}
