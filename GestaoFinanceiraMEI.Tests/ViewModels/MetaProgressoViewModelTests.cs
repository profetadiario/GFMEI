using GestaoFinanceiraMEI.Models;
using GestaoFinanceiraMEI.ViewModels;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.ViewModels;

[TestFixture]
public class MetaProgressoViewModelTests
{
    private static MetaProgressoViewModel CriarViewModel(decimal valorMeta, decimal valorAlcancado) => new()
    {
        Meta = new MetaFinanceira { ValorMeta = valorMeta },
        ValorAlcancado = valorAlcancado
    };

    [Test]
    public void PercentualAtingido_ValorMetaZero_RetornaZero_ENaoLancaDivisaoPorZero()
    {
        var viewModel = CriarViewModel(valorMeta: 0, valorAlcancado: 500m);

        Assert.That(viewModel.PercentualAtingido, Is.EqualTo(0));
    }

    [Test]
    public void PercentualAtingido_MetadeDoValorAlcancado_Retorna50()
    {
        var viewModel = CriarViewModel(valorMeta: 1000m, valorAlcancado: 500m);

        Assert.That(viewModel.PercentualAtingido, Is.EqualTo(50m));
    }

    [Test]
    public void PercentualAtingido_AlcancadoMaiorQueAMeta_FicaLimitadoEm100()
    {
        var viewModel = CriarViewModel(valorMeta: 1000m, valorAlcancado: 9999m);

        Assert.That(viewModel.PercentualAtingido, Is.EqualTo(100m));
    }

    [Test]
    public void PercentualAtingido_LucroLiquidoNegativo_Prejuizo_FicaLimitadoEmZero()
    {
        var viewModel = CriarViewModel(valorMeta: 1000m, valorAlcancado: -50m);

        Assert.That(viewModel.PercentualAtingido, Is.EqualTo(0));
    }

    [Test]
    public void PercentualAtingido_ExatamenteIgualAMeta_Retorna100()
    {
        var viewModel = CriarViewModel(valorMeta: 750m, valorAlcancado: 750m);

        Assert.That(viewModel.PercentualAtingido, Is.EqualTo(100m));
    }
}
