using Tarefas.Application.DTOs;
using Tarefas.Domain.Entities;

namespace Tarefas.Application.Interfaces;

public interface ITarefaService
{
    Task<IEnumerable<Tarefa>> ObterTodasAsync(int pagina, int tamanhoPagina);
    Task<Tarefa?> ObterPorIdAsync(int id);
    Task<Tarefa> AdicionarAsync(CriarTarefaDTO tarefaDTO);
    Task<Tarefa> AtualizarAsync(int id, AtualizarTarefaDTO tarefaDTO);
    Task DeletarAsync(int id);
}