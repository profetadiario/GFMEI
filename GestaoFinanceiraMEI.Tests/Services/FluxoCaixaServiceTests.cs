using GestaoFinanceiraMEI.Data;
using GestaoFinanceiraMEI.Models;
using GestaoFinanceiraMEI.Services;
using GestaoFinanceiraMEI.Tests.TestHelpers;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Services;

[TestFixture]
public class FluxoCaixaServiceTests
{
    private AppDbContext _context = null!;
    private FluxoCaixaService _service = null!;
    private const int UsuarioId = 1;
    private const int OutraUsuariaId = 2;

    [SetUp]
    public void SetUp()
    {
        _context = InMemoryDbContextFactory.Criar();
        _service = new FluxoCaixaService(_context);

        _context.Usuarios.Add(new Usuario { Id = UsuarioId, Nome = "A", Email = "a@x.com", NomeNegocio = "Negócio A", SenhaHash = "h", SenhaSalt = "s" });
        _context.Usuarios.Add(new Usuario { Id = OutraUsuariaId, Nome = "B", Email = "b@x.com", NomeNegocio = "Negócio B", SenhaHash = "h", SenhaSalt = "s" });
        _context.Categorias.Add(new Categoria { Id = 1, Nome = "Vendas", Tipo = TipoTransacao.Receita, UsuarioId = UsuarioId });
        _context.Categorias.Add(new Categoria { Id = 2, Nome = "Aluguel", Tipo = TipoTransacao.Despesa, UsuarioId = UsuarioId });
        _context.SaveChanges();
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    private void AdicionarTransacao(int usuarioId, int categoriaId, TipoTransacao tipo, decimal valor, DateTime data)
    {
        _context.Transacoes.Add(new Transacao
        {
            UsuarioId = usuarioId,
            CategoriaId = categoriaId,
            Tipo = tipo,
            Valor = valor,
            Data = data,
            Descricao = "Teste"
        });
        _context.SaveChanges();
    }

    [Test]
    public async Task ObterResumoAsync_SemLancamentos_RetornaZerados()
    {
        var resumo = await _service.ObterResumoAsync(UsuarioId);

        Assert.That(resumo.TotalReceitas, Is.EqualTo(0));
        Assert.That(resumo.TotalDespesas, Is.EqualTo(0));
        Assert.That(resumo.Saldo, Is.EqualTo(0));
        Assert.That(resumo.HistoricoMensal, Is.Empty);
    }

    [Test]
    public async Task ObterResumoAsync_SomaReceitasEDespesasCorretamente_ECalculaSaldo()
    {
        AdicionarTransacao(UsuarioId, 1, TipoTransacao.Receita, 1000m, new DateTime(2026, 3, 10));
        AdicionarTransacao(UsuarioId, 2, TipoTransacao.Despesa, 300m, new DateTime(2026, 3, 15));

        var resumo = await _service.ObterResumoAsync(UsuarioId);

        Assert.That(resumo.TotalReceitas, Is.EqualTo(1000m));
        Assert.That(resumo.TotalDespesas, Is.EqualTo(300m));
        Assert.That(resumo.Saldo, Is.EqualTo(700m));
    }

    [Test]
    public async Task ObterResumoAsync_FiltraPorUsuaria_IgnorandoLancamentosDeOutraConta()
    {
        AdicionarTransacao(UsuarioId, 1, TipoTransacao.Receita, 1000m, new DateTime(2026, 3, 10));
        AdicionarTransacao(OutraUsuariaId, 1, TipoTransacao.Receita, 9999m, new DateTime(2026, 3, 10));

        var resumo = await _service.ObterResumoAsync(UsuarioId);

        Assert.That(resumo.TotalReceitas, Is.EqualTo(1000m));
    }

    [Test]
    public async Task ObterResumoAsync_FiltraPorMesEAno_QuandoInformados()
    {
        AdicionarTransacao(UsuarioId, 1, TipoTransacao.Receita, 100m, new DateTime(2026, 1, 5));
        AdicionarTransacao(UsuarioId, 1, TipoTransacao.Receita, 200m, new DateTime(2026, 3, 5));
        AdicionarTransacao(UsuarioId, 1, TipoTransacao.Receita, 300m, new DateTime(2025, 3, 5));

        var resumo = await _service.ObterResumoAsync(UsuarioId, mes: 3, ano: 2026);

        Assert.That(resumo.TotalReceitas, Is.EqualTo(200m));
    }

    [Test]
    public async Task ObterResumoAsync_FiltraSomentePorAno_QuandoMesNaoInformado()
    {
        AdicionarTransacao(UsuarioId, 1, TipoTransacao.Receita, 100m, new DateTime(2026, 1, 5));
        AdicionarTransacao(UsuarioId, 1, TipoTransacao.Receita, 200m, new DateTime(2026, 6, 5));
        AdicionarTransacao(UsuarioId, 1, TipoTransacao.Receita, 300m, new DateTime(2025, 6, 5));

        var resumo = await _service.ObterResumoAsync(UsuarioId, mes: null, ano: 2026);

        Assert.That(resumo.TotalReceitas, Is.EqualTo(300m));
    }

    [Test]
    public async Task ObterResumoAsync_HistoricoMensal_AgrupaPorMesEOrdenaCronologicamente()
    {
        AdicionarTransacao(UsuarioId, 1, TipoTransacao.Receita, 100m, new DateTime(2026, 3, 5));
        AdicionarTransacao(UsuarioId, 2, TipoTransacao.Despesa, 40m, new DateTime(2026, 3, 20));
        AdicionarTransacao(UsuarioId, 1, TipoTransacao.Receita, 200m, new DateTime(2026, 1, 5));

        var resumo = await _service.ObterResumoAsync(UsuarioId);

        Assert.That(resumo.HistoricoMensal, Has.Count.EqualTo(2));
        Assert.That(resumo.HistoricoMensal[0].Receitas, Is.EqualTo(200m)); // janeiro vem antes de março
        Assert.That(resumo.HistoricoMensal[1].Receitas, Is.EqualTo(100m));
        Assert.That(resumo.HistoricoMensal[1].Despesas, Is.EqualTo(40m));
    }

    [Test]
    public async Task ObterResumoAsync_HistoricoMensal_MantemApenasOsUltimosSeisMeses()
    {
        for (var mes = 1; mes <= 8; mes++)
            AdicionarTransacao(UsuarioId, 1, TipoTransacao.Receita, mes * 10m, new DateTime(2026, mes, 1));

        var resumo = await _service.ObterResumoAsync(UsuarioId);

        Assert.That(resumo.HistoricoMensal, Has.Count.EqualTo(6));
        // Os dois primeiros meses (jan e fev) devem ter sido descartados
        // pelo TakeLast(6); o mais antigo restante deve ser março (mês 3).
        Assert.That(resumo.HistoricoMensal.First().Receitas, Is.EqualTo(30m));
        Assert.That(resumo.HistoricoMensal.Last().Receitas, Is.EqualTo(80m));
    }

    [Test]
    public async Task ObterResumoAsync_HistoricoMensal_IgnoraFiltroDeMesEAno_ConsiderandoTodosOsLancamentos()
    {
        // O histórico mensal do gráfico é sempre calculado sobre TODOS os
        // lançamentos da usuária, independentemente do filtro mes/ano
        // aplicado aos totais do período.
        AdicionarTransacao(UsuarioId, 1, TipoTransacao.Receita, 100m, new DateTime(2020, 1, 5));
        AdicionarTransacao(UsuarioId, 1, TipoTransacao.Receita, 200m, new DateTime(2026, 6, 5));

        var resumo = await _service.ObterResumoAsync(UsuarioId, mes: 6, ano: 2026);

        Assert.That(resumo.TotalReceitas, Is.EqualTo(200m));
        Assert.That(resumo.HistoricoMensal, Has.Count.EqualTo(2));
    }

    // ---------------------------------------------------------------
    // Retirada pessoal (pró-labore): fora de "Despesas", dentro do saldo
    // ---------------------------------------------------------------

    private const int CategoriaRetiradaPessoal = 10;

    private void AdicionarCategoriaRetiradaPessoal(int usuarioId = UsuarioId, int id = CategoriaRetiradaPessoal)
    {
        _context.Categorias.Add(new Categoria
        {
            Id = id,
            Nome = "Retirada pessoal (pró-labore)",
            Tipo = TipoTransacao.Despesa,
            NaturezaDespesa = NaturezaDespesa.RetiradaPessoal,
            UsuarioId = usuarioId
        });
        _context.SaveChanges();
    }

    [Test]
    public async Task ObterResumoAsync_RetiradaPessoal_FicaForaDeDespesas_MasReduzOSaldo()
    {
        AdicionarCategoriaRetiradaPessoal();
        AdicionarTransacao(UsuarioId, 1, TipoTransacao.Receita, 1000m, new DateTime(2026, 3, 10));
        AdicionarTransacao(UsuarioId, 2, TipoTransacao.Despesa, 300m, new DateTime(2026, 3, 15));
        AdicionarTransacao(UsuarioId, CategoriaRetiradaPessoal, TipoTransacao.Despesa, 200m, new DateTime(2026, 3, 20));

        var resumo = await _service.ObterResumoAsync(UsuarioId);

        Assert.That(resumo.TotalReceitas, Is.EqualTo(1000m));
        Assert.That(resumo.TotalDespesas, Is.EqualTo(300m));
        Assert.That(resumo.TotalRetiradasPessoais, Is.EqualTo(200m));
        Assert.That(resumo.Saldo, Is.EqualTo(500m));
    }

    [Test]
    public async Task ObterResumoAsync_RetiradaPessoal_AparecePropriaLinhaNoHistoricoMensal()
    {
        AdicionarCategoriaRetiradaPessoal();
        AdicionarTransacao(UsuarioId, 2, TipoTransacao.Despesa, 300m, new DateTime(2026, 3, 15));
        AdicionarTransacao(UsuarioId, CategoriaRetiradaPessoal, TipoTransacao.Despesa, 200m, new DateTime(2026, 3, 20));

        var resumo = await _service.ObterResumoAsync(UsuarioId);

        Assert.That(resumo.HistoricoMensal, Has.Count.EqualTo(1));
        Assert.That(resumo.HistoricoMensal[0].Despesas, Is.EqualTo(300m));
        Assert.That(resumo.HistoricoMensal[0].RetiradasPessoais, Is.EqualTo(200m));
    }

    [Test]
    public async Task ObterResumoAsync_RetiradaPessoalDeOutraUsuaria_NaoAfetaOResumoDestaUsuaria()
    {
        // A categoria de retirada da outra usuária tem outro Id: um lançamento
        // desta usuária em categoria comum continua sendo despesa do negócio.
        AdicionarCategoriaRetiradaPessoal(OutraUsuariaId, 11);
        AdicionarTransacao(UsuarioId, 2, TipoTransacao.Despesa, 300m, new DateTime(2026, 3, 15));
        AdicionarTransacao(OutraUsuariaId, 11, TipoTransacao.Despesa, 999m, new DateTime(2026, 3, 15));

        var resumo = await _service.ObterResumoAsync(UsuarioId);

        Assert.That(resumo.TotalDespesas, Is.EqualTo(300m));
        Assert.That(resumo.TotalRetiradasPessoais, Is.EqualTo(0m));
    }

    // ---------------------------------------------------------------
    // ObterPainelAsync: tudo o que o Painel precisa em uma leitura
    // ---------------------------------------------------------------

    private static readonly DateTime HojeFixo = new(2026, 11, 18);

    private void AdicionarCategoriaDespesa(int id, string nome, int usuarioId = UsuarioId)
    {
        _context.Categorias.Add(new Categoria { Id = id, Nome = nome, Tipo = TipoTransacao.Despesa, UsuarioId = usuarioId });
        _context.SaveChanges();
    }

    [Test]
    public async Task ObterPainelAsync_SemLancamentos_RetornaTudoVazio()
    {
        var painel = await _service.ObterPainelAsync(UsuarioId, HojeFixo);

        Assert.That(painel.ResumoAnoCorrente.Saldo, Is.EqualTo(0m));
        Assert.That(painel.ResumoGeral.Saldo, Is.EqualTo(0m));
        Assert.That(painel.GastosPorCategoriaDoMes, Is.Empty);
        Assert.That(painel.MaiorGastoDoMes, Is.Null);
        Assert.That(painel.UltimoLancamento, Is.Null);
    }

    [Test]
    public async Task ObterPainelAsync_SeparaResumoDoAnoCorrenteDoResumoGeral()
    {
        AdicionarTransacao(UsuarioId, 1, TipoTransacao.Receita, 400m, new DateTime(2025, 6, 10));
        AdicionarTransacao(UsuarioId, 1, TipoTransacao.Receita, 1000m, new DateTime(2026, 3, 10));
        AdicionarTransacao(UsuarioId, 2, TipoTransacao.Despesa, 300m, new DateTime(2026, 3, 15));

        var painel = await _service.ObterPainelAsync(UsuarioId, HojeFixo);

        Assert.That(painel.ResumoAnoCorrente.TotalReceitas, Is.EqualTo(1000m));
        Assert.That(painel.ResumoAnoCorrente.TotalDespesas, Is.EqualTo(300m));
        Assert.That(painel.ResumoGeral.TotalReceitas, Is.EqualTo(1400m));
        Assert.That(painel.ResumoGeral.Saldo, Is.EqualTo(1100m));
    }

    [Test]
    public async Task ObterPainelAsync_GastosPorCategoria_SomenteDoMesCorrente_OrdenadosDoMaiorParaOMenor_ComPercentual()
    {
        AdicionarCategoriaDespesa(20, "Insumos");
        AdicionarCategoriaDespesa(21, "Transporte");
        AdicionarTransacao(UsuarioId, 20, TipoTransacao.Despesa, 600m, new DateTime(2026, 11, 3));
        AdicionarTransacao(UsuarioId, 2, TipoTransacao.Despesa, 300m, new DateTime(2026, 11, 5));
        AdicionarTransacao(UsuarioId, 21, TipoTransacao.Despesa, 100m, new DateTime(2026, 11, 9));
        AdicionarTransacao(UsuarioId, 20, TipoTransacao.Despesa, 999m, new DateTime(2026, 10, 3)); // outro mês
        AdicionarTransacao(UsuarioId, 1, TipoTransacao.Receita, 5000m, new DateTime(2026, 11, 3)); // receita não é gasto

        var painel = await _service.ObterPainelAsync(UsuarioId, HojeFixo);

        Assert.That(painel.GastosPorCategoriaDoMes.Select(g => g.Categoria), Is.EqualTo(new[] { "Insumos", "Aluguel", "Transporte" }));
        Assert.That(painel.GastosPorCategoriaDoMes.Select(g => g.Valor), Is.EqualTo(new[] { 600m, 300m, 100m }));
        Assert.That(painel.GastosPorCategoriaDoMes.Select(g => g.Percentual), Is.EqualTo(new[] { 60m, 30m, 10m }));
        Assert.That(painel.MaiorGastoDoMes!.Categoria, Is.EqualTo("Insumos"));
    }

    [Test]
    public async Task ObterPainelAsync_GastosPorCategoria_NaoIncluiRetiradaPessoal()
    {
        AdicionarCategoriaRetiradaPessoal();
        AdicionarTransacao(UsuarioId, 2, TipoTransacao.Despesa, 300m, new DateTime(2026, 11, 5));
        AdicionarTransacao(UsuarioId, CategoriaRetiradaPessoal, TipoTransacao.Despesa, 2000m, new DateTime(2026, 11, 6));

        var painel = await _service.ObterPainelAsync(UsuarioId, HojeFixo);

        Assert.That(painel.GastosPorCategoriaDoMes, Has.Count.EqualTo(1));
        Assert.That(painel.GastosPorCategoriaDoMes[0].Categoria, Is.EqualTo("Aluguel"));
        Assert.That(painel.GastosPorCategoriaDoMes[0].Percentual, Is.EqualTo(100m));
        Assert.That(painel.ResumoAnoCorrente.TotalRetiradasPessoais, Is.EqualTo(2000m));
    }

    [Test]
    public async Task ObterPainelAsync_MaisDeSeisCategorias_AgrupaAsMenoresEmOutrasCategorias_MantendoOMaiorGastoReal()
    {
        var valores = new[] { 700m, 600m, 500m, 400m, 300m, 200m, 100m }; // total 2800
        for (var i = 0; i < valores.Length; i++)
        {
            AdicionarCategoriaDespesa(30 + i, $"Categoria {i + 1}");
            AdicionarTransacao(UsuarioId, 30 + i, TipoTransacao.Despesa, valores[i], new DateTime(2026, 11, 2));
        }

        var painel = await _service.ObterPainelAsync(UsuarioId, HojeFixo);

        Assert.That(painel.GastosPorCategoriaDoMes, Has.Count.EqualTo(6));
        Assert.That(painel.GastosPorCategoriaDoMes[4].Categoria, Is.EqualTo("Categoria 5"));
        Assert.That(painel.GastosPorCategoriaDoMes[5].Categoria, Is.EqualTo("Outras categorias"));
        Assert.That(painel.GastosPorCategoriaDoMes[5].Valor, Is.EqualTo(300m)); // 200 + 100
        Assert.That(painel.GastosPorCategoriaDoMes[5].Percentual, Is.EqualTo(10.7m)); // 300 / 2800
        Assert.That(painel.MaiorGastoDoMes!.Categoria, Is.EqualTo("Categoria 1"));
        Assert.That(painel.MaiorGastoDoMes.Percentual, Is.EqualTo(25m)); // 700 / 2800
    }

    [Test]
    public async Task ObterPainelAsync_DespesaComCategoriaInexistente_VaiParaSemCategoria()
    {
        AdicionarTransacao(UsuarioId, 999, TipoTransacao.Despesa, 80m, new DateTime(2026, 11, 2));

        var painel = await _service.ObterPainelAsync(UsuarioId, HojeFixo);

        Assert.That(painel.GastosPorCategoriaDoMes, Has.Count.EqualTo(1));
        Assert.That(painel.GastosPorCategoriaDoMes[0].Categoria, Is.EqualTo("Sem categoria"));
    }

    [Test]
    public async Task ObterPainelAsync_UltimoLancamento_IgnoraDatasFuturas()
    {
        AdicionarTransacao(UsuarioId, 1, TipoTransacao.Receita, 100m, new DateTime(2026, 11, 10));
        AdicionarTransacao(UsuarioId, 1, TipoTransacao.Receita, 100m, new DateTime(2026, 11, 30)); // agendado

        var painel = await _service.ObterPainelAsync(UsuarioId, HojeFixo);

        Assert.That(painel.UltimoLancamento, Is.EqualTo(new DateTime(2026, 11, 10)));
    }

    [Test]
    public async Task ObterPainelAsync_IgnoraLancamentosDeOutraUsuaria()
    {
        AdicionarTransacao(OutraUsuariaId, 1, TipoTransacao.Despesa, 999m, new DateTime(2026, 11, 17));

        var painel = await _service.ObterPainelAsync(UsuarioId, HojeFixo);

        Assert.That(painel.ResumoGeral.TotalDespesas, Is.EqualTo(0m));
        Assert.That(painel.GastosPorCategoriaDoMes, Is.Empty);
        Assert.That(painel.UltimoLancamento, Is.Null);
    }
}
