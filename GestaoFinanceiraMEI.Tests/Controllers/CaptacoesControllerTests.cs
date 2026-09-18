using GestaoFinanceiraMEI.Controllers;
using GestaoFinanceiraMEI.Data;
using GestaoFinanceiraMEI.Models;
using GestaoFinanceiraMEI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Controllers;

[TestFixture]
public class CaptacoesControllerTests
{
    private AppDbContext _context = null!;
    private CaptacoesController _controller = null!;
    private const int UsuarioId = ControllerTestExtensions.UsuarioIdPadrao;

    [SetUp]
    public void SetUp()
    {
        _context = InMemoryDbContextFactory.Criar();
        _controller = new CaptacoesController(_context).ComoUsuarioAutenticado(UsuarioId);
    }

    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
        _context.Dispose();
    }

    [Test]
    public async Task Index_ListaApenasCaptacoesDaUsuaria_OrdenadasPorDataDecrescente()
    {
        _context.Captacoes.AddRange(
            new CaptacaoRecurso { InstituicaoFinanceira = "Banco A", Valor = 100m, Finalidade = "X", DataObtencao = new DateTime(2026, 1, 1), UsuarioId = UsuarioId },
            new CaptacaoRecurso { InstituicaoFinanceira = "Banco B", Valor = 200m, Finalidade = "Y", DataObtencao = new DateTime(2026, 6, 1), UsuarioId = UsuarioId },
            new CaptacaoRecurso { InstituicaoFinanceira = "Alheia", Valor = 999m, Finalidade = "Z", DataObtencao = DateTime.Today, UsuarioId = 777 });
        await _context.SaveChangesAsync();

        var resultado = await _controller.Index() as ViewResult;
        var lista = resultado!.Model as List<CaptacaoRecurso>;

        Assert.That(lista, Has.Count.EqualTo(2));
        Assert.That(lista![0].InstituicaoFinanceira, Is.EqualTo("Banco B"));
    }

    [Test]
    public void Create_Get_RetornaCaptacaoComDataDeObtencaoHoje()
    {
        var resultado = _controller.Create() as ViewResult;
        var modelo = resultado!.Model as CaptacaoRecurso;

        Assert.That(modelo!.DataObtencao, Is.EqualTo(DateTime.Today));
    }

    [Test]
    public async Task Create_Post_ModelStateInvalido_RetornaView()
    {
        var captacao = new CaptacaoRecurso { InstituicaoFinanceira = "", Valor = 10m, Finalidade = "X" };
        _controller.ModelState.AddModelError("InstituicaoFinanceira", "obrigatório");

        var resultado = await _controller.Create(captacao) as ViewResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.Model, Is.SameAs(captacao));
    }

    [Test]
    public async Task Create_Post_Valido_DefineUsuarioId_Salva_ERedireciona()
    {
        var captacao = new CaptacaoRecurso { InstituicaoFinanceira = "Banco", Valor = 1000m, Finalidade = "Capital de giro", DataObtencao = DateTime.Today };

        var resultado = await _controller.Create(captacao) as RedirectToActionResult;

        Assert.That(resultado, Is.Not.Null);
        var salva = await _context.Captacoes.SingleAsync();
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
        var captacao = new CaptacaoRecurso { Id = 1, InstituicaoFinanceira = "X", Valor = 10m, Finalidade = "Y" };

        Assert.That(await _controller.Edit(2, captacao), Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Edit_Post_NaoEncontrada_RetornaNotFound()
    {
        var captacao = new CaptacaoRecurso { Id = 123, InstituicaoFinanceira = "X", Valor = 10m, Finalidade = "Y" };

        Assert.That(await _controller.Edit(123, captacao), Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Edit_Post_ModelStateInvalido_RetornaView()
    {
        var existente = new CaptacaoRecurso { InstituicaoFinanceira = "Original", Valor = 100m, Finalidade = "X", UsuarioId = UsuarioId };
        _context.Captacoes.Add(existente);
        await _context.SaveChangesAsync();

        var enviada = new CaptacaoRecurso { Id = existente.Id, InstituicaoFinanceira = "", Valor = 100m, Finalidade = "X" };
        _controller.ModelState.AddModelError("InstituicaoFinanceira", "obrigatório");

        var resultado = await _controller.Edit(existente.Id, enviada) as ViewResult;

        Assert.That(resultado, Is.Not.Null);
    }

    [Test]
    public async Task Edit_Post_Valido_AtualizaCampos_ERedireciona()
    {
        var existente = new CaptacaoRecurso { InstituicaoFinanceira = "Original", Valor = 100m, TaxaJurosMensal = 1m, Finalidade = "X", DataObtencao = DateTime.Today, UsuarioId = UsuarioId };
        _context.Captacoes.Add(existente);
        await _context.SaveChangesAsync();

        var enviada = new CaptacaoRecurso
        {
            Id = existente.Id,
            InstituicaoFinanceira = "Atualizada",
            Valor = 500m,
            TaxaJurosMensal = 2.5m,
            Finalidade = "Nova finalidade",
            DataObtencao = new DateTime(2026, 2, 2)
        };

        var resultado = await _controller.Edit(existente.Id, enviada) as RedirectToActionResult;

        Assert.That(resultado, Is.Not.Null);
        var atualizada = await _context.Captacoes.FindAsync(existente.Id);
        Assert.That(atualizada!.InstituicaoFinanceira, Is.EqualTo("Atualizada"));
        Assert.That(atualizada.Valor, Is.EqualTo(500m));
        Assert.That(atualizada.TaxaJurosMensal, Is.EqualTo(2.5m));
        Assert.That(atualizada.Finalidade, Is.EqualTo("Nova finalidade"));
    }

    [Test]
    public async Task Delete_Get_IdNulo_RetornaNotFound() =>
        Assert.That(await _controller.Delete(null), Is.InstanceOf<NotFoundResult>());

    [Test]
    public async Task Delete_Get_Encontrada_RetornaView()
    {
        var captacao = new CaptacaoRecurso { InstituicaoFinanceira = "Banco", Valor = 10m, Finalidade = "X", UsuarioId = UsuarioId };
        _context.Captacoes.Add(captacao);
        await _context.SaveChangesAsync();

        var resultado = await _controller.Delete(captacao.Id) as ViewResult;

        Assert.That(resultado, Is.Not.Null);
    }

    [Test]
    public async Task DeleteConfirmed_NaoEncontrada_RetornaNotFound() =>
        Assert.That(await _controller.DeleteConfirmed(999), Is.InstanceOf<NotFoundResult>());

    [Test]
    public async Task DeleteConfirmed_Encontrada_Exclui_ERedireciona()
    {
        var captacao = new CaptacaoRecurso { InstituicaoFinanceira = "Banco", Valor = 10m, Finalidade = "X", UsuarioId = UsuarioId };
        _context.Captacoes.Add(captacao);
        await _context.SaveChangesAsync();

        var resultado = await _controller.DeleteConfirmed(captacao.Id) as RedirectToActionResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(await _context.Captacoes.FindAsync(captacao.Id), Is.Null);
    }
}
