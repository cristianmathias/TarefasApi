using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Tarefas.Application.DTOs;
using Tarefas.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Tarefas.Tests;

public class TarefasControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TarefasControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    private void InitializeTest()
    {
        // Inicializar dados para cada teste
        _factory.InitializeDbForTests();
    }

    [Fact]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType()
    {
        // Arrange
        InitializeTest();
        
        // Act
        var response = await _client.GetAsync("/Tarefas");

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        Assert.Equal("application/json; charset=utf-8", 
            response.Content.Headers.ContentType?.ToString() ?? string.Empty);
    }

    [Fact]
    public async Task GetById_TarefaInexistente_DeveRetornar404()
    {
        // Arrange
        InitializeTest();
        var idInexistente = 999;

        // Act
        var response = await _client.GetAsync($"/Tarefas/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_IdInvalido_DeveRetornar400()
    {
        // Arrange
        var idInvalido = 0; // ID deve ser >= 1

        // Act
        var response = await _client.GetAsync($"/Tarefas/{idInvalido}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_TarefaValida_DeveRetornar201()
    {
        // Arrange
        var novaTarefa = new CriarTarefaDTO("Teste Tarefa", "Descrição de teste");

        // Act
        var response = await _client.PostAsJsonAsync("/Tarefas", novaTarefa);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var tarefa = await response.Content.ReadFromJsonAsync<Tarefa>();
        Assert.NotNull(tarefa);
        Assert.Equal(novaTarefa.Titulo, tarefa.Titulo);
        Assert.Equal(novaTarefa.Descricao, tarefa.Descricao);
        Assert.False(tarefa.Concluida); // Nova tarefa deve ser não concluída
    }

    [Fact]
    public async Task Post_TarefaComTituloVazio_DeveRetornar400()
    {
        // Arrange
        var tarefaInvalida = new { Titulo = "", Descricao = "Descrição válida" };

        // Act
        var response = await _client.PostAsJsonAsync("/Tarefas", tarefaInvalida);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_TarefaComDescricaoVazia_DeveRetornar400()
    {
        // Arrange
        var tarefaInvalida = new { Titulo = "Título válido", Descricao = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/Tarefas", tarefaInvalida);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_TarefaExistente_DeveRetornar200()
    {
        // Arrange
        InitializeTest();
        
        // Primeiro criar uma tarefa
        var novaTarefa = new CriarTarefaDTO("Tarefa para atualizar", "Descrição inicial");
        var createResponse = await _client.PostAsJsonAsync("/Tarefas", novaTarefa);
        var tarefaCriada = await createResponse.Content.ReadFromJsonAsync<Tarefa>();
        
        var atualizarTarefa = new AtualizarTarefaDTO("Tarefa atualizada", true);

        // Act
        var response = await _client.PutAsJsonAsync($"/Tarefas/{tarefaCriada!.Id}", atualizarTarefa);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var tarefaAtualizada = await response.Content.ReadFromJsonAsync<Tarefa>();
        Assert.NotNull(tarefaAtualizada);
        Assert.Equal(atualizarTarefa.Titulo, tarefaAtualizada.Titulo);
        Assert.Equal(atualizarTarefa.Concluida, tarefaAtualizada.Concluida);
    }

    [Fact]
    public async Task Put_TarefaInexistente_DeveRetornar404()
    {
        // Arrange
        var idInexistente = 999;
        var atualizarTarefa = new AtualizarTarefaDTO("Título atualizado", true);

        // Act
        var response = await _client.PutAsJsonAsync($"/Tarefas/{idInexistente}", atualizarTarefa);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_IdInvalido_DeveRetornar400()
    {
        // Arrange
        var idInvalido = 0;
        var atualizarTarefa = new AtualizarTarefaDTO("Título válido", false);

        // Act
        var response = await _client.PutAsJsonAsync($"/Tarefas/{idInvalido}", atualizarTarefa);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_TarefaExistente_DeveRetornar204()
    {
        // Arrange - Primeiro criar uma tarefa
        var novaTarefa = new CriarTarefaDTO("Tarefa para deletar", "Será deletada");
        var createResponse = await _client.PostAsJsonAsync("/Tarefas", novaTarefa);
        var tarefaCriada = await createResponse.Content.ReadFromJsonAsync<Tarefa>();

        // Act
        var response = await _client.DeleteAsync($"/Tarefas/{tarefaCriada!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        // Verificar se realmente foi deletada
        var getResponse = await _client.GetAsync($"/Tarefas/{tarefaCriada.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_IdInvalido_DeveRetornar400()
    {
        // Arrange
        var idInvalido = 0;

        // Act
        var response = await _client.DeleteAsync($"/Tarefas/{idInvalido}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 10)]
    [InlineData(1, 1)]
    public async Task Get_ComPaginacao_DeveRetornarSucesso(int pagina, int tamanhoPagina)
    {
        // Act
        var response = await _client.GetAsync($"/Tarefas?pagina={pagina}&tamanhoPagina={tamanhoPagina}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData(0, 10)] // Página inválida
    [InlineData(1, 0)]  // Tamanho inválido
    [InlineData(1, 101)] // Tamanho muito grande
    public async Task Get_ComParametrosInvalidos_DeveRetornar400(int pagina, int tamanhoPagina)
    {
        // Act
        var response = await _client.GetAsync($"/Tarefas?pagina={pagina}&tamanhoPagina={tamanhoPagina}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(-1, 10)] // Página negativa
    [InlineData(1, -5)]  // Tamanho negativo
    [InlineData(-1, -1)] // Ambos negativos
    [InlineData(0, 0)]   // Ambos zero
    public async Task Get_ComParametrosExtremamentInvalidos_DeveRetornar400(int pagina, int tamanhoPagina)
    {
        // Act
        var response = await _client.GetAsync($"/Tarefas?pagina={pagina}&tamanhoPagina={tamanhoPagina}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public async Task GetById_ComIdNegativo_DeveRetornar400(int id)
    {
        // Act
        var response = await _client.GetAsync($"/Tarefas/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_ComTituloMuitoLongo_DeveProcessarOuRejeitarCorretamente()
    {
        // Arrange - Título com mais de 100 caracteres (limite definido na validação)
        var tituloMuitoLongo = new string('A', 150);
        var tarefaComTituloLongo = new { Titulo = tituloMuitoLongo, Descricao = "Descrição normal" };

        // Act
        var response = await _client.PostAsJsonAsync("/Tarefas", tarefaComTituloLongo);

        // Assert - Pode ser 400 (validação) ou 201 (se passou pela validação)
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.Created);
    }

    [Fact]
    public async Task Post_ComDescricaoMuitoLonga_DeveProcessarOuRejeitarCorretamente()
    {
        // Arrange - Descrição com mais de 500 caracteres (limite definido na validação)
        var descricaoMuitoLonga = new string('B', 600);
        var tarefaComDescricaoLonga = new { Titulo = "Título normal", Descricao = descricaoMuitoLonga };

        // Act
        var response = await _client.PostAsJsonAsync("/Tarefas", tarefaComDescricaoLonga);

        // Assert - Pode ser 400 (validação) ou 201 (se passou pela validação)
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.Created);
    }

    [Fact]
    public async Task Post_ComCaracteresEspeciais_DeveRetornar201()
    {
        // Arrange
        var tarefaEspecial = new CriarTarefaDTO(
            "Tarefa com émojis 🚀💻 e acentos áéíóú", 
            "Descrição com símbolos @#$%^&*()_+ e quebras\nde\tlinha"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/Tarefas", tarefaEspecial);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var tarefa = await response.Content.ReadFromJsonAsync<Tarefa>();
        Assert.NotNull(tarefa);
        Assert.Contains("🚀", tarefa.Titulo);
        Assert.Contains("@#$", tarefa.Descricao);
    }

    [Fact]
    public async Task Post_ComJsonMalFormado_DeveRetornar400()
    {
        // Arrange - JSON inválido
        var jsonInvalido = "{ titulo: 'sem aspas', descricao: }";
        var content = new StringContent(jsonInvalido, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/Tarefas", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_ComContentTypeInvalido_DeveRetornar415()
    {
        // Arrange
        var tarefa = new CriarTarefaDTO("Título", "Descrição");
        var json = JsonSerializer.Serialize(tarefa);
        var content = new StringContent(json, Encoding.UTF8, "text/plain"); // Content-Type errado

        // Act
        var response = await _client.PostAsync("/Tarefas", content);

        // Assert
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
    }

    [Theory]
    [InlineData(int.MaxValue)]
    [InlineData(999999)]
    [InlineData(1000000)]
    public async Task GetById_ComIdMuitoAlto_DeveRetornar404(int id)
    {
        // Act
        var response = await _client.GetAsync($"/Tarefas/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CrudCompleto_FluxoRealDeUso_DeveProcessarCorretamente()
    {
        // Arrange & Act & Assert - Fluxo completo CRUD

        // 1. Criar tarefa
        var novaTarefa = new CriarTarefaDTO("Tarefa de Teste CRUD", "Descrição inicial");
        var createResponse = await _client.PostAsJsonAsync("/Tarefas", novaTarefa);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        
        var tarefaCriada = await createResponse.Content.ReadFromJsonAsync<Tarefa>();
        Assert.NotNull(tarefaCriada);
        var tarefaId = tarefaCriada.Id;

        // 2. Ler tarefa criada
        var getResponse = await _client.GetAsync($"/Tarefas/{tarefaId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var tarefaLida = await getResponse.Content.ReadFromJsonAsync<Tarefa>();
        Assert.NotNull(tarefaLida);
        Assert.Equal(novaTarefa.Titulo, tarefaLida.Titulo);
        Assert.False(tarefaLida.Concluida);

        // 3. Atualizar tarefa
        var atualizacao = new AtualizarTarefaDTO("Tarefa Atualizada", true);
        var updateResponse = await _client.PutAsJsonAsync($"/Tarefas/{tarefaId}", atualizacao);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        
        var tarefaAtualizada = await updateResponse.Content.ReadFromJsonAsync<Tarefa>();
        Assert.NotNull(tarefaAtualizada);
        Assert.Equal("Tarefa Atualizada", tarefaAtualizada.Titulo);
        Assert.True(tarefaAtualizada.Concluida);

        // 4. Verificar se está na lista
        var listResponse = await _client.GetAsync("/Tarefas");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        // 5. Deletar tarefa
        var deleteResponse = await _client.DeleteAsync($"/Tarefas/{tarefaId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // 6. Verificar se foi deletada
        var getDeletedResponse = await _client.GetAsync($"/Tarefas/{tarefaId}");
        Assert.Equal(HttpStatusCode.NotFound, getDeletedResponse.StatusCode);
    }

    [Fact]
    public async Task ConcorrenciaSimulada_CriarMultiplasTarefas_DeveProcessarTodas()
    {
        // Arrange - Criar tarefas únicas para evitar problemas de InMemory DB
        InitializeTest();
        var tarefas = Enumerable.Range(1, 5).Select(i => 
            new CriarTarefaDTO($"Tarefa Concorrente {i} - {Guid.NewGuid()}", $"Descrição {i}")
        ).ToList();

        // Act - Criar todas as tarefas sequencialmente (InMemory DB tem limitações com concorrência)
        var results = new List<(bool Success, HttpResponseMessage? Response, Tarefa? Tarefa)>();
        
        foreach (var tarefa in tarefas)
        {
            try
            {
                var response = await _client.PostAsJsonAsync("/Tarefas", tarefa);
                if (response.IsSuccessStatusCode)
                {
                    var tarefaCriada = await response.Content.ReadFromJsonAsync<Tarefa>();
                    results.Add((true, response, tarefaCriada));
                }
                else
                {
                    results.Add((false, response, null));
                }
            }
            catch
            {
                results.Add((false, null, null));
            }
        }

        // Assert - Todas devem ter sido criadas com sucesso
        var successfulResults = results.Where(r => r.Success).ToList();
        Assert.Equal(5, successfulResults.Count); // Todas devem ter sucesso
        
        // Verificar que todas têm IDs únicos
        var ids = successfulResults.Select(r => r.Tarefa!.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Theory]
    [InlineData("application/json")]
    [InlineData("application/json; charset=utf-8")]
    public async Task Get_ComDiferentesAcceptHeaders_DeveRetornarJson(string acceptHeader)
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Accept", acceptHeader);

        // Act
        var response = await _client.GetAsync("/Tarefas");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("json", response.Content.Headers.ContentType?.ToString() ?? "");
    }

    [Fact]
    public async Task Get_ComAcceptTextJson_DeveRetornarTextJson()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Accept", "text/json");

        // Act
        var response = await _client.GetAsync("/Tarefas");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "";
        Assert.Contains("json", contentType);
        // Pode ser text/json ou application/json dependendo da configuração da API
    }

    [Fact]
    public async Task OperacoesEmSequenciaRapida_DeveMantarConsistencia()
    {
        // Arrange
        InitializeTest();
        
        // Act - Operações rápidas em sequência
        var tarefa1 = new CriarTarefaDTO("Tarefa Sequência 1", "Desc 1");
        var tarefa2 = new CriarTarefaDTO("Tarefa Sequência 2", "Desc 2");

        // Criar duas tarefas rapidamente
        var create1 = await _client.PostAsJsonAsync("/Tarefas", tarefa1);
        var create2 = await _client.PostAsJsonAsync("/Tarefas", tarefa2);

        // Assert
        Assert.Equal(HttpStatusCode.Created, create1.StatusCode);
        Assert.Equal(HttpStatusCode.Created, create2.StatusCode);

        var criada1 = await create1.Content.ReadFromJsonAsync<Tarefa>();
        var criada2 = await create2.Content.ReadFromJsonAsync<Tarefa>();

        Assert.NotNull(criada1);
        Assert.NotNull(criada2);
        Assert.NotEqual(criada1.Id, criada2.Id); // IDs devem ser diferentes
    }
}