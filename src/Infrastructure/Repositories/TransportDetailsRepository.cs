using AssuranceService.Application.Common;
using AssuranceService.Domain.Models;
using AssuranceService.Infrastructure.Data;

namespace AssuranceService.Infrastructure.Repositories;

public class TransportDetailsRepository : ITransportDetailsRepository
{
    private readonly AssuranceDbContext _context;

    public TransportDetailsRepository(AssuranceDbContext context)
    {
        _context = context;
    }

    public Task AddMaritimeAsync(Maritime maritime, CancellationToken cancellationToken = default)
        => _context.Maritimes.AddAsync(maritime, cancellationToken).AsTask();

    public Task AddAerienAsync(Aerien aerien, CancellationToken cancellationToken = default)
        => _context.Aeriens.AddAsync(aerien, cancellationToken).AsTask();

    public Task AddRoutierAsync(Routier routier, CancellationToken cancellationToken = default)
        => _context.Routiers.AddAsync(routier, cancellationToken).AsTask();

    public Task AddFluvialAsync(Fluvial fluvial, CancellationToken cancellationToken = default)
        => _context.Fluviaux.AddAsync(fluvial, cancellationToken).AsTask();
}
