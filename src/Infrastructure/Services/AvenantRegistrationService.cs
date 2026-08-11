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
        StatutAssuranceCodes.Valide,
        StatutAssuranceCodes.Paye,
        StatutAssuranceCodes.Approuve
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
                .Include(a => a.Voyage)
                .FirstOrDefaultAsync(a => a.Id == cmd.AssuranceId, cancellationToken);

            if (assurance == null)
                throw new InvalidOperationException($"Assurance {cmd.AssuranceId} introuvable.");

            if (!StatutAssuranceCodes.IsValide(assurance.Etat))
                throw new InvalidOperationException(
                    $"Un avenant n'est autorise que pour une police validee (50). Statut actuel : {assurance.Etat}.");

            var avenantId = Guid.NewGuid();
            var historiques = new List<Historique>();
            var now = DateTime.UtcNow;
            var needPrimeRecalc = false;

            if (cmd.Assurance != null)
                needPrimeRecalc |= ApplyAssurancePatch(assurance, cmd.Assurance, historiques);

            if (needPrimeRecalc)
                await RecalculerEtTracerPrimeAsync(assurance, historiques, now, cancellationToken);

            if (historiques.Count == 0)
                throw new InvalidOperationException("Aucune modification dÃ©tectÃ©e. PrÃ©cisez au moins un champ Ã  corriger.");

            var countAvenants = await _context.Avenants.CountAsync(a => a.AssuranceId == assurance.Id, cancellationToken);
            var noAvenant = $"{assurance.NoPolice ?? "POL"}-AV{countAvenants + 1:D3}";

            var avenant = new Avenant
            {
                Id = avenantId,
                AssuranceId = assurance.Id,
                NoPolice = assurance.NoPolice ?? string.Empty,
                NoAvenant = noAvenant,
                Type = cmd.Type.Trim().ToUpperInvariant(),
                Statut = StatutAssuranceCodes.Elabore,
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

        if (p.ModeDeTransport != null)
        {
            var modeDeTransport = ModeDeTransportCodes.Normalize(p.ModeDeTransport);
            if (modeDeTransport != assurance.ModeDeTransport)
            {
                Track(nameof(assurance.ModeDeTransport), assurance.ModeDeTransport, modeDeTransport);
                assurance.ModeDeTransport = modeDeTransport;
            }

            assurance.Voyage ??= new Voyage
            {
                AssuranceId = assurance.Id,
                NomTransporteur = string.Empty,
                PaysProvenance = string.Empty,
                PaysDestination = string.Empty,
                CreerLe = DateTime.UtcNow
            };
        }

        if (Guid.TryParse(p.Garantie, out var garantieId) && garantieId != assurance.GarantieId)
        {
            Track(nameof(assurance.GarantieId), assurance.GarantieId?.ToString(), garantieId.ToString());
            assurance.GarantieId = garantieId;
        }

        return needPrime;
    }

    private Task RecalculerEtTracerPrimeAsync(
        Assurance assurance,
        List<Historique> historiques,
        DateTime now,
        CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Le recalcul automatique de prime par avenant requiert une garantie technique. La garantie est actuellement stockee en texte metier.");
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


