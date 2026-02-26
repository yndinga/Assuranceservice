namespace AssuranceService.Domain.Events;

public record AssuranceProcessFailedEvent
{
    public Guid AssuranceId { get; init; }
    public string NoPolice { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
    public string ErrorCode { get; init; } = string.Empty;
    public DateTime FailedAt { get; init; }
}





