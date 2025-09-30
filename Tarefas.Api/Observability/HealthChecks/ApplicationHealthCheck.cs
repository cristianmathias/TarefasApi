using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Reflection;

namespace Tarefas.Api.Observability.HealthChecks;

/// <summary>
/// Health check geral da aplicação para verificar métricas básicas e informações do sistema
/// </summary>
public class ApplicationHealthCheck : IHealthCheck
{
    private readonly ILogger<ApplicationHealthCheck> _logger;
    private readonly IWebHostEnvironment _environment;
    private static readonly DateTime _startTime = DateTime.UtcNow;

    public ApplicationHealthCheck(ILogger<ApplicationHealthCheck> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Iniciando health check da aplicação");

            var uptime = DateTime.UtcNow - _startTime;
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version?.ToString() ?? "Unknown";
            
            // Informações de memória
            var workingSet = Environment.WorkingSet;
            var gcMemory = GC.GetTotalMemory(false);
            
            // Informações do processo
            var processId = Environment.ProcessId;
            var processorCount = Environment.ProcessorCount;
            
            var data = new Dictionary<string, object>
            {
                ["application_name"] = "Tarefas API",
                ["version"] = version,
                ["environment"] = _environment.EnvironmentName,
                ["uptime_seconds"] = (int)uptime.TotalSeconds,
                ["uptime_formatted"] = uptime.ToString(@"dd\.hh\:mm\:ss"),
                ["process_id"] = processId,
                ["processor_count"] = processorCount,
                ["working_set_mb"] = Math.Round(workingSet / 1024.0 / 1024.0, 2),
                ["gc_memory_mb"] = Math.Round(gcMemory / 1024.0 / 1024.0, 2),
                ["machine_name"] = Environment.MachineName,
                ["framework_version"] = Environment.Version.ToString(),
                ["started_at"] = _startTime.ToString("yyyy-MM-dd HH:mm:ss UTC")
            };

            // Verifica se está em um estado saudável (uptime mínimo, memória não excessiva)
            var message = "Aplicação operacional";
            var status = HealthStatus.Healthy;

            // Verifica uso de memória (alerta se > 500MB)
            if (workingSet > 500 * 1024 * 1024)
            {
                message = "Uso de memória elevado";
                status = HealthStatus.Degraded;
                _logger.LogWarning("Uso de memória elevado detectado: {WorkingSetMB}MB", data["working_set_mb"]);
            }

            // Verifica uptime mínimo (alerta se < 30 segundos - possível restart recente)
            if (uptime.TotalSeconds < 30)
            {
                message = "Aplicação reiniciada recentemente";
                status = HealthStatus.Degraded;
                _logger.LogInformation("Aplicação reiniciada recentemente. Uptime: {Uptime}", uptime);
            }

            _logger.LogDebug("Health check da aplicação concluído. Status: {Status}, Uptime: {Uptime}", 
                status, uptime);

            return Task.FromResult(new HealthCheckResult(status, message, data: data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante o health check da aplicação");
            
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Erro interno da aplicação",
                ex,
                new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["error_type"] = ex.GetType().Name
                }
            ));
        }
    }
}