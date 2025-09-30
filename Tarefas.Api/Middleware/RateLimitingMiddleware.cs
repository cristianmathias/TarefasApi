using System.Collections.Concurrent;
using System.Net;
using Tarefas.Api.Models;
using System.Text.Json;

namespace Tarefas.Api.Middleware;

/// <summary>
/// Middleware para rate limiting simples baseado em IP
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitOptions _options;
    
    // Cache em memória para contadores (em produção usar Redis)
    private static readonly ConcurrentDictionary<string, ClientRequestInfo> Clients = new();
    
    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, RateLimitOptions options)
    {
        _next = next;
        _logger = logger;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_options.EnableRateLimit)
        {
            await _next(context);
            return;
        }

        var clientId = GetClientIdentifier(context);
        var clientInfo = GetOrCreateClientInfo(clientId);
        
        // Verificar se excedeu o limite
        if (clientInfo.RequestCount >= _options.MaxRequests)
        {
            await HandleRateLimitExceeded(context, clientInfo);
            return;
        }

        // Incrementar contador
        clientInfo.RequestCount++;
        
        // Adicionar headers informativos
        AddRateLimitHeaders(context, clientInfo);
        
        await _next(context);
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Em produção, considerar usar identificadores mais sofisticados
        // como user ID, API key, ou combinação IP + User-Agent
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        return !string.IsNullOrEmpty(forwardedFor) 
            ? forwardedFor.Split(',')[0].Trim()
            : context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private ClientRequestInfo GetOrCreateClientInfo(string clientId)
    {
        var now = DateTime.UtcNow;
        
        return Clients.AddOrUpdate(clientId, 
            new ClientRequestInfo { FirstRequest = now, RequestCount = 0 },
            (key, existingInfo) =>
            {
                // Reset contador se passou o período
                if (now - existingInfo.FirstRequest >= _options.TimeWindow)
                {
                    existingInfo.FirstRequest = now;
                    existingInfo.RequestCount = 0;
                }
                return existingInfo;
            });
    }

    private void AddRateLimitHeaders(HttpContext context, ClientRequestInfo clientInfo)
    {
        var remaining = Math.Max(0, _options.MaxRequests - clientInfo.RequestCount);
        var resetTime = clientInfo.FirstRequest.Add(_options.TimeWindow);
        
        context.Response.Headers.TryAdd("X-RateLimit-Limit", _options.MaxRequests.ToString());
        context.Response.Headers.TryAdd("X-RateLimit-Remaining", remaining.ToString());
        context.Response.Headers.TryAdd("X-RateLimit-Reset", ((DateTimeOffset)resetTime).ToUnixTimeSeconds().ToString());
    }

    private async Task HandleRateLimitExceeded(HttpContext context, ClientRequestInfo clientInfo)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString();
        var resetTime = clientInfo.FirstRequest.Add(_options.TimeWindow);
        var retryAfterSeconds = (int)(resetTime - DateTime.UtcNow).TotalSeconds;
        
        _logger.LogWarning("Rate limit excedido para client {ClientId}. CorrelationId: {CorrelationId}", 
            GetClientIdentifier(context), correlationId);

        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        context.Response.ContentType = "application/json";
        context.Response.Headers.TryAdd("Retry-After", retryAfterSeconds.ToString());
        
        AddRateLimitHeaders(context, clientInfo);

        var errorResponse = new ErrorResponse
        {
            Message = "Muitas requisições. Tente novamente mais tarde.",
            CorrelationId = correlationId,
            TraceId = context.TraceIdentifier
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var jsonResponse = JsonSerializer.Serialize(errorResponse, jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }

    // Limpeza periódica do cache (executar em background)
    public static void CleanupExpiredEntries(TimeSpan maxAge)
    {
        var cutoff = DateTime.UtcNow - maxAge;
        var expiredKeys = Clients
            .Where(kvp => kvp.Value.FirstRequest < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            Clients.TryRemove(key, out _);
        }
    }
}

/// <summary>
/// Informações de requisições do cliente
/// </summary>
public class ClientRequestInfo
{
    public DateTime FirstRequest { get; set; }
    public int RequestCount { get; set; }
}

/// <summary>
/// Opções de configuração do rate limiting
/// </summary>
public class RateLimitOptions
{
    public bool EnableRateLimit { get; set; } = true;
    public int MaxRequests { get; set; } = 100; // requests por janela de tempo
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(1); // 1 minuto
}