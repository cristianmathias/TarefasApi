using Tarefas.Api.Extensions;
using Tarefas.Api.Filters;

namespace Tarefas.Api;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

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

        // Application Services (DI)
        builder.Services.AddApplicationServices();

        var app = builder.Build();

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

        app.Run();
    }
}

