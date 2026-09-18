using GestaoFinanceiraMEI.Controllers;
using GestaoFinanceiraMEI.Data;
using GestaoFinanceiraMEI.Models;
using GestaoFinanceiraMEI.Services;
using GestaoFinanceiraMEI.Tests.TestHelpers;
using GestaoFinanceiraMEI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Controllers;

[TestFixture]
public class MetasControllerTests
{
    private AppDbContext _context = null!;
    private Mock<IDreService> _dreServiceMock = null!;
    private MetasController _controller = null!;
    private const int UsuarioId = ControllerTestExtensions.UsuarioIdPadrao;

    [SetUp]
    public void SetUp()
    {
        _context = InMemoryDbContextFactory.Criar();
        _dreServiceMock = new Mock<IDreService>();
        _controller = new MetasController(_context, _dreServiceMock.Object).ComoUsuarioAutenticado(UsuarioId);
    }

    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
        _context.Dispose();
    }

    private static DreMensal DreCom(decimal lucroLiquido) => new() { ReceitaBrutaTotal = lucroLiquido > 0 ? lucroLiquido : 0 };
    // LucroLiquido é calculado (ReceitaBrutaTotal - deduções - CMV - despesas);
    // como todas as outras parcelas ficam em zero, LucroLiquido == ReceitaBrutaTotal.

    [Test]
    public async Task Index_SemMetas_RetornaListaVazia()
    {
        var resultado = await _controller.Index() as ViewResult;
        var lista = resultado!.Model as List<MetaProgressoViewModel>;

        Assert.That(lista, Is.Empty);
    }

    [Test]
    public async Task Index_UsaLucroLiquidoDoDreComoValorAlcancado_OrdenaPorMesReferenciaDecrescente()
    {
        var metaJaneiro = new MetaFinanceira { Descricao = "Jan", ValorMeta = 1000m, MesReferencia = new DateTime(2026, 1, 1), UsuarioId = UsuarioId };
        var metaMarco = new MetaFinanceira { Descricao = "Mar", ValorMeta = 2000m, MesReferencia = new DateTime(2026, 3, 1), UsuarioId = UsuarioId };
        _context.Metas.AddRange(metaJaneiro, metaMarco);
        await _context.SaveChangesAsync();

        _dreServiceMock.Setup(d => d.ObterDreAsync(UsuarioId, 1, 2026)).ReturnsAsync(DreCom(800m));
        _dreServiceMock.Setup(d => d.ObterDreAsync(UsuarioId, 3, 2026)).ReturnsAsync(DreCom(1500m));

        var resultado = await _controller.Index() as ViewResult;
        var lista = resultado!.Model as List<MetaProgressoViewModel>;

        Assert.That(lista, Has.Count.EqualTo(2));
        Assert.That(lista![0].Meta.Descricao, Is.EqualTo("Mar")); // mais recente primeiro
        Assert.That(lista[0].ValorAlcancado, Is.EqualTo(1500m));
        Assert.That(lista[1].ValorAlcancado, Is.EqualTo(800m));
    }

    [Test]
    public async Task Index_IgnoraMetasDeOutraUsuaria()
    {
        _context.Metas.Add(new MetaFinanceira { Descricao = "Alheia", ValorMeta = 100m, UsuarioId = 777 });
        await _context.SaveChangesAsync();

        var resultado = await _controller.Index() as ViewResult;
        var lista = resultado!.Model as List<MetaProgressoViewModel>;

        Assert.That(lista, Is.Empty);
    }

    [Test]
    public void Create_Get_RetornaMetaComMesReferenciaNoPrimeiroDiaDoMesAtual()
    {
        var resultado = _controller.Create() as ViewResult;
        var modelo = resultado!.Model as MetaFinanceira;

        Assert.That(modelo!.MesReferencia, Is.EqualTo(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)));
    }

    [Test]
    public async Task Create_Post_ModelStateInvalido_RetornaView()
    {
        var meta = new MetaFinanceira { Descricao = "", ValorMeta = 100m };
        _controller.ModelState.AddModelError("Descricao", "obrigatório");

        var resultado = await _controller.Create(meta) as ViewResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.Model, Is.SameAs(meta));
    }

    [Test]
    public async Task Create_Post_Valido_DefineUsuarioId_Salva_ERedireciona()
    {
        var meta = new MetaFinanceira { Descricao = "Meta X", ValorMeta = 500m, MesReferencia = new DateTime(2026, 4, 1) };

        var resultado = await _controller.Create(meta) as RedirectToActionResult;

        Assert.That(resultado, Is.Not.Null);
        var salva = await _context.Metas.SingleAsync();
        Assert.That(salva.UsuarioId, Is.EqualTo(UsuarioId));
    }

    [Test]
    public async Task Edit_Get_IdNulo_RetornaNotFound() =>
        Assert.That(await _controller.Edit(null), Is.InstanceOf<NotFoundResult>());

    [Test]
    public async Task Edit_Get_NaoEncontrada_RetornaNotFound() =>
        Assert.That(await _controller.Edit(999), Is.InstanceOf<NotFoundResult>());

    [Test]
    public async Task Edit_Post_IdDiferente_RetornaNotFound()
    {
        var meta = new MetaFinanceira { Id = 1, Descricao = "X", ValorMeta = 10m };

        Assert.That(await _controller.Edit(2, meta), Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Edit_Post_ModelStateInvalido_RetornaView()
    {
        var existente = new MetaFinanceira { Descricao = "Original", ValorMeta = 100m, UsuarioId = UsuarioId };
        _context.Metas.Add(existente);
        await _context.SaveChangesAsync();

        var enviada = new MetaFinanceira { Id = existente.Id, Descricao = "", ValorMeta = 100m };
        _controller.ModelState.AddModelError("Descricao", "obrigatório");

        var resultado = await _controller.Edit(existente.Id, enviada) as ViewResult;

        Assert.That(resultado, Is.Not.Null);
    }

    [Test]
    public async Task Edit_Post_Valido_AtualizaCampos_ERedireciona()
    {
        var existente = new MetaFinanceira { Descricao = "Original", ValorMeta = 100m, MesReferencia = new DateTime(2026, 1, 1), UsuarioId = UsuarioId };
        _context.Metas.Add(existente);
        await _context.SaveChangesAsync();

        var enviada = new MetaFinanceira { Id = existente.Id, Descricao = "Atualizada", ValorMeta = 900m, MesReferencia = new DateTime(2026, 5, 1) };

        var resultado = await _controller.Edit(existente.Id, enviada) as RedirectToActionResult;

        Assert.That(resultado, Is.Not.Null);
        var atualizada = await _context.Metas.FindAsync(existente.Id);
        Assert.That(atualizada!.Descricao, Is.EqualTo("Atualizada"));
        Assert.That(atualizada.ValorMeta, Is.EqualTo(900m));
    }

    [Test]
    public async Task Delete_Get_IdNulo_RetornaNotFound() =>
        Assert.That(await _controller.Delete(null), Is.InstanceOf<NotFoundResult>());

    [Test]
    public async Task DeleteConfirmed_NaoEncontrada_RetornaNotFound() =>
        Assert.That(await _controller.DeleteConfirmed(999), Is.InstanceOf<NotFoundResult>());

    [Test]
    public async Task DeleteConfirmed_Encontrada_Exclui_ERedireciona()
    {
        var meta = new MetaFinanceira { Descricao = "X", ValorMeta = 10m, UsuarioId = UsuarioId };
        _context.Metas.Add(meta);
        await _context.SaveChangesAsync();

        var resultado = await _controller.DeleteConfirmed(meta.Id) as RedirectToActionResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(await _context.Metas.FindAsync(meta.Id), Is.Null);
    }
}
