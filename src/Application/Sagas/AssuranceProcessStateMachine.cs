using MassTransit;
using MassTransit.Saga;
using AssuranceService.Domain.Events;

namespace AssuranceService.Application.Sagas;

public class AssuranceProcessStateMachine : MassTransitStateMachine<AssuranceProcessState>
{
    public State Created { get; private set; } = null!;
    public State MarchandisesProcessing { get; private set; } = null!;
    public State PrimeCalculating { get; private set; } = null!;
    public State GarantiesAssigning { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Failed { get; private set; } = null!;

    public Event<AssuranceProcessStartedEvent> ProcessStarted { get; private set; } = null!;
    public Event<AssuranceCreatedEvent> AssuranceCreated { get; private set; } = null!;
    public Event<MarchandiseAddedEvent> MarchandiseAdded { get; private set; } = null!;
    public Event<PrimeCalculatedEvent> PrimeCalculated { get; private set; } = null!;
    public Event<AssuranceProcessCompletedEvent> ProcessCompleted { get; private set; } = null!;
    public Event<AssuranceProcessFailedEvent> ProcessFailed { get; private set; } = null!;

    public AssuranceProcessStateMachine()
    {
        InstanceState(x => x.CurrentState);

        // Define the events
        Event(() => ProcessStarted, x => x.CorrelateById(context => context.Message.AssuranceId));
        Event(() => AssuranceCreated, x => x.CorrelateById(context => context.Message.AssuranceId));
        Event(() => MarchandiseAdded, x => x.CorrelateById(context => context.Message.AssuranceId));
        Event(() => PrimeCalculated, x => x.CorrelateById(context => context.Message.AssuranceId));
        Event(() => ProcessCompleted, x => x.CorrelateById(context => context.Message.AssuranceId));
        Event(() => ProcessFailed, x => x.CorrelateById(context => context.Message.AssuranceId));

        // Initial state
        Initially(
            When(ProcessStarted)
                .Then(context =>
                {
                    context.Saga.AssuranceId = context.Message.AssuranceId;
                    context.Saga.NoPolice = context.Message.NoPolice;
                    context.Saga.TypeContrat = context.Message.TypeContrat;
                    context.Saga.StartedAt = context.Message.StartedAt;
                })
                .TransitionTo(Created)
                .Publish(context => new AssuranceCreatedEvent
                {
                    AssuranceId = context.Message.AssuranceId,
                    NoPolice = context.Message.NoPolice,
                    TypeContrat = context.Message.TypeContrat,
                    CreatedAt = DateTime.UtcNow
                })
        );

        // Assurance created state
        During(Created,
            When(AssuranceCreated)
                .Then(context =>
                {
                    context.Saga.AssuranceCreated = true;
                })
                .TransitionTo(MarchandisesProcessing)
        );

        // Marchandises processing state
        During(MarchandisesProcessing,
            When(MarchandiseAdded)
                .Then(context =>
                {
                    context.Saga.MarchandisesAdded = true;
                })
                .TransitionTo(PrimeCalculating)
                .Publish(context => new PrimeCalculatedEvent
                {
                    PrimeId = Guid.NewGuid(),
                    AssuranceId = context.Message.AssuranceId,
                    ValeurFCFA = context.Message.Valeur,
                    CalculatedAt = DateTime.UtcNow
                })
        );

        // Prime calculating state
        During(PrimeCalculating,
            When(PrimeCalculated)
                .Then(context =>
                {
                    context.Saga.PrimeCalculated = true;
                })
                .TransitionTo(GarantiesAssigning)
        );

        // Garanties assigning state
        During(GarantiesAssigning,
            When(ProcessCompleted)
                .Then(context =>
                {
                    context.Saga.ProcessCompleted = true;
                    context.Saga.CompletedAt = DateTime.UtcNow;
                })
                .TransitionTo(Completed)
                .Finalize()
        );

        // Failure handling
        DuringAny(
            When(ProcessFailed)
                .Then(context =>
                {
                    context.Saga.ErrorMessage = context.Message.ErrorMessage;
                    context.Saga.ErrorCode = context.Message.ErrorCode;
                })
                .TransitionTo(Failed)
                .Finalize()
        );
    }
}

// Helper events for timeouts and retries
public record ProcessTimeoutEvent
{
    public Guid AssuranceId { get; init; }
}

public record ProcessRetryEvent
{
    public Guid AssuranceId { get; init; }
}
