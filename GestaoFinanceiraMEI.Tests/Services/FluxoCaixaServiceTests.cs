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
}
