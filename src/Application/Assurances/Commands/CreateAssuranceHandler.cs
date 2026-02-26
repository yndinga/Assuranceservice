using AssuranceService.Application.Common;
using AssuranceService.Application.Services;
using AssuranceService.Domain.Models;
using AssuranceService.Domain.Events;
using MediatR;
using MassTransit;

namespace AssuranceService.Application.Assurances.Commands;

public class CreateAssuranceHandler : IRequestHandler<CreateAssuranceCommand, Guid>
{
    private readonly IAssuranceRepository _assuranceRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly INumeroGeneratorService _numeroGeneratorService;

    public CreateAssuranceHandler(
        IAssuranceRepository assuranceRepository,
        IPublishEndpoint publishEndpoint,
        INumeroGeneratorService numeroGeneratorService)
    {
        _assuranceRepository = assuranceRepository;
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
            Statut = "Elaborer",
            GarantieId = request.GarantieId,
            AssureurId = request.AssureurId,
            IntermediaireId = request.IntermediaireId,
            OCRE = request.OCRE ?? string.Empty,
            CreerPar = "System", // TODO: Récupérer l'utilisateur connecté
            ModifierPar = "System",
            CreerLe = DateTime.UtcNow
        };

        var createdAssurance = await _assuranceRepository.CreateAsync(assurance);

        // VisaAssurance : rempli uniquement lors de la signature (pas à la création)
        // Voyages : ajoutés via POST /voyages après la création de l'assurance

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
