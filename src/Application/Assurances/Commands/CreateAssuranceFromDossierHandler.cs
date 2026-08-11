using System.Globalization;
using AssuranceService.Application.Common;
using AssuranceService.Application.DTOs;
using AssuranceService.Domain.Constants;
using AssuranceService.Domain.Models;
using MediatR;

namespace AssuranceService.Application.Assurances.Commands;

public sealed class CreateAssuranceFromDossierHandler : IRequestHandler<CreateAssuranceFromDossierCommand, AssuranceDetailDto>
{
    private const int DossierOuvert = 50;

    private readonly IDeclarationDossierClient _declarationClient;
    private readonly IAssuranceRepository _assuranceRepository;
    private readonly ITransportDetailsRepository _transportDetailsRepository;
    private readonly IPortRepository _portRepository;
    private readonly IPrimeRepository _primeRepository;
    private readonly IPrimeCalculatorService _primeCalculatorService;
    private readonly INumeroGeneratorService _numeroGeneratorService;
    private readonly ICurrentUserService _currentUser;

    public CreateAssuranceFromDossierHandler(
        IDeclarationDossierClient declarationClient,
        IAssuranceRepository assuranceRepository,
        ITransportDetailsRepository transportDetailsRepository,
        IPortRepository portRepository,
        IPrimeRepository primeRepository,
        IPrimeCalculatorService primeCalculatorService,
        INumeroGeneratorService numeroGeneratorService,
        ICurrentUserService currentUser)
    {
        _declarationClient = declarationClient;
        _assuranceRepository = assuranceRepository;
        _transportDetailsRepository = transportDetailsRepository;
        _portRepository = portRepository;
        _primeRepository = primeRepository;
        _primeCalculatorService = primeCalculatorService;
        _numeroGeneratorService = numeroGeneratorService;
        _currentUser = currentUser;
    }

