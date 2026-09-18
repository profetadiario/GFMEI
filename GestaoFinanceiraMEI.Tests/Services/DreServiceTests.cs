using GestaoFinanceiraMEI.Data;
using GestaoFinanceiraMEI.Models;
using GestaoFinanceiraMEI.Services;
using GestaoFinanceiraMEI.Tests.TestHelpers;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Services;

[TestFixture]
public class DreServiceTests
{
    private AppDbContext _context = null!;
    private DreService _service = null!;
    private const int UsuarioId = 1;

    private const int CategoriaVendas = 1;
    private const int CategoriaCmv = 2;
    private const int CategoriaDespesaFixa = 3;
    private const int CategoriaDespesaVariavel = 4;
    private const int CategoriaImposto = 5;

    [SetUp]
    public void SetUp()
    {
        _context = InMemoryDbContextFactory.Criar();
        _service = new DreService(_context);

        _context.Usuarios.Add(new Usuario { Id = UsuarioId, Nome = "A", Email = "a@x.com", NomeNegocio = "Negócio A", SenhaHash = "h", SenhaSalt = "s" });

        _context.Categorias.AddRange(
            new Categoria { Id = CategoriaVendas, Nome = "Vendas", Tipo = TipoTransacao.Receita, UsuarioId = UsuarioId },
            new Categoria { Id = CategoriaCmv, Nome = "Fornecedores", Tipo = TipoTransacao.Despesa, NaturezaDespesa = NaturezaDespesa.CustoMercadoriaVendida, UsuarioId = UsuarioId },
            new Categoria { Id = CategoriaDespesaFixa, Nome = "Aluguel", Tipo = TipoTransacao.Despesa, NaturezaDespesa = NaturezaDespesa.DespesaFixa, UsuarioId = UsuarioId },
            new Categoria { Id = CategoriaDespesaVariavel, Nome = "Transporte", Tipo = TipoTransacao.Despesa, NaturezaDespesa = NaturezaDespesa.DespesaVariavel, UsuarioId = UsuarioId },
            new Categoria { Id = CategoriaImposto, Nome = "DAS", Tipo = TipoTransacao.Despesa, NaturezaDespesa = NaturezaDespesa.DeducaoOuImposto, UsuarioId = UsuarioId }
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
    public async Task ObterDreAsync_SemLancamentos_RetornaTudoZerado_ENaoHouveMovimentacao()
    {
        var dre = await _service.ObterDreAsync(UsuarioId, 3, 2026);

        Assert.That(dre.ReceitaBrutaTotal, Is.EqualTo(0));
        Assert.That(dre.DeducoesEImpostos, Is.EqualTo(0));
        Assert.That(dre.CustoMercadoriaVendida, Is.EqualTo(0));
        Assert.That(dre.DespesasVariaveis, Is.EqualTo(0));
        Assert.That(dre.DespesasFixas, Is.EqualTo(0));
        Assert.That(dre.HouveMovimentacao, Is.False);
        Assert.That(dre.PodeExibirGraficoPizza, Is.False);
    }

    [Test]
    public async Task ObterDreAsync_ClassificaCadaDespesaPelaNaturezaDaCategoria()
    {
        AdicionarTransacao(CategoriaVendas, TipoTransacao.Receita, 10000m, 3, 2026);
        AdicionarTransacao(CategoriaCmv, TipoTransacao.Despesa, 3000m, 3, 2026);
        AdicionarTransacao(CategoriaDespesaFixa, TipoTransacao.Despesa, 1000m, 3, 2026);
        AdicionarTransacao(CategoriaDespesaVariavel, TipoTransacao.Despesa, 500m, 3, 2026);
        AdicionarTransacao(CategoriaImposto, TipoTransacao.Despesa, 600m, 3, 2026);

        var dre = await _service.ObterDreAsync(UsuarioId, 3, 2026);

        Assert.That(dre.ReceitaBrutaTotal, Is.EqualTo(10000m));
        Assert.That(dre.DeducoesEImpostos, Is.EqualTo(600m));
        Assert.That(dre.CustoMercadoriaVendida, Is.EqualTo(3000m));
        Assert.That(dre.DespesasFixas, Is.EqualTo(1000m));
        Assert.That(dre.DespesasVariaveis, Is.EqualTo(500m));
        Assert.That(dre.HouveMovimentacao, Is.True);
    }

    [Test]
    public async Task ObterDreAsync_CalculaCorretamenteAsLinhasDerivadasDaEstruturaDaDre()
    {
        AdicionarTransacao(CategoriaVendas, TipoTransacao.Receita, 10000m, 3, 2026);
        AdicionarTransacao(CategoriaImposto, TipoTransacao.Despesa, 1000m, 3, 2026);
        AdicionarTransacao(CategoriaCmv, TipoTransacao.Despesa, 3000m, 3, 2026);
        AdicionarTransacao(CategoriaDespesaVariavel, TipoTransacao.Despesa, 500m, 3, 2026);
        AdicionarTransacao(CategoriaDespesaFixa, TipoTransacao.Despesa, 2000m, 3, 2026);

        var dre = await _service.ObterDreAsync(UsuarioId, 3, 2026);

        // Receita Líquida = 10000 - 1000 = 9000
        Assert.That(dre.ReceitaLiquida, Is.EqualTo(9000m));
        // Lucro Bruto = 9000 - 3000 (CMV) = 6000
        Assert.That(dre.LucroBruto, Is.EqualTo(6000m));
        // Lucro Líquido = 6000 - 500 (variáveis) - 2000 (fixas) = 3500
        Assert.That(dre.LucroLiquido, Is.EqualTo(3500m));
        Assert.That(dre.PodeExibirGraficoPizza, Is.True);
    }

    [Test]
    public async Task ObterDreAsync_QuandoDespesasSuperamAReceita_LucroLiquidoNegativo_ENaoExibeGrafico()
    {
        AdicionarTransacao(CategoriaVendas, TipoTransacao.Receita, 1000m, 3, 2026);
        AdicionarTransacao(CategoriaDespesaFixa, TipoTransacao.Despesa, 5000m, 3, 2026);

        var dre = await _service.ObterDreAsync(UsuarioId, 3, 2026);

        Assert.That(dre.LucroLiquido, Is.EqualTo(-4000m));
        Assert.That(dre.HouveMovimentacao, Is.True);
        Assert.That(dre.PodeExibirGraficoPizza, Is.False);
    }

    [Test]
    public async Task ObterDreAsync_SemReceitaBruta_MesmoComLucroLiquidoZero_NaoExibeGrafico()
    {
        // PodeExibirGraficoPizza exige ReceitaBrutaTotal > 0: sem receita
        // nenhuma, uma "pizza" de 0% não faz sentido, mesmo que por
        // coincidência LucroLiquido seja >= 0 (0 - 0 - 0 = 0).
        var dre = await _service.ObterDreAsync(UsuarioId, 3, 2026);

        Assert.That(dre.ReceitaBrutaTotal, Is.EqualTo(0));
        Assert.That(dre.LucroLiquido, Is.EqualTo(0));
        Assert.That(dre.PodeExibirGraficoPizza, Is.False);
    }

    [Test]
    public async Task ObterDreAsync_FiltraPorMesEAno_IgnorandoOutrosPeriodos()
    {
        AdicionarTransacao(CategoriaVendas, TipoTransacao.Receita, 1000m, 3, 2026);
        AdicionarTransacao(CategoriaVendas, TipoTransacao.Receita, 5000m, 4, 2026);
        AdicionarTransacao(CategoriaVendas, TipoTransacao.Receita, 9000m, 3, 2025);

        var dre = await _service.ObterDreAsync(UsuarioId, 3, 2026);

        Assert.That(dre.ReceitaBrutaTotal, Is.EqualTo(1000m));
    }

    [Test]
    public async Task ObterDreAsync_TransacaoComCategoriaInexistente_TrataComoDespesaVariavel()
    {
        // Simula uma transação cuja categoria referenciada não existe mais
        // (Categoria de navegação fica null após o Include) — o
        // null-coalescing do DreService deve tratá-la como DespesaVariavel
        // em vez de lançar exceção ou perder o valor do cálculo.
        _context.Transacoes.Add(new Transacao
        {
            UsuarioId = UsuarioId,
            CategoriaId = 9999,
            Tipo = TipoTransacao.Despesa,
            Valor = 250m,
            Data = new DateTime(2026, 3, 10),
            Descricao = "Categoria removida"
        });
        _context.SaveChanges();

        var dre = await _service.ObterDreAsync(UsuarioId, 3, 2026);

        Assert.That(dre.DespesasVariaveis, Is.EqualTo(250m));
        Assert.That(dre.CustoMercadoriaVendida, Is.EqualTo(0));
    }

    [Test]
    public async Task ObterDreAsync_IgnoraLancamentosDeOutraUsuaria()
    {
        const int outraUsuariaId = 2;
        _context.Usuarios.Add(new Usuario { Id = outraUsuariaId, Nome = "B", Email = "b@x.com", NomeNegocio = "Negócio B", SenhaHash = "h", SenhaSalt = "s" });
        _context.Categorias.Add(new Categoria { Id = 100, Nome = "Vendas B", Tipo = TipoTransacao.Receita, UsuarioId = outraUsuariaId });
        _context.SaveChanges();

        _context.Transacoes.Add(new Transacao
        {
            UsuarioId = outraUsuariaId,
            CategoriaId = 100,
            Tipo = TipoTransacao.Receita,
            Valor = 99999m,
            Data = new DateTime(2026, 3, 10),
            Descricao = "De outra usuária"
        });
        _context.SaveChanges();

        var dre = await _service.ObterDreAsync(UsuarioId, 3, 2026);

        Assert.That(dre.ReceitaBrutaTotal, Is.EqualTo(0));
    }
}
