using Moq;
using Tarefas.Domain.Interfaces;
using Tarefas.Application.Services;
using Tarefas.Domain.Entities;
using Tarefas.Application.DTOs;
using Xunit;

namespace Tarefas.Tests;

public class TarefaServiceTests
{
    private readonly Mock<ITarefaRepository> _mockRepo;
    private readonly TarefaService _service;

    public TarefaServiceTests()
    {
        _mockRepo = new Mock<ITarefaRepository>();
        _service = new TarefaService(_mockRepo.Object);
    }

    [Fact]
    public async Task AdicionarAsync_DeveAdicionarTarefa()
    {
        // Arrange
        var criarTarefaDTO = new CriarTarefaDTO("Test Title", "Test Description");
        var tarefa = new Tarefa { Id = 1, Titulo = "Test Title", Descricao = "Test Description", Concluida = false };
        _mockRepo.Setup(repo => repo.AdicionarAsync(It.IsAny<Tarefa>())).ReturnsAsync(tarefa);

        // Act
        var result = await _service.AdicionarAsync(criarTarefaDTO);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Title", result.Titulo);
        Assert.Equal("Test Description", result.Descricao);
        Assert.False(result.Concluida); // Nova tarefa deve ser não concluída
        _mockRepo.Verify(repo => repo.AdicionarAsync(It.IsAny<Tarefa>()), Times.Once);
    }

    [Fact]
    public async Task ObterPorIdAsync_TarefaExistente_DeveRetornarTarefa()
    {
        // Arrange
        var tarefaId = 1;
        var tarefa = new Tarefa { Id = tarefaId, Titulo = "Teste", Descricao = "Descrição", Concluida = false };
        _mockRepo.Setup(repo => repo.ObterPorIdAsync(tarefaId)).ReturnsAsync(tarefa);

        // Act
        var result = await _service.ObterPorIdAsync(tarefaId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tarefaId, result.Id);
        Assert.Equal("Teste", result.Titulo);
        _mockRepo.Verify(repo => repo.ObterPorIdAsync(tarefaId), Times.Once);
    }

