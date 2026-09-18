using GestaoFinanceiraMEI.Models;
using GestaoFinanceiraMEI.Tests.TestHelpers;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Models;

[TestFixture]
public class CaptacaoRecursoTests
{
    private static CaptacaoRecurso CriarCaptacaoValida() => new()
    {
        InstituicaoFinanceira = "Banco do Povo",
        Valor = 5000m,
        TaxaJurosMensal = 1.5m,
        Finalidade = "Capital de giro",
        DataObtencao = DateTime.Today,
        UsuarioId = 1
    };

    [Test]
    public void CaptacaoRecurso_Valida_NaoGeraErrosDeValidacao()
    {
        var captacao = CriarCaptacaoValida();

        var erros = ValidationTestHelper.Validar(captacao);

        Assert.That(erros, Is.Empty);
    }

    [Test]
    public void InstituicaoFinanceira_Vazia_GeraErroDeRequired()
    {
        var captacao = CriarCaptacaoValida();
        captacao.InstituicaoFinanceira = string.Empty;

        var erros = ValidationTestHelper.Validar(captacao);

        Assert.That(erros.TemErroEm(nameof(CaptacaoRecurso.InstituicaoFinanceira)), Is.True);
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(10_000_000.01)]
    public void Valor_ForaDoIntervaloPermitido_GeraErroDeRange(double valorInvalido)
    {
        var captacao = CriarCaptacaoValida();
        captacao.Valor = (decimal)valorInvalido;

        var erros = ValidationTestHelper.Validar(captacao);

        Assert.That(erros.TemErroEm(nameof(CaptacaoRecurso.Valor)), Is.True);
    }

    [TestCase(-0.01)]
    [TestCase(100.01)]
    public void TaxaJurosMensal_ForaDoIntervaloPermitido_GeraErroDeRange(double taxaInvalida)
    {
        var captacao = CriarCaptacaoValida();
        captacao.TaxaJurosMensal = (decimal)taxaInvalida;

        var erros = ValidationTestHelper.Validar(captacao);

        Assert.That(erros.TemErroEm(nameof(CaptacaoRecurso.TaxaJurosMensal)), Is.True);
    }

    [TestCase(0)]
    [TestCase(100)]
    [TestCase(50)]
    public void TaxaJurosMensal_NosLimitesOuDentroDoIntervalo_NaoGeraErro(double taxaValida)
    {
        var captacao = CriarCaptacaoValida();
        captacao.TaxaJurosMensal = (decimal)taxaValida;

        var erros = ValidationTestHelper.Validar(captacao);

        Assert.That(erros.TemErroEm(nameof(CaptacaoRecurso.TaxaJurosMensal)), Is.False);
    }

    [Test]
    public void Finalidade_Vazia_GeraErroDeRequired()
    {
        var captacao = CriarCaptacaoValida();
        captacao.Finalidade = string.Empty;

        var erros = ValidationTestHelper.Validar(captacao);

        Assert.That(erros.TemErroEm(nameof(CaptacaoRecurso.Finalidade)), Is.True);
    }

    [Test]
    public void DataObtencao_TemValorPadraoIgualAHoje()
    {
        var captacao = new CaptacaoRecurso();

        Assert.That(captacao.DataObtencao, Is.EqualTo(DateTime.Today));
    }
}
