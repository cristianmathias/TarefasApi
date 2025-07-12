using Tarefas.Domain.Entities;
using Tarefas.Domain.Interfaces;

namespace Tarefas.Infrastructure.Repositories;

public class TarefaRepository : ITarefaRepository
{
    private readonly List<Tarefa> _tarefas = new();
    private int _proximoId = 1;

    public Task<IEnumerable<Tarefa>> ObterTodasAsync(int pagina, int tamanhoPagina)
    {
        var tarefasPaginadas = _tarefas
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToList();
        return Task.FromResult<IEnumerable<Tarefa>>(tarefasPaginadas);
    }

    public Task<Tarefa?> ObterPorIdAsync(int id)
    {
        return Task.FromResult(_tarefas.FirstOrDefault(t => t.Id == id));
    }

    public Task<Tarefa> AdicionarAsync(Tarefa tarefa)
    {
        tarefa.Id = _proximoId++;
        _tarefas.Add(tarefa);
        return Task.FromResult(tarefa);
    }

    public Task<Tarefa> AtualizarAsync(Tarefa tarefa)
    {
        var tarefaExistente = _tarefas.FirstOrDefault(t => t.Id == tarefa.Id);
        if (tarefaExistente != null)
        {
            tarefaExistente.Titulo = tarefa.Titulo;
            tarefaExistente.Concluida = tarefa.Concluida;
        }
        return Task.FromResult(tarefaExistente!);
    }

    public Task DeletarAsync(int id)
    {
        var tarefa = _tarefas.FirstOrDefault(t => t.Id == id);
        if (tarefa != null)
        {
            _tarefas.Remove(tarefa);
        }
        return Task.CompletedTask;
    }
}