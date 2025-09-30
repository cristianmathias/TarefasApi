using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tarefas.Domain.Entities;

namespace Tarefas.Infrastructure.Data.Configurations;

/// <summary>
/// Configuração do Entity Framework para a entidade Tarefa
/// </summary>
public class TarefaConfiguration : IEntityTypeConfiguration<Tarefa>
{
    public void Configure(EntityTypeBuilder<Tarefa> builder)
    {
        // Tabela
        builder.ToTable("Tarefas");

        // Chave primária
        builder.HasKey(t => t.Id);
        
        // Propriedades
        builder.Property(t => t.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(t => t.Titulo)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Título da tarefa");

        builder.Property(t => t.Descricao)
            .IsRequired()
            .HasMaxLength(1000)
            .HasComment("Descrição detalhada da tarefa");

        builder.Property(t => t.Concluida)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indica se a tarefa foi concluída");

        builder.Property(t => t.DataCriacao)
            .IsRequired()
            .HasDefaultValueSql("datetime('now')")
            .HasComment("Data de criação da tarefa");

        builder.Property(t => t.DataAtualizacao)
            .IsRequired()
            .HasDefaultValueSql("datetime('now')")
            .HasComment("Data da última atualização");

        builder.Property(t => t.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Soft delete flag");

        builder.Property(t => t.DataExclusao)
            .IsRequired(false)
            .HasComment("Data da exclusão lógica");

        // Índices
        builder.HasIndex(t => t.Concluida)
            .HasDatabaseName("IX_Tarefas_Concluida");

        builder.HasIndex(t => t.DataCriacao)
            .HasDatabaseName("IX_Tarefas_DataCriacao");

        builder.HasIndex(t => t.IsDeleted)
            .HasDatabaseName("IX_Tarefas_IsDeleted");

        // Índice composto para consultas filtradas
        builder.HasIndex(t => new { t.Concluida, t.IsDeleted })
            .HasDatabaseName("IX_Tarefas_Concluida_IsDeleted");
    }
}