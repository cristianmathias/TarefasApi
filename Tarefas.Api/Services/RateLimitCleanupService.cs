using Tarefas.Api.Middleware;

namespace Tarefas.Api.Services;

/// <summary>
/// Background service para limpeza do cache de rate limiting
/// </summary>
public class RateLimitCleanupService : BackgroundService
{
    private readonly ILogger<RateLimitCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);
    private readonly TimeSpan _maxAge = TimeSpan.FromHours(1);

    public RateLimitCleanupService(ILogger<RateLimitCleanupService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Rate limit cleanup service iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_cleanupInterval, stoppingToken);
                
                RateLimitingMiddleware.CleanupExpiredEntries(_maxAge);
                
                _logger.LogDebug("Limpeza do cache de rate limiting executada");
            }
            catch (OperationCanceledException)
            {
                // Expected quando o serviço é parado
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante limpeza do cache de rate limiting");
            }
        }

        _logger.LogInformation("Rate limit cleanup service finalizado");
    }
}