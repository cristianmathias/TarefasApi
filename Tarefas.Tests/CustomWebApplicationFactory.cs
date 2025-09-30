using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tarefas.Api;
using Tarefas.Infrastructure.Data;

namespace Tarefas.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove completamente o DbContext existente e suas dependências
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TarefasDbContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            var dbContextOptionsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions));
            if (dbContextOptionsDescriptor != null)
            {
                services.Remove(dbContextOptionsDescriptor);
            }

            // Remove o TarefasDbContext se estiver registrado
            var contextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TarefasDbContext));
            if (contextDescriptor != null)
            {
                services.Remove(contextDescriptor);
            }

            // Adiciona banco em memória para testes (limpo)
            services.AddDbContext<TarefasDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid().ToString());
                options.EnableSensitiveDataLogging();
            });

            // Desabilitar logs desnecessários para testes
            services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
        });

        builder.UseEnvironment("Testing");
    }

    /// <summary>
    /// Cria e popula dados no banco de teste
    /// </summary>
    public void SeedTestData(Action<TarefasDbContext> seedAction)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TarefasDbContext>();
        
        // Garantir que o banco está criado e limpo
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        
        // Executar seed personalizado
        seedAction(context);
        context.SaveChanges();
    }

    /// <summary>
    /// Popula dados padrão para testes
    /// </summary>
    public void SeedDefaultTestData()
    {
        SeedTestData(context =>
        {
            context.Tarefas.AddRange(
                new Domain.Entities.Tarefa 
                { 
                    Id = 1, 
                    Titulo = "Tarefa Teste 1", 
                    Descricao = "Descrição 1", 
                    Concluida = false,
                    DataCriacao = DateTime.UtcNow.AddDays(-1),
                    DataAtualizacao = DateTime.UtcNow.AddDays(-1)
                },
                new Domain.Entities.Tarefa 
                { 
                    Id = 2, 
                    Titulo = "Tarefa Teste 2", 
                    Descricao = "Descrição 2", 
                    Concluida = true,
                    DataCriacao = DateTime.UtcNow.AddDays(-2),
                    DataAtualizacao = DateTime.UtcNow.AddHours(-1)
                }
            );
        });
    }
}