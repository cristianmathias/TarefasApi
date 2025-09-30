using Serilog;
using Serilog.Events;
using Serilog.Enrichers.CorrelationId;

namespace Tarefas.Api.Observability.Logging;

/// <summary>
/// Configuração centralizada do Serilog para logging estruturado
/// </summary>
public static class SerilogConfiguration
{
    /// <summary>
    /// Configura o Serilog com múltiplos sinks e enrichers
    /// </summary>
    public static LoggerConfiguration ConfigureSerilog(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var loggerConfig = new LoggerConfiguration()
            // Nível de log baseado no ambiente
            .MinimumLevel.Is(GetLogLevel(environment))
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            
            // Enrichers para adicionar contexto aos logs
            .Enrich.FromLogContext()
            .Enrich.WithCorrelationId()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithProcessName()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Application", "TarefasAPI")
            .Enrich.WithProperty("Environment", environment.EnvironmentName)
            
            // Console output estruturado
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
                restrictedToMinimumLevel: LogEventLevel.Debug
            )
            
            // File output com rolling
            .WriteTo.File(
                path: "logs/tarefas-api-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{CorrelationId}] {Message:lj} {Properties:j}{NewLine}{Exception}",
                restrictedToMinimumLevel: LogEventLevel.Information
            )
            
            // File separado para erros
            .WriteTo.File(
                path: "logs/errors/tarefas-api-errors-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 90,
                restrictedToMinimumLevel: LogEventLevel.Error,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{CorrelationId}] {Message:lj} {Properties:j}{NewLine}{Exception}"
            );

        // Seq sink para desenvolvimento (se configurado)
        var seqServerUrl = configuration.GetValue<string>("Serilog:SeqServerUrl");
        if (!string.IsNullOrEmpty(seqServerUrl))
        {
            loggerConfig = loggerConfig.WriteTo.Seq(
                serverUrl: seqServerUrl,
                apiKey: configuration.GetValue<string>("Serilog:SeqApiKey"),
                restrictedToMinimumLevel: LogEventLevel.Debug
            );
        }

        // Configurações específicas para produção
        if (environment.IsProduction())
        {
            loggerConfig = loggerConfig
                .MinimumLevel.Override("Tarefas.Api", LogEventLevel.Information)
                .WriteTo.File(
                    path: "logs/audit/audit-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 365,
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{CorrelationId}] {SourceContext} {Message:lj} {Properties:j}{NewLine}{Exception}",
                    shared: true
                );
        }

        return loggerConfig;
    }

    /// <summary>
    /// Determina o nível de log baseado no ambiente
    /// </summary>
    private static LogEventLevel GetLogLevel(IWebHostEnvironment environment)
    {
        return environment.EnvironmentName.ToLowerInvariant() switch
        {
            "development" => LogEventLevel.Debug,
            "testing" => LogEventLevel.Information,
            "staging" => LogEventLevel.Information,
            "production" => LogEventLevel.Information,
            _ => LogEventLevel.Information
        };
    }

    /// <summary>
    /// Configura propriedades de contexto para requests HTTP
    /// </summary>
    public static void EnrichFromRequest(IDiagnosticContext diagnosticContext, HttpContext httpContext)
    {
        diagnosticContext.Set("RequestMethod", httpContext.Request.Method);
        diagnosticContext.Set("RequestPath", httpContext.Request.Path);
        diagnosticContext.Set("RequestQuery", httpContext.Request.QueryString.ToString());
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.FirstOrDefault());
        diagnosticContext.Set("ClientIpAddress", GetClientIpAddress(httpContext));
        
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            diagnosticContext.Set("UserId", httpContext.User.Identity.Name);
        }

        // Adiciona headers customizados se existirem
        if (httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
        {
            diagnosticContext.Set("ExternalCorrelationId", correlationId.FirstOrDefault());
        }
    }

    /// <summary>
    /// Obtém o IP do cliente considerando proxies
    /// </summary>
    private static string? GetClientIpAddress(HttpContext httpContext)
    {
        // Verifica headers de proxy primeiro
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return httpContext.Connection.RemoteIpAddress?.ToString();
    }
}