    public async Task<AssuranceDetailDto> Handle(CreateAssuranceFromDossierCommand request, CancellationToken cancellationToken)
    {
        var typePartenaire = NormalizeTypePartenaire(request.TypePartenaire);
        var isAssureurDirect = string.Equals(typePartenaire, TypePartenaireCodes.Assureur, StringComparison.OrdinalIgnoreCase);
        var typeContrat = NormalizeRequired(request.TypeContrat, "Type contrat");
        var garantie = NormalizeRequired(request.Garantie, "Garantie");
        var assureur = NormalizeRequired(request.Assureur, "Assureur");
        var intermediaire = isAssureurDirect
            ? NormalizeOptional(request.Intermediaire)
            : NormalizeRequired(request.Intermediaire, "Intermediaire");
        var duree = NormalizeOptional(request.Duree);

        var dossier = await _declarationClient.GetDossierAsync(request.DossierId, cancellationToken);
        if (dossier is null)
            throw new InvalidOperationException("Dossier introuvable ou non visible.");

        if (dossier.EffectiveId == Guid.Empty)
            throw new InvalidOperationException("Le dossier source est invalide.");

        if (dossier.Etat != DossierOuvert)
            throw new InvalidOperationException($"La souscription AFI exige une DI/ASI ouverte (etat 50). Etat actuel : {dossier.Etat}.");

        EnsureCurrentOrganisationCanCreateAssurance(dossier);

        var commande = dossier.Commandes.FirstOrDefault(c => c.EffectiveId != Guid.Empty);
        if (commande is null)
            throw new InvalidOperationException("Aucune commande source trouvee pour ce dossier.");

        var ligne = commande.LignesCommandes
            .OrderBy(l => l.NoLigne == 0 ? int.MaxValue : l.NoLigne)
            .FirstOrDefault();

        var modeDeTransport = ModeDeTransportCodes.Normalize(dossier.ModeDeTransport);
        var now = DateTime.UtcNow;
        var user = string.IsNullOrWhiteSpace(_currentUser.UserName) ? "System" : _currentUser.UserName;
        var organisationCode = NormalizeOptional(_currentUser.OrganisationCode);

        var assurance = new Assurance
        {
            NoFacture = commande.NoFacture,
            ImportateurNom = dossier.ImportateurNom ?? string.Empty,
            ImportateurNIU = dossier.ImportateurNIU ?? string.Empty,
            TypeContrat = typeContrat,
            Duree = duree,
            DureeJours = ParseDurationDays(duree),
            Etat = StatutAssuranceCodes.Elabore,
            GarantieId = ParseOptionalGuid(garantie),
            Partenaire = assureur,
            Intermediaire = intermediaire,
            TypePartenaire = typePartenaire,
            OCRE = NormalizeOptional(dossier.OCRE) ?? organisationCode ?? string.Empty,
            PCRE = dossier.PCRE ?? string.Empty,
            ModeDeTransport = modeDeTransport,
            CreerPar = user,
            ModifierPar = user,
            CreerLe = now,
            ModifierLe = now
        };

        var createdAssurance = await _assuranceRepository.CreateAsync(assurance);
        var voyage = await _transportDetailsRepository.AddVoyageAsync(new Voyage
        {
            AssuranceId = createdAssurance.Id,
            NomTransporteur = NormalizeOptional(request.NomTransporteur) ?? string.Empty,
            LieuSejour = NormalizeOptional(request.LieuSejour),
            DureeSejour = NormalizeOptional(request.DureeSejour),
            PaysProvenance = commande.PaysProvenance ?? string.Empty,
            PaysDestination = commande.PaysDestination ?? string.Empty,
            Designation = NormalizeOptional(ligne?.Designation) ?? commande.Intitule ?? string.Empty,
            Nature = NormalizeOptional(request.Nature),
            Specificites = NormalizeOptional(request.Specificites),
            Conditionnement = NormalizeOptional(request.Conditionnement) ?? NormalizeOptional(ligne?.Colisage),
            DescriptionConditionnement = NormalizeOptional(request.DescriptionConditionnement),
            Devise = commande.Devise,
            MasseBrute = FormatDecimal(commande.MasseBrute ?? ligne?.MasseBrute ?? commande.MasseNette ?? ligne?.MasseNette),
            UniteStatistique = NormalizeOptional(request.UniteStatistique) ?? NormalizeOptional(ligne?.UniteStatistique),
            Marque = NormalizeOptional(request.Marque) ?? NormalizeOptional(ligne?.Marque),
            CreerPar = user,
            ModifierPar = user,
            CreerLe = now,
            ModifierLe = now
        }, cancellationToken);

        await AddTransportDetailsAsync(modeDeTransport, voyage.Id, commande, request, cancellationToken);
        await _assuranceRepository.SaveChangesAsync();
        await CreateInitialPrimeAsync(createdAssurance.Id, createdAssurance.GarantieId, commande, ligne, user);

        var saved = await _assuranceRepository.GetByIdAsync(createdAssurance.Id);
        return (saved ?? createdAssurance).ToDetailDto();
    }

    private async Task AddTransportDetailsAsync(
        string modeDeTransport,
        Guid voyageId,
        DeclarationCommandeSource commande,
        CreateAssuranceFromDossierCommand request,
        CancellationToken cancellationToken)
    {
        switch (modeDeTransport)
        {
            case ModeDeTransportCodes.Maritime:
                var maritimeEmbarquementCode = NormalizeCode(commande.PortEmbarquement);
                var maritimeDebarquementCode = NormalizeCode(commande.PortDebarquement);
                await _transportDetailsRepository.AddMaritimeAsync(new Maritime
                {
                    VoyageId = voyageId,
                    PortEmbarquementCode = maritimeEmbarquementCode,
                    PortDebarquementCode = maritimeDebarquementCode,
                    NumeroBL = NormalizeOptional(request.NumeroBL),
                    NomNavire = NormalizeOptional(request.NomNavire),
                    TypeNavire = NormalizeOptional(request.TypeNavire)
                }, cancellationToken);
                break;

            case ModeDeTransportCodes.Aerien:
                await _transportDetailsRepository.AddAerienAsync(new Aerien
                {
                    VoyageId = voyageId,
                    AeroportEmbarquementCode = NormalizeCode(commande.AeroportEmbarquement),
                    AeroportDebarquementCode = NormalizeCode(commande.AeroportDebarquement),
                    NumeroLTA = NormalizeOptional(request.NumeroLTA)
                }, cancellationToken);
                break;

            case ModeDeTransportCodes.Routier:
                await _transportDetailsRepository.AddRoutierAsync(new Routier
                {
                    VoyageId = voyageId,
                    RouteNationaleCode = NormalizeCode(commande.RouteNationale),
                    NumeroLV = NormalizeOptional(request.NumeroLV)
                }, cancellationToken);
                break;

            case ModeDeTransportCodes.Fluvial:
                var fluvialEmbarquementCode = NormalizeCode(commande.FleuveEmbarquement ?? commande.PortEmbarquement);
                var fluvialDebarquementCode = NormalizeCode(commande.FleuveDebarquement ?? commande.PortDebarquement);
                await _transportDetailsRepository.AddFluvialAsync(new Fluvial
                {
                    VoyageId = voyageId,
                    PortEmbarquementCode = fluvialEmbarquementCode,
                    PortDebarquementCode = fluvialDebarquementCode,
                    NomNavire = NormalizeOptional(request.NomNavire),
                    TypeNavire = NormalizeOptional(request.TypeNavire)
                }, cancellationToken);
                break;
        }
    }

