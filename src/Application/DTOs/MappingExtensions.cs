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
            GarantieId = assurance.GarantieId,
            GarantieNom = assurance.Garantie?.Nom,
            AssureurId = assurance.AssureurId,
            IntermediaireId = assurance.IntermediaireId,
            OCRE = assurance.OCRE,
            CreerPar = assurance.CreerPar,
            ModifierPar = assurance.ModifierPar,
            CreerLe = assurance.CreerLe,
            ModifierLe = assurance.ModifierLe
        };
    }
    
    public static AssuranceDetailDto ToDetailDto(this Assurance assurance)
    {
        var baseDto = assurance.ToDto();
        
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
            GarantieId = baseDto.GarantieId,
            GarantieNom = baseDto.GarantieNom,
            AssureurId = baseDto.AssureurId,
            IntermediaireId = baseDto.IntermediaireId,
            OCRE = baseDto.OCRE,
            CreerPar = baseDto.CreerPar,
            ModifierPar = baseDto.ModifierPar,
            CreerLe = baseDto.CreerLe,
            ModifierLe = baseDto.ModifierLe,
            Marchandises = assurance.Marchandises?.Select(m => m.ToDto()).ToList() ?? new List<MarchandiseDto>(),
            Primes = assurance.Primes?.Select(p => p.ToDto()).ToList() ?? new List<PrimeDto>(),
            Voyages = assurance.Voyage != null ? new List<VoyageDto> { assurance.Voyage.ToDto() } : new List<VoyageDto>(),
            Visas = assurance.Visas?.Select(v => v.ToDto()).ToList() ?? new List<VisaAssuranceDto>()
        };
    }
    
    #endregion
    
    #region Marchandise Mappings
    
    public static MarchandiseDto ToDto(this Marchandise marchandise)
    {
        return new MarchandiseDto
        {
            ID = marchandise.Id,
            AssuranceId = marchandise.AssuranceId,
            Designation = marchandise.Designation,
            Nature = marchandise.Nature,
            Specificites = marchandise.Specificites,
            Conditionnement = marchandise.Conditionnement,
            Description = marchandise.Description,
            ValeurFCFA = marchandise.ValeurFCFA,
            ValeurDevise = marchandise.ValeurDevise,
            Devise = marchandise.Devise,
            MasseBrute = marchandise.MasseBrute,
            UniteStatistique = marchandise.UniteStatistique,
            Marque = marchandise.Marque,
            CreerLe = marchandise.CreerLe,
            ModifierLe = marchandise.ModifierLe
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
    
    #region Voyage Mappings
    
    public static VoyageDto ToDto(this Voyage voyage)
    {
        return new VoyageDto
        {
            ID              = voyage.Id,
            AssuranceId     = voyage.AssuranceId,
            ModuleId        = voyage.ModuleId,
            ModuleCode      = voyage.Module?.Code,
            NomTransporteur = voyage.NomTransporteur,
            NomNavire       = voyage.NomNavire,
            TypeNavire      = voyage.TypeNavire,
            PaysProvenance  = voyage.PaysProvenance,
            PaysDestination = voyage.PaysDestination,
            Maritime = voyage.Maritime is null ? null : new MaritimeDto
            {
                PortEmbarquementId   = voyage.Maritime.PortEmbarquementId,
                PortEmbarquementNom  = voyage.Maritime.PortEmbarquement?.Nom,
                PortDebarquementId   = voyage.Maritime.PortDebarquementId,
                PortDebarquementNom  = voyage.Maritime.PortDebarquement?.Nom
            },
            Aerien = voyage.Aerien is null ? null : new AerienDto
            {
                AeroportEmbarquement = voyage.Aerien.AeroportEmbarquement,
                AeroportDebarquement = voyage.Aerien.AeroportDebarquement
            },
            Routier = voyage.Routier is null ? null : new RoutierDto
            {
                RouteNationale = voyage.Routier.RouteNationale
            },
            Fluvial = voyage.Fluvial is null ? null : new FluvialDto
            {
                PortEmbarquementId   = voyage.Fluvial.PortEmbarquementId,
                PortEmbarquementNom  = voyage.Fluvial.PortEmbarquement?.Nom,
                PortDebarquementId   = voyage.Fluvial.PortDebarquementId,
                PortDebarquementNom  = voyage.Fluvial.PortDebarquement?.Nom
            },
            CreerLe    = voyage.CreerLe,
            ModifierLe = voyage.ModifierLe
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
