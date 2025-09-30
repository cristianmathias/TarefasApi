# ADR-002: PagedResult vs Entity Framework Paging

## Status
- **Data**: 2024-12-29
- **Status**: ✅ Aceito
- **Decisores**: Cristian Mathias
- **Tags**: paging, entity-framework, api-design, performance

## Contexto
A API precisa implementar paginação para endpoints que retornam listas de dados. A questão surge: usar paginação básica do Entity Framework (`Skip()` + `Take()`) ou criar uma abstração customizada `PagedResult<T>`?

### Problemas identificados:
1. EF Core oferece apenas `Skip()` e `Take()` básicos
2. APIs REST modernas precisam de metadados de paginação
3. Frontend precisa saber se há próximas páginas, total de registros, etc.
4. Múltiplas fontes de dados (EF, in-memory, APIs externas)
5. Consistência entre endpoints da API

## Decisão
**Implementar `PagedResult<T>` customizado** que complementa (não substitui) a paginação do Entity Framework.

### Estrutura implementada:
```csharp
public class PagedResult<T>
{
    public IEnumerable<T> Data { get; set; }
    public PaginationMetadata Pagination { get; set; }
}

public class PaginationMetadata  
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
}
```

### Response JSON:
```json
{
  "data": [...],
  "pagination": {
    "currentPage": 1,
    "pageSize": 10,
    "totalCount": 150,
    "totalPages": 15,
    "hasNext": true,
    "hasPrevious": false
  }
}
```

## Consequências

### Positivas
- ✅ **Frontend-Friendly**: Metadados automáticos para UI de paginação
- ✅ **API Consistency**: Response padronizado entre todos os endpoints
- ✅ **Rich Metadata**: HasNext, HasPrevious, TotalPages calculados automaticamente
- ✅ **Abstração de Persistência**: Funciona com EF, in-memory, APIs externas
- ✅ **Performance**: Evita queries separadas para contagem (na implementação EF)
- ✅ **Testabilidade**: Fácil de mockar e testar isoladamente
- ✅ **Type Safety**: Fortemente tipado e reutilizável

### Negativas  
- ❌ **Código Extra**: Mais classes para manter
- ❌ **Overhead Mínimo**: Wrapper adicional (negligível)
- ❌ **Learning Curve**: Devs precisam entender a abstração

## Alternativas Consideradas

### 1. **EF Core Puro**
```csharp
// Simples mas problemático
var items = await context.Tarefas.Skip((page-1)*size).Take(size).ToListAsync();
var total = await context.Tarefas.CountAsync(); // Query extra!
return Ok(new { items, total }); // Sem metadados
```
- ✅ Simplicidade
- ❌ Sem metadados (HasNext, TotalPages)
- ❌ Performance ruim (2 queries)
- ❌ Response inconsistente
- ❌ Não funciona com outras fontes de dados

### 2. **IPagedList (3rd party)**
```csharp
// PagedList NuGet package
var pagedList = await context.Tarefas.ToPagedListAsync(page, size);
```
- ✅ Biblioteca madura
- ❌ Dependência externa
- ❌ Não agnóstica de persistência
- ❌ Menos controle sobre response format

### 3. **Azure Cosmos DB Continuation Token**
- ✅ Excelente para NoSQL
- ❌ Específico para Cosmos DB
- ❌ Não funciona com EF/SQL

### 4. **GraphQL Connections**
- ✅ Padrão maduro
- ❌ Complexidade desnecessária para REST
- ❌ Overkill para caso de uso simples

## Comparação Detalhada

### Sem PagedResult (EF Puro):
```csharp
[HttpGet]
public async Task<IActionResult> Get(int page = 1, int size = 10)
{
    var items = await repository.GetAllAsync(page, size);
    var total = await repository.CountAsync(); // Query extra!
    
    return Ok(new { items, total, page }); // Inconsistente
}
```

**Problemas:**
- 2 queries ao banco (performance)
- Response inconsistente
- Frontend precisa calcular HasNext manualmente
- Não funciona com in-memory repository

### Com PagedResult:
```csharp
[HttpGet]
public async Task<PagedResult<Tarefa>> Get(int page = 1, int size = 10)
{
    return await tarefaService.GetAllPagedAsync(page, size);
}
```

**Benefícios:**
- 1 query otimizada (na implementação EF)
- Response padronizado
- Metadados automáticos
- Funciona com qualquer persistência

## Implementação na Fase 2 (EF Core)
```csharp
public async Task<PagedResult<T>> GetPagedAsync<T>(
    IQueryable<T> query, 
    int page, 
    int pageSize)
{
    var totalCount = await query.CountAsync();
    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return new PagedResult<T>(items, totalCount, page, pageSize);
}
```

## Casos de Uso
- ✅ **APIs REST** (nosso caso)
- ✅ **Múltiplas fontes** de dados  
- ✅ **Frontend apps** que precisam de paginação
- ✅ **Enterprise applications** com consistência
- ✅ **Performance crítica** (queries otimizadas)

## Referencias
- [Microsoft Docs - EF Core Pagination](https://docs.microsoft.com/en-us/ef/core/querying/pagination)
- [REST API Pagination Best Practices](https://restfulapi.net/rest-api-design-tutorial-with-example/)
- [PagedList Library](https://github.com/TroyGoode/PagedList)