using System.Globalization;
using AssuranceService.Application.Avenants.Commands;
using AssuranceService.Application.Avenants.DTOs;
using AssuranceService.Application.Common;
using AssuranceService.Domain.Constants;
using AssuranceService.Domain.Models;
using AssuranceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssuranceService.Infrastructure.Services;

public class AvenantRegistrationService : IAvenantRegistrationService
{
    private static readonly string[] StatutsAutorisesAvenant =
    {
        StatutAssuranceCodes.Validé,
        StatutAssuranceCodes.Payé,
        StatutAssuranceCodes.Approuvé
    };

    private readonly AssuranceDbContext _context;
    private readonly IPrimeCalculatorService _primeCalculator;
    private readonly ICurrentUserService _currentUser;

    public AvenantRegistrationService(
        AssuranceDbContext context,
        IPrimeCalculatorService primeCalculator,
        ICurrentUserService currentUser)
    {
        _context = context;
        _primeCalculator = primeCalculator;
        _currentUser = currentUser;
    }

    public async Task<EnregistrerAvenantResponse> EnregistrerAsync(EnregistrerAvenantCommand cmd, CancellationToken cancellationToken = default)
    {
        await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var assurance = await _context.Assurances
                .Include(a => a.Primes)
                .FirstOrDefaultAsync(a => a.Id == cmd.AssuranceId, cancellationToken);

            if (assurance == null)
                throw new InvalidOperationException($"Assurance {cmd.AssuranceId} introuvable.");

            if (!StatutsAutorisesAvenant.Contains(assurance.Statut))
                throw new InvalidOperationException(
                    $"Un avenant n'est autorisé que pour une police validée / payée / approuvée (13, 16, 17). Statut actuel : {assurance.Statut}.");

            var avenantId = Guid.NewGuid();
            var historiques = new List<Historique>();
            var now = DateTime.UtcNow;
            var needPrimeRecalc = false;

            if (cmd.Assurance != null)
                needPrimeRecalc |= ApplyAssurancePatch(assurance, cmd.Assurance, historiques);

            if (needPrimeRecalc)
                await RecalculerEtTracerPrimeAsync(assurance, historiques, now, cancellationToken);

            if (historiques.Count == 0)
                throw new InvalidOperationException("Aucune modification détectée. Précisez au moins un champ à corriger.");

            var countAvenants = await _context.Avenants.CountAsync(a => a.AssuranceId == assurance.Id, cancellationToken);
            var noAvenant = $"{assurance.NoPolice ?? "POL"}-AV{countAvenants + 1:D3}";

            var avenant = new Avenant
            {
                Id = avenantId,
                AssuranceId = assurance.Id,
                NoPolice = assurance.NoPolice ?? string.Empty,
                NoAvenant = noAvenant,
                Statut = StatutAssuranceCodes.Elaboré,
                Motif = cmd.Motif,
                CreerLe = now,
                ModifierLe = now,
                CreerPar = _currentUser.UserName,
                ModifierPar = _currentUser.UserName
            };

            foreach (var h in historiques)
            {
                h.Id = Guid.NewGuid();
                h.AssuranceId = assurance.Id;
                h.AvenantId = avenantId;
                h.CreerLe = now;
                h.CreerPar = _currentUser.UserName;
                h.ModifierPar = _currentUser.UserName;
                h.ModifierLe = now;
            }

            _context.Avenants.Add(avenant);
            await _context.Historiques.AddRangeAsync(historiques, cancellationToken);
            assurance.ModifierPar = _currentUser.UserName;
            assurance.ModifierLe = now;

            await _context.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            return new EnregistrerAvenantResponse(avenantId, noAvenant, historiques.Count);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static bool ApplyAssurancePatch(
        Assurance assurance,
        AvenantAssurancePatch p,
        List<Historique> historiques)
    {
        var needPrime = false;

        void Track(string nom, string? avant, string? apres)
        {
            if (avant == apres) return;
            historiques.Add(new Historique
            {
                CibleEntite = HistoriqueCibles.Assurance,
                ReferenceId = assurance.Id,
                NomChamp = nom,
                ValeurAvant = avant,
                ValeurApres = apres
            });
        }

        if (p.ImportateurNom != null && p.ImportateurNom != assurance.ImportateurNom)
        {
            Track(nameof(assurance.ImportateurNom), assurance.ImportateurNom, p.ImportateurNom);
            assurance.ImportateurNom = p.ImportateurNom;
        }

        if (p.ImportateurNIU != null && p.ImportateurNIU != assurance.ImportateurNIU)
        {
            Track(nameof(assurance.ImportateurNIU), assurance.ImportateurNIU, p.ImportateurNIU);
            assurance.ImportateurNIU = p.ImportateurNIU;
        }

        if (p.NoPolice != null && p.NoPolice != assurance.NoPolice)
        {
            Track(nameof(assurance.NoPolice), assurance.NoPolice, p.NoPolice);
            assurance.NoPolice = p.NoPolice;
        }

        if (p.NumeroCert != null && p.NumeroCert != assurance.NumeroCert)
        {
            Track(nameof(assurance.NumeroCert), assurance.NumeroCert, p.NumeroCert);
            assurance.NumeroCert = p.NumeroCert;
        }

        if (p.DateDebut.HasValue && p.DateDebut != assurance.DateDebut)
        {
            Track(nameof(assurance.DateDebut), F(assurance.DateDebut), F(p.DateDebut));
            assurance.DateDebut = p.DateDebut;
        }

        if (p.DateFin.HasValue && p.DateFin != assurance.DateFin)
        {
            Track(nameof(assurance.DateFin), F(assurance.DateFin), F(p.DateFin));
            assurance.DateFin = p.DateFin;
        }

        if (p.TypeContrat != null && p.TypeContrat != assurance.TypeContrat)
        {
            Track(nameof(assurance.TypeContrat), assurance.TypeContrat, p.TypeContrat);
            assurance.TypeContrat = p.TypeContrat;
        }

        if (p.Duree != null && p.Duree != assurance.Duree)
        {
            Track(nameof(assurance.Duree), assurance.Duree, p.Duree);
            assurance.Duree = p.Duree;
        }

        if (p.Module != null && p.Module != assurance.Module)
        {
            Track(nameof(assurance.Module), assurance.Module, p.Module);
            assurance.Module = p.Module;
        }

        if (p.GarantieId.HasValue && p.GarantieId != assurance.GarantieId)
        {
            Track(nameof(assurance.GarantieId), F(assurance.GarantieId), F(p.GarantieId));
            assurance.GarantieId = p.GarantieId;
            needPrime = true;
        }

        return needPrime;
    }

    private async Task RecalculerEtTracerPrimeAsync(
        Assurance assurance,
        List<Historique> historiques,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (!assurance.GarantieId.HasValue)
            throw new InvalidOperationException("GarantieId requis pour recalculer la prime.");

        var valeurTotale = assurance.Primes.OrderByDescending(p => p.CreerLe).FirstOrDefault()?.ValeurDevise ?? 0m;
        var devise = assurance.Devise;
        if (string.IsNullOrWhiteSpace(devise))
            throw new InvalidOperationException("Aucune marchandise n'a de devise renseignée pour le recalcul de prime.");

        var prime = assurance.Primes.OrderByDescending(p => p.CreerLe).FirstOrDefault()
                    ?? throw new InvalidOperationException("Aucune prime trouvée pour cette assurance ; impossible de recalculer.");

        var avant = new
        {
            prime.Taux,
            prime.ValeurFCFA,
            prime.ValeurDevise,
            prime.PrimeNette,
            prime.Accessoires,
            prime.Taxe,
            prime.PrimeTotale
        };

        var calc = await _primeCalculator.CalculerPrimeAsync(new PrimeCalculationRequest
        {
            AssuranceId = assurance.Id,
            GarantieId = assurance.GarantieId.Value,
            ValeurDevise = valeurTotale,
            Devise = devise
        });

        prime.Taux = calc.Taux;
        prime.ValeurFCFA = calc.ValeurFCFA;
        prime.ValeurDevise = valeurTotale;
        prime.PrimeNette = calc.PrimeNette;
        prime.Accessoires = (double)calc.Accessoires;
        prime.Taxe = calc.Taxe;
        prime.PrimeTotale = calc.PrimeTotale;
        prime.ModifierLe = now;
        prime.ModifierPar = _currentUser.UserName;

        void TrackP(string nom, string? a, string? b)
        {
            if (a == b) return;
            historiques.Add(new Historique
            {
                CibleEntite = HistoriqueCibles.Prime,
                ReferenceId = prime.Id,
                NomChamp = nom,
                ValeurAvant = a,
                ValeurApres = b
            });
        }

        TrackP(nameof(prime.Taux), F(avant.Taux), F(prime.Taux));
        TrackP(nameof(prime.ValeurFCFA), F(avant.ValeurFCFA), F(prime.ValeurFCFA));
        TrackP(nameof(prime.ValeurDevise), F(avant.ValeurDevise), F(prime.ValeurDevise));
        TrackP(nameof(prime.PrimeNette), F(avant.PrimeNette), F(prime.PrimeNette));
        TrackP(nameof(prime.Accessoires), F(avant.Accessoires), F(prime.Accessoires));
        TrackP(nameof(prime.Taxe), F(avant.Taxe), F(prime.Taxe));
        TrackP(nameof(prime.PrimeTotale), F(avant.PrimeTotale), F(prime.PrimeTotale));
    }

    private static string? F(object? o) => o switch
    {
        null => null,
        DateTime dt => dt.ToString("o", CultureInfo.InvariantCulture),
        decimal d => d.ToString(CultureInfo.InvariantCulture),
        double d => d.ToString(CultureInfo.InvariantCulture),
        float f => f.ToString(CultureInfo.InvariantCulture),
        Guid g => g.ToString(),
        _ => o.ToString()
    };
}
