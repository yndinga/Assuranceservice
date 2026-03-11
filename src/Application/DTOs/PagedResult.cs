namespace AssuranceService.Application.DTOs;

/// <summary>
/// Réponse paginée (équivalent Laravel paginate).
/// </summary>
public record PagedResult<T>
{
    public IReadOnlyList<T> Data { get; init; } = new List<T>();
    public int Total { get; init; }
    public int Page { get; init; }
    public int PerPage { get; init; }
}
