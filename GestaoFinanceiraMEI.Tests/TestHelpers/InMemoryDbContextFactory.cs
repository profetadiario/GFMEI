using GestaoFinanceiraMEI.Data;
using Microsoft.EntityFrameworkCore;

namespace GestaoFinanceiraMEI.Tests.TestHelpers;

/// <summary>
/// Cria instâncias de <see cref="AppDbContext"/> usando o provider InMemory
/// do Entity Framework Core, cada uma com um banco isolado (nome único por
/// chamada), para que os testes não compartilhem estado entre si e possam
/// rodar em paralelo com segurança.
/// </summary>
public static class InMemoryDbContextFactory
{
    public static AppDbContext Criar(string? nomeBanco = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(nomeBanco ?? Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new AppDbContext(options);
    }
}
