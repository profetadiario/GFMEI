using GestaoFinanceiraMEI.Models;
using Microsoft.EntityFrameworkCore;

namespace GestaoFinanceiraMEI.Data;

/// <summary>
/// Contexto de acesso a dados do sistema (Entity Framework Core + SQL Server / MS SQL Express).
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Transacao> Transacoes => Set<Transacao>();
    public DbSet<MetaFinanceira> Metas => Set<MetaFinanceira>();
    public DbSet<CaptacaoRecurso> Captacoes => Set<CaptacaoRecurso>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Categoria>()
            .HasOne(c => c.Usuario)
            .WithMany(u => u.Categorias)
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Transacao>()
            .HasOne(t => t.Usuario)
            .WithMany(u => u.Transacoes)
            .HasForeignKey(t => t.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Transacao>()
            .HasOne(t => t.Categoria)
            .WithMany(c => c.Transacoes)
            .HasForeignKey(t => t.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MetaFinanceira>()
            .HasOne(m => m.Usuario)
            .WithMany(u => u.Metas)
            .HasForeignKey(m => m.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CaptacaoRecurso>()
            .HasOne(c => c.Usuario)
            .WithMany(u => u.Captacoes)
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
