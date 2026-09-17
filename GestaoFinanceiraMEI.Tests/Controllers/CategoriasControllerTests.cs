using GestaoFinanceiraMEI.Controllers;
using GestaoFinanceiraMEI.Data;
using GestaoFinanceiraMEI.Models;
using GestaoFinanceiraMEI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Controllers;

[TestFixture]
public class CategoriasControllerTests
{
    private AppDbContext _context = null!;
    private CategoriasController _controller = null!;
    private const int UsuarioId = ControllerTestExtensions.UsuarioIdPadrao;
    private const int OutraUsuariaId = 999;

    [SetUp]
    public void SetUp()
    {
        _context = InMemoryDbContextFactory.Criar();
        _controller = new CategoriasController(_context).ComoUsuarioAutenticado(UsuarioId);
    }

    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
        _context.Dispose();
    }

    [Test]
    public async Task Index_ListaApenasCategoriasDaUsuariaLogada_OrdenadasPorTipoENome()
    {
        _context.Categorias.AddRange(
            new Categoria { Nome = "Zebra", Tipo = TipoTransacao.Receita, UsuarioId = UsuarioId },
            new Categoria { Nome = "Abacaxi", Tipo = TipoTransacao.Receita, UsuarioId = UsuarioId },
            new Categoria { Nome = "De outra usuária", Tipo = TipoTransacao.Receita, UsuarioId = OutraUsuariaId });
        await _context.SaveChangesAsync();

        var resultado = await _controller.Index() as ViewResult;
        var modelo = resultado!.Model as List<Categoria>;

        Assert.That(modelo, Has.Count.EqualTo(2));
        Assert.That(modelo![0].Nome, Is.EqualTo("Abacaxi"));
        Assert.That(modelo[1].Nome, Is.EqualTo("Zebra"));
    }

    [Test]
    public void Create_Get_RetornaViewComCategoriaVazia()
    {
        var resultado = _controller.Create() as ViewResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.Model, Is.InstanceOf<Categoria>());
    }

    [Test]
    public async Task Create_Post_ModelStateInvalido_RetornaViewComOMesmoModelo()
    {
        var categoria = new Categoria { Nome = "", Tipo = TipoTransacao.Receita };
        _controller.ModelState.AddModelError("Nome", "Informe o nome da categoria.");

        var resultado = await _controller.Create(categoria) as ViewResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.Model, Is.SameAs(categoria));
    }

    [Test]
    public async Task Create_Post_Valido_DefineUsuarioId_Salva_ERedirecionaParaIndex()
    {
        var categoria = new Categoria { Nome = "Vendas online", Tipo = TipoTransacao.Receita };

        var resultado = await _controller.Create(categoria) as RedirectToActionResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.ActionName, Is.EqualTo("Index"));

        var salva = await _context.Categorias.SingleAsync(c => c.Nome == "Vendas online");
        Assert.That(salva.UsuarioId, Is.EqualTo(UsuarioId));
    }

    [Test]
    public async Task Edit_Get_IdNulo_RetornaNotFound()
    {
        var resultado = await _controller.Edit(null);

        Assert.That(resultado, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Edit_Get_CategoriaNaoEncontrada_RetornaNotFound()
    {
        var resultado = await _controller.Edit(999);

        Assert.That(resultado, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Edit_Get_CategoriaDeOutraUsuaria_RetornaNotFound()
    {
        var categoria = new Categoria { Nome = "Alheia", Tipo = TipoTransacao.Receita, UsuarioId = OutraUsuariaId };
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        var resultado = await _controller.Edit(categoria.Id);

        Assert.That(resultado, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Edit_Get_CategoriaEncontrada_RetornaViewComACategoria()
    {
        var categoria = new Categoria { Nome = "Vendas", Tipo = TipoTransacao.Receita, UsuarioId = UsuarioId };
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        var resultado = await _controller.Edit(categoria.Id) as ViewResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That((resultado!.Model as Categoria)!.Id, Is.EqualTo(categoria.Id));
    }

    [Test]
    public async Task Edit_Post_IdDiferenteDoModelo_RetornaNotFound()
    {
        var categoria = new Categoria { Id = 1, Nome = "X", Tipo = TipoTransacao.Receita };

        var resultado = await _controller.Edit(2, categoria);

        Assert.That(resultado, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Edit_Post_CategoriaNaoEncontradaOuDeOutraUsuaria_RetornaNotFound()
    {
        var categoria = new Categoria { Id = 123, Nome = "X", Tipo = TipoTransacao.Receita };

        var resultado = await _controller.Edit(123, categoria);

        Assert.That(resultado, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Edit_Post_ModelStateInvalido_RetornaViewComOMesmoModelo()
    {
        var existente = new Categoria { Nome = "Original", Tipo = TipoTransacao.Despesa, UsuarioId = UsuarioId };
        _context.Categorias.Add(existente);
        await _context.SaveChangesAsync();

        var enviado = new Categoria { Id = existente.Id, Nome = "", Tipo = TipoTransacao.Despesa };
        _controller.ModelState.AddModelError("Nome", "Informe o nome da categoria.");

        var resultado = await _controller.Edit(existente.Id, enviado) as ViewResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.Model, Is.SameAs(enviado));
    }

    [Test]
    public async Task Edit_Post_Valido_AtualizaNomeTipoENaturezaDespesa()
    {
        var existente = new Categoria { Nome = "Original", Tipo = TipoTransacao.Despesa, NaturezaDespesa = NaturezaDespesa.DespesaVariavel, UsuarioId = UsuarioId };
        _context.Categorias.Add(existente);
        await _context.SaveChangesAsync();

        var enviado = new Categoria { Id = existente.Id, Nome = "Atualizada", Tipo = TipoTransacao.Despesa, NaturezaDespesa = NaturezaDespesa.DespesaFixa };

        var resultado = await _controller.Edit(existente.Id, enviado) as RedirectToActionResult;

        Assert.That(resultado, Is.Not.Null);
        var atualizada = await _context.Categorias.FindAsync(existente.Id);
        Assert.That(atualizada!.Nome, Is.EqualTo("Atualizada"));
        Assert.That(atualizada.NaturezaDespesa, Is.EqualTo(NaturezaDespesa.DespesaFixa));
    }

    [Test]
    public async Task Delete_Get_IdNulo_RetornaNotFound()
    {
        var resultado = await _controller.Delete(null);

        Assert.That(resultado, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Delete_Get_CategoriaEncontrada_RetornaView()
    {
        var categoria = new Categoria { Nome = "Vendas", Tipo = TipoTransacao.Receita, UsuarioId = UsuarioId };
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        var resultado = await _controller.Delete(categoria.Id) as ViewResult;

        Assert.That(resultado, Is.Not.Null);
    }

    [Test]
    public async Task DeleteConfirmed_CategoriaNaoEncontrada_RetornaNotFound()
    {
        var resultado = await _controller.DeleteConfirmed(999);

        Assert.That(resultado, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task DeleteConfirmed_CategoriaEmUso_NaoExclui_DefineTempDataErro_ERedireciona()
    {
        var categoria = new Categoria { Nome = "Vendas", Tipo = TipoTransacao.Receita, UsuarioId = UsuarioId };
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        _context.Transacoes.Add(new Transacao { Descricao = "Venda", Valor = 10m, Tipo = TipoTransacao.Receita, CategoriaId = categoria.Id, UsuarioId = UsuarioId });
        await _context.SaveChangesAsync();

        var resultado = await _controller.DeleteConfirmed(categoria.Id) as RedirectToActionResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(_controller.TempData["Erro"], Is.Not.Null);
        Assert.That(await _context.Categorias.FindAsync(categoria.Id), Is.Not.Null);
    }

    [Test]
    public async Task DeleteConfirmed_CategoriaSemUso_Exclui_ERedireciona()
    {
        var categoria = new Categoria { Nome = "Vendas", Tipo = TipoTransacao.Receita, UsuarioId = UsuarioId };
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        var resultado = await _controller.DeleteConfirmed(categoria.Id) as RedirectToActionResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(await _context.Categorias.FindAsync(categoria.Id), Is.Null);
    }
}
