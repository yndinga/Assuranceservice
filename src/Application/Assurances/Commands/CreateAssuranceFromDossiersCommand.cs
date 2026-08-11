using AssuranceService.Application.DTOs;
using MediatR;

namespace AssuranceService.Application.Assurances.Commands;

public sealed record AssuranceLigneSelection(
    Guid DossierId,
    Guid LigneDIId,
    decimal? Quantite,
    decimal? MasseBrute,
    decimal? MasseNette,
    decimal? Volume,
    decimal? ValeurDevise,
    decimal? ValeurXAF);

public sealed record CreateAssuranceFromDossiersCommand(
    IReadOnlyCollection<Guid> DossierIds,
    IReadOnlyCollection<AssuranceLigneSelection> Lignes,
    string? TypeContrat,
    string? Garantie,
    string? Assureur,
    string? Intermediaire,
    string? TypePartenaire,
    string? Duree,
    string? NomTransporteur,
    string? LieuSejour,
    string? DureeSejour,
    string? Nature,
    string? Specificites,
    string? Conditionnement,
    string? DescriptionConditionnement,
    string? Marque,
    string? UniteStatistique,
    string? NumeroBL,
    string? NomNavire,
    string? TypeNavire,
    string? NumeroLTA,
    string? NumeroLV) : IRequest<AssuranceDetailDto>;
