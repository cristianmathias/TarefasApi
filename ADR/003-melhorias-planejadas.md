# ADR-003: Roadmap de Melhorias Arquiteturais

## Status
- **Data**: 2024-12-29
- **Status**: 🔄 Em Progresso (FASE 1 Concluída)
- **Decisores**: Cristian Mathias  
- **Tags**: roadmap, security, persistence, observability, architecture

## Contexto
A API de Tarefas foi desenvolvida inicialmente com funcionalidades básicas seguindo Clean Architecture. Para torná-la enterprise-ready e production-ready, identificamos a necessidade de melhorias em 4 áreas críticas:

1. **Segurança e Robustez** - Middleware, CORS, rate limiting
2. **Persistência Thread-Safe** - Entity Framework, transactions
3. **Observabilidade** - Logging estruturado, health checks, métricas
4. **Funcionalidades de Negócio** - Filtros, busca, soft delete

## Decisão
**Implementar melhorias em 4 fases incrementais**, mantendo compatibilidade total e seguindo princípios de:
- ✅ Zero breaking changes
- ✅ Cobertura de testes mantida (100%)
- ✅ Performance igual ou melhor
- ✅ Documentação atualizada

## Fases do Roadmap

### 🛡️ **FASE 1 - Segurança e Robustez** ✅ CONCLUÍDA
**Status**: ✅ Implementada e commitada (commit: 726559f)

#### Componentes Implementados:
- ✅ **ExceptionMiddleware** - Tratamento global com tipos específicos de erro
- ✅ **CorrelationIdMiddleware** - Rastreamento de requisições  
- ✅ **RateLimitingMiddleware** - Limitação configurável por ambiente
- ✅ **Security Headers** - Proteção contra ataques comuns (XSS, clickjacking)
- ✅ **CORS Configurável** - Políticas por ambiente (dev/prod)
- ✅ **Validation Pipeline** - Model validation automático com responses padronizados
- ✅ **Service Extensions** - Organização modular da configuração
- ✅ **Background Services** - Limpeza automática de cache
- ✅ **Swagger Enhancements** - Documentação com correlation ID

#### Benefícios Alcançados:
- 🔒 Rate limiting: 100 req/min (prod), 1000 req/min (dev)
- 📊 Correlation IDs em 100% das requisições
- 🛡️ Headers de segurança automáticos
- ⚡ Middleware pipeline otimizado
- 🧪 83/83 testes passando (100% compatibilidade)

### 💾 **FASE 2 - Persistência Thread-Safe** 🔄 Planejada
- [ ] **Entity Framework Core**
  - SQLite para desenvolvimento
  - Migrations automáticas  
  - Connection string configurável
  
- [ ] **Repositório Thread-Safe** 
  - Implementação EF Core
  - Async/await real (não Task.FromResult)
  - Transaction scope
  
- [ ] **Paginação Robusta**
  - PagedResult<T> wrapper (já criado)
  - Total de registros em query única
  - Metadata de paginação (HasNext, HasPrevious)

### 📊 **FASE 3 - Observabilidade e Monitoring** 🔄 Planejada
- [ ] **Logging Estruturado (Serilog)**
  - Logs em JSON estruturado
  - Diferentes níveis por ambiente
  - Correlation IDs para rastreamento
  
- [ ] **Health Checks**
  - /health endpoint
  - Verificação database/dependencies
  - Status detalhado dos componentes
  
- [ ] **Métricas Básicas**
  - Request count/duration  
  - Response status codes
  - Custom metrics de negócio

### 🔄 **FASE 4 - Funcionalidades de Negócio** 🔄 Planejada
- [ ] **Filtros e Busca**
  - Filtro por status (concluída/pendente)
  - Busca por título/descrição
  - Filtros por data de criação
  
- [ ] **Ordenação**
  - Por data, título, status
  - Ascending/Descending
  - Query parameters padronizados
  
- [ ] **Soft Delete**
  - Flag IsDeleted na entidade
  - Filtro automático nos queries
  - Endpoint para restaurar

## Consequências

### Positivas
- ✅ **Approach Incremental**: Implementação por fases reduz risco
- ✅ **Zero Downtime**: Compatibilidade total mantida
- ✅ **Enterprise Ready**: Features de produção implementadas
- ✅ **Developer Experience**: Logging, debugging e monitoring melhorados
- ✅ **Performance**: Otimizações em cada fase
- ✅ **Quality Assurance**: Testes mantidos e expandidos

### Negativas
- ❌ **Complexidade**: Mais código para manter
- ❌ **Dependencies**: Algumas dependências externas (Serilog, EF Core)
- ❌ **Learning Curve**: Time precisa entender novas abstrações
- ❌ **Migration Effort**: Mudança gradual requer coordenação

## Alternativas Consideradas

### 1. **Big Bang Implementation**
- ✅ Mudanças todas de uma vez
- ❌ Alto risco de breaking changes
- ❌ Difícil de testar
- ❌ Rollback complexo

### 2. **Microservices Approach**
- ✅ Isolamento total
- ❌ Over-engineering para o escopo atual
- ❌ Complexidade de deployment
- ❌ Overhead desnecessário

### 3. **Framework Switch (FastAPI, Go, etc.)**
- ✅ Tecnologias diferentes
- ❌ Reescrita completa
- ❌ Perda do investment atual
- ❌ Ramp-up time alto

## Métricas de Sucesso

### FASE 1 (Concluída):
- ✅ **Testes**: 83/83 passando (100%)
- ✅ **Performance**: <50ms response time médio
- ✅ **Security**: Rate limiting funcional
- ✅ **Reliability**: 0 erros não tratados

### Targets FASE 2-4:
- **Testes**: 100+ testes (target: >90% coverage)
- **Performance**: <100ms response time médio
- **Reliability**: 99.9% uptime em health checks  
- **Security**: Zero vulnerabilidades críticas
- **Maintainability**: Code coverage >90%

## Cronograma Estimado
- ✅ **FASE 1**: Concluída (2024-12-29)
- 🔄 **FASE 2**: 1-2 semanas
- 🔄 **FASE 3**: 1 semana  
- 🔄 **FASE 4**: 1 semana

## Referencias
- [Clean Architecture - Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Microsoft .NET Architecture Guides](https://docs.microsoft.com/en-us/dotnet/architecture/)
- [ASP.NET Core Best Practices](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/best-practices)