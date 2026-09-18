using GestaoFinanceiraMEI.Models;
using GestaoFinanceiraMEI.Tests.TestHelpers;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Models;

[TestFixture]
public class UsuarioTests
{
    private static Usuario CriarUsuarioValido() => new()
    {
        Nome = "Maria Silva",
        Email = "maria@example.com",
        NomeNegocio = "Ateliê da Maria",
        SenhaHash = "hash",
        SenhaSalt = "salt"
    };

    [Test]
    public void Usuario_Valido_NaoGeraErrosDeValidacao()
    {
        var usuario = CriarUsuarioValido();

        var erros = ValidationTestHelper.Validar(usuario);

        Assert.That(erros, Is.Empty);
    }

    [Test]
    public void Nome_Vazio_GeraErroDeRequired()
    {
        var usuario = CriarUsuarioValido();
        usuario.Nome = string.Empty;

        var erros = ValidationTestHelper.Validar(usuario);

        Assert.That(erros.TemErroEm(nameof(Usuario.Nome)), Is.True);
    }

    [Test]
    public void Nome_MaiorQueLimite_GeraErroDeStringLength()
    {
        var usuario = CriarUsuarioValido();
        usuario.Nome = new string('a', 121);

        var erros = ValidationTestHelper.Validar(usuario);

        Assert.That(erros.TemErroEm(nameof(Usuario.Nome)), Is.True);
    }

    [Test]
    public void Email_Vazio_GeraErroDeRequired()
    {
        var usuario = CriarUsuarioValido();
        usuario.Email = string.Empty;

        var erros = ValidationTestHelper.Validar(usuario);

        Assert.That(erros.TemErroEm(nameof(Usuario.Email)), Is.True);
    }

    [TestCase("nao-e-um-email")]
    [TestCase("faltou-arroba.com")]
    public void Email_ForaDoFormato_GeraErroDeEmailAddress(string emailInvalido)
    {
        var usuario = CriarUsuarioValido();
        usuario.Email = emailInvalido;

        var erros = ValidationTestHelper.Validar(usuario);

        Assert.That(erros.TemErroEm(nameof(Usuario.Email)), Is.True);
    }

    [Test]
    public void NomeNegocio_Vazio_GeraErroDeRequired()
    {
        var usuario = CriarUsuarioValido();
        usuario.NomeNegocio = string.Empty;

        var erros = ValidationTestHelper.Validar(usuario);

        Assert.That(erros.TemErroEm(nameof(Usuario.NomeNegocio)), Is.True);
    }

    [Test]
    public void SenhaHash_Vazia_GeraErroDeRequired()
    {
        var usuario = CriarUsuarioValido();
        usuario.SenhaHash = string.Empty;

        var erros = ValidationTestHelper.Validar(usuario);

        Assert.That(erros.TemErroEm(nameof(Usuario.SenhaHash)), Is.True);
    }

    [Test]
    public void SenhaSalt_Vazia_GeraErroDeRequired()
    {
        var usuario = CriarUsuarioValido();
        usuario.SenhaSalt = string.Empty;

        var erros = ValidationTestHelper.Validar(usuario);

        Assert.That(erros.TemErroEm(nameof(Usuario.SenhaSalt)), Is.True);
    }

    [Test]
    public void DataCadastro_TemValorPadraoPróximoDeAgora()
    {
        var antes = DateTime.Now.AddSeconds(-5);
        var usuario = new Usuario();
        var depois = DateTime.Now.AddSeconds(5);

        Assert.That(usuario.DataCadastro, Is.InRange(antes, depois));
    }

    [Test]
    public void ColecoesDeNavegacao_ComecamVazias_ENuncaSaoNulas()
    {
        var usuario = new Usuario();

        Assert.That(usuario.Categorias, Is.Not.Null.And.Empty);
        Assert.That(usuario.Transacoes, Is.Not.Null.And.Empty);
        Assert.That(usuario.Metas, Is.Not.Null.And.Empty);
        Assert.That(usuario.Captacoes, Is.Not.Null.And.Empty);
    }
}
