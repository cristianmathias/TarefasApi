using Tarefas.Domain.Entities;
using Tarefas.Infrastructure.Repositories;
using Xunit;

namespace Tarefas.Tests;

/// <summary>
/// Testes específicos para edge cases do repositório em memória
/// </summary>
public class TarefaRepositoryEdgeCasesTests
{
    [Fact]
    public async Task AdicionarAsync_DeveGerarIdsSequenciais()
    {
        // Arrange
        var repository = new TarefaRepository();
        var tarefa1 = new Tarefa { Titulo = "Primeira", Descricao = "Desc 1", Concluida = false };
        var tarefa2 = new Tarefa { Titulo = "Segunda", Descricao = "Desc 2", Concluida = false };

        // Act
        var resultado1 = await repository.AdicionarAsync(tarefa1);
        var resultado2 = await repository.AdicionarAsync(tarefa2);

        // Assert
        Assert.Equal(1, resultado1.Id);
        Assert.Equal(2, resultado2.Id);
    }

    [Fact]
    public async Task ObterTodasAsync_ComRepositorioVazio_DeveRetornarListaVazia()
    {
        // Arrange
        var repository = new TarefaRepository();

        // Act
        var resultado = await repository.ObterTodasAsync(1, 10);

        // Assert
        Assert.NotNull(resultado);
        Assert.Empty(resultado);
    }

    [Fact]
    public async Task ObterTodasAsync_ComPaginacaoAlemDoLimite_DeveRetornarVazio()
    {
        // Arrange
        var repository = new TarefaRepository();
        await repository.AdicionarAsync(new Tarefa { Titulo = "Única", Descricao = "Desc", Concluida = false });

        // Act - Buscar página 2 quando só existe 1 item
        var resultado = await repository.ObterTodasAsync(2, 10);

        // Assert
        Assert.NotNull(resultado);
        Assert.Empty(resultado);
    }

    [Theory]
    [InlineData(1, 1, 1)] // 1 item, página 1, tamanho 1 = 1 resultado
    [InlineData(5, 1, 3)] // 5 itens, página 1, tamanho 3 = 3 resultados  
    [InlineData(5, 2, 3)] // 5 itens, página 2, tamanho 3 = 2 resultados
    [InlineData(10, 3, 4)] // 10 itens, página 3, tamanho 4 = 2 resultados
    public async Task ObterTodasAsync_PaginacaoCorreta(int totalItens, int pagina, int tamanhoPagina)
    {
        // Arrange
        var repository = new TarefaRepository();
        for (int i = 1; i <= totalItens; i++)
        {
            await repository.AdicionarAsync(new Tarefa 
            { 
                Titulo = $"Tarefa {i}", 
                Descricao = $"Desc {i}", 
                Concluida = i % 2 == 0 
            });
        }

        // Act
        var resultado = await repository.ObterTodasAsync(pagina, tamanhoPagina);

        // Assert
        var esperado = Math.Max(0, Math.Min(tamanhoPagina, totalItens - (pagina - 1) * tamanhoPagina));
        Assert.Equal(esperado, resultado.Count());
    }

