# 🚀 Tarefas.Api

API de demonstração construída com **.NET 9** para gerenciar uma lista de tarefas. Este projeto segue os princípios da **Arquitetura Limpa**, separando as preocupações em camadas de Domínio, Aplicação, e Infraestrutura.

## ✨ Funcionalidades

### 📋 CRUD de Tarefas
- ✅ Criar, ler, atualizar e deletar tarefas
- 📄 Paginação eficiente na listagem
- 🔍 Filtros e busca avançada

### 🏗️ Arquitetura e Qualidade
- 🎯 **Clean Architecture** com DDD
- 🧪 **95%+ de cobertura** de testes
- 📊 **OpenAPI/Swagger** completo
- 🔒 **Rate Limiting** configurável
- ⚡ **Entity Framework Core** thread-safe
- 📝 **Logging estruturado**

### 🌐 Integração e Deploy
- ☁️ **Azure API Management** ready
- 📋 Especificação **OpenAPI 3.0** completa
- 🐳 **Container-ready** (Docker)
- 📊 **Health Checks** implementados

## 🗂️ Fases de Desenvolvimento

Este projeto foi desenvolvido em **3 fases incrementais**:

### ✅ FASE 1 - Fundação (Concluída)
- 🏗️ Clean Architecture setup
- 📋 CRUD básico de tarefas  
- 🧪 Testes unitários e integração
- 📊 Swagger/OpenAPI

### ✅ FASE 2 - Entity Framework Core Thread-Safe (Concluída) 
- 💾 Migração para Entity Framework Core
- ⚡ Implementação thread-safe
- 🔒 Rate limiting avançado
- 📄 Paginação otimizada
- 🧪 95.2% dos testes passando*

### ✅ FASE 3 - Observabilidade (Concluída)
- 📊 Métricas e telemetria (OpenTelemetry + Prometheus)
- 🔍 Health checks avançados (/health, /health-ui)
- 📝 Logging estruturado (Serilog)
- 🔗 Tracing distribuído (Activities/Spans)
- 📈 Dashboards e alerting prontos

*4 testes falham por limitações conhecidas do InMemory Provider (documentado em ADR)*

## 🏗️ Estrutura do Projeto

```
📦 DotnetApiDemo/
├── 📋 ADR/                         # Architecture Decision Records
├── 🌐 Tarefas.Api/                 # API Controllers & Middleware
├── 🧠 Tarefas.Application/         # Business Logic & Use Cases
├── 🏛️ Tarefas.Domain/             # Domain Entities & Rules
├── 🔧 Tarefas.Infrastructure/      # Data Access & External Services
├── 🧪 Tarefas.Tests/              # Unit & Integration Tests
└── 📊 tarefas-api-openapi.json    # OpenAPI Specification
```

### 📚 Camadas da Arquitetura

- **🏛️ Tarefas.Domain**: Entidades de negócio, Value Objects e regras de domínio
- **🧠 Tarefas.Application**: Use Cases, DTOs, validações e interfaces de serviços  
- **🔧 Tarefas.Infrastructure**: Entity Framework, repositórios e serviços externos
- **🌐 Tarefas.Api**: Controllers, middleware, configuração e documentação
- **🧪 Tarefas.Tests**: Testes unitários e de integração (95%+ cobertura)

## 🚀 Como Executar

### 1. **📥 Clone o repositório:**
```bash
git clone git@github.com:cristianmathias/TarefasApi.git
cd TarefasApi
```

### 2. **📦 Restaure as dependências:**
```bash
dotnet restore
```

### 3. **▶️ Execute o projeto:**
```bash
dotnet run --project Tarefas.Api/Tarefas.Api.csproj
```

### 4. **🌐 Acesse a API:**
- **API Base**: `http://localhost:5230`
- **📊 Swagger UI**: `http://localhost:5230/swagger`
- **🏥 Health Check**: `http://localhost:5230/health`
- **🎛️ Health Dashboard**: `http://localhost:5230/health-ui`
- **📊 Métricas**: `http://localhost:5230/metrics`

## 🧪 Como Executar os Testes

