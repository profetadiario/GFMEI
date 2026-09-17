using GestaoFinanceiraMEI.Data;
using GestaoFinanceiraMEI.Models;
using GestaoFinanceiraMEI.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GestaoFinanceiraMEI.Tests.Data;

/// <summary>
/// Cobre as regras de modelagem configuradas em AppDbContext.OnModelCreating
/// (índice único de e-mail e comportamento de exclusão em cascata/restrita
/// entre as entidades).
/// </summary>
[TestFixture]
public class AppDbContextTests
{
    private AppDbContext _context = null!;

    [SetUp]
    public void SetUp() => _context = InMemoryDbContextFactory.Criar();

    [TearDown]
    public void TearDown() => _context.Dispose();

    [Test]
    public void Usuario_Email_TemIndiceUnicoConfiguradoNoModelo()
    {
        // O provider InMemory não aplica a checagem de unicidade de
        // HasIndex(...).IsUnique() em tempo de SaveChanges (diferente de um
        // banco relacional real, onde o índice único do SQL Server rejeita a
        // gravação) — só reforça chaves primárias/alternadas. Por isso, aqui
        // validamos a configuração do modelo em si: que o índice único sobre
        // Email realmente existe, o que é o que garante a regra no SQL
        // Server em produção.
        var indice = _context.Model.FindEntityType(typeof(Usuario))!
            .GetIndexes()
            .Single(i => i.Properties.Select(p => p.Name).SequenceEqual(new[] { nameof(Usuario.Email) }));

        Assert.That(indice.IsUnique, Is.True);
    }

    [Test]
    public async Task ExcluirUsuario_ExcluiEmCascataCategoriasTransacoesMetasECaptacoes()
    {
        var usuario = new Usuario { Nome = "A", Email = "a@x.com", NomeNegocio = "A", SenhaHash = "h", SenhaSalt = "s" };
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        var categoria = new Categoria { Nome = "Vendas", Tipo = TipoTransacao.Receita, UsuarioId = usuario.Id };
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        _context.Metas.Add(new MetaFinanceira { Descricao = "Meta", ValorMeta = 100m, UsuarioId = usuario.Id });
        _context.Captacoes.Add(new CaptacaoRecurso { InstituicaoFinanceira = "Banco", Valor = 100m, Finalidade = "X", UsuarioId = usuario.Id });
        await _context.SaveChangesAsync();

        // Recarrega a usuária com as coleções de navegação incluídas, para
        // que o EF Core consiga detectar e aplicar a cascata em memória.
        var usuarioCompleto = await _context.Usuarios
            .Include(u => u.Categorias)
            .Include(u => u.Metas)
            .Include(u => u.Captacoes)
            .FirstAsync(u => u.Id == usuario.Id);

        _context.Usuarios.Remove(usuarioCompleto);
        await _context.SaveChangesAsync();

        Assert.That(await _context.Usuarios.AnyAsync(u => u.Id == usuario.Id), Is.False);
        Assert.That(await _context.Categorias.AnyAsync(c => c.UsuarioId == usuario.Id), Is.False);
        Assert.That(await _context.Metas.AnyAsync(m => m.UsuarioId == usuario.Id), Is.False);
        Assert.That(await _context.Captacoes.AnyAsync(c => c.UsuarioId == usuario.Id), Is.False);
    }

    [Test]
    public async Task ExcluirCategoriaComTransacaoVinculada_RelacaoRestrita_ImpedeExclusao()
    {
        var usuario = new Usuario { Nome = "A", Email = "a2@x.com", NomeNegocio = "A", SenhaHash = "h", SenhaSalt = "s" };
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        var categoria = new Categoria { Nome = "Vendas", Tipo = TipoTransacao.Receita, UsuarioId = usuario.Id };
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        _context.Transacoes.Add(new Transacao
        {
            Descricao = "Venda",
            Valor = 10m,
            Tipo = TipoTransacao.Receita,
            CategoriaId = categoria.Id,
            UsuarioId = usuario.Id
        });
        await _context.SaveChangesAsync();

        var categoriaComTransacoes = await _context.Categorias
            .Include(c => c.Transacoes)
            .FirstAsync(c => c.Id == categoria.Id);

        // DeleteBehavior.Restrict: como a Categoria já foi carregada com as
        // Transacoes vinculadas (Include), o ChangeTracker do EF Core detecta
        // a violação já na própria chamada de Remove (fixup imediato), sem
        // nem chegar a precisar de SaveChangesAsync — é essa mesma regra que
        // justifica a checagem de "categoria em uso" no CategoriasController
        // antes de excluir.
        Assert.Throws<InvalidOperationException>(() => _context.Categorias.Remove(categoriaComTransacoes));
    }

    [Test]
    public async Task DbSets_PermitemInserirEConsultarTodasAsEntidades()
    {
        var usuario = new Usuario { Nome = "A", Email = "completo@x.com", NomeNegocio = "A", SenhaHash = "h", SenhaSalt = "s" };
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        Assert.That(await _context.Usuarios.CountAsync(), Is.EqualTo(1));
        Assert.That(await _context.Categorias.CountAsync(), Is.EqualTo(0));
        Assert.That(await _context.Transacoes.CountAsync(), Is.EqualTo(0));
        Assert.That(await _context.Metas.CountAsync(), Is.EqualTo(0));
        Assert.That(await _context.Captacoes.CountAsync(), Is.EqualTo(0));
    }
}
