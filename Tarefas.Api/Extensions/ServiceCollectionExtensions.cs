using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Tarefas.Api.Filters;
using Tarefas.Api.Models;
using Tarefas.Api.Middleware;
using Tarefas.Api.Services;
using Tarefas.Application.Interfaces;
using Tarefas.Application.Services;
using Tarefas.Domain.Interfaces;
using Tarefas.Infrastructure.Repositories;

namespace Tarefas.Api.Extensions;

/// <summary>
/// Extensões para configuração de serviços
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configura CORS com políticas seguras
    /// </summary>
    public static IServiceCollection AddCustomCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            // Política para desenvolvimento
            options.AddPolicy("Development", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });

            // Política para produção (mais restritiva)
            options.AddPolicy("Production", builder =>
            {
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                    ?? new[] { "https://localhost", "https://*.azurewebsites.net" };
                
                builder
                    .WithOrigins(allowedOrigins)
                    .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                    .WithHeaders("Content-Type", "Authorization", "X-Correlation-ID")
                    .AllowCredentials()
                    .SetPreflightMaxAge(TimeSpan.FromHours(1));
            });
        });

        return services;
    }

    /// <summary>
    /// Configura rate limiting
    /// </summary>
    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var rateLimitOptions = new RateLimitOptions();
        configuration.GetSection("RateLimit").Bind(rateLimitOptions);
        
        services.AddSingleton(rateLimitOptions);
        
        // Background service para limpeza do cache
        services.AddHostedService<RateLimitCleanupService>();
        
        return services;
    }

    /// <summary>
    /// Configura validação de modelo
    /// </summary>
    public static IServiceCollection AddCustomValidation(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"))
                    .ToList();

                var correlationId = context.HttpContext.Items["CorrelationId"]?.ToString();

                var response = new ErrorResponse
                {
                    Message = "Dados de entrada inválidos",
                    Errors = errors,
                    CorrelationId = correlationId,
                    TraceId = context.HttpContext.TraceIdentifier
                };

                return new BadRequestObjectResult(response);
            };
        });

        return services;
    }

    /// <summary>
    /// Configura Swagger com segurança
    /// </summary>
    public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Tarefas API",
                Version = "v1",
                Description = "API REST para gerenciamento de tarefas seguindo os princípios da Clean Architecture",
                Contact = new OpenApiContact
                {
                    Name = "Equipe de Desenvolvimento",
                    Email = "dev@tarefas.com"
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // Incluir comentários XML
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            // Configurar esquemas de segurança (para futuro)
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header usando Bearer scheme. Exemplo: \"Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            // Headers customizados
            c.OperationFilter<CorrelationIdHeaderFilter>();
        });

        return services;
    }

    /// <summary>
    /// Configura injeção de dependência da aplicação
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Services
        services.AddScoped<ITarefaService, TarefaService>();
        
        // Repositories (atualmente em memória, será migrado para EF)
        services.AddSingleton<ITarefaRepository, TarefaRepository>();

        return services;
    }
}