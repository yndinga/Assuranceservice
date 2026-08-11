namespace AssuranceService.Domain.Constants;

/// <summary>Codes modes de transport GUOT : MA, AR, RO, FL.</summary>
public static class ModeDeTransportCodes
{
    public const string Maritime = "MA";
    public const string Aerien = "AR";
    public const string Routier = "RO";
    public const string Fluvial = "FL";

    private const string LegacyAerien = "AE";

    /// <summary>Normalise le code (trim, majuscules) et convertit l'ancien code AE vers AR.</summary>
    public static string Normalize(string? code)
    {
        var normalized = (code ?? string.Empty).Trim().ToUpperInvariant();
        return normalized == LegacyAerien ? Aerien : normalized;
    }
}
