using System.Globalization;
using AssuranceService.Application.Common;
using AssuranceService.Application.DTOs;
using AssuranceService.Domain.Constants;
using AssuranceService.Domain.Models;
using MediatR;

namespace AssuranceService.Application.Assurances.Commands;

public sealed class CreateAssuranceFromDossiersHandler
    : IRequestHandler<CreateAssuranceFromDossiersCommand, AssuranceDetailDto>
{
    private const int DossierOuvert = 50;

    private readonly IDeclarationDossierClient _declarationClient;
    private readonly IAssuranceRepository _assuranceRepository;
    private readonly ITransportDetailsRepository _transportDetailsRepository;
    private readonly IPrimeRepository _primeRepository;
    private readonly IPrimeCalculatorService _primeCalculatorService;
    private readonly ICurrentUserService _currentUser;
    private readonly IDeclarationInvoiceImporter _invoiceImporter;

    public CreateAssuranceFromDossiersHandler(
        IDeclarationDossierClient declarationClient,
        IAssuranceRepository assuranceRepository,
        ITransportDetailsRepository transportDetailsRepository,
        IPrimeRepository primeRepository,
        IPrimeCalculatorService primeCalculatorService,
        ICurrentUserService currentUser,
        IDeclarationInvoiceImporter invoiceImporter)
    {
        _declarationClient = declarationClient;
        _assuranceRepository = assuranceRepository;
        _transportDetailsRepository = transportDetailsRepository;
        _primeRepository = primeRepository;
        _primeCalculatorService = primeCalculatorService;
        _currentUser = currentUser;
        _invoiceImporter = invoiceImporter;
    }

    public async Task<AssuranceDetailDto> Handle(
        CreateAssuranceFromDossiersCommand request,
        CancellationToken cancellationToken)
    {
        var dossiers = await LoadDossiersAsync(request.DossierIds, cancellationToken);
        var sources = ResolveSelectedLines(dossiers, request.Lignes);
        EnsureCompatibleEmbarquement(sources);

        var consumptions = await _assuranceRepository.GetLineConsumptionsAsync(
            sources.Select(source => source.Ligne.Id).Distinct().ToArray(),
            cancellationToken: cancellationToken);

        var now = DateTime.UtcNow;
        var user = NormalizeOptional(_currentUser.UserName) ?? "System";
        var organisationCode = NormalizeOptional(_currentUser.OrganisationCode) ?? string.Empty;
        var typePartenaire = NormalizeTypePartenaire(request.TypePartenaire);
        var directAssureur = string.Equals(typePartenaire, TypePartenaireCodes.Assureur, StringComparison.OrdinalIgnoreCase);
        var firstSource = sources[0];
        var modes = sources.Select(source => ModeDeTransportCodes.Normalize(source.Dossier.ModeDeTransport))
            .Where(mode => !string.IsNullOrWhiteSpace(mode))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var assurance = new Assurance
        {
            Id = Guid.NewGuid(),
            NoFacture = JoinLimited(sources.Select(source => source.Commande.NoFacture), 250),
            ImportateurNom = firstSource.Dossier.ImportateurNom ?? string.Empty,
            ImportateurNIU = firstSource.Dossier.ImportateurNIU ?? string.Empty,
            TypeContrat = NormalizeRequired(request.TypeContrat, "Type contrat"),
            Duree = NormalizeOptional(request.Duree),
            DureeJours = ParseDurationDays(request.Duree),
            Etat = StatutAssuranceCodes.Elabore,
            GarantieId = ParseOptionalGuid(NormalizeRequired(request.Garantie, "Garantie")),
            Partenaire = NormalizeRequired(request.Assureur, "Assureur"),
            Intermediaire = directAssureur
                ? NormalizeOptional(request.Intermediaire)
                : NormalizeRequired(request.Intermediaire, "Intermediaire"),
            TypePartenaire = typePartenaire,
            OCRE = NormalizeOptional(firstSource.Dossier.OCRE) ?? organisationCode,
            PCRE = firstSource.Dossier.PCRE ?? string.Empty,
            ModeDeTransport = modes.Length == 1 ? modes[0] : "MULTI",
            CreerPar = user,
            ModifierPar = user,
            CreerLe = now,
            ModifierLe = now
        };

        var declarationEntities = CreateDeclarationSnapshots(assurance, sources, user, now);
        foreach (var source in sources)
        {
            var consumption = consumptions.GetValueOrDefault(source.Ligne.Id)
                ?? new AssuranceLineConsumption(0m, 0m, 0m, 0m, 0m, 0m);
            var line = CreateLineSnapshot(
                assurance,
                declarationEntities[source.Dossier.Id],
                source,
                consumption,
                user,
                now);

            assurance.Lignes.Add(line);
            declarationEntities[source.Dossier.Id].Lignes.Add(line);
        }

        if (assurance.Lignes.Count == 0)
            throw new InvalidOperationException("L'assurance doit contenir au moins une ligne DI.");

        await _assuranceRepository.CreateAsync(assurance);
        await CreateVoyageSnapshotAsync(assurance, firstSource, modes, request, user, now, cancellationToken);
        await _assuranceRepository.SaveChangesAsync();
        await CreateInitialPrimeAsync(assurance, user);
        await _invoiceImporter.ImportAsync(
            assurance.Id,
            sources
                .GroupBy(source => source.Dossier.Id)
                .Select(group => new DeclarationInvoiceSelection(
                    group.Key,
                    group.First().Dossier.NoDossier,
                    group.Select(source => source.Commande.Id).Distinct().ToArray()))
                .ToArray(),
            user,
            cancellationToken);

        var saved = await _assuranceRepository.GetByIdAsync(assurance.Id);
        return (saved ?? assurance).ToDetailDto();
    }

    private async Task<IReadOnlyList<DeclarationDossierSource>> LoadDossiersAsync(
        IReadOnlyCollection<Guid> dossierIds,
        CancellationToken cancellationToken)
    {
        var tasks = dossierIds.Distinct().Select(id => _declarationClient.GetDossierAsync(id, cancellationToken));
        var dossiers = await Task.WhenAll(tasks);
        var result = new List<DeclarationDossierSource>(dossiers.Length);

        foreach (var dossier in dossiers)
        {
            if (dossier is null || dossier.EffectiveId == Guid.Empty)
                throw new InvalidOperationException("Une DI sélectionnée est introuvable ou non visible.");
            if (dossier.Etat != DossierOuvert)
                throw new InvalidOperationException($"La DI {dossier.NoDossier ?? dossier.Id.ToString()} doit être ouverte (état 50). État actuel : {dossier.Etat}.");

            EnsureCurrentOrganisationCanCreateAssurance(dossier);
            result.Add(dossier);
        }

        return result;
    }

    private static List<SelectedSource> ResolveSelectedLines(
        IReadOnlyList<DeclarationDossierSource> dossiers,
        IReadOnlyCollection<AssuranceLigneSelection> selections)
    {
        var dossiersById = dossiers.ToDictionary(dossier => dossier.Id);
        var result = new List<SelectedSource>(selections.Count);

        foreach (var selection in selections)
        {
            if (!dossiersById.TryGetValue(selection.DossierId, out var dossier))
                throw new InvalidOperationException($"La ligne {selection.LigneDIId} référence une DI qui n'a pas été sélectionnée.");

            var matches = dossier.Commandes
                .SelectMany(commande => commande.LignesCommandes.Select(ligne => new { Commande = commande, Ligne = ligne }))
                .Where(item => item.Ligne.Id == selection.LigneDIId)
                .ToArray();

            if (matches.Length != 1)
                throw new InvalidOperationException($"La ligne DI {selection.LigneDIId} est introuvable ou ambiguë dans la DI {dossier.NoDossier ?? dossier.Id.ToString()}.");

            result.Add(new SelectedSource(dossier, matches[0].Commande, matches[0].Ligne, selection));
        }

        return result;
    }

    private static void EnsureCompatibleEmbarquement(IReadOnlyList<SelectedSource> sources)
    {
        var first = sources[0];
        var expectedCountry = GetCountryCode(first.Commande);
        var expectedPort = GetPortCode(first.Commande);

        foreach (var source in sources.Skip(1))
        {
            var country = GetCountryCode(source.Commande);
            var port = GetPortCode(source.Commande);
            if (!string.Equals(country, expectedCountry, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(port, expectedPort, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"La DI {source.Dossier.NoDossier ?? source.Dossier.Id.ToString()} est incompatible : " +
                    $"pays/port d'embarquement {country}/{port}, attendu {expectedCountry}/{expectedPort}.");
            }
        }
    }

    private static Dictionary<Guid, AssuranceDeclaration> CreateDeclarationSnapshots(
        Assurance assurance,
        IReadOnlyList<SelectedSource> sources,
        string user,
        DateTime now)
    {
        var result = new Dictionary<Guid, AssuranceDeclaration>();
        foreach (var group in sources.GroupBy(source => source.Dossier.Id))
        {
            var source = group.First();
            var declaration = new AssuranceDeclaration
            {
                Id = Guid.NewGuid(),
                AssuranceId = assurance.Id,
                DeclarationId = source.Dossier.Id,
                NumeroDI = source.Dossier.NoDossier,
                PaysEmbarquementCode = GetCountryCode(source.Commande),
                PortEmbarquementCode = GetPortCode(source.Commande),
                Assurance = assurance,
                CreerPar = user,
                ModifierPar = user,
                CreerLe = now,
                ModifierLe = now
            };
            assurance.Declarations.Add(declaration);
            result.Add(source.Dossier.Id, declaration);
        }

        return result;
    }

    private static AssuranceLigne CreateLineSnapshot(
        Assurance assurance,
        AssuranceDeclaration declaration,
        SelectedSource source,
        AssuranceLineConsumption consumed,
        string user,
        DateTime now)
    {
        var selection = source.Selection;
        var line = source.Ligne;
        var quantite = ResolveAmount(selection.Quantite, line.Quantite, consumed.Quantite, "quantité", line.Id);
        var masseBrute = ResolveAmount(selection.MasseBrute, line.MasseBrute, consumed.MasseBrute, "masse brute", line.Id);
        var masseNette = ResolveAmount(selection.MasseNette, line.MasseNette, consumed.MasseNette, "masse nette", line.Id);
        var volume = ResolveAmount(selection.Volume, line.Volume, consumed.Volume, "volume", line.Id);
        var valeurDevise = ResolveAmount(selection.ValeurDevise, line.ValeurDevise, consumed.ValeurDevise, "valeur en devise", line.Id);
        var valeurXaf = ResolveAmount(selection.ValeurXAF, line.ValeurFCFA, consumed.ValeurXAF, "valeur XAF", line.Id);

        if (new[] { quantite, masseBrute, masseNette, volume, valeurDevise, valeurXaf }.All(value => value <= 0m))
            throw new InvalidOperationException($"La ligne DI {line.Id} ne contient aucun reliquat positif à assurer.");

        var estPartielle = IsPartial(quantite, line.Quantite)
            || IsPartial(masseBrute, line.MasseBrute)
            || IsPartial(masseNette, line.MasseNette)
            || IsPartial(volume, line.Volume)
            || IsPartial(valeurDevise, line.ValeurDevise)
            || IsPartial(valeurXaf, line.ValeurFCFA);

        return new AssuranceLigne
        {
            Id = Guid.NewGuid(),
            AssuranceId = assurance.Id,
            AssuranceDeclarationId = declaration.Id,
            DeclarationId = source.Dossier.Id,
            SourceCommandeId = source.Commande.Id,
            SourceLigneDIId = line.Id,
            NoLigne = line.NoLigne,
            PositionTarifaire = line.PositionTarifaire ?? string.Empty,
            Designation = line.Designation ?? string.Empty,
            Marque = line.Marque ?? string.Empty,
            Colisage = line.Colisage ?? string.Empty,
            Quantite = quantite,
            MasseBrute = masseBrute,
            MasseNette = masseNette,
            Volume = volume,
            PrixUnitaire = line.PrixUnitaire,
            ValeurDevise = valeurDevise,
            ValeurXAF = valeurXaf,
            Devise = NormalizeOptional(source.Commande.Devise),
            UniteStatistique = NormalizeOptional(line.UniteStatistique),
            PaysOrigine = NormalizeOptional(line.PaysOrigine),
            EstPartielle = estPartielle,
            Assurance = assurance,
            AssuranceDeclaration = declaration,
            CreerPar = user,
            ModifierPar = user,
            CreerLe = now,
            ModifierLe = now
        };
    }

    private async Task CreateVoyageSnapshotAsync(
        Assurance assurance,
        SelectedSource firstSource,
        IReadOnlyCollection<string> modes,
        CreateAssuranceFromDossiersCommand request,
        string user,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var voyage = await _transportDetailsRepository.AddVoyageAsync(new Voyage
        {
            AssuranceId = assurance.Id,
            NomTransporteur = NormalizeOptional(request.NomTransporteur) ?? string.Empty,
            LieuSejour = NormalizeOptional(request.LieuSejour),
            DureeSejour = NormalizeOptional(request.DureeSejour),
            PaysProvenance = GetCountryCode(firstSource.Commande),
            PaysDestination = firstSource.Commande.PaysDestination ?? string.Empty,
            Designation = firstSource.Ligne.Designation ?? firstSource.Commande.Intitule ?? string.Empty,
            Nature = NormalizeOptional(request.Nature),
            Specificites = NormalizeOptional(request.Specificites),
            Conditionnement = NormalizeOptional(request.Conditionnement) ?? NormalizeOptional(firstSource.Ligne.Colisage),
            DescriptionConditionnement = NormalizeOptional(request.DescriptionConditionnement),
            Devise = modes.Count == 1 ? firstSource.Commande.Devise : null,
            MasseBrute = FormatDecimal(assurance.Lignes.Sum(line => line.MasseBrute ?? 0m)),
            UniteStatistique = NormalizeOptional(request.UniteStatistique) ?? NormalizeOptional(firstSource.Ligne.UniteStatistique),
            Marque = NormalizeOptional(request.Marque) ?? NormalizeOptional(firstSource.Ligne.Marque),
            CreerPar = user,
            ModifierPar = user,
            CreerLe = now,
            ModifierLe = now
        }, cancellationToken);

        if (modes.Count != 1)
            return;

        switch (modes.Single())
        {
            case ModeDeTransportCodes.Maritime:
                await _transportDetailsRepository.AddMaritimeAsync(new Maritime
                {
                    VoyageId = voyage.Id,
                    PortEmbarquementCode = NormalizeCode(firstSource.Commande.PortEmbarquement),
                    PortDebarquementCode = NormalizeCode(firstSource.Commande.PortDebarquement),
                    NumeroBL = NormalizeOptional(request.NumeroBL),
                    NomNavire = NormalizeOptional(request.NomNavire),
                    TypeNavire = NormalizeOptional(request.TypeNavire)
                }, cancellationToken);
                break;
            case ModeDeTransportCodes.Aerien:
                await _transportDetailsRepository.AddAerienAsync(new Aerien
                {
                    VoyageId = voyage.Id,
                    AeroportEmbarquementCode = NormalizeCode(firstSource.Commande.AeroportEmbarquement),
                    AeroportDebarquementCode = NormalizeCode(firstSource.Commande.AeroportDebarquement),
                    NumeroLTA = NormalizeOptional(request.NumeroLTA)
                }, cancellationToken);
                break;
            case ModeDeTransportCodes.Routier:
                await _transportDetailsRepository.AddRoutierAsync(new Routier
                {
                    VoyageId = voyage.Id,
                    RouteNationaleCode = NormalizeCode(firstSource.Commande.RouteNationale),
                    NumeroLV = NormalizeOptional(request.NumeroLV)
                }, cancellationToken);
                break;
            case ModeDeTransportCodes.Fluvial:
                await _transportDetailsRepository.AddFluvialAsync(new Fluvial
                {
                    VoyageId = voyage.Id,
                    PortEmbarquementCode = NormalizeCode(firstSource.Commande.FleuveEmbarquement ?? firstSource.Commande.PortEmbarquement),
                    PortDebarquementCode = NormalizeCode(firstSource.Commande.FleuveDebarquement ?? firstSource.Commande.PortDebarquement),
                    NomNavire = NormalizeOptional(request.NomNavire),
                    TypeNavire = NormalizeOptional(request.TypeNavire)
                }, cancellationToken);
                break;
        }
    }

    private async Task CreateInitialPrimeAsync(Assurance assurance, string user)
    {
        var valeurXaf = assurance.Lignes.Sum(line => line.ValeurXAF ?? 0m);
        var devises = assurance.Lignes.Select(line => NormalizeCode(line.Devise))
            .Where(code => code is not null)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var valeurDevise = devises.Length == 1
            ? assurance.Lignes.Sum(line => line.ValeurDevise ?? 0m)
            : 0m;

        if (valeurDevise <= 0m && valeurXaf <= 0m)
            return;

        var prime = new Prime
        {
            AssuranceId = assurance.Id,
            ValeurDevise = valeurDevise,
            ValeurFCFA = valeurXaf,
            Statut = StatutAssuranceCodes.Paye,
            CreerPar = user,
            ModifierPar = user,
            CreerLe = DateTime.UtcNow,
            ModifierLe = DateTime.UtcNow
        };

        if (assurance.GarantieId.HasValue && valeurDevise > 0m && devises.Length == 1)
        {
            var calculation = await _primeCalculatorService.CalculerPrimeAsync(new PrimeCalculationRequest
            {
                AssuranceId = assurance.Id,
                GarantieId = assurance.GarantieId.Value,
                ValeurDevise = valeurDevise,
                ValeurFCFA = valeurXaf > 0m ? valeurXaf : null,
                Devise = devises[0]!
            });
            prime.ValeurFCFA = calculation.ValeurFCFA;
            prime.Taux = calculation.Taux;
            prime.Accessoires = (double)calculation.Accessoires;
            prime.Taxe = calculation.Taxe;
            prime.PrimeNette = calculation.PrimeNette;
            prime.PrimeTotale = calculation.PrimeTotale;
        }

        await _primeRepository.CreateAsync(prime);
    }

    private void EnsureCurrentOrganisationCanCreateAssurance(DeclarationDossierSource dossier)
    {
        var organisation = NormalizeCode(_currentUser.OrganisationCode);
        if (organisation is null)
            throw new InvalidOperationException("Organisation connectée introuvable. Impossible de créer l'assurance.");

        var importateur = string.Equals(NormalizeCode(dossier.OCRE), organisation, StringComparison.OrdinalIgnoreCase);
        var transitaire = string.Equals(NormalizeCode(dossier.Transitaire), organisation, StringComparison.OrdinalIgnoreCase);
        if (!importateur && !transitaire)
            throw new InvalidOperationException($"L'organisation connectée ne peut pas utiliser la DI {dossier.NoDossier ?? dossier.Id.ToString()}.");
    }

    private static decimal ResolveAmount(decimal? requested, decimal? source, decimal consumed, string label, Guid lineId)
    {
        var sourceValue = source ?? 0m;
        var remaining = Math.Max(0m, sourceValue - consumed);
        var selected = requested ?? remaining;
        if (selected < 0m)
            throw new InvalidOperationException($"La {label} de la ligne {lineId} ne peut pas être négative.");
        if (selected > remaining)
            throw new InvalidOperationException($"La {label} assurée de la ligne {lineId} ({selected}) dépasse le reliquat disponible ({remaining}).");
        return selected;
    }

    private static bool IsPartial(decimal selected, decimal? source)
    {
        var sourceValue = source.GetValueOrDefault();
        return sourceValue > 0m && selected < sourceValue;
    }

    private static string GetCountryCode(DeclarationCommandeSource commande) =>
        NormalizeCode(commande.PaysEmbarquement ?? commande.PaysProvenance)
        ?? throw new InvalidOperationException("Le pays d'embarquement est obligatoire sur chaque DI sélectionnée.");

    private static string GetPortCode(DeclarationCommandeSource commande) =>
        NormalizeCode(commande.PortEmbarquement
                      ?? commande.FleuveEmbarquement
                      ?? commande.AeroportEmbarquement
                      ?? commande.RouteNationale)
        ?? throw new InvalidOperationException("Le port ou point d'embarquement est obligatoire sur chaque DI sélectionnée.");

    private static string NormalizeRequired(string? value, string label) =>
        NormalizeOptional(value) ?? throw new InvalidOperationException($"{label} requis avant création AFI.");

    private static string NormalizeTypePartenaire(string? value)
    {
        var normalized = NormalizeCode(value);
        return normalized is TypePartenaireCodes.Courtier or TypePartenaireCodes.AgentGeneral
            ? normalized
            : TypePartenaireCodes.Assureur;
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string? NormalizeCode(string? value) => NormalizeOptional(value)?.ToUpperInvariant();
    private static Guid? ParseOptionalGuid(string? value) => Guid.TryParse(value, out var id) ? id : null;
    private static int? ParseDurationDays(string? value) =>
        int.TryParse(value?.Trim(), out var days) && days > 0 ? days : null;
    private static string? FormatDecimal(decimal? value) => value?.ToString("0.#####", CultureInfo.InvariantCulture);

    private static string? JoinLimited(IEnumerable<string?> values, int maxLength)
    {
        var joined = string.Join(", ", values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value!.Trim()).Distinct());
        return joined.Length <= maxLength ? joined : joined[..maxLength];
    }

    private sealed record SelectedSource(
        DeclarationDossierSource Dossier,
        DeclarationCommandeSource Commande,
        DeclarationLigneCommandeSource Ligne,
        AssuranceLigneSelection Selection);
}
