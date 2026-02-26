using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AssuranceService.Application.Sagas;

namespace AssuranceService.Infrastructure.Data;

public class AssuranceProcessStateMap : SagaClassMap<AssuranceProcessState>
{
    protected override void Configure(EntityTypeBuilder<AssuranceProcessState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState)
            .HasMaxLength(64);

        entity.Property(x => x.AssuranceId);
        entity.Property(x => x.NoPolice)
            .HasMaxLength(255);
        entity.Property(x => x.TypeContrat)
            .HasMaxLength(25);
        entity.Property(x => x.Importateur)
            .HasMaxLength(250);

        entity.Property(x => x.StartedAt);
        entity.Property(x => x.CompletedAt);
        entity.Property(x => x.ErrorMessage)
            .HasMaxLength(1000);
        entity.Property(x => x.ErrorCode)
            .HasMaxLength(50);

        entity.Property(x => x.AssuranceCreated);
        entity.Property(x => x.MarchandisesAdded);
        entity.Property(x => x.PrimeCalculated);
        entity.Property(x => x.GarantiesAssigned);
        entity.Property(x => x.ProcessCompleted);

        entity.Property(x => x.RetryCount);
        entity.Property(x => x.LastRetryAt);
    }
}

