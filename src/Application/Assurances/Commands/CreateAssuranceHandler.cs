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

    public CreateAssuranceHandler(
        IAssuranceRepository assuranceRepository,
        ITransportDetailsRepository transportDetailsRepository,
        IPortRepository portRepository,
        IPrimeCalculatorService primeCalculatorService,
        IPrimeRepository primeRepository,
        IPublishEndpoint publishEndpoint,
        INumeroGeneratorService numeroGeneratorService)
    {
        _assuranceRepository = assuranceRepository;
        _transportDetailsRepository = transportDetailsRepository;
        _portRepository = portRepository;
        _primeCalculatorService = primeCalculatorService;
        _primeRepository = primeRepository;
        _publishEndpoint = publishEndpoint;
        _numeroGeneratorService = numeroGeneratorService;
    }

    public async Task<Guid> Handle(CreateAssuranceCommand request, CancellationToken cancellationToken)
    {
        // Génération du NumeroCert dès la création: {Compteur:D6}{JJMM} — ex: 0001200707
        var numeroCert = await _numeroGeneratorService.GenerateNumeroCertAsync();

        var assurance = new Assurance
        {
            NumeroCert = numeroCert,
            ImportateurNom = request.ImportateurNom,
            ImportateurNIU = request.ImportateurNIU ?? string.Empty,
            DateDebut = request.DateDebut,
            DateFin = request.DateFin,
            TypeContrat = request.TypeContrat,
            Duree = request.Duree,
            Statut = request.Statut ?? StatutAssuranceCodes.Elaboré,
            Module = request.Module,
            GarantieId = request.GarantieId,
            AssureurId = request.AssureurId,
            IntermediaireId = request.IntermediaireId,
            OCRE = request.OCRE ?? string.Empty,
            Designation = request.Designation,
            Nature = request.Nature,
            Specificites = request.Specificites,
            Conditionnement = request.Conditionnement,
            Description = request.Description,
            Devise = request.Devise,
            MasseBrute = request.MasseBrute,
            UniteStatistique = request.UniteStatistique,
            Marque = request.Marque,
            NomTransporteur = request.NomTransporteur,
            NomNavire = request.NomNavire,
            TypeNavire = request.TypeNavire,
            LieuSejour = request.LieuSejour,
            DureeSejour = request.DureeSejour,
            PaysProvenance = request.PaysProvenance,
            PaysDestination = request.PaysDestination,
            CreerPar = "System", // TODO: Récupérer l'utilisateur connecté
            ModifierPar = "System",
            CreerLe = DateTime.UtcNow
        };

        var createdAssurance = await _assuranceRepository.CreateAsync(assurance);

        var module = (request.Module ?? string.Empty).Trim().ToUpperInvariant();
        switch (module)
        {
            case "MA":
                if (!request.PortEmbarquement.HasValue || !request.PortDebarquement.HasValue)
                    throw new ArgumentException("PortEmbarquement et PortDebarquement sont requis pour le module MA.");
                if (!await _portRepository.ExistsAsync(request.PortEmbarquement.Value, cancellationToken))
                    throw new ArgumentException("Le PortEmbarquement n'existe pas.");
                if (!await _portRepository.ExistsAsync(request.PortDebarquement.Value, cancellationToken))
                    throw new ArgumentException("Le PortDebarquement n'existe pas.");
                await _transportDetailsRepository.AddMaritimeAsync(new Maritime
                {
                    AssuranceId = createdAssurance.Id,
                    PortEmbarquementId = request.PortEmbarquement.Value,
                    PortDebarquementId = request.PortDebarquement.Value
                }, cancellationToken);
                break;

            case "AE":
                await _transportDetailsRepository.AddAerienAsync(new Aerien
                {
                    AssuranceId = createdAssurance.Id,
                    AeroportEmbarquement = request.AeroportEmbarquement ?? string.Empty,
                    AeroportDebarquement = request.AeroportDebarquement
                }, cancellationToken);
                break;

            case "RO":
                await _transportDetailsRepository.AddRoutierAsync(new Routier
                {
                    AssuranceId = createdAssurance.Id,
                    RouteNationale = request.RouteNationale ?? string.Empty
                }, cancellationToken);
                break;

            case "FL":
                if (!request.PortEmbarquement.HasValue)
                    throw new ArgumentException("PortEmbarquement est requis pour le module FL.");
                if (!await _portRepository.ExistsAsync(request.PortEmbarquement.Value, cancellationToken))
                    throw new ArgumentException("Le PortEmbarquement n'existe pas.");
                if (request.PortDebarquement.HasValue &&
                    !await _portRepository.ExistsAsync(request.PortDebarquement.Value, cancellationToken))
                    throw new ArgumentException("Le PortDebarquement n'existe pas.");
                await _transportDetailsRepository.AddFluvialAsync(new Fluvial
                {
                    AssuranceId = createdAssurance.Id,
                    PortEmbarquementId = request.PortEmbarquement.Value,
                    PortDebarquementId = request.PortDebarquement
                }, cancellationToken);
                break;
        }

        // Calcul de prime au moment de la création si les données sont disponibles
        var valeurDevise = request.ValeurDevise ?? 0m;
        var devise = string.IsNullOrWhiteSpace(request.Devise) ? "XOF" : request.Devise!;
        if (request.GarantieId.HasValue && valeurDevise > 0)
        {
            var calc = await _primeCalculatorService.CalculerPrimeAsync(new PrimeCalculationRequest
            {
                AssuranceId = createdAssurance.Id,
                GarantieId = request.GarantieId.Value,
                ValeurDevise = valeurDevise,
                Devise = devise
            });

            var existingPrime = (await _primeRepository.GetByAssuranceIdAsync(createdAssurance.Id)).FirstOrDefault();
            if (existingPrime != null)
            {
                existingPrime.Taux = calc.Taux;
                existingPrime.ValeurFCFA = calc.ValeurFCFA;
                existingPrime.ValeurDevise = valeurDevise;
                existingPrime.PrimeNette = calc.PrimeNette;
                existingPrime.Accessoires = (double)calc.Accessoires;
                existingPrime.Taxe = calc.Taxe;
                existingPrime.PrimeTotale = calc.PrimeTotale;
                existingPrime.Statut = "Calculée";
                existingPrime.ModifierLe = DateTime.UtcNow;
                await _primeRepository.UpdateAsync(existingPrime);
            }
            else
            {
                await _primeRepository.CreateAsync(new Prime
                {
                    AssuranceId = createdAssurance.Id,
                    Taux = calc.Taux,
                    ValeurFCFA = calc.ValeurFCFA,
                    ValeurDevise = valeurDevise,
                    PrimeNette = calc.PrimeNette,
                    Accessoires = (double)calc.Accessoires,
                    Taxe = calc.Taxe,
                    PrimeTotale = calc.PrimeTotale,
                    Statut = "Calculée",
                    CreerPar = "System",
                    ModifierPar = "System",
                    CreerLe = DateTime.UtcNow
                });
            }
        }

        // VisaAssurance : rempli uniquement lors de la signature (pas à la création)

        // Sauvegarder
        await _assuranceRepository.SaveChangesAsync();

        // Publier l'événement pour démarrer le processus SAGA
        await _publishEndpoint.Publish(new AssuranceProcessStartedEvent
        {
            AssuranceId = createdAssurance.Id,
            NoPolice = createdAssurance.NoPolice ?? string.Empty,
            TypeContrat = createdAssurance.TypeContrat,
            StartedAt = DateTime.UtcNow
        }, cancellationToken);

        return createdAssurance.Id;
    }
}
