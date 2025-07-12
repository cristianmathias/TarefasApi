using Tarefas.Domain.Entities;

namespace Tarefas.Domain.Interfaces;

public interface ITarefaRepository
{
    Task<IEnumerable<Tarefa>> ObterTodasAsync(int pagina, int tamanhoPagina);
    Task<Tarefa?> ObterPorIdAsync(int id);
    Task<Tarefa> AdicionarAsync(Tarefa tarefa);
    Task<Tarefa> AtualizarAsync(Tarefa tarefa);
    Task DeletarAsync(int id);
}