    [Fact]
    public async Task AtualizarAsync_TarefaInexistente_DeveRetornarNull()
    {
        // Arrange
        var repository = new TarefaRepository();
        var tarefaInexistente = new Tarefa { Id = 999, Titulo = "Inexistente", Descricao = "Desc", Concluida = true };

        // Act
        var resultado = await repository.AtualizarAsync(tarefaInexistente);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task AtualizarAsync_TarefaExistente_DeveAtualizarCampos()
    {
        // Arrange
        var repository = new TarefaRepository();
        var tarefaOriginal = new Tarefa { Titulo = "Original", Descricao = "Desc Original", Concluida = false };
        var criada = await repository.AdicionarAsync(tarefaOriginal);

        // Modificar a tarefa
        criada.Titulo = "Título Atualizado";
        criada.Concluida = true;

        // Act
        var resultado = await repository.AtualizarAsync(criada);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Título Atualizado", resultado.Titulo);
        Assert.True(resultado.Concluida);
        Assert.Equal("Desc Original", resultado.Descricao); // Descrição não mudou

        // Verificar que foi realmente atualizada no repositório
        var verificacao = await repository.ObterPorIdAsync(criada.Id);
        Assert.Equal("Título Atualizado", verificacao?.Titulo);
    }

    [Fact]
    public async Task DeletarAsync_TarefaExistente_DeveRemover()
    {
        // Arrange
        var repository = new TarefaRepository();
        var tarefa = new Tarefa { Titulo = "Para Deletar", Descricao = "Será removida", Concluida = false };
        var criada = await repository.AdicionarAsync(tarefa);

        // Act
        await repository.DeletarAsync(criada.Id);

        // Assert
        var verificacao = await repository.ObterPorIdAsync(criada.Id);
        Assert.Null(verificacao);
    }

    [Fact]
    public async Task DeletarAsync_TarefaInexistente_NaoDeveLancarErro()
    {
        // Arrange
        var repository = new TarefaRepository();

        // Act & Assert - Não deve lançar exceção
        await repository.DeletarAsync(999);
    }

    [Fact]
    public async Task OperacoesConcorrentes_DeveSerThreadSafe()
    {
        // Arrange
        var repository = new TarefaRepository();
        const int numeroTarefas = 100;

        // Act - Criar tarefas concorrentemente
        var tasks = Enumerable.Range(1, numeroTarefas).Select(async i =>
        {
            var tarefa = new Tarefa 
            { 
                Titulo = $"Tarefa Concorrente {i}", 
                Descricao = $"Descrição {i}", 
                Concluida = i % 2 == 0 
            };
            return await repository.AdicionarAsync(tarefa);
        });

        var resultados = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(numeroTarefas, resultados.Length);
        
        // Todos os IDs devem ser únicos
        var ids = resultados.Select(r => r.Id).ToArray();
        Assert.Equal(numeroTarefas, ids.Distinct().Count());
        
        // IDs devem estar na sequência correta (1 a numeroTarefas)
        Array.Sort(ids);
        for (int i = 0; i < numeroTarefas; i++)
        {
            Assert.Equal(i + 1, ids[i]);
        }
    }

    [Fact]
    public async Task EstadoDoRepositorio_AposVariasOperacoes_DeveMantarConsistencia()
    {
        // Arrange
        var repository = new TarefaRepository();

        // Act & Assert - Série de operações para testar consistência
        
        // 1. Adicionar algumas tarefas
        var tarefa1 = await repository.AdicionarAsync(new Tarefa { Titulo = "T1", Descricao = "D1", Concluida = false });
        var tarefa2 = await repository.AdicionarAsync(new Tarefa { Titulo = "T2", Descricao = "D2", Concluida = false });
        var tarefa3 = await repository.AdicionarAsync(new Tarefa { Titulo = "T3", Descricao = "D3", Concluida = false });

        Assert.Equal(1, tarefa1.Id);
        Assert.Equal(2, tarefa2.Id);
        Assert.Equal(3, tarefa3.Id);

        // 2. Verificar contagem total
        var todasTarefas = await repository.ObterTodasAsync(1, 100);
        Assert.Equal(3, todasTarefas.Count());

        // 3. Atualizar uma tarefa
        tarefa2.Titulo = "T2 Atualizada";
        tarefa2.Concluida = true;
        await repository.AtualizarAsync(tarefa2);

        // 4. Deletar uma tarefa
        await repository.DeletarAsync(tarefa1.Id);

        // 5. Verificar estado final
        var estadoFinal = await repository.ObterTodasAsync(1, 100);
        Assert.Equal(2, estadoFinal.Count());
        
        var t2Verificacao = await repository.ObterPorIdAsync(tarefa2.Id);
        Assert.NotNull(t2Verificacao);
        Assert.Equal("T2 Atualizada", t2Verificacao.Titulo);
        Assert.True(t2Verificacao.Concluida);

        var t1Verificacao = await repository.ObterPorIdAsync(tarefa1.Id);
        Assert.Null(t1Verificacao);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    [InlineData(-1, -1)]
    public async Task ObterTodasAsync_ComParametrosInvalidos_DeveProcessarSemErro(int pagina, int tamanhoPagina)
    {
        // Arrange
        var repository = new TarefaRepository();
        await repository.AdicionarAsync(new Tarefa { Titulo = "Teste", Descricao = "Desc", Concluida = false });

        // Act & Assert - Não deve lançar exceção
        var resultado = await repository.ObterTodasAsync(pagina, tamanhoPagina);
        
        // O comportamento depende da implementação, mas não deve quebrar
        Assert.NotNull(resultado);
    }
}