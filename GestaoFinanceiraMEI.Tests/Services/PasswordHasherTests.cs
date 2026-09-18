using GestaoFinanceiraMEI.Services;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Services;

[TestFixture]
public class PasswordHasherTests
{
    [Test]
    public void CriarHash_RetornaHashESaltNaoVazios_EDiferentesDaSenhaOriginal()
    {
        var (hash, salt) = PasswordHasher.CriarHash("minhaSenha123");

        Assert.That(hash, Is.Not.Null.And.Not.Empty);
        Assert.That(salt, Is.Not.Null.And.Not.Empty);
        Assert.That(hash, Is.Not.EqualTo("minhaSenha123"));
    }

    [Test]
    public void CriarHash_RetornaHashESaltEmBase64Valido()
    {
        var (hash, salt) = PasswordHasher.CriarHash("minhaSenha123");

        Assert.DoesNotThrow(() => Convert.FromBase64String(hash));
        Assert.DoesNotThrow(() => Convert.FromBase64String(salt));
    }

    [Test]
    public void CriarHash_ChamadasDiferentes_GeramSaltsDiferentes_PorSerAleatorio()
    {
        var (_, salt1) = PasswordHasher.CriarHash("mesmaSenha");
        var (_, salt2) = PasswordHasher.CriarHash("mesmaSenha");

        Assert.That(salt1, Is.Not.EqualTo(salt2));
    }

    [Test]
    public void Verificar_ComSenhaCorreta_RetornaTrue()
    {
        var (hash, salt) = PasswordHasher.CriarHash("senhaCorreta");

        var resultado = PasswordHasher.Verificar("senhaCorreta", hash, salt);

        Assert.That(resultado, Is.True);
    }

    [Test]
    public void Verificar_ComSenhaErrada_RetornaFalse()
    {
        var (hash, salt) = PasswordHasher.CriarHash("senhaCorreta");

        var resultado = PasswordHasher.Verificar("senhaErrada", hash, salt);

        Assert.That(resultado, Is.False);
    }

    [Test]
    public void Verificar_ComHashAdulterado_RetornaFalse()
    {
        var (hash, salt) = PasswordHasher.CriarHash("senhaCorreta");
        var hashAdulterado = Convert.ToBase64String(new byte[32]); // hash "zerado"

        var resultado = PasswordHasher.Verificar("senhaCorreta", hashAdulterado, salt);

        Assert.That(resultado, Is.False);
    }

    [Test]
    public void Verificar_ComSensibilidadeAMaiusculasEMinusculas()
    {
        var (hash, salt) = PasswordHasher.CriarHash("SenhaComMaiuscula");

        Assert.That(PasswordHasher.Verificar("senhacommaiuscula", hash, salt), Is.False);
        Assert.That(PasswordHasher.Verificar("SenhaComMaiuscula", hash, salt), Is.True);
    }
}