    private static string? NormalizeCode(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim().ToUpperInvariant();
    }

    private void EnsureCurrentOrganisationCanCreateAssurance(DeclarationDossierSource dossier)
    {
        var organisationCode = NormalizeCode(_currentUser.OrganisationCode);
        if (organisationCode is null)
            throw new InvalidOperationException("Organisation connectee introuvable. Impossible de creer l'assurance.");

        var isImportateur = string.Equals(NormalizeCode(dossier.OCRE), organisationCode, StringComparison.OrdinalIgnoreCase);
        var isTransitaire = string.Equals(NormalizeCode(dossier.Transitaire), organisationCode, StringComparison.OrdinalIgnoreCase);

        if (!isImportateur && !isTransitaire)
            throw new InvalidOperationException("Seul l'importateur ou le transitaire de la declaration peut creer l'assurance.");
    }

    private static string? FormatDecimal(decimal? value)
    {
        return value?.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private async Task CreateInitialPrimeAsync(
        Guid assuranceId,
        Guid? garantieId,
        DeclarationCommandeSource commande,
        DeclarationLigneCommandeSource? ligne,
        string user)
    {
        var valeurDevise = commande.ValeurDevise ?? ligne?.ValeurDevise;
        var valeurFCFA = commande.ValeurFCFA ?? ligne?.ValeurFCFA;

        if (!valeurDevise.HasValue && !valeurFCFA.HasValue)
            return;

        var prime = new Prime
        {
            AssuranceId = assuranceId,
            ValeurDevise = valeurDevise ?? 0m,
            ValeurFCFA = valeurFCFA ?? 0m,
            Statut = "16",
            CreerPar = user,
            ModifierPar = user,
            CreerLe = DateTime.UtcNow,
            ModifierLe = DateTime.UtcNow
        };

        var devise = NormalizeOptional(commande.Devise);
        if (garantieId.HasValue && valeurDevise.HasValue && devise is not null)
        {
            var calcul = await _primeCalculatorService.CalculerPrimeAsync(new PrimeCalculationRequest
            {
                AssuranceId = assuranceId,
                GarantieId = garantieId.Value,
                ValeurDevise = valeurDevise.Value,
                ValeurFCFA = valeurFCFA,
                Devise = devise
            });

            prime.ValeurFCFA = calcul.ValeurFCFA;
            prime.Taux = calcul.Taux;
            prime.Accessoires = (double)calcul.Accessoires;
            prime.Taxe = calcul.Taxe;
            prime.PrimeNette = calcul.PrimeNette;
            prime.PrimeTotale = calcul.PrimeTotale;
        }

        await _primeRepository.CreateAsync(prime);
    }

    private static string NormalizeRequired(string? value, string label)
    {
        var normalized = NormalizeOptional(value);
        if (normalized is null)
            throw new InvalidOperationException($"{label} requis avant creation AFI.");

        return normalized;
    }

    private static string NormalizeTypePartenaire(string? value)
    {
        var normalized = NormalizeOptional(value)?.ToUpperInvariant();
        return normalized is TypePartenaireCodes.Courtier or TypePartenaireCodes.AgentGeneral
            ? normalized
            : TypePartenaireCodes.Assureur;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static Guid? ParseOptionalGuid(string? value)
    {
        return Guid.TryParse(value, out var id) ? id : null;
    }

    private static int? ParseDurationDays(string? value)
    {
        return int.TryParse(value?.Trim(), out var days) && days > 0 ? days : null;
    }
}