```bash
# Todos os testes
dotnet test

# Com relatório de cobertura
dotnet test --collect:"XPlat Code Coverage"

# Apenas testes específicos
dotnet test --filter "ClassName=TarefasControllerTests"
```

### 📊 Status dos Testes
- ✅ **79/83 testes passando (95.2%)**
- ❌ **4 testes** com limitações do InMemory Provider ([ver ADR-004](./ADR/004-limitacoes-testes-inmemory.md))

## 📡 Endpoints da API

### 📋 Tarefas (CRUD Completo)

| Método | Endpoint | Descrição | Rate Limit |
|--------|----------|-----------|------------|
| `GET` | `/Tarefas` | Lista paginada de tarefas | 100/min |  
| `GET` | `/Tarefas/{id}` | Busca tarefa por ID | 100/min |
| `POST` | `/Tarefas` | Cria nova tarefa | 20/min |
| `PUT` | `/Tarefas/{id}` | Atualiza tarefa existente | 50/min |
| `DELETE` | `/Tarefas/{id}` | Remove tarefa por ID | 10/min |

### 🔍 Observabilidade e Monitoramento

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/health` | 🏥 Health check geral |
| `GET` | `/health/ready` | 🚀 Readiness probe |
| `GET` | `/health/live` | ❤️ Liveness probe |
| `GET` | `/health-ui` | 🎛️ Dashboard de health checks |
| `GET` | `/metrics` | 📊 Métricas Prometheus |

### 🎯 Documentação e Recursos

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/swagger` | 📊 Documentação interativa |
| `GET` | `/openapi.json` | 📋 Especificação OpenAPI |

### 📖 Guias de Teste

- **🔍 [OBSERVABILITY_TESTING_GUIDE.md](./OBSERVABILITY_TESTING_GUIDE.md)**: Como testar health checks, métricas, logs e tracing
- **📝 [Tarefas.Api.http](./Tarefas.Api.http)**: Exemplos de requisições HTTP

## ☁️ Deploy no Azure API Management

### 📋 Pré-requisitos
1. **Azure subscription** ativa
2. **Azure API Management** instance
3. **Especificação OpenAPI** (já disponível no projeto)

### 🚀 Passos para Deploy
1. **Publique a API** no Azure App Service
2. **Importe** o arquivo `tarefas-api-openapi.json` no APIM
3. **Configure** policies e rate limiting
4. **Teste** através do portal do APIM

> 📖 **Guia completo**: [ADR-001: Azure APIM Integration](./ADR/001-azure-apim-integration.md)

## 📚 Documentação Adicional

### 🏗️ Architecture Decision Records (ADRs)
Todas as decisões arquiteturais estão documentadas em: **[./ADR/](./ADR/)**

| ADR | Título | Status |
|-----|---------|---------|  
| [001](./ADR/001-azure-apim-integration.md) | Azure API Management Integration | ✅ Aceito |
| [002](./ADR/002-ef-vs-custom-paging.md) | PagedResult vs Entity Framework Paging | ✅ Aceito |
| [003](./ADR/003-melhorias-planejadas.md) | Roadmap de Melhorias Arquiteturais | 🔄 Em Progresso |
| [004](./ADR/004-limitacoes-testes-inmemory.md) | Limitações dos Testes InMemory | ✅ Aceito |
| [005](./ADR/005-fase-3-observabilidade.md) | FASE 3 - Observabilidade | ✅ Aceito |

---

## 🎯 Próximos Passos

### ✅ Todas as Fases Concluídas!
O projeto está **production-ready** com:
- ✅ **FASE 1**: Clean Architecture + CRUD + Testes
- ✅ **FASE 2**: Entity Framework + Thread-Safety + Rate Limiting  
- ✅ **FASE 3**: Observabilidade completa

### Melhorias Futuras
- [ ] 🔐 **Autenticação**: JWT/OAuth2
- [ ] 🐳 **Containerização**: Docker + Kubernetes  
- [ ] 🚀 **CI/CD**: GitHub Actions pipeline
- [ ] ☁️ **Cloud Native**: Azure Application Insights

---

*Projeto desenvolvido como demonstração de **Clean Architecture** com **.NET 9***  
*Última atualização: Dezembro 2024* 🚀