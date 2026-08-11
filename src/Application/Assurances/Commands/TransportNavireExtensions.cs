using AssuranceService.Domain.Constants;
using AssuranceService.Domain.Models;

namespace AssuranceService.Application.Assurances.Commands;

internal static class TransportNavireExtensions
{
    internal static void ApplyNavireDetails(this Voyage? voyage, string modeDeTransport, string? nomNavire, string? typeNavire)
    {
        if (voyage == null)
        {
            return;
        }

        var nom = nomNavire?.Trim();
        var type = typeNavire?.Trim();

        if (modeDeTransport == ModeDeTransportCodes.Maritime && voyage.Maritime != null)
        {
            if (!string.IsNullOrEmpty(nom))
            {
                voyage.Maritime.NomNavire = nom;
            }

            if (!string.IsNullOrEmpty(type))
            {
                voyage.Maritime.TypeNavire = type;
            }

            return;
        }

        if (modeDeTransport == ModeDeTransportCodes.Fluvial && voyage.Fluvial != null)
        {
            if (!string.IsNullOrEmpty(nom))
            {
                voyage.Fluvial.NomNavire = nom;
            }

            if (!string.IsNullOrEmpty(type))
            {
                voyage.Fluvial.TypeNavire = type;
            }
        }
    }
}
