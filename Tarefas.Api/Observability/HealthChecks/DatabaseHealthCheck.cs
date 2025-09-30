using Microsoft.Extensions.Diagnostics.HealthChecks;
using Tarefas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Tarefas.Api.Observability.HealthChecks;

/// <summary>
/// Health check personalizado para verificar a conectividade e integridade do banco de dados
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly TarefasDbContext _context;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(TarefasDbContext context, ILogger<DatabaseHealthCheck> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Iniciando health check do banco de dados");

            // Verifica se é possível conectar ao banco
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            
            if (!canConnect)
            {
                _logger.LogWarning("Falha na conexão com o banco de dados");
                return HealthCheckResult.Unhealthy("Não foi possível conectar ao banco de dados");
            }

            // Verifica se há tabelas criadas
            var tableExists = await _context.Tarefas.AnyAsync(cancellationToken);
            
            // Conta o número de registros para verificar performance
            var totalTarefas = await _context.Tarefas.CountAsync(t => !t.IsDeleted, cancellationToken);
            
            var data = new Dictionary<string, object>
            {
                ["database_provider"] = _context.Database.ProviderName ?? "Unknown",
                ["total_tarefas"] = totalTarefas,
                ["can_connect"] = canConnect,
                ["table_exists"] = tableExists
            };

            _logger.LogDebug("Health check do banco de dados concluído com sucesso. Total de tarefas: {TotalTarefas}", totalTarefas);

            return HealthCheckResult.Healthy(
                "Banco de dados operacional", 
                data
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante o health check do banco de dados");
            
            return HealthCheckResult.Unhealthy(
                "Erro ao verificar o banco de dados", 
                ex,
                new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["error_type"] = ex.GetType().Name
                }
            );
        }
    }
}