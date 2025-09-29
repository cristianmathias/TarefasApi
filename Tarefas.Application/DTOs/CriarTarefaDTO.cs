using System.ComponentModel.DataAnnotations;

namespace Tarefas.Application.DTOs;

/// <summary>
/// DTO para criação de uma nova tarefa
/// </summary>
/// <param name="Titulo">Título da tarefa (obrigatório, máximo 100 caracteres)</param>
/// <param name="Descricao">Descrição da tarefa (obrigatório, máximo 500 caracteres)</param>
public record CriarTarefaDTO(
    [Required(ErrorMessage = "O título é obrigatório")]
    [StringLength(100, ErrorMessage = "O título deve ter no máximo 100 caracteres")]
    string Titulo,
    
    [Required(ErrorMessage = "A descrição é obrigatória")]
    [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
    string Descricao);