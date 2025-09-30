using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Tarefas.Api.Observability.HealthChecks;

/// <summary>
/// Health check para verificar o status do sistema de rate limiting
/// </summary>
public class RateLimitHealthCheck : IHealthCheck
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RateLimitHealthCheck> _logger;

    public RateLimitHealthCheck(IServiceProvider serviceProvider, ILogger<RateLimitHealthCheck> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Iniciando health check do rate limiting");

            // Tenta obter o serviço de rate limiting cleanup
            var rateLimitService = _serviceProvider.GetService<Services.RateLimitCleanupService>();
            
            if (rateLimitService == null)
            {
                _logger.LogWarning("Serviço de rate limiting não encontrado");
                return Task.FromResult(HealthCheckResult.Degraded(
                    "Serviço de rate limiting não está registrado",
                    data: new Dictionary<string, object>
                    {
                        ["service_registered"] = false,
                        ["service_type"] = "RateLimitCleanupService"
                    }
                ));
            }

            // Para o serviço de cleanup, apenas verificamos se está registrado
            var data = new Dictionary<string, object>
            {
                ["service_registered"] = true,
                ["service_type"] = "RateLimitCleanupService",
                ["cleanup_service_active"] = true
            };

            _logger.LogDebug("Health check do rate limiting concluído com sucesso");

            return Task.FromResult(HealthCheckResult.Healthy(
                "Sistema de rate limiting operacional",
                data
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante o health check do rate limiting");
            
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Erro no sistema de rate limiting",
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