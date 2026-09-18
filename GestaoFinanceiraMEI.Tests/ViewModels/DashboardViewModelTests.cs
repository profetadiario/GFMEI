using GestaoFinanceiraMEI.Models;
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
}
