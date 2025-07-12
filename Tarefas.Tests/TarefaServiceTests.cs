using Moq;
using Tarefas.Domain.Interfaces;
using Tarefas.Application.Services;
using Tarefas.Domain.Entities;
using Tarefas.Application.DTOs;

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
        _mockRepo.Verify(repo => repo.AdicionarAsync(It.IsAny<Tarefa>()), Times.Once);
    }
}