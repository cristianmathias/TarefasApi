namespace Tarefas.Api.Middleware;

/// <summary>
/// Middleware para geração e propagação de Correlation ID
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrGenerateCorrelationId(context);
        
        // Armazenar no contexto para uso posterior
        context.Items["CorrelationId"] = correlationId;
        
        // Adicionar ao response header
        context.Response.Headers.TryAdd(CorrelationIdHeader, correlationId);
        
        // Configurar logger scope com correlation ID
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["TraceId"] = context.TraceIdentifier
        });

        _logger.LogInformation("Request iniciado: {Method} {Path}", 
            context.Request.Method, context.Request.Path);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        await _next(context);
        
        stopwatch.Stop();
        
        _logger.LogInformation("Request finalizado: {Method} {Path} - Status: {StatusCode} - Duration: {ElapsedMs}ms",
            context.Request.Method, 
            context.Request.Path, 
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }

    private static string GetOrGenerateCorrelationId(HttpContext context)
    {
        // Verificar se já existe no header da request
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var existingCorrelationId) &&
            !string.IsNullOrEmpty(existingCorrelationId))
        {
            return existingCorrelationId.ToString();
        }

        // Gerar novo correlation ID
        return Guid.NewGuid().ToString("N")[..12]; // 12 caracteres para facilitar logs
    }
}