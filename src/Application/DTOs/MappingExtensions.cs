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
        var voyage = assurance.Voyage;

        return new AssuranceDto
        {
            ID = assurance.Id,
            NumeroAFI = assurance.NumeroAFI,
            NoPolice = assurance.NoPolice,
            NumeroCert = assurance.NumeroCert,
            NoFacture = assurance.NoFacture,
            ImportateurNom = assurance.ImportateurNom,
            ImportateurNIU = assurance.ImportateurNIU,
            DateDebut = assurance.DateDebut,
            DateFin = assurance.DateFin,
            TypeContrat = assurance.TypeContrat,
            Duree = assurance.DureeJours?.ToString() ?? assurance.Duree,
            DureeJours = assurance.DureeJours,
            Statut = assurance.Etat,
            ModeDeTransport = assurance.ModeDeTransport,
            GarantieId = assurance.GarantieId,
            AssureurId = assurance.Partenaire,
            IntermediaireId = assurance.Intermediaire,
            Garantie = assurance.Garantie?.Nom ?? assurance.GarantieId?.ToString() ?? string.Empty,
            Assureur = assurance.Partenaire ?? string.Empty,
            Intermediaire = assurance.Intermediaire ?? string.Empty,
            TypePartenaire = assurance.TypePartenaire,
            OCRE = assurance.OCRE,
            Designation = voyage?.Designation,
            Nature = voyage?.Nature,
            Specificites = voyage?.Specificites,
            Conditionnement = voyage?.Conditionnement,
            Description = voyage?.DescriptionConditionnement,
            ValeurFCFA = latestPrime?.ValeurFCFA,
            ValeurDevise = latestPrime?.ValeurDevise,
            ValeurXAFTotale = assurance.Lignes.Count > 0 ? assurance.ValeurXAFTotale : latestPrime?.ValeurFCFA ?? 0m,
            ValeurDeviseTotale = assurance.Lignes.Count > 0 ? assurance.ValeurDeviseTotale : latestPrime?.ValeurDevise ?? 0m,
            MasseBruteTotale = assurance.MasseBruteTotale,
            MasseNetteTotale = assurance.MasseNetteTotale,
            VolumeTotal = assurance.VolumeTotal,
            Devise = voyage?.Devise,
            MasseBrute = voyage?.MasseBrute,
            UniteStatistique = voyage?.UniteStatistique,
            Marque = voyage?.Marque,
            NomTransporteur = voyage?.NomTransporteur,
            NomNavire = voyage?.Maritime?.NomNavire ?? voyage?.Fluvial?.NomNavire,
            TypeNavire = voyage?.Maritime?.TypeNavire ?? voyage?.Fluvial?.TypeNavire,
            Transit = voyage?.Transit,
            PaysProvenance = voyage?.PaysProvenance,
            PaysDestination = voyage?.PaysDestination,
            PaysEmbarquementCode = assurance.Declarations.Select(item => item.PaysEmbarquementCode).FirstOrDefault(),
            PortEmbarquementCommunCode = assurance.Declarations.Select(item => item.PortEmbarquementCode).FirstOrDefault(),
            PortEmbarquement = null,
            PortDebarquement = null,
            PortEmbarquementCode = voyage?.Maritime?.PortEmbarquementCode ?? voyage?.Fluvial?.PortEmbarquementCode,
            PortDebarquementCode = voyage?.Maritime?.PortDebarquementCode ?? voyage?.Fluvial?.PortDebarquementCode,
            AeroportEmbarquement = voyage?.Aerien?.AeroportEmbarquementCode,
            AeroportDebarquement = voyage?.Aerien?.AeroportDebarquementCode,
            RouteNationale = voyage?.Routier?.RouteNationaleCode,
            NumeroBL = voyage?.Maritime?.NumeroBL,
            NumeroLTA = voyage?.Aerien?.NumeroLTA,
            NumeroLV = voyage?.Routier?.NumeroLV,
            CreerPar = assurance.CreerPar ?? string.Empty,
            ModifierPar = assurance.ModifierPar ?? string.Empty,
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
            NumeroAFI = baseDto.NumeroAFI,
            NoPolice = baseDto.NoPolice,
            NumeroCert = baseDto.NumeroCert,
            NoFacture = baseDto.NoFacture,
            ImportateurNom = baseDto.ImportateurNom,
            ImportateurNIU = baseDto.ImportateurNIU,
            DateDebut = baseDto.DateDebut,
            DateFin = baseDto.DateFin,
            TypeContrat = baseDto.TypeContrat,
            Duree = baseDto.Duree,
            DureeJours = baseDto.DureeJours,
            Statut = baseDto.Statut,
            ModeDeTransport = baseDto.ModeDeTransport,
            GarantieId = baseDto.GarantieId,
            AssureurId = baseDto.AssureurId,
            IntermediaireId = baseDto.IntermediaireId,
            Garantie = baseDto.Garantie,
            Assureur = baseDto.Assureur,
            Intermediaire = baseDto.Intermediaire,
            TypePartenaire = baseDto.TypePartenaire,
            OCRE = baseDto.OCRE,
            Designation = baseDto.Designation,
            Nature = baseDto.Nature,
            Specificites = baseDto.Specificites,
            Conditionnement = baseDto.Conditionnement,
            Description = baseDto.Description,
            ValeurFCFA = baseDto.ValeurFCFA,
            ValeurDevise = baseDto.ValeurDevise,
            ValeurXAFTotale = baseDto.ValeurXAFTotale,
            ValeurDeviseTotale = baseDto.ValeurDeviseTotale,
            MasseBruteTotale = baseDto.MasseBruteTotale,
            MasseNetteTotale = baseDto.MasseNetteTotale,
            VolumeTotal = baseDto.VolumeTotal,
            Devise = baseDto.Devise,
            MasseBrute = baseDto.MasseBrute,
            UniteStatistique = baseDto.UniteStatistique,
            Marque = baseDto.Marque,
            NomTransporteur = baseDto.NomTransporteur,
            NomNavire = baseDto.NomNavire,
            TypeNavire = baseDto.TypeNavire,
            Transit = baseDto.Transit,
            PaysProvenance = baseDto.PaysProvenance,
            PaysDestination = baseDto.PaysDestination,
            PaysEmbarquementCode = baseDto.PaysEmbarquementCode,
            PortEmbarquementCommunCode = baseDto.PortEmbarquementCommunCode,
            PortEmbarquement = baseDto.PortEmbarquement,
            PortDebarquement = baseDto.PortDebarquement,
            PortEmbarquementCode = baseDto.PortEmbarquementCode,
            PortDebarquementCode = baseDto.PortDebarquementCode,
            AeroportEmbarquement = baseDto.AeroportEmbarquement,
            AeroportDebarquement = baseDto.AeroportDebarquement,
            RouteNationale = baseDto.RouteNationale,
            NumeroBL = baseDto.NumeroBL,
            NumeroLTA = baseDto.NumeroLTA,
            NumeroLV = baseDto.NumeroLV,
            CreerPar = baseDto.CreerPar,
            ModifierPar = baseDto.ModifierPar,
            CreerLe = baseDto.CreerLe,
            ModifierLe = baseDto.ModifierLe,
            Primes = assurance.Primes?.Select(p => p.ToDto()).ToList() ?? new List<PrimeDto>(),
            Visas = assurance.Visas?.Select(v => v.ToDto()).ToList() ?? new List<VisaAssuranceDto>(),
            Declarations = assurance.Declarations?.Select(item => new AssuranceDeclarationDto
            {
                Id = item.Id,
                DeclarationId = item.DeclarationId,
                NumeroDI = item.NumeroDI,
                PaysEmbarquementCode = item.PaysEmbarquementCode,
                PortEmbarquementCode = item.PortEmbarquementCode
            }).ToList() ?? new List<AssuranceDeclarationDto>(),
            Lignes = assurance.Lignes?.OrderBy(item => item.NoLigne).Select(item => new AssuranceLigneDto
            {
                Id = item.Id,
                DeclarationId = item.DeclarationId,
                SourceCommandeId = item.SourceCommandeId,
                SourceLigneDIId = item.SourceLigneDIId,
                NoLigne = item.NoLigne,
                PositionTarifaire = item.PositionTarifaire,
                Designation = item.Designation,
                Marque = item.Marque,
                Colisage = item.Colisage,
                Quantite = item.Quantite,
                MasseBrute = item.MasseBrute,
                MasseNette = item.MasseNette,
                Volume = item.Volume,
                PrixUnitaire = item.PrixUnitaire,
                ValeurDevise = item.ValeurDevise,
                ValeurXAF = item.ValeurXAF,
                Devise = item.Devise,
                UniteStatistique = item.UniteStatistique,
                PaysOrigine = item.PaysOrigine,
                EstPartielle = item.EstPartielle
            }).ToList() ?? new List<AssuranceLigneDto>()
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
            TypePartenaire = visa.TypePartenaire,
            Organisation = visa.Organisation,
            VisaOK = visa.VisaOK,
            VisaContent = visa.VisaContent,
            Message = visa.Message,
            Licence = visa.Licence,
            DateVisa = visa.DateVisa,
            Statut = visa.Statut,
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
