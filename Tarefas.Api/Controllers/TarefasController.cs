using Tarefas.Application.DTOs;
using Tarefas.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Tarefas.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class TarefasController : ControllerBase
{
    private readonly ITarefaService _tarefaService;

    public TarefasController(ITarefaService tarefaService)
    {
        _tarefaService = tarefaService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(int pagina = 1, int tamanhoPagina = 10)
    {
        var tarefas = await _tarefaService.ObterTodasAsync(pagina, tamanhoPagina);
        return Ok(tarefas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var tarefa = await _tarefaService.ObterPorIdAsync(id);
        if (tarefa == null)
        {
            return NotFound();
        }
        return Ok(tarefa);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CriarTarefaDTO criarTarefaDTO)
    {
        var tarefa = await _tarefaService.AdicionarAsync(criarTarefaDTO);
        return CreatedAtAction(nameof(Get), new { id = tarefa.Id }, tarefa);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] AtualizarTarefaDTO atualizarTarefaDTO)
    {
        try
        {
            var tarefa = await _tarefaService.AtualizarAsync(id, atualizarTarefaDTO);
            return Ok(tarefa);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _tarefaService.DeletarAsync(id);
        return NoContent();
    }
}