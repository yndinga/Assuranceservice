namespace AssuranceService.Application.Common;

public sealed record DeclarationInvoiceSelection(
    Guid DossierId,
    string? NumeroDI,
    IReadOnlyCollection<Guid> CommandeIds);

public interface IDeclarationInvoiceImporter
{
    Task<int> ImportAsync(
        Guid assuranceId,
        IReadOnlyCollection<DeclarationInvoiceSelection> selections,
        string user,
        CancellationToken cancellationToken = default);
}
