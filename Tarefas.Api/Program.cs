using Tarefas.Api.Extensions;
using Tarefas.Infrastructure.Extensions;
using Tarefas.Api.Observability.Logging;
using Tarefas.Api.Observability.HealthChecks;
using Tarefas.Api.Observability.Metrics;
using Tarefas.Api.Observability.Tracing;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Serilog;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Tarefas.Api;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ============================================================================
        // OBSERVABILIDADE - LOGGING (Serilog)
        // ============================================================================
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            var webHostEnvironment = services.GetRequiredService<IWebHostEnvironment>();
            SerilogConfiguration.ConfigureSerilog(context.Configuration, webHostEnvironment)
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services);
        });

        // ============================================================================
        // OBSERVABILIDADE - HEALTH CHECKS
        // ============================================================================
        if (!builder.Environment.IsEnvironment("Testing"))
        {
            builder.Services.AddHealthChecks()
                .AddCheck<ApplicationHealthCheck>("application")
                .AddCheck<DatabaseHealthCheck>("database") 
                .AddCheck<RateLimitHealthCheck>("rate_limit")
                .AddDbContextCheck<Tarefas.Infrastructure.Data.TarefasDbContext>("ef_database");
        }

        // Health Checks UI (apenas se não estiver em ambiente de teste)
        if (!builder.Environment.IsEnvironment("Testing"))
        {
            builder.Services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(30); // Avalia a cada 30 segundos
                setup.MaximumHistoryEntriesPerEndpoint(50); // Mantém histórico de 50 entradas
                setup.AddHealthCheckEndpoint("TarefasAPI", "/health");
                setup.AddHealthCheckEndpoint("TarefasAPI-Ready", "/health/ready");  
                setup.AddHealthCheckEndpoint("TarefasAPI-Live", "/health/live");
            }).AddInMemoryStorage();
        }

        // ============================================================================
        // OBSERVABILIDADE - MÉTRICAS E TELEMETRIA
        // ============================================================================
        builder.Services.AddSingleton<BusinessMetrics>();
        builder.Services.AddSingleton<TelemetryService>();

        // OpenTelemetry
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService("TarefasAPI", "1.0.0")
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = builder.Environment.EnvironmentName,
                    ["service.instance.id"] = Environment.MachineName
                }))
            .WithTracing(tracing => tracing
                .AddSource("TarefasAPI")
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.EnrichWithHttpRequest = (activity, request) =>
                    {
                        activity.SetTag("http.client_ip", request.HttpContext.Connection.RemoteIpAddress?.ToString());
                        activity.SetTag("http.user_agent", request.Headers.UserAgent.FirstOrDefault());
                    };
                    options.EnrichWithHttpResponse = (activity, response) =>
                    {
                        activity.SetTag("http.response.size", response.ContentLength);
                    };
                })
                .AddEntityFrameworkCoreInstrumentation(options =>
                {
                    options.SetDbStatementForStoredProcedure = true;
                    options.SetDbStatementForText = true;
                })
                .AddHttpClientInstrumentation()
                .AddConsoleExporter())
            .WithMetrics(metrics => metrics
                .AddMeter("TarefasAPI.Business")
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddConsoleExporter()
                .AddPrometheusExporter());

        // ===== CONFIGURAÇÃO DE SERVIÇOS =====
        
        // Controllers com validação customizada
        builder.Services.AddControllers();
        builder.Services.AddCustomValidation();

        // API Explorer e Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddCustomSwagger();

        // CORS
        builder.Services.AddCustomCors(builder.Configuration);

        // Rate Limiting
        builder.Services.AddRateLimiting(builder.Configuration);

        // Application Services (DI + EF Core + Repositories)
        builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);

        var app = builder.Build();

        // ============================================================================
        // PIPELINE DE MIDDLEWARES - OBSERVABILIDADE
        // ============================================================================

        // Serilog request logging
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = SerilogConfiguration.EnrichFromRequest;
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        });

        // Health Checks Endpoints (apenas se não estiver em ambiente de teste)
        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                ResultStatusCodes =
                {
                    [HealthStatus.Healthy] = StatusCodes.Status200OK,
                    [HealthStatus.Degraded] = StatusCodes.Status200OK,
                    [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                }
            });

            app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready") || check.Name == "database",
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions  
            {
                Predicate = check => check.Name == "application",
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        }

        // Health Checks UI (apenas se não estiver em ambiente de teste)
        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.MapHealthChecksUI(setup =>
            {
                setup.UIPath = "/health-ui";
                setup.ApiPath = "/health-ui-api";  
            });
        }

        // OpenTelemetry Prometheus endpoint
        app.MapPrometheusScrapingEndpoint();

        // ===== INICIALIZAÇÃO DO BANCO DE DADOS =====
        await InitializeDatabaseAsync(app);

        // ===== CONFIGURAÇÃO DO PIPELINE =====
        
        // Segurança (middleware de segurança, correlation ID, rate limiting, exception handling)
        app.UseCustomSecurity(app.Environment);

        // CORS
        app.UseCustomCors(app.Environment);

        // Swagger (somente desenvolvimento)
        app.UseCustomSwagger(app.Environment);

        // Authorization (placeholder para futuras implementações)
        app.UseAuthorization();

        // Controllers
        app.MapControllers();

        // ============================================================================
        // INICIALIZAÇÃO DA OBSERVABILIDADE  
        // ============================================================================
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("🚀 TarefasAPI iniciando...");
        logger.LogInformation("Ambiente: {Environment}", app.Environment.EnvironmentName);
        logger.LogInformation("Health Checks disponíveis em: /health, /health/ready, /health/live");
        logger.LogInformation("Health Checks UI disponível em: /health-ui");
        logger.LogInformation("Métricas Prometheus disponíveis em: /metrics");

        try
        {
            logger.LogInformation("✅ TarefasAPI iniciado com sucesso!");
            app.Run();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "❌ Falha crítica durante inicialização da aplicação");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    /// Inicializa banco de dados automaticamente na startup
    /// </summary>
    private static async Task InitializeDatabaseAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        try
        {
            logger.LogInformation("🔧 Inicializando banco de dados...");
            await initializer.InitializeAsync();
            logger.LogInformation("✅ Banco de dados inicializado com sucesso");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Erro crítico durante inicialização do banco de dados");
            
            // Em produção, talvez queiramos falhar fast
            if (app.Environment.IsProduction())
            {
                throw;
            }
            
            // Em desenvolvimento, apenas logar e continuar
            logger.LogWarning("⚠️ Continuando execução mesmo com erro de banco (ambiente de desenvolvimento)");
        }
    }
}

