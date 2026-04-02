namespace AssuranceService.Application.Avenants.DTOs;

public record AvenantListItemDto(
    Guid Id,
    string NoPolice,
    string NoAvenant,
    string Statut,
    string? Motif,
    DateTime CreerLe,
    int NombreHistoriques);

public record HistoriqueDto(
    Guid Id,
    Guid AvenantId,
    string CibleEntite,
    Guid? ReferenceId,
    string NomChamp,
    string? ValeurAvant,
    string? ValeurApres,
    string? Commentaire,
    DateTime CreerLe);

public record AvenantDetailDto(
    Guid Id,
    Guid AssuranceId,
    string NoPolice,
    string NoAvenant,
    string Statut,
    string? Motif,
    DateTime CreerLe,
    IReadOnlyList<HistoriqueDto> Historiques);

public record HistoriqueLigneListeDto(
    Guid Id,
    Guid AvenantId,
    string NoAvenant,
    string CibleEntite,
    Guid? ReferenceId,
    string NomChamp,
    string? ValeurAvant,
    string? ValeurApres,
    DateTime CreerLe);

public record EnregistrerAvenantResponse(Guid AvenantId, string NoAvenant, int LignesHistorique);
