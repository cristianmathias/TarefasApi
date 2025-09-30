using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tarefas.Domain.Entities;
using Tarefas.Domain.Interfaces;
using Tarefas.Infrastructure.Data;

namespace Tarefas.Infrastructure.Repositories;

/// <summary>
/// Implementação thread-safe do repositório de tarefas usando Entity Framework Core
/// </summary>
public class EfTarefaRepository : ITarefaRepository
{
    private readonly TarefasDbContext _context;
    private readonly ILogger<EfTarefaRepository> _logger;

    public EfTarefaRepository(TarefasDbContext context, ILogger<EfTarefaRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todas as tarefas com paginação (thread-safe)
    /// </summary>
    public async Task<IEnumerable<Tarefa>> ObterTodasAsync(int pagina, int tamanhoPagina)
    {
        try
        {
            _logger.LogDebug("Obtendo tarefas - Página: {Pagina}, TamanhoPagina: {TamanhoPagina}", pagina, tamanhoPagina);

            // Validação de parâmetros
            if (pagina <= 0) pagina = 1;
            if (tamanhoPagina <= 0) tamanhoPagina = 10;
            if (tamanhoPagina > 100) tamanhoPagina = 100;

            var tarefas = await _context.Tarefas
                .AsNoTracking() // Performance: não trackear entidades para leitura
                .OrderByDescending(t => t.DataCriacao) // Mais recentes primeiro
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            _logger.LogDebug("Obtidas {Count} tarefas", tarefas.Count);
            return tarefas;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter tarefas paginadas");
            throw;
        }
    }

    /// <summary>
    /// Obtém tarefa por ID (thread-safe)
    /// </summary>
    public async Task<Tarefa?> ObterPorIdAsync(int id)
    {
        try
        {
            _logger.LogDebug("Obtendo tarefa por ID: {Id}", id);

            if (id <= 0)
            {
                _logger.LogWarning("ID inválido fornecido: {Id}", id);
                return null;
            }

            var tarefa = await _context.Tarefas
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tarefa == null)
            {
                _logger.LogDebug("Tarefa não encontrada com ID: {Id}", id);
            }

            return tarefa;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter tarefa por ID: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Adiciona nova tarefa (thread-safe)
    /// </summary>
    public async Task<Tarefa> AdicionarAsync(Tarefa tarefa)
    {
        try
        {
            _logger.LogDebug("Adicionando nova tarefa: {Titulo}", tarefa.Titulo);

            if (tarefa == null)
            {
                throw new ArgumentNullException(nameof(tarefa));
            }

            // Garantir que campos de auditoria estão corretos
            tarefa.Id = 0; // EF irá gerar
            tarefa.DataCriacao = DateTime.UtcNow;
            tarefa.DataAtualizacao = DateTime.UtcNow;
            tarefa.IsDeleted = false;
            tarefa.DataExclusao = null;

            await _context.Tarefas.AddAsync(tarefa);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tarefa adicionada com sucesso. ID: {Id}", tarefa.Id);
            return tarefa;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar tarefa: {Titulo}", tarefa?.Titulo ?? "NULL");
            throw;
        }
    }

    /// <summary>
    /// Atualiza tarefa existente (thread-safe)
    /// </summary>
    public async Task<Tarefa?> AtualizarAsync(Tarefa tarefa)
    {
        try
        {
            _logger.LogDebug("Atualizando tarefa ID: {Id}", tarefa.Id);

            if (tarefa == null)
            {
                throw new ArgumentNullException(nameof(tarefa));
            }

            var tarefaExistente = await _context.Tarefas
                .FirstOrDefaultAsync(t => t.Id == tarefa.Id);

            if (tarefaExistente == null)
            {
                _logger.LogWarning("Tentativa de atualizar tarefa inexistente. ID: {Id}", tarefa.Id);
                return null;
            }

            // Atualizar apenas campos permitidos (não sobrescrever auditoria)
            tarefaExistente.Titulo = tarefa.Titulo;
            tarefaExistente.Descricao = tarefa.Descricao;
            tarefaExistente.Concluida = tarefa.Concluida;
            // DataAtualizacao será atualizada automaticamente pelo DbContext

            await _context.SaveChangesAsync();

            _logger.LogInformation("Tarefa atualizada com sucesso. ID: {Id}", tarefa.Id);
            return tarefaExistente;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar tarefa ID: {Id}", tarefa?.Id ?? 0);
            throw;
        }
    }

    /// <summary>
    /// Remove tarefa (soft delete - thread-safe)
    /// </summary>
    public async Task DeletarAsync(int id)
    {
        try
        {
            _logger.LogDebug("Deletando tarefa ID: {Id}", id);

            if (id <= 0)
            {
                _logger.LogWarning("ID inválido para deleção: {Id}", id);
                return;
            }

            var tarefa = await _context.Tarefas
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tarefa == null)
            {
                _logger.LogWarning("Tentativa de deletar tarefa inexistente. ID: {Id}", id);
                return;
            }

            // Soft delete
            tarefa.IsDeleted = true;
            tarefa.DataExclusao = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Tarefa deletada (soft delete) com sucesso. ID: {Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar tarefa ID: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Obtém contagem total de tarefas (para paginação)
    /// </summary>
    public async Task<int> ContarAsync()
    {
        try
        {
            var count = await _context.Tarefas.CountAsync();
            _logger.LogDebug("Total de tarefas: {Count}", count);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao contar tarefas");
            throw;
        }
    }

    /// <summary>
    /// Busca tarefas por título ou descrição
    /// </summary>
    public async Task<IEnumerable<Tarefa>> BuscarAsync(string termo, int pagina = 1, int tamanhoPagina = 10)
    {
        try
        {
            _logger.LogDebug("Buscando tarefas com termo: {Termo}", termo);

            if (string.IsNullOrWhiteSpace(termo))
            {
                return await ObterTodasAsync(pagina, tamanhoPagina);
            }

            // Validação de parâmetros
            if (pagina <= 0) pagina = 1;
            if (tamanhoPagina <= 0) tamanhoPagina = 10;
            if (tamanhoPagina > 100) tamanhoPagina = 100;

            var termoNormalizado = termo.Trim().ToLowerInvariant();

            var tarefas = await _context.Tarefas
                .AsNoTracking()
                .Where(t => EF.Functions.Like(t.Titulo.ToLower(), $"%{termoNormalizado}%") ||
                           EF.Functions.Like(t.Descricao.ToLower(), $"%{termoNormalizado}%"))
                .OrderByDescending(t => t.DataCriacao)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            _logger.LogDebug("Encontradas {Count} tarefas para o termo: {Termo}", tarefas.Count, termo);
            return tarefas;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar tarefas com termo: {Termo}", termo);
            throw;
        }
    }

    /// <summary>
    /// Filtra tarefas por status de conclusão
    /// </summary>
    public async Task<IEnumerable<Tarefa>> FiltrarPorStatusAsync(bool concluida, int pagina = 1, int tamanhoPagina = 10)
    {
        try
        {
            _logger.LogDebug("Filtrando tarefas por status - Concluída: {Concluida}", concluida);

            // Validação de parâmetros
            if (pagina <= 0) pagina = 1;
            if (tamanhoPagina <= 0) tamanhoPagina = 10;
            if (tamanhoPagina > 100) tamanhoPagina = 100;

            var tarefas = await _context.Tarefas
                .AsNoTracking()
                .Where(t => t.Concluida == concluida)
                .OrderByDescending(t => t.DataCriacao)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            _logger.LogDebug("Filtradas {Count} tarefas com status concluída: {Concluida}", tarefas.Count, concluida);
            return tarefas;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao filtrar tarefas por status: {Concluida}", concluida);
            throw;
        }
    }
}