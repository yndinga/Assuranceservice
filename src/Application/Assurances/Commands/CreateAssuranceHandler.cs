using AssuranceService.Application.Common;
using AssuranceService.Application.Services;
using AssuranceService.Domain.Constants;
using AssuranceService.Domain.Models;
using AssuranceService.Domain.Events;
using MediatR;
using MassTransit;

namespace AssuranceService.Application.Assurances.Commands;

public class CreateAssuranceHandler : IRequestHandler<CreateAssuranceCommand, Guid>
{
    private readonly IAssuranceRepository _assuranceRepository;
    private readonly ITransportDetailsRepository _transportDetailsRepository;
    private readonly IPortRepository _portRepository;
    private readonly IPrimeCalculatorService _primeCalculatorService;
    private readonly IPrimeRepository _primeRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly INumeroGeneratorService _numeroGeneratorService;
    private readonly ICurrentUserService _currentUser;

    public CreateAssuranceHandler(
        IAssuranceRepository assuranceRepository,
        ITransportDetailsRepository transportDetailsRepository,
        IPortRepository portRepository,
        IPrimeCalculatorService primeCalculatorService,
        IPrimeRepository primeRepository,
        IPublishEndpoint publishEndpoint,
        INumeroGeneratorService numeroGeneratorService,
        ICurrentUserService currentUser)
    {
        _assuranceRepository = assuranceRepository;
        _transportDetailsRepository = transportDetailsRepository;
        _portRepository = portRepository;
        _primeCalculatorService = primeCalculatorService;
        _primeRepository = primeRepository;
        _publishEndpoint = publishEndpoint;
        _numeroGeneratorService = numeroGeneratorService;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateAssuranceCommand request, CancellationToken cancellationToken)
    {
        var modeDeTransport = ModeDeTransportCodes.Normalize(request.ModeDeTransport);
        var typePartenaire = NormalizeTypePartenaire(request.TypePartenaire);
        var partenaire = NormalizeOptional(request.Assureur);
        var intermediaire = NormalizeOptional(request.Intermediaire);

        var assurance = new Assurance
        {
            ImportateurNom = request.ImportateurNom,
            ImportateurNIU = request.ImportateurNIU ?? string.Empty,
            TypeContrat = request.TypeContrat,
            Duree = request.Duree,
            DureeJours = ParseDurationDays(request.Duree),
            Etat = request.Statut ?? StatutAssuranceCodes.Elaboré,
            GarantieId = ParseOptionalGuid(request.Garantie),
            Partenaire = partenaire,
            Intermediaire = intermediaire,
            TypePartenaire = typePartenaire,
            OCRE = NormalizeOptional(request.OCRE) ?? NormalizeOptional(_currentUser.OrganisationCode) ?? string.Empty,
            ModeDeTransport = modeDeTransport,
            CreerPar = "System",
            ModifierPar = "System",
            CreerLe = DateTime.UtcNow
        };

        var createdAssurance = await _assuranceRepository.CreateAsync(assurance);

        var voyage = await _transportDetailsRepository.AddVoyageAsync(new Voyage
        {
            AssuranceId = createdAssurance.Id,
            NomTransporteur = request.NomTransporteur ?? string.Empty,
            LieuSejour = request.LieuSejour,
            DureeSejour = request.DureeSejour,
            PaysProvenance = request.PaysProvenance ?? string.Empty,
            PaysDestination = request.PaysDestination ?? string.Empty,
            Designation = request.Designation ?? string.Empty,
            Nature = request.Nature,
            Specificites = request.Specificites,
            Conditionnement = request.Conditionnement,
            DescriptionConditionnement = request.Description,
            Devise = request.Devise,
            MasseBrute = request.MasseBrute,
            UniteStatistique = request.UniteStatistique,
            Marque = request.Marque,
            CreerPar = "System",
            ModifierPar = "System",
            CreerLe = DateTime.UtcNow
        }, cancellationToken);

        switch (modeDeTransport)
        {
            case ModeDeTransportCodes.Maritime:
                await _transportDetailsRepository.AddMaritimeAsync(new Maritime
                {
                    VoyageId = voyage.Id,
                    PortEmbarquementCode = NormalizeCode(request.PortEmbarquement),
                    PortDebarquementCode = NormalizeCode(request.PortDebarquement),
                    NumeroBL = request.NumeroBL?.Trim(),
                    NomNavire = request.NomNavire?.Trim(),
                    TypeNavire = request.TypeNavire?.Trim()
                }, cancellationToken);
                break;

            case ModeDeTransportCodes.Aerien:
                await _transportDetailsRepository.AddAerienAsync(new Aerien
                {
                    VoyageId = voyage.Id,
                    AeroportEmbarquementCode = NormalizeCode(request.AeroportEmbarquement),
                    AeroportDebarquementCode = NormalizeCode(request.AeroportDebarquement),
                    NumeroLTA = request.NumeroLTA?.Trim()
                }, cancellationToken);
                break;

            case ModeDeTransportCodes.Routier:
                await _transportDetailsRepository.AddRoutierAsync(new Routier
                {
                    VoyageId = voyage.Id,
                    RouteNationaleCode = NormalizeCode(request.RouteNationale),
                    NumeroLV = request.NumeroLV?.Trim()
                }, cancellationToken);
                break;

            case ModeDeTransportCodes.Fluvial:
                await _transportDetailsRepository.AddFluvialAsync(new Fluvial
                {
                    VoyageId = voyage.Id,
                    PortEmbarquementCode = NormalizeCode(request.PortEmbarquement),
                    PortDebarquementCode = NormalizeCode(request.PortDebarquement),
                    NomNavire = request.NomNavire?.Trim(),
                    TypeNavire = request.TypeNavire?.Trim()
                }, cancellationToken);
                break;
        }

        // La prime reste creee via l'endpoint /primes tant que Garantie est stockee en texte metier.

        await _assuranceRepository.SaveChangesAsync();
        await CreateInitialPrimeAsync(createdAssurance.Id, createdAssurance.GarantieId, request);

        await _publishEndpoint.Publish(new AssuranceProcessStartedEvent
        {
            AssuranceId = createdAssurance.Id,
            NoPolice = createdAssurance.NoPolice ?? string.Empty,
            TypeContrat = createdAssurance.TypeContrat,
            StartedAt = DateTime.UtcNow
        }, cancellationToken);

        return createdAssurance.Id;
    }

