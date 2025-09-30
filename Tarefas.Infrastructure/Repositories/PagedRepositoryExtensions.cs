using Microsoft.EntityFrameworkCore;

namespace Tarefas.Infrastructure.Repositories;

/// <summary>
/// Extensões para implementar paginação eficiente com EF Core
/// </summary>
public static class PagedRepositoryExtensions
{
    /// <summary>
    /// Converte um IQueryable em resultado paginado simples (sem PagedResult ainda)
    /// </summary>
    public static async Task<(IEnumerable<T> Data, int TotalCount)> ToPagedDataAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // Validação de parâmetros
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        // Query otimizada: executar count e dados em paralelo
        var countTask = query.CountAsync(cancellationToken);
        var dataTask = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Aguardar ambas as operações
        await Task.WhenAll(countTask, dataTask);

        var totalCount = await countTask;
        var data = await dataTask;

        return (data, totalCount);
    }

    /// <summary>
    /// Versão síncrona (usar apenas quando necessário)
    /// </summary>
    public static (IEnumerable<T> Data, int TotalCount) ToPagedData<T>(
        this IQueryable<T> query,
        int page,
        int pageSize)
    {
        // Validação de parâmetros
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var totalCount = query.Count();
        var data = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (data, totalCount);
    }

    /// <summary>
    /// Extensão para aplicar ordenação dinâmica
    /// </summary>
    public static IQueryable<T> ApplyOrdering<T>(
        this IQueryable<T> query,
        string? sortBy,
        bool descending = false)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query;
        }

        var property = typeof(T).GetProperty(sortBy);
        if (property == null)
        {
            return query;
        }

        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), "x");
        var propertyAccess = System.Linq.Expressions.Expression.Property(parameter, property);
        var orderByExpression = System.Linq.Expressions.Expression.Lambda(propertyAccess, parameter);

        string methodName = descending ? "OrderByDescending" : "OrderBy";
        var resultExpression = System.Linq.Expressions.Expression.Call(
            typeof(System.Linq.Queryable),
            methodName,
            new Type[] { typeof(T), property.PropertyType },
            query.Expression,
            System.Linq.Expressions.Expression.Quote(orderByExpression));

        return query.Provider.CreateQuery<T>(resultExpression);
    }
}