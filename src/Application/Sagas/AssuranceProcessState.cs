using MassTransit;

namespace AssuranceService.Application.Sagas;

public class AssuranceProcessState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;
    
    // Assurance data
    public Guid AssuranceId { get; set; }
    public string NoPolice { get; set; } = string.Empty;
    public string TypeContrat { get; set; } = string.Empty;
    public string Importateur { get; set; } = string.Empty;
    
    // Process tracking
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    
    // Steps completion
    public bool AssuranceCreated { get; set; }
    public bool MarchandisesAdded { get; set; }
    public bool PrimeCalculated { get; set; }
    public bool GarantiesAssigned { get; set; }
    public bool ProcessCompleted { get; set; }
    
    // Retry tracking
    public int RetryCount { get; set; }
    public DateTime? LastRetryAt { get; set; }
}





