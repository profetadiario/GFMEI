using GestaoFinanceiraMEI.Tests.TestHelpers;
using GestaoFinanceiraMEI.ViewModels;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.ViewModels;

[TestFixture]
public class RegistroViewModelTests
{
    private static RegistroViewModel CriarRegistroValido() => new()
    {
        Nome = "Maria Silva",
        Email = "maria@example.com",
        NomeNegocio = "Ateliê da Maria",
        Senha = "senha123",
        ConfirmarSenha = "senha123"
    };

    [Test]
    public void RegistroValido_NaoGeraErrosDeValidacao()
    {
        var registro = CriarRegistroValido();

        var erros = ValidationTestHelper.Validar(registro);

        Assert.That(erros, Is.Empty);
    }

    [Test]
    public void Nome_Vazio_GeraErroDeRequired()
    {
        var registro = CriarRegistroValido();
        registro.Nome = string.Empty;

        var erros = ValidationTestHelper.Validar(registro);

        Assert.That(erros.TemErroEm(nameof(RegistroViewModel.Nome)), Is.True);
    }

    [Test]
    public void Email_Invalido_GeraErro()
    {
        var registro = CriarRegistroValido();
        registro.Email = "invalido";

        var erros = ValidationTestHelper.Validar(registro);

        Assert.That(erros.TemErroEm(nameof(RegistroViewModel.Email)), Is.True);
    }

    [Test]
    public void NomeNegocio_Vazio_GeraErroDeRequired()
    {
        var registro = CriarRegistroValido();
        registro.NomeNegocio = string.Empty;

        var erros = ValidationTestHelper.Validar(registro);

        Assert.That(erros.TemErroEm(nameof(RegistroViewModel.NomeNegocio)), Is.True);
    }

    [TestCase("")]
    [TestCase("abc12")]
    public void Senha_VaziaOuMenorQueSeisCaracteres_GeraErro(string senhaInvalida)
    {
        var registro = CriarRegistroValido();
        registro.Senha = senhaInvalida;
        registro.ConfirmarSenha = senhaInvalida;

        var erros = ValidationTestHelper.Validar(registro);

        Assert.That(erros.TemErroEm(nameof(RegistroViewModel.Senha)), Is.True);
    }

    [Test]
    public void ConfirmarSenha_DiferenteDaSenha_GeraErroDeCompare()
    {
        var registro = CriarRegistroValido();
        registro.ConfirmarSenha = "outraSenha";

        var erros = ValidationTestHelper.Validar(registro);

        Assert.That(erros.TemErroEm(nameof(RegistroViewModel.ConfirmarSenha)), Is.True);
    }

    [Test]
    public void ConfirmarSenha_Vazia_GeraErroDeRequired()
    {
        var registro = CriarRegistroValido();
        registro.ConfirmarSenha = string.Empty;

        var erros = ValidationTestHelper.Validar(registro);

        Assert.That(erros.TemErroEm(nameof(RegistroViewModel.ConfirmarSenha)), Is.True);
    }
}
