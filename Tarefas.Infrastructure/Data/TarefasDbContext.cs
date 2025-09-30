using Microsoft.EntityFrameworkCore;
using Tarefas.Domain.Entities;
using Tarefas.Infrastructure.Data.Configurations;

namespace Tarefas.Infrastructure.Data;

/// <summary>
/// Contexto do Entity Framework para a aplicação Tarefas
/// </summary>
public class TarefasDbContext : DbContext
{
    public TarefasDbContext(DbContextOptions<TarefasDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// DbSet para tarefas
    /// </summary>
    public DbSet<Tarefa> Tarefas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Aplicar configurações
        modelBuilder.ApplyConfiguration(new TarefaConfiguration());
        
        // Query Filter global para Soft Delete
        modelBuilder.Entity<Tarefa>().HasQueryFilter(t => !t.IsDeleted);
        
        // Seed data para desenvolvimento
        SeedData(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Configuração padrão para development (será sobrescrita pela injeção de dependência)
            optionsBuilder.UseSqlite("Data Source=tarefas.db");
        }
        
        // Habilitar logs sensíveis apenas em desenvolvimento
        #if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
        #endif
    }

    /// <summary>
    /// Override SaveChangesAsync para atualizar timestamps automaticamente
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Override SaveChanges para atualizar timestamps automaticamente
    /// </summary>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Atualiza automaticamente DataCriacao e DataAtualizacao
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Tarefa && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var tarefa = (Tarefa)entry.Entity;
            
            if (entry.State == EntityState.Added)
            {
                tarefa.DataCriacao = DateTime.UtcNow;
            }
            
            tarefa.DataAtualizacao = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Seed data para desenvolvimento e testes
    /// </summary>
    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tarefa>().HasData(
            new Tarefa
            {
                Id = 1,
                Titulo = "Implementar API REST",
                Descricao = "Criar endpoints para CRUD de tarefas usando Clean Architecture",
                Concluida = true,
                DataCriacao = DateTime.UtcNow.AddDays(-7),
                DataAtualizacao = DateTime.UtcNow.AddDays(-1),
                IsDeleted = false
            },
            new Tarefa
            {
                Id = 2,
                Titulo = "Configurar Entity Framework",
                Descricao = "Adicionar EF Core com SQLite para persistência de dados",
                Concluida = false,
                DataCriacao = DateTime.UtcNow.AddDays(-3),
                DataAtualizacao = DateTime.UtcNow.AddDays(-3),
                IsDeleted = false
            },
            new Tarefa
            {
                Id = 3,
                Titulo = "Implementar Testes",
                Descricao = "Criar testes unitários e de integração abrangentes",
                Concluida = true,
                DataCriacao = DateTime.UtcNow.AddDays(-5),
                DataAtualizacao = DateTime.UtcNow.AddHours(-2),
                IsDeleted = false
            }
        );
    }
}