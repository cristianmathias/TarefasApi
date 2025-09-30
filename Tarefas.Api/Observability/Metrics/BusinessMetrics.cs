using System.Diagnostics.Metrics;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Tarefas.Api.Observability.Metrics;

/// <summary>
/// Métricas de negócio específicas da aplicação de Tarefas
/// </summary>
public class BusinessMetrics : IDisposable
{
    private readonly Meter _meter;
    
    // Counters para operações de negócio
    private readonly Counter<long> _tarefasCriadasCounter;
    private readonly Counter<long> _tarefasConcluidasCounter;
    private readonly Counter<long> _tarefasExcluidasCounter;
    private readonly Counter<long> _consultasPaginadasCounter;
    
    // Histogramas para latência de operações
    private readonly Histogram<double> _operacaoCrudLatencia;
    private readonly Histogram<double> _consultaPaginadaLatencia;

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BusinessMetrics> _logger;

    public BusinessMetrics(IServiceProvider serviceProvider, ILogger<BusinessMetrics> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        _meter = new Meter("TarefasAPI.Business", "1.0.0");

        // Inicializar counters
        _tarefasCriadasCounter = _meter.CreateCounter<long>(
            "tarefas_criadas_total",
            "número",
            "Total de tarefas criadas desde o início da aplicação"
        );

        _tarefasConcluidasCounter = _meter.CreateCounter<long>(
            "tarefas_concluidas_total", 
            "número",
            "Total de tarefas marcadas como concluídas"
        );

        _tarefasExcluidasCounter = _meter.CreateCounter<long>(
            "tarefas_excluidas_total",
            "número", 
            "Total de tarefas excluídas (soft delete)"
        );

        _consultasPaginadasCounter = _meter.CreateCounter<long>(
            "consultas_paginadas_total",
            "número",
            "Total de consultas paginadas realizadas"
        );

        // Inicializar gauges observáveis
        _meter.CreateObservableGauge<int>(
            "tarefas_ativas_atual",
            () => GetCurrentActiveCount(),
            "número",
            "Número atual de tarefas ativas (não excluídas)"
        );

        _meter.CreateObservableGauge<int>(
            "tarefas_concluidas_atual",
            () => GetCurrentCompletedCount(),
            "número", 
            "Número atual de tarefas concluídas"
        );

        // Inicializar histogramas
        _operacaoCrudLatencia = _meter.CreateHistogram<double>(
            "operacao_crud_duracao_ms",
            "milissegundos",
            "Duração das operações CRUD em milissegundos"
        );

        _consultaPaginadaLatencia = _meter.CreateHistogram<double>(
            "consulta_paginada_duracao_ms", 
            "milissegundos",
            "Duração das consultas paginadas em milissegundos"
        );

        _logger.LogDebug("BusinessMetrics inicializado com sucesso");
    }

    /// <summary>
    /// Registra a criação de uma nova tarefa
    /// </summary>
    public void RecordTarefaCriada(string? categoria = null)
    {
        var tags = new KeyValuePair<string, object?>[]
        {
            new("categoria", categoria ?? "default")
        };
        
        _tarefasCriadasCounter.Add(1, tags);
        _logger.LogDebug("Métrica registrada: Tarefa criada");
    }

    /// <summary>
    /// Registra a conclusão de uma tarefa
    /// </summary>
    public void RecordTarefaConcluida()
    {
        _tarefasConcluidasCounter.Add(1);
        _logger.LogDebug("Métrica registrada: Tarefa concluída");
    }

    /// <summary>
    /// Registra a exclusão de uma tarefa
    /// </summary>
    public void RecordTarefaExcluida()
    {
        _tarefasExcluidasCounter.Add(1);
        _logger.LogDebug("Métrica registrada: Tarefa excluída");
    }

    /// <summary>
    /// Registra uma consulta paginada
    /// </summary>
    public void RecordConsultaPaginada(int pagina, int tamanhoPagina, int totalResultados)
    {
        var tags = new KeyValuePair<string, object?>[]
        {
            new("pagina", pagina.ToString()),
            new("tamanho_pagina", tamanhoPagina.ToString()),
            new("tem_resultados", (totalResultados > 0).ToString())
        };

        _consultasPaginadasCounter.Add(1, tags);
        _logger.LogDebug("Métrica registrada: Consulta paginada - Página {Pagina}, Total {Total}", 
            pagina, totalResultados);
    }

    /// <summary>
    /// Registra a latência de uma operação CRUD
    /// </summary>
    public void RecordOperacaoCrudLatencia(double latenciaMs, string operacao, bool sucesso)
    {
        var tags = new KeyValuePair<string, object?>[]
        {
            new("operacao", operacao),
            new("sucesso", sucesso.ToString())
        };

        _operacaoCrudLatencia.Record(latenciaMs, tags);
        _logger.LogDebug("Métrica registrada: Latência {Operacao} - {LatenciaMs}ms", operacao, latenciaMs);
    }

    /// <summary>
    /// Registra a latência de uma consulta paginada
    /// </summary>
    public void RecordConsultaPaginadaLatencia(double latenciaMs, int totalResultados)
    {
        var categoria = totalResultados switch
        {
            0 => "vazio",
            <= 10 => "pequeno",
            <= 100 => "medio", 
            _ => "grande"
        };

        var tags = new KeyValuePair<string, object?>[]
        {
            new("resultados_categoria", categoria)
        };

        _consultaPaginadaLatencia.Record(latenciaMs, tags);
    }

    /// <summary>
    /// Obtém contagem atual de tarefas ativas
    /// </summary>
    private int GetCurrentActiveCount()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<Tarefas.Infrastructure.Data.TarefasDbContext>();
            
            return context.Tarefas.Count(t => !t.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter contagem de tarefas ativas");
            return 0;
        }
    }

    /// <summary>
    /// Obtém contagem atual de tarefas concluídas
    /// </summary>
    private int GetCurrentCompletedCount()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<Tarefas.Infrastructure.Data.TarefasDbContext>();
            
            return context.Tarefas.Count(t => !t.IsDeleted && t.Concluida);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter contagem de tarefas concluídas");
            return 0;
        }
    }

    public void Dispose()
    {
        _meter?.Dispose();
        _logger.LogDebug("BusinessMetrics disposed");
    }
}