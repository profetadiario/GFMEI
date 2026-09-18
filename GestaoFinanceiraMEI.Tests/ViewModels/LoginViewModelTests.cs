using GestaoFinanceiraMEI.Tests.TestHelpers;
using GestaoFinanceiraMEI.ViewModels;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.ViewModels;

[TestFixture]
public class LoginViewModelTests
{
    private static LoginViewModel CriarLoginValido() => new()
    {
        Email = "maria@example.com",
        Senha = "qualquer-senha"
    };

    [Test]
    public void LoginValido_NaoGeraErrosDeValidacao()
    {
        var login = CriarLoginValido();

        var erros = ValidationTestHelper.Validar(login);

        Assert.That(erros, Is.Empty);
    }

    [Test]
    public void Email_Vazio_GeraErroDeRequired()
    {
        var login = CriarLoginValido();
        login.Email = string.Empty;

        var erros = ValidationTestHelper.Validar(login);

        Assert.That(erros.TemErroEm(nameof(LoginViewModel.Email)), Is.True);
    }

    [Test]
    public void Email_ForaDoFormato_GeraErro()
    {
        var login = CriarLoginValido();
        login.Email = "nao-e-email";

        var erros = ValidationTestHelper.Validar(login);

        Assert.That(erros.TemErroEm(nameof(LoginViewModel.Email)), Is.True);
    }

    [Test]
    public void Senha_Vazia_GeraErroDeRequired()
    {
        var login = CriarLoginValido();
        login.Senha = string.Empty;

        var erros = ValidationTestHelper.Validar(login);

        Assert.That(erros.TemErroEm(nameof(LoginViewModel.Senha)), Is.True);
    }
}
