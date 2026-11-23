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
        _calc.Setup(c => c.ObterValoresSugeridosAsync(UsuarioId, It.IsAny<DateTime>()))
            .ReturnsAsync(new SugestaoValoresMensais
            {
                MesesConsiderados = 2,
                CustosFixos = new() { new() { Nome = "Pró-labore (seu salário)", MediaMensal = 2000m }, new() { Nome = "Aluguel", MediaMensal = 300m } },
                DespesasFixas = new() { new() { Nome = "Impostos (DAS)", MediaMensal = 76.90m } },
                DespesasVariaveis = new() { new() { Nome = "Transporte", MediaMensal = 150m } }
            });
        _controller = new CalculadoraController(_calc.Object).ComoUsuarioAutenticado(UsuarioId);
    }

    [TearDown]
    public void TearDown() => _controller.Dispose();

    [Test]
    public async Task Index_PrimeiraVisita_PreencheOsValoresDoMesComASugestao_ENaoCalcula()
    {
        var view = await _controller.Index(new EntradaCalculadoraPreco()) as ViewResult;
        var modelo = (CalculadoraPrecoViewModel)view!.Model!;

        Assert.That(modelo.Resultado, Is.Null);
        Assert.That(modelo.Entrada.CustosFixos.Select(i => (i.Nome, i.Valor)),
            Is.EqualTo(new[] { ("Pró-labore (seu salário)", (decimal?)2000m), ("Aluguel", (decimal?)300m) }));
        Assert.That(modelo.Entrada.DespesasFixas.Single().Valor, Is.EqualTo(76.90m));
        Assert.That(modelo.Entrada.DespesasVariaveis.Single().Nome, Is.EqualTo("Transporte"));
        Assert.That(modelo.Entrada.LucroPercentual, Is.EqualTo(20m));
        Assert.That(modelo.Sugestao.MesesConsiderados, Is.EqualTo(2));
        _calc.Verify(c => c.Calcular(It.IsAny<EntradaCalculadoraPreco>()), Times.Never);
    }

    [Test]
    public async Task Index_ProLaboreSemLancamento_ApareceComValorEmBranco()
    {
        _calc.Setup(c => c.ObterValoresSugeridosAsync(UsuarioId, It.IsAny<DateTime>()))
            .ReturnsAsync(new SugestaoValoresMensais { CustosFixos = new() { new() { Nome = "Pró-labore (seu salário)" } } });

        var modelo = (CalculadoraPrecoViewModel)((ViewResult)await _controller.Index(new EntradaCalculadoraPreco())).Model!;

        var item = modelo.Entrada.CustosFixos.Single();
        Assert.That(item.Nome, Is.EqualTo("Pró-labore (seu salário)"));
        Assert.That(item.Valor, Is.Null);
    }

    [Test]
    public async Task Index_FormularioPreenchido_Calcula_ENaoSobrescreveOsValores()
    {
        var entrada = new EntradaCalculadoraPreco
        {
            DiasTrabalho = 24, HorasPorDia = 8, MinutosPorUnidade = 90, CustoVariavelUnidade = 14.95m, LucroPercentual = 30m,
            CustosFixos = new() { new() { Nome = "Energia", Valor = 150m } }
        };
        var esperado = new ResultadoCalculadoraPreco { PrecoSugerido = 58.77m };
        _calc.Setup(c => c.Calcular(entrada)).Returns(esperado);

        var view = await _controller.Index(entrada) as ViewResult;
        var modelo = (CalculadoraPrecoViewModel)view!.Model!;

        Assert.That(modelo.Resultado, Is.SameAs(esperado));
        Assert.That(modelo.Entrada.CustosFixos.Single().Nome, Is.EqualTo("Energia"));
    }

    [Test]
    public void ComLinhasEmBranco_MantemOsItensEAcrescentaDuasLinhas()
    {
        var linhas = CalculadoraPrecoViewModel.ComLinhasEmBranco(new() { new() { Nome = "Energia", Valor = 150m }, new() });

        Assert.That(linhas, Has.Count.EqualTo(1 + CalculadoraPrecoViewModel.LinhasEmBranco));
        Assert.That(linhas[0].Nome, Is.EqualTo("Energia"));
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
