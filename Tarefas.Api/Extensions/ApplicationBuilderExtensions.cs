using Tarefas.Api.Middleware;

namespace Tarefas.Api.Extensions;

/// <summary>
/// Extensões para configuração do pipeline de middleware
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configura middleware de segurança
    /// </summary>
    public static IApplicationBuilder UseCustomSecurity(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        // HTTPS redirection (sempre primeiro)
        app.UseHttpsRedirection();

        // Security headers
        app.UseSecurityHeaders();

        // Correlation ID (antes de outros middlewares para rastreamento)
        app.UseMiddleware<CorrelationIdMiddleware>();

        // Rate limiting
        app.UseMiddleware<RateLimitingMiddleware>();

        // Exception handling (deve estar cedo no pipeline)
        app.UseMiddleware<ExceptionMiddleware>();

        return app;
    }

    /// <summary>
    /// Configura headers de segurança
    /// </summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            // Prevenir clickjacking
            context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
            
            // Prevenir MIME type sniffing
            context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
            
            // XSS protection
            context.Response.Headers.TryAdd("X-XSS-Protection", "1; mode=block");
            
            // Referrer policy
            context.Response.Headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
            
            // Remover headers que expõem informações do servidor
            context.Response.Headers.Remove("Server");
            context.Response.Headers.Remove("X-Powered-By");
            
            await next();
        });

        return app;
    }

    /// <summary>
    /// Configura CORS baseado no ambiente
    /// </summary>
    public static IApplicationBuilder UseCustomCors(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        var corsPolicy = env.IsDevelopment() ? "Development" : "Production";
        app.UseCors(corsPolicy);
        return app;
    }

    /// <summary>
    /// Configura Swagger com segurança
    /// </summary>
    public static IApplicationBuilder UseCustomSwagger(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment() || env.IsEnvironment("Testing"))
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tarefas API v1");
                c.RoutePrefix = "swagger";
                c.DocumentTitle = "Tarefas API Documentation";
                c.DefaultModelExpandDepth(2);
                c.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example);
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                c.EnableDeepLinking();
            });
        }

        return app;
    }
}