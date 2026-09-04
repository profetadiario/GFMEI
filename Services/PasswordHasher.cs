using System.Security.Cryptography;

namespace GestaoFinanceiraMEI.Services;

/// <summary>
/// Utilitário para hash e verificação de senhas usando PBKDF2
/// (Rfc2898DeriveBytes), nativo do .NET — sem dependências externas.
/// </summary>
public static class PasswordHasher
{
    private const int TamanhoSalt = 16;
    private const int TamanhoChave = 32;
    private const int Iteracoes = 100_000;

    public static (string Hash, string Salt) CriarHash(string senha)
    {
        var salt = RandomNumberGenerator.GetBytes(TamanhoSalt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iteracoes, HashAlgorithmName.SHA256, TamanhoChave);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public static bool Verificar(string senha, string hashArmazenado, string saltArmazenado)
    {
        var salt = Convert.FromBase64String(saltArmazenado);
        var hashCalculado = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iteracoes, HashAlgorithmName.SHA256, TamanhoChave);
        var hashArmazenadoBytes = Convert.FromBase64String(hashArmazenado);
        return CryptographicOperations.FixedTimeEquals(hashCalculado, hashArmazenadoBytes);
    }
}
