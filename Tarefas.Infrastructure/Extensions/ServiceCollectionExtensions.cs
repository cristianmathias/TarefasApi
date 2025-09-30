using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tarefas.Domain.Interfaces;
using Tarefas.Infrastructure.Data;
using Tarefas.Infrastructure.Repositories;

namespace Tarefas.Infrastructure.Extensions;

/// <summary>
/// Extensões para configuração do Entity Framework e persistência
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configura Entity Framework com SQLite
    /// </summary>
    public static IServiceCollection AddEntityFramework(
        this IServiceCollection services, 
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Connection string baseada no ambiente
        var connectionString = GetConnectionString(configuration, environment);
        
        services.AddDbContext<TarefasDbContext>(options =>
        {
            options.UseSqlite(connectionString, sqliteOptions =>
            {
                sqliteOptions.MigrationsAssembly("Tarefas.Infrastructure");
                sqliteOptions.CommandTimeout(30);
            });

            // Configurações baseadas no ambiente
            if (environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
                options.LogTo(Console.WriteLine, LogLevel.Information);
            }
            
            // Configurações de performance
            options.EnableServiceProviderCaching();
            // options.EnableModelValidation(); // Removido - não existe no EF Core 9
        });

        return services;
    }

    /// <summary>
    /// Registra repositórios thread-safe
    /// </summary>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Registrar repositório EF como implementação principal
        services.AddScoped<ITarefaRepository, EfTarefaRepository>();
        
        // Manter repositório in-memory como fallback/testing (se necessário)
        services.AddTransient<TarefaRepository>(); // In-memory repository

        return services;
    }

    /// <summary>
    /// Configura inicialização automática do banco
    /// </summary>
    public static IServiceCollection AddDatabaseInitialization(this IServiceCollection services)
    {
        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
        return services;
    }

    /// <summary>
    /// Determina connection string baseada no ambiente
    /// </summary>
    private static string GetConnectionString(IConfiguration configuration, IHostEnvironment environment)
    {
        // Em ambiente de teste, não configurar nada (será sobrescrito pelos testes)
        if (environment.EnvironmentName == "Testing")
        {
            return ""; // Será ignorado nos testes
        }

        // Verificar se há connection string configurada
        var configConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(configConnectionString))
        {
            return configConnectionString;
        }

        // Connection string baseada no ambiente
        return environment.EnvironmentName switch
        {
            "Development" => "Data Source=tarefas-dev.db;Cache=Shared",
            "Production" => "Data Source=/app/data/tarefas-prod.db;Cache=Shared",
            _ => "Data Source=tarefas.db;Cache=Shared"
        };
    }
}

/// <summary>
/// Interface para inicialização do banco de dados
/// </summary>
public interface IDatabaseInitializer
{
    Task InitializeAsync();
    Task MigrateAsync();
    Task SeedAsync();
}

/// <summary>
/// Implementação da inicialização do banco
/// </summary>
public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly TarefasDbContext _context;
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly IHostEnvironment _environment;

    public DatabaseInitializer(
        TarefasDbContext context, 
        ILogger<DatabaseInitializer> logger,
        IHostEnvironment environment)
    {
        _context = context;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Inicializa banco completo (migração + seed)
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Iniciando inicialização do banco de dados...");
            
            await MigrateAsync();
            await SeedAsync();
            
            _logger.LogInformation("Banco de dados inicializado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante inicialização do banco de dados");
            throw;
        }
    }

    /// <summary>
    /// Executa migrações pendentes
    /// </summary>
    public async Task MigrateAsync()
    {
        try
        {
            _logger.LogInformation("Verificando migrações pendentes...");
            
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                _logger.LogInformation("Aplicando {Count} migrações pendentes", pendingMigrations.Count());
                await _context.Database.MigrateAsync();
                _logger.LogInformation("Migrações aplicadas com sucesso");
            }
            else
            {
                _logger.LogInformation("Nenhuma migração pendente encontrada");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante aplicação de migrações");
            throw;
        }
    }

    /// <summary>
    /// Popula dados iniciais (apenas em desenvolvimento)
    /// </summary>
    public async Task SeedAsync()
    {
        try
        {
            if (_environment.IsProduction())
            {
                _logger.LogInformation("Pulando seed em produção");
                return;
            }

            _logger.LogInformation("Verificando necessidade de seed...");
            
            if (!await _context.Tarefas.AnyAsync())
            {
                _logger.LogInformation("Populando dados iniciais...");
                
                // Os dados de seed já estão definidos no DbContext
                // Apenas garantir que sejam criados
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Dados iniciais populados com sucesso");
            }
            else
            {
                _logger.LogInformation("Dados já existem, pulando seed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante seed de dados");
            throw;
        }
    }
}