using Tarefas.Api.Extensions;
using Tarefas.Infrastructure.Extensions;

namespace Tarefas.Api;

public partial class Program
{
    public static async Task Main(string[] args)
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

        // Application Services (DI + EF Core + Repositories)
        builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);

        var app = builder.Build();

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

        app.Run();
    }

    /// <summary>
    /// Inicializa banco de dados automaticamente na startup
    /// </summary>
    private static async Task InitializeDatabaseAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
        
        try
        {
            await initializer.InitializeAsync();
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Erro crítico durante inicialização do banco de dados");
            
            // Em produção, talvez queiramos falhar fast
            if (app.Environment.IsProduction())
            {
                throw;
            }
            
            // Em desenvolvimento, apenas logar e continuar
            logger.LogWarning("Continuando execução mesmo com erro de banco (ambiente de desenvolvimento)");
        }
    }
}

