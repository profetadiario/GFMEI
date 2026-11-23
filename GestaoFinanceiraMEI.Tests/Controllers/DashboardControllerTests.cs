using System.Globalization;
using GestaoFinanceiraMEI.Controllers;
using GestaoFinanceiraMEI.Data;
using GestaoFinanceiraMEI.Models;
using GestaoFinanceiraMEI.Services;
using GestaoFinanceiraMEI.Tests.TestHelpers;
using GestaoFinanceiraMEI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Controllers;

[TestFixture]
public class DashboardControllerTests
{
    private AppDbContext _context = null!;
    private Mock<IFluxoCaixaService> _fluxoCaixaServiceMock = null!;
    private Mock<IDreService> _dreServiceMock = null!;
    private DashboardController _controller = null!;
    private const int UsuarioId = ControllerTestExtensions.UsuarioIdPadrao;
    private static readonly DateTime Hoje = DateTime.Today;
    private static readonly CultureInfo PtBr = new("pt-BR");

    [SetUp]
    public void SetUp()
    {
        _context = InMemoryDbContextFactory.Criar();
        _fluxoCaixaServiceMock = new Mock<IFluxoCaixaService>();
        _dreServiceMock = new Mock<IDreService>();

        _fluxoCaixaServiceMock
            .Setup(s => s.ObterPainelAsync(UsuarioId, Hoje))
            .ReturnsAsync(new PainelFinanceiro
            {
                ResumoAnoCorrente = new ResumoFinanceiro { TotalReceitas = 5000m, TotalDespesas = 2000m },
                ResumoGeral = new ResumoFinanceiro
                {
                    TotalReceitas = 20000m,
                    TotalDespesas = 8000m,
                    HistoricoMensal = new List<ResumoMensal> { new() { Mes = "jan/2026", Receitas = 100m, Despesas = 50m } }
                },
                GastosPorCategoriaDoMes = new List<GastoPorCategoria>
                {
                    new() { Categoria = "Insumos", Valor = 700m, Percentual = 70m },
                    new() { Categoria = "Transporte", Valor = 300m, Percentual = 30m }
                },
                MaiorGastoDoMes = new GastoPorCategoria { Categoria = "Insumos", Valor = 700m, Percentual = 70m },
                UltimoLancamento = Hoje.AddDays(-5)
            });
        _dreServiceMock
            .Setup(d => d.ObterDreAsync(UsuarioId, Hoje.Month, Hoje.Year))
            .ReturnsAsync(new DreMensal { ReceitaBrutaTotal = 3000m, DespesasFixas = 500m });
            // LucroLiquido = 3000 - 500 = 2500

        _controller = new DashboardController(_context, _fluxoCaixaServiceMock.Object, _dreServiceMock.Object)
            .ComoUsuarioAutenticado(UsuarioId);
    }

    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
        _context.Dispose();
    }

    private static string PeriodoResumoEsperado()
    {
        var nomeMesInicio = new DateTime(Hoje.Year, 1, 1).ToString("MMMM", PtBr).ToUpper(PtBr);
        var nomeMesFim = new DateTime(Hoje.Year, Hoje.Month, 1).ToString("MMMM", PtBr).ToUpper(PtBr);
        return Hoje.Month == 1 ? nomeMesInicio : $"{nomeMesInicio} A {nomeMesFim}";
    }

    [Test]
    public async Task Index_MontaViewModel_ComResumoAcumuladoDoAno_ESaldoGeralHistorico()
    {
        var resultado = await _controller.Index() as ViewResult;
        var modelo = resultado!.Model as DashboardViewModel;

        Assert.That(modelo, Is.Not.Null);
        Assert.That(modelo!.ResumoDoMes.TotalReceitas, Is.EqualTo(5000m));
        Assert.That(modelo.ResumoDoMes.TotalDespesas, Is.EqualTo(2000m));
        Assert.That(modelo.SaldoGeral, Is.EqualTo(12000m)); // 20000 - 8000 (resumo geral, sem filtro)
        Assert.That(modelo.HistoricoMensal, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Index_RepassaGastosPorCategoria_MaiorGasto_EUltimoLancamento_DoPainel()
    {
        var resultado = await _controller.Index() as ViewResult;
        var modelo = resultado!.Model as DashboardViewModel;

        Assert.That(modelo!.GastosPorCategoria, Has.Count.EqualTo(2));
        Assert.That(modelo.MaiorGastoDoMes!.Categoria, Is.EqualTo("Insumos"));
        Assert.That(modelo.UltimoLancamento, Is.EqualTo(Hoje.AddDays(-5)));
        Assert.That(modelo.DataReferencia, Is.EqualTo(Hoje));
        Assert.That(modelo.MensagemLembrete, Is.EqualTo("Você não lança nada há 5 dias."));
    }

    [Test]
    public async Task Index_LeOsLancamentosEmUmaUnicaChamadaAoServico()
    {
        await _controller.Index();

        _fluxoCaixaServiceMock.Verify(s => s.ObterPainelAsync(UsuarioId, Hoje), Times.Once);
        _fluxoCaixaServiceMock.Verify(s => s.ObterResumoAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>()), Times.Never);
    }

    [Test]
    public async Task Index_PeriodoResumo_SeguemARegraJaneiroOuIntervalo()
    {
        var resultado = await _controller.Index() as ViewResult;
        var modelo = resultado!.Model as DashboardViewModel;

        Assert.That(modelo!.PeriodoResumo, Is.EqualTo(PeriodoResumoEsperado()));
    }

    [Test]
    public async Task Index_ValorAlcancadoMeta_UsaLucroLiquidoDoDre_NaoAReceitaBruta()
    {
        var resultado = await _controller.Index() as ViewResult;
        var modelo = resultado!.Model as DashboardViewModel;

        Assert.That(modelo!.ValorAlcancadoMeta, Is.EqualTo(2500m));
    }

    [Test]
    public async Task Index_SemMetaCadastradaNoMes_MetaDoMesFicaNula()
    {
        var resultado = await _controller.Index() as ViewResult;
        var modelo = resultado!.Model as DashboardViewModel;

        Assert.That(modelo!.MetaDoMes, Is.Null);
    }

    [Test]
    public async Task Index_ComMetaCadastradaNoMesAtual_PreencheMetaDoMes()
    {
        _context.Metas.Add(new MetaFinanceira
        {
            Descricao = "Meta do mês",
            ValorMeta = 4000m,
            MesReferencia = new DateTime(Hoje.Year, Hoje.Month, 1),
            UsuarioId = UsuarioId
        });
        await _context.SaveChangesAsync();

        var resultado = await _controller.Index() as ViewResult;
        var modelo = resultado!.Model as DashboardViewModel;

        Assert.That(modelo!.MetaDoMes, Is.Not.Null);
        Assert.That(modelo.MetaDoMes!.Descricao, Is.EqualTo("Meta do mês"));
    }

    [Test]
    public async Task Index_SomaTotalCaptadoDaUsuaria_IgnorandoOutrasContas()
    {
        _context.Captacoes.Add(new CaptacaoRecurso { InstituicaoFinanceira = "Banco", Valor = 1000m, Finalidade = "X", UsuarioId = UsuarioId });
        _context.Captacoes.Add(new CaptacaoRecurso { InstituicaoFinanceira = "Banco", Valor = 500m, Finalidade = "Y", UsuarioId = UsuarioId });
        _context.Captacoes.Add(new CaptacaoRecurso { InstituicaoFinanceira = "Alheio", Valor = 9999m, Finalidade = "Z", UsuarioId = 777 });
        await _context.SaveChangesAsync();

        var resultado = await _controller.Index() as ViewResult;
        var modelo = resultado!.Model as DashboardViewModel;

        Assert.That(modelo!.TotalCaptado, Is.EqualTo(1500m));
    }

    [Test]
    public async Task Index_NomeNegocio_VemDoClaimNomeNegocio()
    {
        var resultado = await _controller.Index() as ViewResult;
        var modelo = resultado!.Model as DashboardViewModel;

        Assert.That(modelo!.NomeNegocio, Is.EqualTo(ControllerTestExtensions.NomeNegocioPadrao));
    }
}
