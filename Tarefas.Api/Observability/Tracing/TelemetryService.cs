using System.Diagnostics;

namespace Tarefas.Api.Observability.Tracing;

/// <summary>
/// Serviço para facilitar a criação de activities/spans personalizados
/// </summary>
public class TelemetryService
{
    private static readonly ActivitySource ActivitySource = new("TarefasAPI", "1.0.0");
    private readonly ILogger<TelemetryService> _logger;

    public TelemetryService(ILogger<TelemetryService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Cria uma nova activity para uma operação de negócio
    /// </summary>
    public Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        var activity = ActivitySource.StartActivity(name, kind);
        if (activity != null)
        {
            _logger.LogDebug("Activity iniciada: {ActivityName} [{TraceId}]", name, activity.TraceId);
        }
        return activity;
    }

    /// <summary>
    /// Adiciona tags padronizadas para operações CRUD
    /// </summary>
    public static void EnrichCrudActivity(Activity? activity, string operation, string entityType, object? entityId = null)
    {
        if (activity == null) return;

        activity.SetTag("operation.type", operation);
        activity.SetTag("entity.type", entityType);
        
        if (entityId != null)
        {
            activity.SetTag("entity.id", entityId.ToString());
        }
    }

    /// <summary>
    /// Adiciona informações de paginação à activity
    /// </summary>
    public static void EnrichPaginationActivity(Activity? activity, int page, int pageSize, int totalResults)
    {
        if (activity == null) return;

        activity.SetTag("pagination.page", page);
        activity.SetTag("pagination.page_size", pageSize);
        activity.SetTag("pagination.total_results", totalResults);
        activity.SetTag("pagination.total_pages", (int)Math.Ceiling((double)totalResults / pageSize));
    }

    /// <summary>
    /// Adiciona informações de erro à activity
    /// </summary>
    public static void EnrichErrorActivity(Activity? activity, Exception exception)
    {
        if (activity == null) return;

        activity.SetStatus(ActivityStatusCode.Error, exception.Message);
        activity.SetTag("error.type", exception.GetType().Name);
        activity.SetTag("error.message", exception.Message);
        
        if (!string.IsNullOrEmpty(exception.StackTrace))
        {
            activity.SetTag("error.stack_trace", exception.StackTrace);
        }
    }

    /// <summary>
    /// Adiciona informações de sucesso à activity
    /// </summary>
    public static void EnrichSuccessActivity(Activity? activity, object? result = null)
    {
        if (activity == null) return;

        activity.SetStatus(ActivityStatusCode.Ok);
        
        if (result != null)
        {
            activity.SetTag("result.type", result.GetType().Name);
        }
    }

    /// <summary>
    /// Adiciona informações de rate limiting à activity
    /// </summary>
    public static void EnrichRateLimitActivity(Activity? activity, string clientId, bool allowed, int remainingRequests)
    {
        if (activity == null) return;

        activity.SetTag("rate_limit.client_id", clientId);
        activity.SetTag("rate_limit.allowed", allowed);
        activity.SetTag("rate_limit.remaining_requests", remainingRequests);
    }

    /// <summary>
    /// Executa uma operação dentro de uma activity com tratamento automático de erros
    /// </summary>
    public async Task<T> ExecuteWithActivityAsync<T>(
        string activityName, 
        Func<Activity?, Task<T>> operation,
        ActivityKind kind = ActivityKind.Internal)
    {
        using var activity = StartActivity(activityName, kind);
        
        try
        {
            var result = await operation(activity);
            EnrichSuccessActivity(activity, result);
            return result;
        }
        catch (Exception ex)
        {
            EnrichErrorActivity(activity, ex);
            _logger.LogError(ex, "Erro durante execução da activity {ActivityName}", activityName);
            throw;
        }
    }

    /// <summary>
    /// Executa uma operação dentro de uma activity (versão síncrona)
    /// </summary>
    public T ExecuteWithActivity<T>(
        string activityName,
        Func<Activity?, T> operation,
        ActivityKind kind = ActivityKind.Internal)
    {
        using var activity = StartActivity(activityName, kind);
        
        try
        {
            var result = operation(activity);
            EnrichSuccessActivity(activity, result);
            return result;
        }
        catch (Exception ex)
        {
            EnrichErrorActivity(activity, ex);
            _logger.LogError(ex, "Erro durante execução da activity {ActivityName}", activityName);
            throw;
        }
    }

    /// <summary>
    /// Obtém o ActivitySource para uso direto
    /// </summary>
    public static ActivitySource GetActivitySource() => ActivitySource;

    /// <summary>
    /// Cleanup do ActivitySource
    /// </summary>
    public static void Dispose()
    {
        ActivitySource?.Dispose();
    }
}