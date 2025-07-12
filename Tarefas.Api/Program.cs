using Tarefas.Application.Interfaces;
using Tarefas.Application.Services;
using Tarefas.Domain.Interfaces;
using Tarefas.Infrastructure.Repositories;


namespace Tarefas.Api;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddSwaggerGen();
        }

        // Dependency Injection
        builder.Services.AddScoped<ITarefaService, TarefaService>();
        builder.Services.AddSingleton<ITarefaRepository, TarefaRepository>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}

