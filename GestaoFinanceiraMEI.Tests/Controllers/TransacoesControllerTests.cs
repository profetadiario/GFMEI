using GestaoFinanceiraMEI.Controllers;
using GestaoFinanceiraMEI.Data;
using GestaoFinanceiraMEI.Models;
using GestaoFinanceiraMEI.Services;
using GestaoFinanceiraMEI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Controllers;

[TestFixture]
public class TransacoesControllerTests
{
    private AppDbContext _context = null!;
    private TransacoesController _controller = null!;
    private const int UsuarioId = ControllerTestExtensions.UsuarioIdPadrao;
    private int _categoriaReceitaId;
    private int _categoriaDespesaId;

    [SetUp]
    public void SetUp()
    {
        _context = InMemoryDbContextFactory.Criar();
        _controller = new TransacoesController(_context).ComoUsuarioAutenticado(UsuarioId);

        var categoriaReceita = new Categoria { Nome = "Vendas", Tipo = TipoTransacao.Receita, UsuarioId = UsuarioId };
        var categoriaDespesa = new Categoria { Nome = "Aluguel", Tipo = TipoTransacao.Despesa, UsuarioId = UsuarioId };
        _context.Categorias.AddRange(categoriaReceita, categoriaDespesa);
        _context.SaveChanges();
        _categoriaReceitaId = categoriaReceita.Id;
        _categoriaDespesaId = categoriaDespesa.Id;
    }

    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
        _context.Dispose();
    }

    private void AdicionarTransacao(int categoriaId, TipoTransacao tipo, decimal valor, DateTime data)
    {
        _context.Transacoes.Add(new Transacao { Descricao = "T", Valor = valor, Tipo = tipo, Data = data, CategoriaId = categoriaId, UsuarioId = UsuarioId });
        _context.SaveChanges();
    }

    [Test]
    public async Task Index_SemFiltro_UsaMesEAnoAtuais_ECalculaTotais()
    {
        var hoje = DateTime.Today;
        AdicionarTransacao(_categoriaReceitaId, TipoTransacao.Receita, 500m, hoje);
        AdicionarTransacao(_categoriaDespesaId, TipoTransacao.Despesa, 150m, hoje);
        AdicionarTransacao(_categoriaReceitaId, TipoTransacao.Receita, 999m, hoje.AddYears(-2)); // fora do período

        var resultado = await _controller.Index(null, null) as ViewResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(_controller.ViewData["Mes"], Is.EqualTo(hoje.Month));
        Assert.That(_controller.ViewData["Ano"], Is.EqualTo(hoje.Year));
        Assert.That(_controller.ViewData["TotalReceitas"], Is.EqualTo(500m));
        Assert.That(_controller.ViewData["TotalDespesas"], Is.EqualTo(150m));
    }

    [Test]
    public async Task Index_ComFiltro_UsaMesEAnoInformados_OrdenaPorDataDecrescente()
    {
        AdicionarTransacao(_categoriaReceitaId, TipoTransacao.Receita, 100m, new DateTime(2026, 3, 1));
        AdicionarTransacao(_categoriaReceitaId, TipoTransacao.Receita, 200m, new DateTime(2026, 3, 20));

        var resultado = await _controller.Index(3, 2026) as ViewResult;
        var lista = resultado!.Model as List<Transacao>;

        Assert.That(lista, Has.Count.EqualTo(2));
        Assert.That(lista![0].Valor, Is.EqualTo(200m)); // mais recente primeiro
    }

    [Test]
    public async Task Index_IgnoraTransacoesDeOutraUsuaria()
    {
        _context.Categorias.Add(new Categoria { Id = 500, Nome = "De outra", Tipo = TipoTransacao.Receita, UsuarioId = 777 });
        await _context.SaveChangesAsync();
        _context.Transacoes.Add(new Transacao { Descricao = "Alheia", Valor = 9999m, Tipo = TipoTransacao.Receita, Data = DateTime.Today, CategoriaId = 500, UsuarioId = 777 });
        await _context.SaveChangesAsync();

        var resultado = await _controller.Index(null, null) as ViewResult;
        var lista = resultado!.Model as List<Transacao>;

        Assert.That(lista, Is.Empty);
    }

    [Test]
    public async Task Create_Get_CarregaCategoriasDaUsuariaNoViewBag()
    {
        var resultado = await _controller.Create() as ViewResult;

        Assert.That(resultado, Is.Not.Null);
        // Vendas, Aluguel e o pró-labore, criado na hora para quem ainda não tinha (decisão 10).
        var categorias = _controller.ViewBag.Categorias as List<SelectListItem>;
        Assert.That(categorias, Has.Count.EqualTo(3));
        var proLabore = await _context.Categorias.SingleAsync(c => c.Nome == CategoriasPadrao.NomeProLabore);
        Assert.That(proLabore.NaturezaDespesa, Is.EqualTo(NaturezaDespesa.DespesaFixa));
        Assert.That((int?)_controller.ViewBag.ProLaboreId, Is.EqualTo(proLabore.Id));
        Assert.That((resultado!.Model as Transacao)!.Data, Is.EqualTo(DateTime.Today));
    }

    [Test]
    public async Task Create_Post_CategoriaIncompativelComTipo_AdicionaErro_ERecarregaCategorias()
    {
        var transacao = new Transacao { Descricao = "X", Valor = 10m, Tipo = TipoTransacao.Receita, Data = DateTime.Today, CategoriaId = _categoriaDespesaId };

        var resultado = await _controller.Create(transacao) as ViewResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(_controller.ModelState.IsValid, Is.False);
        Assert.That(_controller.ViewBag.Categorias, Is.Not.Null);
    }

    [Test]
    public async Task Create_Post_ModelStateInvalidoPorOutroMotivo_RecarregaCategorias_ERetornaView()
    {
        var transacao = new Transacao { Descricao = "X", Valor = 0m, Tipo = TipoTransacao.Receita, Data = DateTime.Today, CategoriaId = _categoriaReceitaId };
        _controller.ModelState.AddModelError("Valor", "O valor deve ser maior que zero.");

        var resultado = await _controller.Create(transacao) as ViewResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(_controller.ViewBag.Categorias, Is.Not.Null);
    }

    [Test]
    public async Task Create_Post_Valido_DefineUsuarioId_Salva_ERedireciona()
    {
        var transacao = new Transacao { Descricao = "Venda", Valor = 100m, Tipo = TipoTransacao.Receita, Data = DateTime.Today, CategoriaId = _categoriaReceitaId };

        var resultado = await _controller.Create(transacao) as RedirectToActionResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.ActionName, Is.EqualTo("Index"));
        var salva = await _context.Transacoes.SingleAsync();
        Assert.That(salva.UsuarioId, Is.EqualTo(UsuarioId));
    }

    [Test]
    public async Task Edit_Get_IdNulo_RetornaNotFound()
    {
        Assert.That(await _controller.Edit(null), Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Edit_Get_NaoEncontrada_RetornaNotFound()
    {
        Assert.That(await _controller.Edit(999), Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Edit_Get_Encontrada_RetornaViewComACategoriaCarregada()
    {
        var transacao = new Transacao { Descricao = "X", Valor = 10m, Tipo = TipoTransacao.Receita, Data = DateTime.Today, CategoriaId = _categoriaReceitaId, UsuarioId = UsuarioId };
        _context.Transacoes.Add(transacao);
        await _context.SaveChangesAsync();

        var resultado = await _controller.Edit(transacao.Id) as ViewResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(_controller.ViewBag.Categorias, Is.Not.Null);
    }

    [Test]
    public async Task Edit_Post_IdDiferente_RetornaNotFound()
    {
        var transacao = new Transacao { Id = 1, Descricao = "X", Valor = 10m, Tipo = TipoTransacao.Receita, CategoriaId = _categoriaReceitaId };

        Assert.That(await _controller.Edit(2, transacao), Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Edit_Post_NaoEncontrada_RetornaNotFound()
    {
        var transacao = new Transacao { Id = 123, Descricao = "X", Valor = 10m, Tipo = TipoTransacao.Receita, CategoriaId = _categoriaReceitaId };

        Assert.That(await _controller.Edit(123, transacao), Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Edit_Post_CategoriaIncompativel_AdicionaErro_ERecarregaCategorias()
    {
        var existente = new Transacao { Descricao = "X", Valor = 10m, Tipo = TipoTransacao.Receita, Data = DateTime.Today, CategoriaId = _categoriaReceitaId, UsuarioId = UsuarioId };
        _context.Transacoes.Add(existente);
        await _context.SaveChangesAsync();

        var enviado = new Transacao { Id = existente.Id, Descricao = "X", Valor = 10m, Tipo = TipoTransacao.Receita, Data = DateTime.Today, CategoriaId = _categoriaDespesaId };

        var resultado = await _controller.Edit(existente.Id, enviado) as ViewResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(_controller.ModelState.IsValid, Is.False);
    }

    [Test]
    public async Task Edit_Post_Valido_AtualizaCampos_ERedireciona()
    {
        var existente = new Transacao { Descricao = "Original", Valor = 10m, Tipo = TipoTransacao.Receita, Data = DateTime.Today, CategoriaId = _categoriaReceitaId, UsuarioId = UsuarioId };
        _context.Transacoes.Add(existente);
        await _context.SaveChangesAsync();

        var enviado = new Transacao { Id = existente.Id, Descricao = "Atualizada", Valor = 250m, Tipo = TipoTransacao.Receita, Data = new DateTime(2026, 5, 5), CategoriaId = _categoriaReceitaId };

        var resultado = await _controller.Edit(existente.Id, enviado) as RedirectToActionResult;

        Assert.That(resultado, Is.Not.Null);
        var atualizada = await _context.Transacoes.FindAsync(existente.Id);
        Assert.That(atualizada!.Descricao, Is.EqualTo("Atualizada"));
        Assert.That(atualizada.Valor, Is.EqualTo(250m));
    }

    [Test]
    public async Task Delete_Get_IdNulo_RetornaNotFound()
    {
        Assert.That(await _controller.Delete(null), Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Delete_Get_NaoEncontrada_RetornaNotFound()
    {
        Assert.That(await _controller.Delete(999), Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Delete_Get_Encontrada_RetornaViewComCategoriaCarregada()
    {
        var transacao = new Transacao { Descricao = "X", Valor = 10m, Tipo = TipoTransacao.Receita, Data = DateTime.Today, CategoriaId = _categoriaReceitaId, UsuarioId = UsuarioId };
        _context.Transacoes.Add(transacao);
        await _context.SaveChangesAsync();

        var resultado = await _controller.Delete(transacao.Id) as ViewResult;
        var modelo = resultado!.Model as Transacao;

        Assert.That(modelo!.Categoria, Is.Not.Null);
    }

    [Test]
    public async Task DeleteConfirmed_NaoEncontrada_RetornaNotFound()
    {
        Assert.That(await _controller.DeleteConfirmed(999), Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task DeleteConfirmed_Encontrada_Exclui_ERedireciona()
    {
        var transacao = new Transacao { Descricao = "X", Valor = 10m, Tipo = TipoTransacao.Receita, Data = DateTime.Today, CategoriaId = _categoriaReceitaId, UsuarioId = UsuarioId };
        _context.Transacoes.Add(transacao);
        await _context.SaveChangesAsync();

        var resultado = await _controller.DeleteConfirmed(transacao.Id) as RedirectToActionResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(await _context.Transacoes.FindAsync(transacao.Id), Is.Null);
    }

    // ---------------------------------------------------------------
    // Lançamento rápido: descrição opcional, "salvar e lançar outro"
    // e retirada pessoal
    // ---------------------------------------------------------------

    [Test]
    public async Task Create_Post_DescricaoVazia_UsaONomeDaCategoria()
    {
        var transacao = new Transacao { Descricao = "", Valor = 80m, Tipo = TipoTransacao.Despesa, Data = DateTime.Today, CategoriaId = _categoriaDespesaId };
        // Simula o erro "Descrição obrigatória" que o MVC registra antes da action.
        _controller.ModelState.AddModelError("Descricao", "Informe uma descrição.");

        var resultado = await _controller.Create(transacao) as RedirectToActionResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That((await _context.Transacoes.SingleAsync()).Descricao, Is.EqualTo("Aluguel"));
    }

    [Test]
    public async Task Create_Post_DescricaoInformada_NaoESubstituida()
    {
        var transacao = new Transacao { Descricao = "Aluguel de novembro", Valor = 80m, Tipo = TipoTransacao.Despesa, Data = DateTime.Today, CategoriaId = _categoriaDespesaId };

        await _controller.Create(transacao);

        Assert.That((await _context.Transacoes.SingleAsync()).Descricao, Is.EqualTo("Aluguel de novembro"));
    }

    [Test]
    public async Task Create_Post_AcaoOutro_SalvaERedirecionaParaNovoLancamento_ComMensagem()
    {
        var transacao = new Transacao { Descricao = "Venda", Valor = 100m, Tipo = TipoTransacao.Receita, Data = DateTime.Today, CategoriaId = _categoriaReceitaId };

        var resultado = await _controller.Create(transacao, acao: "outro") as RedirectToActionResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.ActionName, Is.EqualTo("Create"));
        Assert.That(_controller.TempData["Sucesso"], Is.EqualTo("Lançamento salvo. Pode lançar o próximo."));
        Assert.That(await _context.Transacoes.CountAsync(), Is.EqualTo(1));
    }

    private async Task<int> CriarProLaboreAsync() =>
        await CategoriasPadrao.GarantirProLaboreAsync(_context, UsuarioId);

    [Test]
    public async Task Create_Get_ProLabore_EhIdempotente_NaoDuplicaACategoria()
    {
        await _controller.Create();
        await _controller.Create();

        Assert.That(await _context.Categorias.CountAsync(c => c.Nome == CategoriasPadrao.NomeProLabore), Is.EqualTo(1));
    }

    [Test]
    public async Task Create_Get_NaoUsaOProLaboreDeOutraUsuaria()
    {
        _context.Categorias.Add(new Categoria { Id = 600, Nome = CategoriasPadrao.NomeProLabore, Tipo = TipoTransacao.Despesa, NaturezaDespesa = NaturezaDespesa.DespesaFixa, UsuarioId = 777 });
        await _context.SaveChangesAsync();

        await _controller.Create();

        Assert.That((int?)_controller.ViewBag.ProLaboreId, Is.Not.EqualTo(600));
        Assert.That(await _context.Categorias.CountAsync(c => c.Nome == CategoriasPadrao.NomeProLabore && c.UsuarioId == UsuarioId), Is.EqualTo(1));
    }

    [Test]
    public async Task Create_Post_ProLabore_PrimeiroDoMes_SalvaSemAviso()
    {
        var proLaboreId = await CriarProLaboreAsync();
        var transacao = new Transacao { Descricao = "", Valor = 2000m, Tipo = TipoTransacao.Despesa, Data = new DateTime(2026, 10, 31), CategoriaId = proLaboreId };

        var resultado = await _controller.Create(transacao) as RedirectToActionResult;

        Assert.That(resultado, Is.Not.Null);
        var salva = await _context.Transacoes.SingleAsync();
        Assert.That(salva.Descricao, Is.EqualTo(CategoriasPadrao.NomeProLabore));
        Assert.That(_controller.ViewData["AvisoProLabore"], Is.Null);
    }

    [Test]
    public async Task Create_Post_ProLaboreRepetidoNoMes_Avisa_ENaoSalva()
    {
        var proLaboreId = await CriarProLaboreAsync();
        AdicionarTransacao(proLaboreId, TipoTransacao.Despesa, 2000m, new DateTime(2026, 10, 31));
        var outro = new Transacao { Descricao = "", Valor = 2000m, Tipo = TipoTransacao.Despesa, Data = new DateTime(2026, 10, 15), CategoriaId = proLaboreId };

        var resultado = await _controller.Create(outro) as ViewResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That((string?)_controller.ViewData["AvisoProLabore"], Does.Contain("já lançou pró-labore em outubro/2026").And.Contain("2.000,00"));
        Assert.That(await _context.Transacoes.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task Create_Post_ProLaboreRepetido_ConfirmadoPelaUsuaria_Salva()
    {
        var proLaboreId = await CriarProLaboreAsync();
        AdicionarTransacao(proLaboreId, TipoTransacao.Despesa, 2000m, new DateTime(2026, 10, 31));
        var outro = new Transacao { Descricao = "Complemento", Valor = 300m, Tipo = TipoTransacao.Despesa, Data = new DateTime(2026, 10, 31), CategoriaId = proLaboreId };

        var resultado = await _controller.Create(outro, confirmarProLabore: TransacoesController.ConfirmacaoProLabore) as RedirectToActionResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(await _context.Transacoes.CountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task Create_Post_ProLaboreEmOutroMes_NaoAvisa()
    {
        var proLaboreId = await CriarProLaboreAsync();
        AdicionarTransacao(proLaboreId, TipoTransacao.Despesa, 2000m, new DateTime(2026, 9, 30));
        var outubro = new Transacao { Descricao = "", Valor = 2000m, Tipo = TipoTransacao.Despesa, Data = new DateTime(2026, 10, 31), CategoriaId = proLaboreId };

        var resultado = await _controller.Create(outubro) as RedirectToActionResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(await _context.Transacoes.CountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task Create_Post_OutraDespesaNoMesDoProLabore_NaoAvisa()
    {
        var proLaboreId = await CriarProLaboreAsync();
        AdicionarTransacao(proLaboreId, TipoTransacao.Despesa, 2000m, new DateTime(2026, 10, 31));
        var aluguel = new Transacao { Descricao = "", Valor = 300m, Tipo = TipoTransacao.Despesa, Data = new DateTime(2026, 10, 31), CategoriaId = _categoriaDespesaId };

        var resultado = await _controller.Create(aluguel) as RedirectToActionResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(_controller.ViewData["AvisoProLabore"], Is.Null);
    }

    [Test]
    public async Task Create_Post_DescricaoVaziaECategoriaInvalida_ContinuaInvalido()
    {
        var transacao = new Transacao { Descricao = "", Valor = 10m, Tipo = TipoTransacao.Receita, Data = DateTime.Today, CategoriaId = _categoriaDespesaId };
        _controller.ModelState.AddModelError("Descricao", "Informe uma descrição.");

        var resultado = await _controller.Create(transacao) as ViewResult;

        Assert.That(resultado, Is.Not.Null);
        Assert.That(_controller.ModelState.IsValid, Is.False);
        Assert.That(await _context.Transacoes.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task Index_ProLaboreELancamentoAntigoDeRetirada_EntramNasDespesas()
    {
        var proLaboreId = await CriarProLaboreAsync();
        var antiga = new Categoria { Nome = "Retirada pessoal", Tipo = TipoTransacao.Despesa, NaturezaDespesa = NaturezaDespesa.RetiradaPessoal, UsuarioId = UsuarioId };
        _context.Categorias.Add(antiga);
        _context.SaveChanges();

        var hoje = DateTime.Today;
        AdicionarTransacao(_categoriaReceitaId, TipoTransacao.Receita, 1000m, hoje);
        AdicionarTransacao(_categoriaDespesaId, TipoTransacao.Despesa, 300m, hoje);
        AdicionarTransacao(proLaboreId, TipoTransacao.Despesa, 200m, hoje);
        AdicionarTransacao(antiga.Id, TipoTransacao.Despesa, 50m, hoje);

        await _controller.Index(null, null);

        Assert.That(_controller.ViewData["TotalReceitas"], Is.EqualTo(1000m));
        Assert.That(_controller.ViewData["TotalDespesas"], Is.EqualTo(550m));
        Assert.That(_controller.ViewData.ContainsKey("TotalRetiradas"), Is.False);
    }
}
