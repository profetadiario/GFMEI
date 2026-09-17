using GestaoFinanceiraMEI.Data;
using GestaoFinanceiraMEI.Models;
using GestaoFinanceiraMEI.Services;
using GestaoFinanceiraMEI.Tests.TestHelpers;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Services;

[TestFixture]
public class FluxoCaixaMensalServiceTests
{
    private AppDbContext _context = null!;
    private FluxoCaixaMensalService _service = null!;
    private const int UsuarioId = 1;
    private const int CategoriaVendas = 1;
    private const int CategoriaAluguel = 2;

    [SetUp]
    public void SetUp()
    {
        _context = InMemoryDbContextFactory.Criar();
        _service = new FluxoCaixaMensalService(_context);

        _context.Usuarios.Add(new Usuario { Id = UsuarioId, Nome = "A", Email = "a@x.com", NomeNegocio = "Negócio A", SenhaHash = "h", SenhaSalt = "s" });
        _context.Categorias.AddRange(
            new Categoria { Id = CategoriaVendas, Nome = "Vendas", Tipo = TipoTransacao.Receita, UsuarioId = UsuarioId },
            new Categoria { Id = CategoriaAluguel, Nome = "Aluguel", Tipo = TipoTransacao.Despesa, NaturezaDespesa = NaturezaDespesa.DespesaFixa, UsuarioId = UsuarioId }
        );
        _context.SaveChanges();
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    private void AdicionarTransacao(int categoriaId, TipoTransacao tipo, decimal valor, int mes, int ano)
    {
        _context.Transacoes.Add(new Transacao
        {
            UsuarioId = UsuarioId,
            CategoriaId = categoriaId,
            Tipo = tipo,
            Valor = valor,
            Data = new DateTime(ano, mes, 10),
            Descricao = "Teste"
        });
        _context.SaveChanges();
    }

    [Test]
    public async Task ObterFluxoAnualAsync_AnoSemLancamentos_RetornaTudoZeradoParaOsDozeMeses()
    {
        var fluxo = await _service.ObterFluxoAnualAsync(UsuarioId, 2026);

        Assert.That(fluxo.Ano, Is.EqualTo(2026));
        Assert.That(fluxo.NomesMeses, Has.Length.EqualTo(12));
        Assert.That(fluxo.SaldoInicial, Has.All.EqualTo(0m));
        Assert.That(fluxo.SaldoFinal, Has.All.EqualTo(0m));
        Assert.That(fluxo.Entradas, Is.Empty);
        Assert.That(fluxo.Saidas, Is.Empty);
    }

    [Test]
    public async Task ObterFluxoAnualAsync_SaldoInicialDeJaneiro_VemDoAcumuladoDeAnosAnteriores()
    {
        AdicionarTransacao(CategoriaVendas, TipoTransacao.Receita, 5000m, 12, 2025);
        AdicionarTransacao(CategoriaAluguel, TipoTransacao.Despesa, 1000m, 12, 2025);

        var fluxo = await _service.ObterFluxoAnualAsync(UsuarioId, 2026);

        // Saldo acumulado até 31/12/2025 = 5000 - 1000 = 4000, que vira o
        // saldo inicial de janeiro/2026 (índice 0).
        Assert.That(fluxo.SaldoInicial[0], Is.EqualTo(4000m));
    }

    [Test]
    public async Task ObterFluxoAnualAsync_SaldoFinalDeUmMes_ViraSaldoInicialDoMesSeguinte()
    {
        AdicionarTransacao(CategoriaVendas, TipoTransacao.Receita, 1000m, 1, 2026);
        AdicionarTransacao(CategoriaAluguel, TipoTransacao.Despesa, 300m, 2, 2026);

        var fluxo = await _service.ObterFluxoAnualAsync(UsuarioId, 2026);

        Assert.That(fluxo.SaldoInicial[0], Is.EqualTo(0m)); // janeiro começa do zero
        Assert.That(fluxo.SaldoOperacional[0], Is.EqualTo(1000m));
        Assert.That(fluxo.SaldoFinal[0], Is.EqualTo(1000m));

        Assert.That(fluxo.SaldoInicial[1], Is.EqualTo(1000m)); // fevereiro herda o saldo final de janeiro
        Assert.That(fluxo.SaldoOperacional[1], Is.EqualTo(-300m));
        Assert.That(fluxo.SaldoFinal[1], Is.EqualTo(700m));

        // Meses sem lançamento continuam carregando o saldo adiante.
        Assert.That(fluxo.SaldoInicial[2], Is.EqualTo(700m));
        Assert.That(fluxo.SaldoFinal[11], Is.EqualTo(700m));
    }

    [Test]
    public async Task ObterFluxoAnualAsync_TotaisMensais_SomamEntradasESaidasDoMesCorreto()
    {
        AdicionarTransacao(CategoriaVendas, TipoTransacao.Receita, 100m, 5, 2026);
        AdicionarTransacao(CategoriaVendas, TipoTransacao.Receita, 50m, 5, 2026);
        AdicionarTransacao(CategoriaAluguel, TipoTransacao.Despesa, 30m, 5, 2026);

        var fluxo = await _service.ObterFluxoAnualAsync(UsuarioId, 2026);

        Assert.That(fluxo.TotalEntradas[4], Is.EqualTo(150m)); // maio = índice 4
        Assert.That(fluxo.TotalSaidas[4], Is.EqualTo(30m));
        Assert.That(fluxo.TotalEntradas[0], Is.EqualTo(0m));
    }

    [Test]
    public async Task ObterFluxoAnualAsync_AgrupaEntradasESaidasPorNomeDeCategoria()
    {
        AdicionarTransacao(CategoriaVendas, TipoTransacao.Receita, 100m, 1, 2026);
        AdicionarTransacao(CategoriaVendas, TipoTransacao.Receita, 200m, 2, 2026);
        AdicionarTransacao(CategoriaAluguel, TipoTransacao.Despesa, 50m, 1, 2026);

        var fluxo = await _service.ObterFluxoAnualAsync(UsuarioId, 2026);

        Assert.That(fluxo.Entradas, Has.Count.EqualTo(1));
        var linhaVendas = fluxo.Entradas.Single(l => l.Nome == "Vendas");
        Assert.That(linhaVendas.Valores[0], Is.EqualTo(100m));
        Assert.That(linhaVendas.Valores[1], Is.EqualTo(200m));

        Assert.That(fluxo.Saidas, Has.Count.EqualTo(1));
        var linhaAluguel = fluxo.Saidas.Single(l => l.Nome == "Aluguel");
        Assert.That(linhaAluguel.Valores[0], Is.EqualTo(50m));
    }

    [Test]
    public async Task ObterFluxoAnualAsync_TransacaoSemCategoria_AgrupaComo_SemCategoria()
    {
        _context.Transacoes.Add(new Transacao
        {
            UsuarioId = UsuarioId,
            CategoriaId = 9999,
            Tipo = TipoTransacao.Despesa,
            Valor = 75m,
            Data = new DateTime(2026, 3, 10),
            Descricao = "Categoria removida"
        });
        _context.SaveChanges();

        var fluxo = await _service.ObterFluxoAnualAsync(UsuarioId, 2026);

        var linha = fluxo.Saidas.Single();
        Assert.That(linha.Nome, Is.EqualTo("Sem categoria"));
        Assert.That(linha.Valores[2], Is.EqualTo(75m));
    }

    [Test]
    public async Task ObterFluxoAnualAsync_NomesDosMeses_SaoAsTresLetrasEmMaiusculoEmPortugues()
    {
        var fluxo = await _service.ObterFluxoAnualAsync(UsuarioId, 2026);

        Assert.That(fluxo.NomesMeses[0], Is.EqualTo("JAN"));
        Assert.That(fluxo.NomesMeses[11], Is.EqualTo("DEZ"));
    }

    [Test]
    public async Task ObterFluxoAnualAsync_IgnoraLancamentosDeOutraUsuaria()
    {
        const int outraUsuariaId = 2;
        _context.Usuarios.Add(new Usuario { Id = outraUsuariaId, Nome = "B", Email = "b@x.com", NomeNegocio = "Negócio B", SenhaHash = "h", SenhaSalt = "s" });
        _context.Categorias.Add(new Categoria { Id = 200, Nome = "Vendas B", Tipo = TipoTransacao.Receita, UsuarioId = outraUsuariaId });
        _context.SaveChanges();

        _context.Transacoes.Add(new Transacao
        {
            UsuarioId = outraUsuariaId,
            CategoriaId = 200,
            Tipo = TipoTransacao.Receita,
            Valor = 99999m,
            Data = new DateTime(2026, 1, 10),
            Descricao = "De outra usuária"
        });
        _context.SaveChanges();

        var fluxo = await _service.ObterFluxoAnualAsync(UsuarioId, 2026);

        Assert.That(fluxo.TotalEntradas[0], Is.EqualTo(0m));
        Assert.That(fluxo.Entradas, Is.Empty);
    }
}
