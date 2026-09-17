using GestaoFinanceiraMEI.Models;
using GestaoFinanceiraMEI.Tests.TestHelpers;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Models;

[TestFixture]
public class MetaFinanceiraTests
{
    private static MetaFinanceira CriarMetaValida() => new()
    {
        Descricao = "Meta de faturamento de setembro",
        ValorMeta = 3000m,
        MesReferencia = new DateTime(2026, 9, 1),
        UsuarioId = 1
    };

    [Test]
    public void MetaFinanceira_Valida_NaoGeraErrosDeValidacao()
    {
        var meta = CriarMetaValida();

        var erros = ValidationTestHelper.Validar(meta);

        Assert.That(erros, Is.Empty);
    }

    [Test]
    public void Descricao_Vazia_GeraErroDeRequired()
    {
        var meta = CriarMetaValida();
        meta.Descricao = string.Empty;

        var erros = ValidationTestHelper.Validar(meta);

        Assert.That(erros.TemErroEm(nameof(MetaFinanceira.Descricao)), Is.True);
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(10_000_000.01)]
    public void ValorMeta_ForaDoIntervaloPermitido_GeraErroDeRange(double valorInvalido)
    {
        var meta = CriarMetaValida();
        meta.ValorMeta = (decimal)valorInvalido;

        var erros = ValidationTestHelper.Validar(meta);

        Assert.That(erros.TemErroEm(nameof(MetaFinanceira.ValorMeta)), Is.True);
    }

    [Test]
    public void MesReferencia_TemValorPadraoNoPrimeiroDiaDoMesAtual()
    {
        var meta = new MetaFinanceira();
        var esperado = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        Assert.That(meta.MesReferencia, Is.EqualTo(esperado));
    }
}
