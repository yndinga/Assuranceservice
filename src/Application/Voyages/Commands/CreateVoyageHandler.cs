using AssuranceService.Application.Common;
using AssuranceService.Domain.Models;
using MediatR;

namespace AssuranceService.Application.Voyages.Commands;

public class CreateVoyageHandler : IRequestHandler<CreateVoyageCommand, Guid>
{
    private readonly IVoyageRepository _voyageRepository;
    private readonly IAssuranceRepository _assuranceRepository;
    private readonly IModuleRepository _moduleRepository;

    private static readonly HashSet<string> _codesTransport = new(StringComparer.OrdinalIgnoreCase) { "MA", "AE", "RO", "FL" };

    public CreateVoyageHandler(IVoyageRepository voyageRepository, IAssuranceRepository assuranceRepository, IModuleRepository moduleRepository)
    {
        _voyageRepository = voyageRepository;
        _assuranceRepository = assuranceRepository;
        _moduleRepository = moduleRepository;
    }

    public async Task<Guid> Handle(CreateVoyageCommand request, CancellationToken cancellationToken)
    {
        var assurance = await _assuranceRepository.GetByIdAsync(request.AssuranceId);
        if (assurance == null)
            throw new ArgumentException($"Assurance {request.AssuranceId} introuvable.");
        if (assurance.Voyage != null)
            throw new InvalidOperationException("Cette assurance a déjà un voyage. Une assurance = un voyage.");

        var module = await _moduleRepository.GetByIdAsync(request.ModuleId, cancellationToken);
        if (module == null)
            throw new ArgumentException($"Module {request.ModuleId} introuvable.");
        if (!_codesTransport.Contains(module.Code?.Trim() ?? string.Empty))
            throw new ArgumentException($"Module invalide pour un voyage. Code attendu : MA, AE, RO ou FL (reçu : {module.Code}).");

        var voyage = new Voyage
        {
            AssuranceId     = request.AssuranceId,
            ModuleId        = request.ModuleId,
            NomTransporteur = request.NomTransporteur,
            NomNavire       = request.NomNavire,
            TypeNavire      = request.TypeNavire,
            PaysProvenance  = request.PaysProvenance,
            PaysDestination = request.PaysDestination,
            CreerPar        = "System",
            ModifierPar     = "System",
            CreerLe         = DateTime.UtcNow
        };

        var created = await _voyageRepository.CreateAsync(voyage);

        var code = module.Code?.Trim().ToUpperInvariant() ?? string.Empty;

        // Créer la table transport enfant selon le module (MA, AE, RO, FL)
        switch (code)
        {
            case "MA":
                if (!request.PortEmbarquementId.HasValue || !request.PortDebarquementId.HasValue)
                    throw new ArgumentException("PortEmbarquementId et PortDebarquementId sont requis pour un transport Maritime.");
                await _voyageRepository.AddMaritimeAsync(new Maritime
                {
                    VoyageId           = created.Id,
                    PortEmbarquementId = request.PortEmbarquementId.Value,
                    PortDebarquementId = request.PortDebarquementId.Value
                });
                break;

            case "AE":
                await _voyageRepository.AddAerienAsync(new Aerien
                {
                    VoyageId              = created.Id,
                    AeroportEmbarquement  = request.AeroportEmbarquement ?? string.Empty,
                    AeroportDebarquement  = request.AeroportDebarquement
                });
                break;

            case "RO":
                await _voyageRepository.AddRoutierAsync(new Routier
                {
                    VoyageId       = created.Id,
                    RouteNationale = request.RouteNationale ?? string.Empty
                });
                break;

            case "FL":
                if (!request.PortEmbarquementId.HasValue)
                    throw new ArgumentException("PortEmbarquementId est requis pour un transport Fluvial.");
                await _voyageRepository.AddFluvialAsync(new Fluvial
                {
                    VoyageId           = created.Id,
                    PortEmbarquementId = request.PortEmbarquementId.Value,
                    PortDebarquementId = request.PortDebarquementId
                });
                break;
        }

        await _voyageRepository.SaveChangesAsync();
        return created.Id;
    }
}
