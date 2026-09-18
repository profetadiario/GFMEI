using GestaoFinanceiraMEI.Controllers;
using GestaoFinanceiraMEI.Services;
using GestaoFinanceiraMEI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Controllers;

[TestFixture]
public class FluxoCaixaControllerTests
{
    private Mock<IFluxoCaixaMensalService> _fluxoServiceMock = null!;
    private FluxoCaixaController _controller = null!;
    private const int UsuarioId = ControllerTestExtensions.UsuarioIdPadrao;

    [SetUp]
    public void SetUp()
    {
        _fluxoServiceMock = new Mock<IFluxoCaixaMensalService>();
        _controller = new FluxoCaixaController(_fluxoServiceMock.Object).ComoUsuarioAutenticado(UsuarioId);
    }

    [TearDown]
    public void TearDown() => _controller.Dispose();

    [Test]
    public async Task Index_SemAnoInformado_UsaOAnoAtual()
    {
        var anoAtual = DateTime.Today.Year;
        var fluxoEsperado = new FluxoCaixaAnual { Ano = anoAtual };
        _fluxoServiceMock.Setup(s => s.ObterFluxoAnualAsync(UsuarioId, anoAtual)).ReturnsAsync(fluxoEsperado);

        var resultado = await _controller.Index(null) as ViewResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.Model, Is.SameAs(fluxoEsperado));
        _fluxoServiceMock.Verify(s => s.ObterFluxoAnualAsync(UsuarioId, anoAtual), Times.Once);
    }

    [Test]
    public async Task Index_ComAnoInformado_UsaOAnoInformado()
    {
        var fluxoEsperado = new FluxoCaixaAnual { Ano = 2024 };
        _fluxoServiceMock.Setup(s => s.ObterFluxoAnualAsync(UsuarioId, 2024)).ReturnsAsync(fluxoEsperado);

        var resultado = await _controller.Index(2024) as ViewResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.Model, Is.SameAs(fluxoEsperado));
    }

    [Test]
    public async Task Index_PassaOUsuarioIdDaUsuariaAutenticada()
    {
        var controllerOutraUsuaria = new FluxoCaixaController(_fluxoServiceMock.Object).ComoUsuarioAutenticado(usuarioId: 77);
        _fluxoServiceMock.Setup(s => s.ObterFluxoAnualAsync(77, 2026)).ReturnsAsync(new FluxoCaixaAnual());

        await controllerOutraUsuaria.Index(2026);

        _fluxoServiceMock.Verify(s => s.ObterFluxoAnualAsync(77, 2026), Times.Once);
    }
}
