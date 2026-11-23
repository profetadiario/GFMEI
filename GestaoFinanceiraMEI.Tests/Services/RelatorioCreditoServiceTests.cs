using GestaoFinanceiraMEI.Data;
using GestaoFinanceiraMEI.Models;
using GestaoFinanceiraMEI.Services;
using GestaoFinanceiraMEI.Tests.TestHelpers;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Services;

[TestFixture]
public class RelatorioCreditoServiceTests
{
    private AppDbContext _context = null!;
    private RelatorioCreditoService _service = null!;
    private const int UsuarioId = 1;
    private const int OutraUsuariaId = 2;
    private static readonly DateTime Hoje = new(2026, 11, 19);

    private const int Vendas = 1, Aluguel = 2, Insumos = 3, Das = 4, Transporte = 5, Retirada = 6, VendasOutra = 7;

    [SetUp]
    public void SetUp()
    {
        _context = InMemoryDbContextFactory.Criar();
        _service = new RelatorioCreditoService(_context);

        _context.Usuarios.Add(new Usuario { Id = UsuarioId, Nome = "A", Email = "a@x.com", NomeNegocio = "A", SenhaHash = "h", SenhaSalt = "s" });
        _context.Usuarios.Add(new Usuario { Id = OutraUsuariaId, Nome = "B", Email = "b@x.com", NomeNegocio = "B", SenhaHash = "h", SenhaSalt = "s" });
        _context.Categorias.AddRange(
            new Categoria { Id = Vendas, Nome = "Vendas", Tipo = TipoTransacao.Receita, UsuarioId = UsuarioId },
            new Categoria { Id = Aluguel, Nome = "Aluguel", Tipo = TipoTransacao.Despesa, NaturezaDespesa = NaturezaDespesa.DespesaFixa, UsuarioId = UsuarioId },
            new Categoria { Id = Insumos, Nome = "Insumos", Tipo = TipoTransacao.Despesa, NaturezaDespesa = NaturezaDespesa.CustoMercadoriaVendida, UsuarioId = UsuarioId },
            new Categoria { Id = Das, Nome = "DAS", Tipo = TipoTransacao.Despesa, NaturezaDespesa = NaturezaDespesa.DeducaoOuImposto, UsuarioId = UsuarioId },
            new Categoria { Id = Transporte, Nome = "Transporte", Tipo = TipoTransacao.Despesa, NaturezaDespesa = NaturezaDespesa.DespesaVariavel, UsuarioId = UsuarioId },
            new Categoria { Id = Retirada, Nome = "Retirada pessoal", Tipo = TipoTransacao.Despesa, NaturezaDespesa = NaturezaDespesa.RetiradaPessoal, UsuarioId = UsuarioId },
            new Categoria { Id = VendasOutra, Nome = "Vendas", Tipo = TipoTransacao.Receita, UsuarioId = OutraUsuariaId });
        _context.SaveChanges();
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    private void Lancar(int categoriaId, TipoTransacao tipo, decimal valor, DateTime data, int usuarioId = UsuarioId)
    {
        _context.Transacoes.Add(new Transacao { UsuarioId = usuarioId, CategoriaId = categoriaId, Tipo = tipo, Valor = valor, Data = data, Descricao = "t" });
        _context.SaveChanges();
    }

    private void Receita(decimal v, DateTime d) => Lancar(Vendas, TipoTransacao.Receita, v, d);
    private void Despesa(int cat, decimal v, DateTime d) => Lancar(cat, TipoTransacao.Despesa, v, d);

    [Test]
    public async Task SemLancamentos_TemDozeMesesZerados_ESemDados()
    {
        var r = await _service.ObterRelatorioAsync(UsuarioId, Hoje);

        Assert.That(r.Meses, Has.Count.EqualTo(12));
        Assert.That(r.TemDados, Is.False);
        Assert.That(r.SobraMedia, Is.EqualTo(0));
        Assert.That(r.AvaliarParcela(100m), Is.EqualTo(SituacaoParcela.SemDados));
    }

    [Test]
    public async Task Janela_VaiDeDezembroDoAnoAnteriorANovembro()
    {
        var r = await _service.ObterRelatorioAsync(UsuarioId, Hoje);

        Assert.That((r.Meses[0].Mes, r.Meses[0].Ano), Is.EqualTo((12, 2025)));
        Assert.That((r.Meses[^1].Mes, r.Meses[^1].Ano), Is.EqualTo((11, 2026)));
        Assert.That(r.Meses[^1].Nome, Does.Contain("2026"));
    }

    [Test]
    public async Task ClassificaDespesasComoODre_EProLaboreEntraNasDespesasFixas()
    {
        var d = new DateTime(2026, 11, 5);
        Receita(4050m, d);
        Despesa(Das, 285m, d);
        Despesa(Insumos, 980m, d);
        Despesa(Transporte, 520m, d);
        Despesa(Aluguel, 750m, d);
        Despesa(Retirada, 500m, d); // lançamento antigo: vale como pró-labore (despesa fixa)

        var r = await _service.ObterRelatorioAsync(UsuarioId, Hoje);
        var nov = r.Meses[^1];

        Assert.That(nov.DeducoesEImpostos, Is.EqualTo(285m));
        Assert.That(nov.CustoMercadoriaVendida, Is.EqualTo(980m));
        Assert.That(nov.DespesasVariaveis, Is.EqualTo(520m));
        Assert.That(nov.DespesasFixas, Is.EqualTo(1250m));
        Assert.That(nov.LucroLiquido, Is.EqualTo(1015m));
        Assert.That(nov.Sobra, Is.EqualTo(1015m)); // sobra = receitas − despesas, pró-labore dentro
        Assert.That(r.LucroLiquido, Is.EqualTo(1015m));
    }

    [Test]
    public async Task MediasConsideramSoOsMesesAPartirDoPrimeiroLancamento()
    {
        // Começou a usar em setembro: set, out e nov = 3 meses considerados.
        Receita(3000m, new DateTime(2026, 9, 10));
        Despesa(Aluguel, 1000m, new DateTime(2026, 9, 10));
        Receita(3000m, new DateTime(2026, 10, 10));
        Despesa(Aluguel, 1000m, new DateTime(2026, 10, 10));
        // novembro sem lançamentos: conta como mês considerado (sobra 0)

        var r = await _service.ObterRelatorioAsync(UsuarioId, Hoje);

        Assert.That(r.MesesConsiderados, Is.EqualTo(3));
        Assert.That(r.FaturamentoTotal, Is.EqualTo(6000m));
        Assert.That(r.FaturamentoMedio, Is.EqualTo(2000m));
        Assert.That(r.SobraMedia, Is.EqualTo(Math.Round(4000m / 3, 2)));
        Assert.That(r.MesesComFaturamento, Is.EqualTo(2));
    }

    [Test]
    public async Task SaldoFinal_AcumulaInclusiveHistoricoAntesDaJanela()
    {
        Receita(1000m, new DateTime(2025, 6, 1)); // antes da janela
        Receita(500m, new DateTime(2026, 1, 10));
        Despesa(Retirada, 200m, new DateTime(2026, 2, 10));

        var r = await _service.ObterRelatorioAsync(UsuarioId, Hoje);

        Assert.That(r.Meses[0].SaldoFinal, Is.EqualTo(1000m)); // dez/2025
        Assert.That(r.Meses[1].SaldoFinal, Is.EqualTo(1500m)); // jan/2026
        Assert.That(r.Meses[2].SaldoFinal, Is.EqualTo(1300m)); // fev/2026: a despesa reduz o caixa
        Assert.That(r.SaldoAtual, Is.EqualTo(1300m));
        Assert.That(r.FaturamentoTotal, Is.EqualTo(500m)); // o de 2025-06 fica fora dos 12 meses
    }

    [Test]
    public async Task IgnoraLancamentosFuturosEDeOutraUsuaria()
    {
        Receita(1000m, new DateTime(2026, 11, 1));
        Receita(9999m, new DateTime(2026, 12, 1)); // mês que vem
        Lancar(VendasOutra, TipoTransacao.Receita, 5000m, new DateTime(2026, 11, 1), OutraUsuariaId);

        var r = await _service.ObterRelatorioAsync(UsuarioId, Hoje);

        Assert.That(r.FaturamentoTotal, Is.EqualTo(1000m));
        Assert.That(r.SaldoAtual, Is.EqualTo(1000m));
    }

    [Test]
    public async Task SomaAsCaptacoesDaUsuaria()
    {
        _context.Captacoes.AddRange(
            new CaptacaoRecurso { InstituicaoFinanceira = "X", Valor = 5000m, Finalidade = "f", UsuarioId = UsuarioId },
            new CaptacaoRecurso { InstituicaoFinanceira = "Y", Valor = 3200m, Finalidade = "f", UsuarioId = UsuarioId },
            new CaptacaoRecurso { InstituicaoFinanceira = "Z", Valor = 100m, Finalidade = "f", UsuarioId = OutraUsuariaId });
        _context.SaveChanges();

        var r = await _service.ObterRelatorioAsync(UsuarioId, Hoje);

        Assert.That(r.TotalCaptado, Is.EqualTo(8200m));
        Assert.That(r.QuantidadeCaptacoes, Is.EqualTo(2));
    }

    // ---------------- indicador de capacidade de pagamento ----------------

    private static RelatorioCredito ComSobraMedia(decimal sobra)
    {
        var r = new RelatorioCredito { MesesConsiderados = 1 };
        for (var i = 0; i < 11; i++) r.Meses.Add(new MesRelatorio());
        r.Meses.Add(new MesRelatorio { Receitas = sobra > 0 ? sobra : 0, DespesasFixas = sobra < 0 ? -sobra : 0 });
        return r;
    }

    [Test]
    public void ParcelaConfortavel_Eh30PorCentoDaSobraMedia()
    {
        Assert.That(ComSobraMedia(1000m).ParcelaConfortavel, Is.EqualTo(300m));
        Assert.That(ComSobraMedia(-200m).ParcelaConfortavel, Is.EqualTo(0m));
    }

    [TestCase(300, SituacaoParcela.Confortavel)]
    [TestCase(301, SituacaoParcela.Atencao)]
    [TestCase(600, SituacaoParcela.Atencao)]
    [TestCase(601, SituacaoParcela.Arriscada)]
    public void AvaliarParcela_UsaAsFaixasDe30E60PorCento(decimal parcela, SituacaoParcela esperada)
    {
        Assert.That(ComSobraMedia(1000m).AvaliarParcela(parcela), Is.EqualTo(esperada));
    }

    [Test]
    public void AvaliarParcela_SemSobraPositiva_EhArriscada()
    {
        var r = ComSobraMedia(-200m);

        Assert.That(r.AvaliarParcela(1m), Is.EqualTo(SituacaoParcela.Arriscada));
        Assert.That(r.PercentualDaSobra(1m), Is.Null);
    }

    [Test]
    public void PercentualDaSobra_ArredondaUmaCasa()
    {
        Assert.That(ComSobraMedia(1515m).PercentualDaSobra(400m), Is.EqualTo(26.4m));
    }
}
