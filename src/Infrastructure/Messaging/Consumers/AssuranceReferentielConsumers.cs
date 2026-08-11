using AssuranceService.Application.Common;
using AssuranceService.Domain.Models.Referentiel;
using MassTransit;
using ReferentielService.Contracts;
using RouteEntity = AssuranceService.Domain.Models.Referentiel.Route;

namespace AssuranceService.Infrastructure.Messaging.Consumers;

public sealed class DeviseReferentielConsumer(IReferentielSyncService sync) :
    IConsumer<DeviseCreated>, IConsumer<DeviseUpdated>, IConsumer<DeviseDeleted>
{
    public Task Consume(ConsumeContext<DeviseCreated> c) => ReferentielConsumerHelpers.Insert<Devise>(sync, c.Message.Id, c.Message.PayloadJson, c.CancellationToken);
    public Task Consume(ConsumeContext<DeviseUpdated> c) => ReferentielConsumerHelpers.Upsert<Devise>(sync, c.Message.Id, c.Message.PayloadJson, c.CancellationToken);
    public Task Consume(ConsumeContext<DeviseDeleted> c) => ReferentielConsumerHelpers.Delete<Devise>(sync, c.Message.Id, c.CancellationToken);
}

public sealed class EtatReferentielConsumer(IReferentielSyncService sync) :
    IConsumer<EtatCreated>, IConsumer<EtatUpdated>, IConsumer<EtatDeleted>
{
    public Task Consume(ConsumeContext<EtatCreated> c) => ReferentielConsumerHelpers.Insert<Etat>(sync, c.Message.Id, c.Message.PayloadJson, c.CancellationToken);
    public Task Consume(ConsumeContext<EtatUpdated> c) => ReferentielConsumerHelpers.Upsert<Etat>(sync, c.Message.Id, c.Message.PayloadJson, c.CancellationToken);
    public Task Consume(ConsumeContext<EtatDeleted> c) => ReferentielConsumerHelpers.Delete<Etat>(sync, c.Message.Id, c.CancellationToken);
}

public sealed class PortReferentielConsumer(IReferentielSyncService sync) :
    IConsumer<PortCreated>, IConsumer<PortUpdated>, IConsumer<PortDeleted>
{
    public Task Consume(ConsumeContext<PortCreated> c) => ReferentielConsumerHelpers.Insert<Port>(sync, c.Message.Id, c.Message.PayloadJson, c.CancellationToken);
    public Task Consume(ConsumeContext<PortUpdated> c) => ReferentielConsumerHelpers.Upsert<Port>(sync, c.Message.Id, c.Message.PayloadJson, c.CancellationToken);
    public Task Consume(ConsumeContext<PortDeleted> c) => ReferentielConsumerHelpers.Delete<Port>(sync, c.Message.Id, c.CancellationToken);
}

public sealed class TauxDeChangeReferentielConsumer(IReferentielSyncService sync) :
    IConsumer<TauxDeChangeCreated>, IConsumer<TauxDeChangeUpdated>, IConsumer<TauxDeChangeDeleted>
{
    public Task Consume(ConsumeContext<TauxDeChangeCreated> c) => ReferentielConsumerHelpers.Insert<TauxDeChange>(sync, c.Message.Id, c.Message.PayloadJson, c.CancellationToken);
    public Task Consume(ConsumeContext<TauxDeChangeUpdated> c) => ReferentielConsumerHelpers.Upsert<TauxDeChange>(sync, c.Message.Id, c.Message.PayloadJson, c.CancellationToken);
    public Task Consume(ConsumeContext<TauxDeChangeDeleted> c) => ReferentielConsumerHelpers.Delete<TauxDeChange>(sync, c.Message.Id, c.CancellationToken);
}

public sealed class ModeDeTransportsReferentielConsumer(IReferentielSyncService sync) :
    IConsumer<ModeDeTransportsCreated>, IConsumer<ModeDeTransportsUpdated>, IConsumer<ModeDeTransportsDeleted>
{
    public Task Consume(ConsumeContext<ModeDeTransportsCreated> c) => ReferentielConsumerHelpers.Insert<TypeTransport>(sync, c.Message.Id, c.Message.PayloadJson, c.CancellationToken);
    public Task Consume(ConsumeContext<ModeDeTransportsUpdated> c) => ReferentielConsumerHelpers.Upsert<TypeTransport>(sync, c.Message.Id, c.Message.PayloadJson, c.CancellationToken);
    public Task Consume(ConsumeContext<ModeDeTransportsDeleted> c) => ReferentielConsumerHelpers.Delete<TypeTransport>(sync, c.Message.Id, c.CancellationToken);
}

public sealed class AeroportReferentielConsumer(IReferentielSyncService sync) :
    IConsumer<AeroportCreated>, IConsumer<AeroportUpdated>, IConsumer<AeroportDeleted>
{
    public Task Consume(ConsumeContext<AeroportCreated> c) => ReferentielConsumerHelpers.Insert<Aeroport>(sync, c.Message.Id, c.Message.PayloadJson, c.CancellationToken);
    public Task Consume(ConsumeContext<AeroportUpdated> c) => ReferentielConsumerHelpers.Upsert<Aeroport>(sync, c.Message.Id, c.Message.PayloadJson, c.CancellationToken);
    public Task Consume(ConsumeContext<AeroportDeleted> c) => ReferentielConsumerHelpers.Delete<Aeroport>(sync, c.Message.Id, c.CancellationToken);
}

