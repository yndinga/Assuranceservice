using AssuranceService.Application.Common;
using MassTransit;
using ReferentielService.Contracts;

namespace AssuranceService.Infrastructure.Messaging.Consumers;

public sealed class PaysReferentielConsumer(IReferentielSyncService sync) :
    IConsumer<PaysCreated>, IConsumer<PaysUpdated>, IConsumer<PaysDeleted>
{
    public Task Consume(ConsumeContext<PaysCreated> c) =>
        sync.InsertPaysAsync(c.Message.PaysId, c.Message.Code, c.Message.Nom, c.Message.Actif, c.CancellationToken);

    public Task Consume(ConsumeContext<PaysUpdated> c) =>
        sync.UpsertPaysAsync(c.Message.PaysId, c.Message.Code, c.Message.Nom, c.Message.Actif, c.CancellationToken);

    public Task Consume(ConsumeContext<PaysDeleted> c) =>
        sync.DeletePaysAsync(c.Message.PaysId, c.CancellationToken);
}
