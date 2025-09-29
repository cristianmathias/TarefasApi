using Tarefas.Application.DTOs;
using Tarefas.Application.Interfaces;
using Tarefas.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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

    /// <summary>
    /// Obtém uma lista paginada de tarefas
    /// </summary>
    /// <param name="pagina">Número da página (default: 1)</param>
    /// <param name="tamanhoPagina">Quantidade de itens por página (default: 10)</param>
    /// <returns>Lista de tarefas paginada</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Tarefa>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([Range(1, int.MaxValue)] int pagina = 1, [Range(1, 100)] int tamanhoPagina = 10)
    {
        var tarefas = await _tarefaService.ObterTodasAsync(pagina, tamanhoPagina);
        return Ok(tarefas);
    }

    /// <summary>
    /// Obtém uma tarefa específica pelo ID
    /// </summary>
    /// <param name="id">ID da tarefa</param>
    /// <returns>Tarefa encontrada</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Tarefa), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([Range(1, int.MaxValue)] int id)
    {
        var tarefa = await _tarefaService.ObterPorIdAsync(id);
        if (tarefa == null)
        {
            return NotFound();
        }
        return Ok(tarefa);
    }

    /// <summary>
    /// Cria uma nova tarefa
    /// </summary>
    /// <param name="criarTarefaDTO">Dados da tarefa a ser criada</param>
    /// <returns>Tarefa criada</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Tarefa), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post([FromBody] CriarTarefaDTO criarTarefaDTO)
    {
        var tarefa = await _tarefaService.AdicionarAsync(criarTarefaDTO);
        return CreatedAtAction(nameof(Get), new { id = tarefa.Id }, tarefa);
    }

    /// <summary>
    /// Atualiza uma tarefa existente
    /// </summary>
    /// <param name="id">ID da tarefa</param>
    /// <param name="atualizarTarefaDTO">Dados para atualização da tarefa</param>
    /// <returns>Tarefa atualizada</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Tarefa), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Put([Range(1, int.MaxValue)] int id, [FromBody] AtualizarTarefaDTO atualizarTarefaDTO)
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

    /// <summary>
    /// Remove uma tarefa
    /// </summary>
    /// <param name="id">ID da tarefa a ser removida</param>
    /// <returns>Confirmação da remoção</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([Range(1, int.MaxValue)] int id)
    {
        await _tarefaService.DeletarAsync(id);
        return NoContent();
    }
}