public sealed class UniteStatistiqueReferentielConsumer(IReferentielSyncService sync) :
    IConsumer<UniteStatistiqueCreated>, IConsumer<UniteStatistiqueUpdated>, IConsumer<UniteStatistiqueDeleted>
{
    public Task Consume(ConsumeContext<UniteStatistiqueCreated> c) => ReferentielConsumerHelpers.Insert<UniteStatistique>(sync, c.Message.Id, c.Message.PayloadJson, c.CancellationToken);
    public Task Consume(ConsumeContext<UniteStatistiqueUpdated> c) => ReferentielConsumerHelpers.Upsert<UniteStatistique>(sync, c.Message.Id, c.Message.PayloadJson, c.CancellationToken);
    public Task Consume(ConsumeContext<UniteStatistiqueDeleted> c) => ReferentielConsumerHelpers.Delete<UniteStatistique>(sync, c.Message.Id, c.CancellationToken);
}

public sealed class CorridorReferentielConsumer(IReferentielSyncService sync) :
    IConsumer<CorridorCreated>, IConsumer<CorridorUpdated>, IConsumer<CorridorDeleted>
{
    public Task Consume(ConsumeContext<CorridorCreated> c) => ReferentielConsumerHelpers.Insert<Corridor>(sync, c.Message.Id, c.Message.PayloadJson, c.CancellationToken);
    public Task Consume(ConsumeContext<CorridorUpdated> c) => ReferentielConsumerHelpers.Upsert<Corridor>(sync, c.Message.Id, c.Message.PayloadJson, c.CancellationToken);
    public Task Consume(ConsumeContext<CorridorDeleted> c) => ReferentielConsumerHelpers.Delete<Corridor>(sync, c.Message.Id, c.CancellationToken);
}

public sealed class DepartementReferentielConsumer(IReferentielSyncService sync) :
    IConsumer<DepartementCreated>, IConsumer<DepartementUpdated>, IConsumer<DepartementDeleted>
{
    public Task Consume(ConsumeContext<DepartementCreated> c) => ReferentielConsumerHelpers.Insert<Departement>(sync, c.Message.Id, c.Message.PayloadJson, c.CancellationToken);
    public Task Consume(ConsumeContext<DepartementUpdated> c) => ReferentielConsumerHelpers.Upsert<Departement>(sync, c.Message.Id, c.Message.PayloadJson, c.CancellationToken);
    public Task Consume(ConsumeContext<DepartementDeleted> c) => ReferentielConsumerHelpers.Delete<Departement>(sync, c.Message.Id, c.CancellationToken);
}

public sealed class RouteNationaleReferentielConsumer(IReferentielSyncService sync) :
    IConsumer<RouteNationaleCreated>, IConsumer<RouteNationaleUpdated>, IConsumer<RouteNationaleDeleted>
{
    public Task Consume(ConsumeContext<RouteNationaleCreated> c) => ReferentielConsumerHelpers.Insert<RouteNationale>(sync, c.Message.Id, c.Message.PayloadJson, c.CancellationToken);
    public Task Consume(ConsumeContext<RouteNationaleUpdated> c) => ReferentielConsumerHelpers.Upsert<RouteNationale>(sync, c.Message.Id, c.Message.PayloadJson, c.CancellationToken);
    public Task Consume(ConsumeContext<RouteNationaleDeleted> c) => ReferentielConsumerHelpers.Delete<RouteNationale>(sync, c.Message.Id, c.CancellationToken);
}

public sealed class RouteReferentielConsumer(IReferentielSyncService sync) :
    IConsumer<RouteCreated>, IConsumer<RouteUpdated>, IConsumer<RouteDeleted>
{
    public Task Consume(ConsumeContext<RouteCreated> c) => ReferentielConsumerHelpers.Insert<RouteEntity>(sync, c.Message.Id, c.Message.PayloadJson, c.CancellationToken);
    public Task Consume(ConsumeContext<RouteUpdated> c) => ReferentielConsumerHelpers.Upsert<RouteEntity>(sync, c.Message.Id, c.Message.PayloadJson, c.CancellationToken);
    public Task Consume(ConsumeContext<RouteDeleted> c) => ReferentielConsumerHelpers.Delete<RouteEntity>(sync, c.Message.Id, c.CancellationToken);
}

public sealed class TronconReferentielConsumer(IReferentielSyncService sync) :
    IConsumer<TronconCreated>, IConsumer<TronconUpdated>, IConsumer<TronconDeleted>
{
    public Task Consume(ConsumeContext<TronconCreated> c) => ReferentielConsumerHelpers.Insert<Troncon>(sync, c.Message.Id, c.Message.PayloadJson, c.CancellationToken);
    public Task Consume(ConsumeContext<TronconUpdated> c) => ReferentielConsumerHelpers.Upsert<Troncon>(sync, c.Message.Id, c.Message.PayloadJson, c.CancellationToken);
    public Task Consume(ConsumeContext<TronconDeleted> c) => ReferentielConsumerHelpers.Delete<Troncon>(sync, c.Message.Id, c.CancellationToken);
}
