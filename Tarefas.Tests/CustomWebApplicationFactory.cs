using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tarefas.Api;
using Tarefas.Domain.Interfaces;
using Tarefas.Infrastructure.Data;
using Tarefas.Infrastructure.Repositories;

namespace Tarefas.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove TODOS os serviços relacionados ao EF Core existente
            var servicesToRemove = services
                .Where(s => s.ServiceType.ToString().Contains("EntityFramework") ||
                           s.ServiceType.ToString().Contains("DbContext") ||
                           s.ServiceType == typeof(TarefasDbContext) ||
                           (s.ImplementationType != null && s.ImplementationType == typeof(EfTarefaRepository)))
                .ToList();

            foreach (var service in servicesToRemove)
            {
                services.Remove(service);
            }

            // Remove especificamente os serviços que podem causar conflito
            var contextOptions = services.Where(s => s.ServiceType == typeof(DbContextOptions<TarefasDbContext>)).ToList();
            var contextOptionsBase = services.Where(s => s.ServiceType == typeof(DbContextOptions)).ToList();
            var context = services.Where(s => s.ServiceType == typeof(TarefasDbContext)).ToList();

            contextOptions.ForEach(s => services.Remove(s));
            contextOptionsBase.ForEach(s => services.Remove(s));
            context.ForEach(s => services.Remove(s));

            // Adiciona APENAS InMemory (limpo)
            services.AddDbContext<TarefasDbContext>(options =>
            {
                options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                       .EnableSensitiveDataLogging()
                       .LogTo(message => { }, LogLevel.None); // Desabilita logs
            });

            // Reregistra apenas o repositório EF necessário
            services.AddScoped<ITarefaRepository, EfTarefaRepository>();

            // Logs mínimos
            services.AddLogging(loggingBuilder => 
                loggingBuilder.SetMinimumLevel(LogLevel.Error));
        });

        // Usar ambiente específico de teste
        builder.UseEnvironment("Testing");
    }

    /// <summary>
    /// Inicializa o banco de dados e popula com dados de teste
    /// </summary>
    public void InitializeDbForTests()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TarefasDbContext>();
        
        // Garantir que o banco está criado
        context.Database.EnsureCreated();
        
        // Limpar dados existentes
        context.Tarefas.RemoveRange(context.Tarefas);
        context.SaveChanges();
        
        // Adicionar dados de teste
        var tarefas = new[]
        {
            new Domain.Entities.Tarefa
            {
                Id = 1,
                Titulo = "Tarefa Teste 1",
                Descricao = "Descrição da tarefa 1",
                Concluida = false,
                DataCriacao = DateTime.UtcNow.AddDays(-1),
                DataAtualizacao = DateTime.UtcNow.AddDays(-1),
                IsDeleted = false
            },
            new Domain.Entities.Tarefa
            {
                Id = 2,
                Titulo = "Tarefa Teste 2", 
                Descricao = "Descrição da tarefa 2",
                Concluida = true,
                DataCriacao = DateTime.UtcNow.AddDays(-2),
                DataAtualizacao = DateTime.UtcNow.AddHours(-1),
                IsDeleted = false
            }
        };

        context.Tarefas.AddRange(tarefas);
        context.SaveChanges();
    }

    /// <summary>
    /// Limpa o banco entre testes
    /// </summary>
    public void CleanDatabase()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TarefasDbContext>();
        
        context.Tarefas.RemoveRange(context.Tarefas);
        context.SaveChanges();
    }

    /// <summary>
    /// Força reset do auto-increment para testes que precisam de IDs específicos
    /// </summary>
    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TarefasDbContext>();
        
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        context.ChangeTracker.Clear();
    }
}