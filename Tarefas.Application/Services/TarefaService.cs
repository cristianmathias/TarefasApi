using Tarefas.Application.DTOs;
using Tarefas.Application.Interfaces;
using Tarefas.Domain.Entities;
using Tarefas.Domain.Interfaces;

namespace Tarefas.Application.Services;

public class TarefaService : ITarefaService
{
    private readonly ITarefaRepository _tarefaRepository;

    public TarefaService(ITarefaRepository tarefaRepository)
    {
        _tarefaRepository = tarefaRepository;
    }

    public Task<IEnumerable<Tarefa>> ObterTodasAsync(int pagina, int tamanhoPagina)
    {
        return _tarefaRepository.ObterTodasAsync(pagina, tamanhoPagina);
    }

    public Task<Tarefa?> ObterPorIdAsync(int id)
    {
        return _tarefaRepository.ObterPorIdAsync(id);
    }

    public Task<Tarefa> AdicionarAsync(CriarTarefaDTO tarefaDTO)
    {
        var tarefa = new Tarefa
        {
            Titulo = tarefaDTO.Titulo,
            Descricao = tarefaDTO.Descricao,
            Concluida = false
        };
        return _tarefaRepository.AdicionarAsync(tarefa);
    }

    public async Task<Tarefa> AtualizarAsync(int id, AtualizarTarefaDTO tarefaDTO)
    {
        var tarefa = await _tarefaRepository.ObterPorIdAsync(id);
        if (tarefa == null)
        {
            throw new Exception("Tarefa não encontrada");
        }

        tarefa.Titulo = tarefaDTO.Titulo;
        tarefa.Concluida = tarefaDTO.Concluida;

        return await _tarefaRepository.AtualizarAsync(tarefa);
    }

    public Task DeletarAsync(int id)
    {
        return _tarefaRepository.DeletarAsync(id);
    }
}