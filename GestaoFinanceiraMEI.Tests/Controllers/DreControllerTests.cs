using GestaoFinanceiraMEI.Controllers;
using GestaoFinanceiraMEI.Services;
using GestaoFinanceiraMEI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Controllers;

[TestFixture]
public class DreControllerTests
{
    private Mock<IDreService> _dreServiceMock = null!;
    private DreController _controller = null!;
    private const int UsuarioId = ControllerTestExtensions.UsuarioIdPadrao;

    [SetUp]
    public void SetUp()
    {
        _dreServiceMock = new Mock<IDreService>();
        _controller = new DreController(_dreServiceMock.Object).ComoUsuarioAutenticado(UsuarioId);
    }

    [TearDown]
    public void TearDown() => _controller.Dispose();

    [Test]
    public async Task Index_SemFiltro_UsaMesEAnoAtuais()
    {
        var hoje = DateTime.Today;
        var dreEsperado = new DreMensal { Mes = hoje.Month, Ano = hoje.Year, ReceitaBrutaTotal = 1000m };
        _dreServiceMock.Setup(d => d.ObterDreAsync(UsuarioId, hoje.Month, hoje.Year)).ReturnsAsync(dreEsperado);

        var resultado = await _controller.Index(null, null) as ViewResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.Model, Is.SameAs(dreEsperado));
        _dreServiceMock.Verify(d => d.ObterDreAsync(UsuarioId, hoje.Month, hoje.Year), Times.Once);
    }

    [Test]
    public async Task Index_ComFiltro_UsaMesEAnoInformados()
    {
        var dreEsperado = new DreMensal { Mes = 3, Ano = 2026, ReceitaBrutaTotal = 2000m };
        _dreServiceMock.Setup(d => d.ObterDreAsync(UsuarioId, 3, 2026)).ReturnsAsync(dreEsperado);

        var resultado = await _controller.Index(3, 2026) as ViewResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.Model, Is.SameAs(dreEsperado));
    }

    [Test]
    public async Task Index_PassaOUsuarioIdDaUsuariaAutenticada()
    {
        var controllerOutraUsuaria = new DreController(_dreServiceMock.Object).ComoUsuarioAutenticado(usuarioId: 55);
        _dreServiceMock.Setup(d => d.ObterDreAsync(55, It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new DreMensal());

        await controllerOutraUsuaria.Index(1, 2026);

        _dreServiceMock.Verify(d => d.ObterDreAsync(55, 1, 2026), Times.Once);
    }
}
