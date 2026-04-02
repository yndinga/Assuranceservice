using AssuranceService.Domain.Models;

namespace AssuranceService.Application.DTOs;

/// <summary>
/// Extensions pour mapper les entités vers les DTOs
/// </summary>
public static class MappingExtensions
{
    #region Assurance Mappings
    
    public static AssuranceDto ToDto(this Assurance assurance)
    {
        var latestPrime = assurance.Primes?
            .OrderByDescending(p => p.CreerLe)
            .FirstOrDefault();

        return new AssuranceDto
        {
            ID = assurance.Id,
            NoPolice = assurance.NoPolice,
            NumeroCert = assurance.NumeroCert,
            ImportateurNom = assurance.ImportateurNom,
            ImportateurNIU = assurance.ImportateurNIU,
            DateDebut = assurance.DateDebut,
            DateFin = assurance.DateFin,
            TypeContrat = assurance.TypeContrat,
            Duree = assurance.Duree,
            Statut = assurance.Statut,
            Module = assurance.Module,
            GarantieId = assurance.GarantieId,
            GarantieNom = assurance.Garantie?.Nom,
            AssureurId = assurance.AssureurId,
            IntermediaireId = assurance.IntermediaireId,
            OCRE = assurance.OCRE,
            Designation = assurance.Designation,
            Nature = assurance.Nature,
            Specificites = assurance.Specificites,
            Conditionnement = assurance.Conditionnement,
            Description = assurance.Description,
            ValeurFCFA = latestPrime?.ValeurFCFA,
            ValeurDevise = latestPrime?.ValeurDevise,
            Devise = assurance.Devise,
            MasseBrute = assurance.MasseBrute,
            UniteStatistique = assurance.UniteStatistique,
            Marque = assurance.Marque,
            NomTransporteur = assurance.NomTransporteur,
            NomNavire = assurance.NomNavire,
            TypeNavire = assurance.TypeNavire,
            LieuSejour = assurance.LieuSejour,
            DureeSejour = assurance.DureeSejour,
            PaysProvenance = assurance.PaysProvenance,
            PaysDestination = assurance.PaysDestination,
            PortEmbarquement = assurance.Maritime?.PortEmbarquementId ?? assurance.Fluvial?.PortEmbarquementId,
            PortDebarquement = assurance.Maritime?.PortDebarquementId ?? assurance.Fluvial?.PortDebarquementId,
            AeroportEmbarquement = assurance.Aerien?.AeroportEmbarquement,
            AeroportDebarquement = assurance.Aerien?.AeroportDebarquement,
            RouteNationale = assurance.Routier?.RouteNationale,
            CreerPar = assurance.CreerPar,
            ModifierPar = assurance.ModifierPar,
            CreerLe = assurance.CreerLe,
            ModifierLe = assurance.ModifierLe
        };
    }
    
    public static AssuranceDetailDto ToDetailDto(this Assurance assurance)
    {
        var baseDto = assurance.ToDto();
        var latestPrime = assurance.Primes?
            .OrderByDescending(p => p.CreerLe)
            .FirstOrDefault();
        
        return new AssuranceDetailDto
        {
            ID = baseDto.ID,
            NoPolice = baseDto.NoPolice,
            NumeroCert = baseDto.NumeroCert,
            ImportateurNom = baseDto.ImportateurNom,
            ImportateurNIU = baseDto.ImportateurNIU,
            DateDebut = baseDto.DateDebut,
            DateFin = baseDto.DateFin,
            TypeContrat = baseDto.TypeContrat,
            Duree = baseDto.Duree,
            Statut = baseDto.Statut,
            Module = baseDto.Module,
            GarantieId = baseDto.GarantieId,
            GarantieNom = baseDto.GarantieNom,
            AssureurId = baseDto.AssureurId,
            IntermediaireId = baseDto.IntermediaireId,
            OCRE = baseDto.OCRE,
            CreerPar = baseDto.CreerPar,
            ModifierPar = baseDto.ModifierPar,
            CreerLe = baseDto.CreerLe,
            ModifierLe = baseDto.ModifierLe,
            Primes = assurance.Primes?.Select(p => p.ToDto()).ToList() ?? new List<PrimeDto>(),
            Visas = assurance.Visas?.Select(v => v.ToDto()).ToList() ?? new List<VisaAssuranceDto>()
        };
    }
    
    #endregion
    
    #region Prime Mappings
    
    public static PrimeDto ToDto(this Prime prime)
    {
        return new PrimeDto
        {
            ID = prime.Id,
            AssuranceId = prime.AssuranceId,
            Taux = prime.Taux,
            ValeurFCFA = prime.ValeurFCFA,
            ValeurDevise = prime.ValeurDevise,
            PrimeNette = prime.PrimeNette,
            Accessoires = prime.Accessoires,
            Taxe = prime.Taxe,
            PrimeTotale = prime.PrimeTotale,
            Statut = prime.Statut,
            CreerLe = prime.CreerLe,
            ModifierLe = prime.ModifierLe
        };
    }
    
    #endregion
    
    #region VisaAssurance Mappings
    
    public static VisaAssuranceDto ToDto(this VisaAssurance visa)
    {
        return new VisaAssuranceDto
        {
            ID = visa.Id,
            AssuranceId = visa.AssuranceId,
            OrganisationId = visa.OrganisationId,
            VisaOK = visa.VisaOK,
            VisaContent = visa.VisaContent,
            CreerLe = visa.CreerLe,
            ModifierLe = visa.ModifierLe
        };
    }
    
    #endregion
    
    #region Garantie Mappings
    
    public static GarantieDto ToDto(this Garantie garantie)
    {
        return new GarantieDto
        {
            ID = garantie.Id,
            NomGarantie = garantie.Nom,
            Taux = garantie.Taux,
            Accessoires = garantie.Accessoires,
            Actif = garantie.Actif,
            CreerLe = garantie.CreerLe,
            ModifierLe = garantie.ModifierLe
        };
    }
    
    #endregion
    
    #region List Mappings
    
    public static IEnumerable<AssuranceDto> ToDtoList(this IEnumerable<Assurance> assurances)
    {
        return assurances.Select(a => a.ToDto());
    }
    
    public static IEnumerable<AssuranceDetailDto> ToDetailDtoList(this IEnumerable<Assurance> assurances)
    {
        return assurances.Select(a => a.ToDetailDto());
    }
    
    public static IEnumerable<GarantieDto> ToDtoList(this IEnumerable<Garantie> garanties)
    {
        return garanties.Select(g => g.ToDto());
    }
    
    #endregion
}
