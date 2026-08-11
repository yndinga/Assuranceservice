using AssuranceService.Domain.Models.Referentiel;

namespace AssuranceService.Infrastructure.Messaging;

public sealed record ReferentielSyncEntry(string ApiRoute, Type LocalType);

public static class ReferentielSyncCatalog
{
    public static IReadOnlyList<ReferentielSyncEntry> All { get; } =
    [
        new("pays", typeof(Pays)),
        new("devises", typeof(Devise)),
        new("etats", typeof(Etat)),
        new("ports", typeof(Port)),
        new("tauxdechanges", typeof(TauxDeChange)),
        new("modedetransports", typeof(TypeTransport)),
        new("aeroports", typeof(Aeroport)),
        new("unitestatistiques", typeof(UniteStatistique)),
        new("corridors", typeof(Corridor)),
        new("departements", typeof(Departement)),
        new("routenationales", typeof(RouteNationale)),
        new("routes", typeof(Route)),
        new("troncons", typeof(Troncon))
    ];
}