    [Fact]
    public async Task ObterPorIdAsync_TarefaInexistente_DeveRetornarNull()
    {
        // Arrange
        var tarefaId = 999;
        _mockRepo.Setup(repo => repo.ObterPorIdAsync(tarefaId)).ReturnsAsync((Tarefa?)null);

        // Act
        var result = await _service.ObterPorIdAsync(tarefaId);

        // Assert
        Assert.Null(result);
        _mockRepo.Verify(repo => repo.ObterPorIdAsync(tarefaId), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_TarefaExistente_DeveAtualizarTarefa()
    {
        // Arrange
        var tarefaId = 1;
        var tarefaExistente = new Tarefa { Id = tarefaId, Titulo = "Título Antigo", Descricao = "Descrição", Concluida = false };
        var atualizarDTO = new AtualizarTarefaDTO("Título Novo", true);
        var tarefaAtualizada = new Tarefa { Id = tarefaId, Titulo = "Título Novo", Descricao = "Descrição", Concluida = true };

        _mockRepo.Setup(repo => repo.ObterPorIdAsync(tarefaId)).ReturnsAsync(tarefaExistente);
        _mockRepo.Setup(repo => repo.AtualizarAsync(It.IsAny<Tarefa>())).ReturnsAsync(tarefaAtualizada);

        // Act
        var result = await _service.AtualizarAsync(tarefaId, atualizarDTO);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tarefaId, result.Id);
        Assert.Equal("Título Novo", result.Titulo);
        Assert.True(result.Concluida);
        _mockRepo.Verify(repo => repo.ObterPorIdAsync(tarefaId), Times.Once);
        _mockRepo.Verify(repo => repo.AtualizarAsync(It.IsAny<Tarefa>()), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_TarefaInexistente_DeveLancarExcecao()
    {
        // Arrange
        var tarefaId = 999;
        var atualizarDTO = new AtualizarTarefaDTO("Título", false);
        _mockRepo.Setup(repo => repo.ObterPorIdAsync(tarefaId)).ReturnsAsync((Tarefa?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _service.AtualizarAsync(tarefaId, atualizarDTO));
        Assert.Equal("Tarefa não encontrada", exception.Message);
        _mockRepo.Verify(repo => repo.ObterPorIdAsync(tarefaId), Times.Once);
        _mockRepo.Verify(repo => repo.AtualizarAsync(It.IsAny<Tarefa>()), Times.Never);
    }

    [Fact]
    public async Task ObterTodasAsync_DeveChamarRepositorio()
    {
        // Arrange
        var tarefas = new List<Tarefa>
        {
            new() { Id = 1, Titulo = "Tarefa 1", Descricao = "Desc 1", Concluida = false },
            new() { Id = 2, Titulo = "Tarefa 2", Descricao = "Desc 2", Concluida = true }
        };
        var pagina = 1;
        var tamanhoPagina = 10;

        _mockRepo.Setup(repo => repo.ObterTodasAsync(pagina, tamanhoPagina))
                 .ReturnsAsync(tarefas);

        // Act
        var result = await _service.ObterTodasAsync(pagina, tamanhoPagina);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockRepo.Verify(repo => repo.ObterTodasAsync(pagina, tamanhoPagina), Times.Once);
    }

    [Fact]
    public async Task DeletarAsync_DeveChamarRepositorio()
    {
        // Arrange
        var tarefaId = 1;
        _mockRepo.Setup(repo => repo.DeletarAsync(tarefaId)).Returns(Task.CompletedTask);

        // Act
        await _service.DeletarAsync(tarefaId);

        // Assert
        _mockRepo.Verify(repo => repo.DeletarAsync(tarefaId), Times.Once);
    }

    [Theory]
    [InlineData("", "Descrição válida")] // Título vazio
    [InlineData("   ", "Descrição válida")] // Título só com espaços
    [InlineData("Título válido", "")] // Descrição vazia
    [InlineData("Título válido", "   ")] // Descrição só com espaços
    public async Task AdicionarAsync_ComDadosInvalidos_DeveProcessarCorretamente(string titulo, string descricao)
    {
        // Arrange
        var criarTarefaDTO = new CriarTarefaDTO(titulo, descricao);
        var tarefa = new Tarefa { Id = 1, Titulo = titulo, Descricao = descricao, Concluida = false };
        _mockRepo.Setup(repo => repo.AdicionarAsync(It.IsAny<Tarefa>())).ReturnsAsync(tarefa);

        // Act
        var result = await _service.AdicionarAsync(criarTarefaDTO);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(titulo, result.Titulo);
        Assert.Equal(descricao, result.Descricao);
        _mockRepo.Verify(repo => repo.AdicionarAsync(It.IsAny<Tarefa>()), Times.Once);
    }

    [Theory]
    [InlineData(1, 1)] // Primeira página, mínimo tamanho
    [InlineData(1, 100)] // Primeira página, máximo tamanho
    [InlineData(100, 50)] // Página alta, tamanho médio
    [InlineData(int.MaxValue, 1)] // Página máxima, tamanho mínimo
    public async Task ObterTodasAsync_ComPaginacaoExtrema_DeveProcessar(int pagina, int tamanhoPagina)
    {
        // Arrange
        var tarefasVazias = new List<Tarefa>();
        _mockRepo.Setup(repo => repo.ObterTodasAsync(pagina, tamanhoPagina))
                 .ReturnsAsync(tarefasVazias);

        // Act
        var result = await _service.ObterTodasAsync(pagina, tamanhoPagina);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockRepo.Verify(repo => repo.ObterTodasAsync(pagina, tamanhoPagina), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public async Task ObterPorIdAsync_ComIdNegativoOuZero_DeveRetornarNull(int id)
    {
        // Arrange
        _mockRepo.Setup(repo => repo.ObterPorIdAsync(id)).ReturnsAsync((Tarefa?)null);

        // Act
        var result = await _service.ObterPorIdAsync(id);

        // Assert
        Assert.Null(result);
        _mockRepo.Verify(repo => repo.ObterPorIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task AdicionarAsync_ComTituloMuitoLongo_DeveProcessar()
    {
        // Arrange - Título com 1000 caracteres
        var tituloLongo = new string('A', 1000);
        var descricaoLonga = new string('B', 2000);
        var criarTarefaDTO = new CriarTarefaDTO(tituloLongo, descricaoLonga);
        var tarefa = new Tarefa { Id = 1, Titulo = tituloLongo, Descricao = descricaoLonga, Concluida = false };
        
        _mockRepo.Setup(repo => repo.AdicionarAsync(It.IsAny<Tarefa>())).ReturnsAsync(tarefa);

        // Act
        var result = await _service.AdicionarAsync(criarTarefaDTO);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1000, result.Titulo.Length);
        Assert.Equal(2000, result.Descricao.Length);
        _mockRepo.Verify(repo => repo.AdicionarAsync(It.IsAny<Tarefa>()), Times.Once);
    }

    [Fact]
    public async Task AdicionarAsync_ComCaracteresEspeciais_DeveProcessar()
    {
        // Arrange - Caracteres especiais e Unicode
        var tituloEspecial = "Título com émojis 🚀💻 e acentos áéíóú çñ";
        var descricaoEspecial = "Descrição com símbolos @#$%^&*()_+{}[]|\\:;\"'<>,.?/~`";
        var criarTarefaDTO = new CriarTarefaDTO(tituloEspecial, descricaoEspecial);
        var tarefa = new Tarefa { Id = 1, Titulo = tituloEspecial, Descricao = descricaoEspecial, Concluida = false };
        
        _mockRepo.Setup(repo => repo.AdicionarAsync(It.IsAny<Tarefa>())).ReturnsAsync(tarefa);

        // Act
        var result = await _service.AdicionarAsync(criarTarefaDTO);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tituloEspecial, result.Titulo);
        Assert.Equal(descricaoEspecial, result.Descricao);
        Assert.False(result.Concluida);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public async Task DeletarAsync_ComIdInvalido_DeveProcessarSemErro(int id)
    {
        // Arrange
        _mockRepo.Setup(repo => repo.DeletarAsync(id)).Returns(Task.CompletedTask);

        // Act & Assert - Não deve lançar exceção
        await _service.DeletarAsync(id);
        
        _mockRepo.Verify(repo => repo.DeletarAsync(id), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_ComTituloAlteradoParaVazio_DeveProcessar()
    {
        // Arrange
        var tarefaId = 1;
        var tarefaExistente = new Tarefa { Id = tarefaId, Titulo = "Título Original", Descricao = "Descrição", Concluida = false };
        var atualizarDTO = new AtualizarTarefaDTO("", true); // Título vazio
        var tarefaAtualizada = new Tarefa { Id = tarefaId, Titulo = "", Descricao = "Descrição", Concluida = true };

        _mockRepo.Setup(repo => repo.ObterPorIdAsync(tarefaId)).ReturnsAsync(tarefaExistente);
        _mockRepo.Setup(repo => repo.AtualizarAsync(It.IsAny<Tarefa>())).ReturnsAsync(tarefaAtualizada);

        // Act
        var result = await _service.AtualizarAsync(tarefaId, atualizarDTO);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("", result.Titulo);
        Assert.True(result.Concluida);
    }

    [Fact]
    public async Task ObterTodasAsync_ComListaGrande_DeveRetornarCorretamente()
    {
        // Arrange - Simular uma lista com 10.000 tarefas
        var tarefasGrandes = Enumerable.Range(1, 10000)
            .Select(i => new Tarefa 
            { 
                Id = i, 
                Titulo = $"Tarefa {i}", 
                Descricao = $"Descrição da tarefa {i}", 
                Concluida = i % 2 == 0 
            })
            .Take(100) // Simular paginação de 100 itens
            .ToList();

        _mockRepo.Setup(repo => repo.ObterTodasAsync(1, 100))
                 .ReturnsAsync(tarefasGrandes);

        // Act
        var result = await _service.ObterTodasAsync(1, 100);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100, result.Count());
        Assert.Equal("Tarefa 1", result.First().Titulo);
        Assert.Equal("Tarefa 100", result.Last().Titulo);
    }

    [Fact] 
    public async Task AtualizarAsync_ComMudancaCompleta_DeveAtualizarTodosOsCampos()
    {
        // Arrange
        var tarefaId = 42;
        var tarefaOriginal = new Tarefa 
        { 
            Id = tarefaId, 
            Titulo = "Título Antigo", 
            Descricao = "Descrição Antiga", 
            Concluida = false 
        };
        
        var novoTitulo = "Título Completamente Novo com Acentuação éáü";
        var atualizarDTO = new AtualizarTarefaDTO(novoTitulo, true);
        
        var tarefaAtualizada = new Tarefa 
        { 
            Id = tarefaId, 
            Titulo = novoTitulo, 
            Descricao = "Descrição Antiga", // Descrição não muda no DTO atual
            Concluida = true 
        };

        _mockRepo.Setup(repo => repo.ObterPorIdAsync(tarefaId)).ReturnsAsync(tarefaOriginal);
        _mockRepo.Setup(repo => repo.AtualizarAsync(It.IsAny<Tarefa>())).ReturnsAsync(tarefaAtualizada);

        // Act
        var result = await _service.AtualizarAsync(tarefaId, atualizarDTO);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(42, result.Id);
        Assert.Equal(novoTitulo, result.Titulo);
        Assert.True(result.Concluida);
        
        // Verificar que o repositório foi chamado corretamente
        _mockRepo.Verify(repo => repo.ObterPorIdAsync(tarefaId), Times.Once);
        _mockRepo.Verify(repo => repo.AtualizarAsync(It.Is<Tarefa>(t => 
            t.Id == tarefaId && 
            t.Titulo == novoTitulo && 
            t.Concluida == true
        )), Times.Once);
    }
}