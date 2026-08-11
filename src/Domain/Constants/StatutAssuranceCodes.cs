using System;
using System.Linq;

namespace AssuranceService.Domain.Constants;

/// <summary>
/// Codes statuts harmonises avec le referentiel officiel des etats.
/// Les constantes Old* permettent de lire les anciennes donnees sans casser le circuit existant.
/// </summary>
public static class StatutAssuranceCodes
{
    public const string Elabore = "42";
    public const string VisaDemande = "79";
    public const string ModificationSoumise = "68";
    public const string Valide = "50";
    public const string ModificationDemandee = "66";
    public const string Refuse = "80";
    public const string RefuseSecondaire = "55";
    public const string AvenantEnCours = "58";

    public const string OldElabore = "10";
    public const string OldVisaDemande = "11";
    public const string OldModificationSoumise = "12";
    public const string OldValide = "13";
    public const string OldModificationDemandee = "14";
    public const string Paye = "16";
    public const string Approuve = "40";

    public const string Elaboré = Elabore;
    public const string VisaDemandé = VisaDemande;
    public const string Validé = Valide;
    public const string ModificationDemandée = ModificationDemandee;
    public const string Payé = Paye;
    public const string Approuvé = Approuve;

    public static bool IsElabore(string? value) => Is(value, Elabore, OldElabore);
    public static bool IsVisaDemande(string? value) => Is(value, VisaDemande, OldVisaDemande);
    public static bool IsModificationSoumise(string? value) => Is(value, ModificationSoumise, OldModificationSoumise);
    public static bool IsValide(string? value) => Is(value, Valide, OldValide);
    public static bool IsModificationDemandee(string? value) => Is(value, ModificationDemandee, OldModificationDemandee);
    public static bool IsRefuse(string? value) => Is(value, Refuse, RefuseSecondaire);

    public static string NormalizeDecision(string? value)
    {
        var decision = Normalize(value);
        return decision switch
        {
            OldValide => Valide,
            OldModificationDemandee => ModificationDemandee,
            "VALIDER" => Valide,
            "MODIFICATION_DEMANDEE" => ModificationDemandee,
            _ => decision
        };
    }

    private static bool Is(string? value, params string[] expected)
    {
        var normalized = Normalize(value);
        return expected.Any(item => string.Equals(normalized, item, StringComparison.OrdinalIgnoreCase));
    }

    private static string Normalize(string? value) => value?.Trim().ToUpperInvariant() ?? string.Empty;
}


