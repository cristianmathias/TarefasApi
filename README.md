# DotnetApiDemo

API de demonstração construída com .NET 9 para gerenciar uma lista de tarefas. Este projeto segue os princípios da Arquitetura Limpa, separando as preocupações em camadas de Domínio, Aplicação, e Infraestrutura.

## Funcionalidades

- Criar, ler, atualizar e deletar tarefas.
- Paginação na listagem de tarefas.
- Persistência de dados em memória.

## Estrutura do Projeto

- **DotnetApiDemo.Domain**: Contém as entidades de negócio e as interfaces dos repositórios.
- **DotnetApiDemo.Application**: Contém a lógica de negócio, DTOs, e as interfaces dos serviços.
- **DotnetApiDemo.Infrastructure**: Contém as implementações concretas dos repositórios e outros serviços de infraestrutura.
- **DotnetApiDemo**: A camada de apresentação, contendo os controladores da API.

## Como Executar

1. **Clone o repositório:**
   ```bash
   git clone https://github.com/seu-usuario/DotnetApiDemo.git
   cd DotnetApiDemo
   ```

2. **Restaure as dependências:**
   ```bash
   dotnet restore
   ```

3. **Execute o projeto:**
   ```bash
   dotnet run --project DotnetApiDemo/DotnetApiDemo.csproj
   ```

A API estará disponível em `http://localhost:5230`.

Para explorar e testar os endpoints da API de forma interativa, acesse a documentação Swagger em:

[http://localhost:5230/swagger](http://localhost:5230/swagger)

## Como Executar os Testes

Para executar os testes do projeto, utilize o seguinte comando:

```bash
   dotnet test DotnetApiDemo.Tests/DotnetApiDemo.Tests.csproj
   ```

## Endpoints da API

As requisições para os endpoints estão documentadas no arquivo `DotnetApiDemo.http`. Você pode usar a extensão REST Client do VS Code para executá-las.

- `GET /Tarefas`: Retorna uma lista paginada de tarefas.
- `GET /Tarefas/{id}`: Retorna uma tarefa específica.
- `POST /Tarefas`: Adiciona uma nova tarefa.
- `PUT /Tarefas/{id}`: Atualiza uma tarefa existente.
- `DELETE /Tarefas/{id}`: Deleta uma tarefa.
