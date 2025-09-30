using System.ComponentModel.DataAnnotations;

namespace Tarefas.Domain.Entities;

/// <summary>
/// Entidade que representa uma tarefa no sistema
/// </summary>
public class Tarefa
{
    /// <summary>
    /// Identificador único da tarefa
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Título da tarefa
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Titulo { get; set; } = string.Empty;

    /// <summary>
    /// Descrição detalhada da tarefa
    /// </summary>
    [Required]
    [StringLength(1000)]
    public string Descricao { get; set; } = string.Empty;

    /// <summary>
    /// Indica se a tarefa foi concluída
    /// </summary>
    public bool Concluida { get; set; }

    /// <summary>
    /// Data de criação da tarefa
    /// </summary>
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Soft delete flag - tarefa foi deletada logicamente
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Data da exclusão lógica (se aplicável)
    /// </summary>
    public DateTime? DataExclusao { get; set; }
}