# Tarefas.Api

API de demonstração construída com .NET 9 para gerenciar uma lista de tarefas. Este projeto segue os princípios da Arquitetura Limpa, separando as preocupações em camadas de Domínio, Aplicação, e Infraestrutura.

## Funcionalidades

- Criar, ler, atualizar e deletar tarefas.
- Paginação na listagem de tarefas.
- Persistência de dados em memória.

## Estrutura do Projeto

- **Tarefas.Domain**: Contém as entidades de negócio e as interfaces dos repositórios.
- **Tarefas.Application**: Contém a lógica de negócio, DTOs, e as interfaces dos serviços.
- **Tarefas.Infrastructure**: Contém as implementações concretas dos repositórios e outros serviços de infraestrutura.
- **Tarefas.Api**: A camada de apresentação, contendo os controladores da API.

## Como Executar

1. **Clone o repositório:**
   ```bash
   git clone git@github.com:cristianmathias/TarefasApi.git
   cd TarefasApi
   ```

2. **Restaure as dependências:**
   ```bash
   dotnet restore
   ```

3. **Execute o projeto:**
   ```bash
   dotnet run --project Tarefas.Api/Tarefas.Api.csproj
   ```

A API estará disponível em `http://localhost:5230`.

Para explorar e testar os endpoints da API de forma interativa, acesse a documentação Swagger em:

[http://localhost:5230/swagger](http://localhost:5230/swagger)

## Como Executar os Testes

Para executar os testes do projeto, utilize o seguinte comando:

```bash
   dotnet test Tarefas.Tests/Tarefas.Tests.csproj
   ```

## Endpoints da API

As requisições para os endpoints estão documentadas no arquivo `Tarefas.Api.http`. Você pode usar a extensão REST Client do VS Code para executá-las.

- `GET /Tarefas`: Retorna uma lista paginada de tarefas.
- `GET /Tarefas/{id}`: Retorna uma tarefa específica.
- `POST /Tarefas`: Adiciona uma nova tarefa.
- `PUT /Tarefas/{id}`: Atualiza uma tarefa existente.
- `DELETE /Tarefas/{id}`: Deleta uma tarefa.