    private async Task CreateInitialPrimeAsync(Guid assuranceId, Guid? garantieId, CreateAssuranceCommand request)
    {
        if (!request.ValeurDevise.HasValue && !request.ValeurFCFA.HasValue)
            return;

        var prime = new Prime
        {
            AssuranceId = assuranceId,
            ValeurDevise = request.ValeurDevise ?? 0m,
            ValeurFCFA = request.ValeurFCFA ?? 0m,
            Statut = "16",
            CreerPar = "System",
            ModifierPar = "System",
            CreerLe = DateTime.UtcNow,
            ModifierLe = DateTime.UtcNow
        };

        var devise = NormalizeOptional(request.Devise);
        if (garantieId.HasValue && request.ValeurDevise.HasValue && devise is not null)
        {
            var calcul = await _primeCalculatorService.CalculerPrimeAsync(new PrimeCalculationRequest
            {
                AssuranceId = assuranceId,
                GarantieId = garantieId.Value,
                ValeurDevise = request.ValeurDevise.Value,
                ValeurFCFA = request.ValeurFCFA,
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

    private static Guid? ParseOptionalGuid(string? value)
    {
        return Guid.TryParse(value, out var id) ? id : null;
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


    private static string? NormalizeCode(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();
    }

    private static int? ParseDurationDays(string? value)
    {
        return int.TryParse(value?.Trim(), out var days) && days > 0 ? days : null;
    }
}
