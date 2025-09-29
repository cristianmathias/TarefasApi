using System.ComponentModel.DataAnnotations;

namespace Tarefas.Application.DTOs;

/// <summary>
/// DTO para atualização de uma tarefa existente
/// </summary>
/// <param name="Titulo">Título da tarefa (obrigatório, máximo 100 caracteres)</param>
/// <param name="Concluida">Status de conclusão da tarefa</param>
public record AtualizarTarefaDTO(
    [Required(ErrorMessage = "O título é obrigatório")]
    [StringLength(100, ErrorMessage = "O título deve ter no máximo 100 caracteres")]
    string Titulo,
    
    bool Concluida);