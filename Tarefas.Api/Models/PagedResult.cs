namespace Tarefas.Api.Models;

/// <summary>
/// Resultado paginado com metadados
/// </summary>
/// <typeparam name="T">Tipo dos dados paginados</typeparam>
public class PagedResult<T>
{
    public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
    public PaginationMetadata Pagination { get; set; } = new();

    public PagedResult()
    {
    }

    public PagedResult(IEnumerable<T> data, int totalCount, int currentPage, int pageSize)
    {
        Data = data;
        Pagination = new PaginationMetadata
        {
            CurrentPage = currentPage,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            HasNext = currentPage < (int)Math.Ceiling((double)totalCount / pageSize),
            HasPrevious = currentPage > 1
        };
    }
}

/// <summary>
/// Metadados de paginação
/// </summary>
public class PaginationMetadata
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
}