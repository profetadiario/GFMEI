using GestaoFinanceiraMEI.Models;
using GestaoFinanceiraMEI.Services;
using GestaoFinanceiraMEI.ViewModels;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.ViewModels;

[TestFixture]
public class DashboardViewModelTests
{
    [Test]
    public void PercentualMeta_SemMetaDoMes_RetornaZero()
    {
        var viewModel = new DashboardViewModel { MetaDoMes = null, ValorAlcancadoMeta = 500m };

        Assert.That(viewModel.PercentualMeta, Is.EqualTo(0));
    }

    [Test]
    public void PercentualMeta_ValorMetaZero_RetornaZero_ENaoLancaDivisaoPorZero()
    {
        var viewModel = new DashboardViewModel
        {
            MetaDoMes = new MetaFinanceira { ValorMeta = 0 },
            ValorAlcancadoMeta = 500m
        };

        Assert.That(viewModel.PercentualMeta, Is.EqualTo(0));
    }

    [Test]
    public void PercentualMeta_MetadeDoValorAlcancado_Retorna50()
    {
        var viewModel = new DashboardViewModel
        {
            MetaDoMes = new MetaFinanceira { ValorMeta = 1000m },
            ValorAlcancadoMeta = 500m
        };

        Assert.That(viewModel.PercentualMeta, Is.EqualTo(50m));
    }

    [Test]
    public void PercentualMeta_AlcancadoMaiorQueAMeta_FicaLimitadoEm100()
    {
        var viewModel = new DashboardViewModel
        {
            MetaDoMes = new MetaFinanceira { ValorMeta = 1000m },
            ValorAlcancadoMeta = 5000m
        };

        Assert.That(viewModel.PercentualMeta, Is.EqualTo(100m));
    }

    [Test]
    public void PercentualMeta_LucroLiquidoNegativo_Prejuizo_FicaLimitadoEmZero_NaoFicaNegativo()
    {
        // Regressão: antes da correção, Math.Min(100, ...) permitia um
        // percentual negativo quando havia prejuízo no mês (Math.Clamp
        // corrige isso travando o piso em zero).
        var viewModel = new DashboardViewModel
        {
            MetaDoMes = new MetaFinanceira { ValorMeta = 1000m },
            ValorAlcancadoMeta = -300m
        };

        Assert.That(viewModel.PercentualMeta, Is.EqualTo(0));
    }

    [Test]
    public void PercentualMeta_Arredonda1CasaDecimal()
    {
        var viewModel = new DashboardViewModel
        {
            MetaDoMes = new MetaFinanceira { ValorMeta = 3m },
            ValorAlcancadoMeta = 1m
        };

        // 1/3 * 100 = 33.333... -> arredondado para 33.3
        Assert.That(viewModel.PercentualMeta, Is.EqualTo(33.3m));
    }

    [Test]
    public void ValoresPadrao_SaoConsistentesComUmDashboardVazio()
    {
        var viewModel = new DashboardViewModel();

        Assert.That(viewModel.NomeNegocio, Is.EqualTo(string.Empty));
        Assert.That(viewModel.PeriodoResumo, Is.EqualTo(string.Empty));
        Assert.That(viewModel.ResumoDoMes, Is.Not.Null);
        Assert.That(viewModel.HistoricoMensal, Is.Not.Null.And.Empty);
        Assert.That(viewModel.SaldoGeral, Is.EqualTo(0));
        Assert.That(viewModel.TotalCaptado, Is.EqualTo(0));
    }

    [Test]
    public void ValoresPadrao_NovosCampos_NaoTemGastosNemUltimoLancamento()
    {
        var viewModel = new DashboardViewModel();

        Assert.That(viewModel.GastosPorCategoria, Is.Not.Null.And.Empty);
        Assert.That(viewModel.MaiorGastoDoMes, Is.Null);
        Assert.That(viewModel.UltimoLancamento, Is.Null);
    }

    [Test]
    public void FraseMaiorGasto_SemDespesasNoMes_AvisaQueNaoHaDespesas()
    {
        var viewModel = new DashboardViewModel { MaiorGastoDoMes = null };

        Assert.That(viewModel.FraseMaiorGasto, Is.EqualTo("Você ainda não registrou despesas neste mês."));
    }

    [Test]
    public void FraseMaiorGasto_ComMaiorGasto_UsaCategoriaValorEPercentualEmPtBr()
    {
        var viewModel = new DashboardViewModel
        {
            MaiorGastoDoMes = new GastoPorCategoria { Categoria = "Insumos", Valor = 1200m, Percentual = 35.0m }
        };

        Assert.That(viewModel.FraseMaiorGasto, Is.EqualTo("Seu maior gasto este mês foi Insumos: R$ 1.200,00, 35% das saídas."));
    }

    [Test]
    public void DiasSemLancar_NuncaLancou_EhNulo()
    {
        var viewModel = new DashboardViewModel { UltimoLancamento = null };

        Assert.That(viewModel.DiasSemLancar, Is.Null);
    }

    [Test]
    public void DiasSemLancar_ContaOsDiasCorridosDesdeOUltimoLancamento()
    {
        var referencia = new DateTime(2026, 11, 18);
        var viewModel = new DashboardViewModel { DataReferencia = referencia, UltimoLancamento = new DateTime(2026, 11, 13) };

        Assert.That(viewModel.DiasSemLancar, Is.EqualTo(5));
    }

    [Test]
    public void DiasSemLancar_UltimoLancamentoFuturo_NaoFicaNegativo()
    {
        var referencia = new DateTime(2026, 11, 18);
        var viewModel = new DashboardViewModel { DataReferencia = referencia, UltimoLancamento = new DateTime(2026, 11, 25) };

        Assert.That(viewModel.DiasSemLancar, Is.EqualTo(0));
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void MensagemLembrete_ComMenosDeTresDiasSemLancar_NaoMostraAviso(int dias)
    {
        var referencia = new DateTime(2026, 11, 18);
        var viewModel = new DashboardViewModel { DataReferencia = referencia, UltimoLancamento = referencia.AddDays(-dias) };

        Assert.That(viewModel.MensagemLembrete, Is.Null);
    }

    [TestCase(3)]
    [TestCase(10)]
    public void MensagemLembrete_ComTresOuMaisDiasSemLancar_MostraQuantosDias(int dias)
    {
        var referencia = new DateTime(2026, 11, 18);
        var viewModel = new DashboardViewModel { DataReferencia = referencia, UltimoLancamento = referencia.AddDays(-dias) };

        Assert.That(viewModel.MensagemLembrete, Is.EqualTo($"Você não lança nada há {dias} dias."));
    }

    [Test]
    public void MensagemLembrete_NuncaLancouNada_ConvidaAComecar()
    {
        var viewModel = new DashboardViewModel { UltimoLancamento = null };

        Assert.That(viewModel.MensagemLembrete, Is.EqualTo("Você ainda não registrou nenhum lançamento. Que tal começar agora?"));
    }
}
