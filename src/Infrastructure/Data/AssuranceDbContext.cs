using Microsoft.EntityFrameworkCore;
using AssuranceService.Application.Common;
using AssuranceService.Domain.Models;
using AssuranceService.Domain.Models.Commons;
using AssuranceService.Domain.Models.Referentiel;
using AssuranceService.Application.Sagas;
using AssuranceService.Domain.Constants;

namespace AssuranceService.Infrastructure.Data;

public class AssuranceDbContext : DbContext
{
    private readonly ICurrentUserService _currentUser;
    // ModÃ¨les d'assurance
    public DbSet<Assurance> Assurances => Set<Assurance>();
    public DbSet<AssuranceDeclaration> AssuranceDeclarations => Set<AssuranceDeclaration>();
    public DbSet<AssuranceLigne> AssuranceLignes => Set<AssuranceLigne>();
    public DbSet<Voyage> Voyages => Set<Voyage>();
    public DbSet<Prime> Primes => Set<Prime>();
    public DbSet<Garantie> Garanties => Set<Garantie>();
    public DbSet<Cotation> Cotations => Set<Cotation>();
    public DbSet<VisaAssurance> VisaAssurances => Set<VisaAssurance>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Avenant> Avenants => Set<Avenant>();
    public DbSet<Historique> Historiques => Set<Historique>();
    public DbSet<Commentaire> Commentaires => Set<Commentaire>();

    // Types de transport liÃ©s au Voyage (1:1 Assurance â†’ Voyage â†’ transport)
    public DbSet<Aerien> Aeriens => Set<Aerien>();
    public DbSet<Maritime> Maritimes => Set<Maritime>();
    public DbSet<Routier> Routiers => Set<Routier>();
    public DbSet<Fluvial> Fluviaux => Set<Fluvial>();

    // SAGA State
    public DbSet<AssuranceProcessState> AssuranceProcessStates => Set<AssuranceProcessState>();

    // RÃ©fÃ©rentiel (mÃªme structure que DeclarationImportationService)
    public DbSet<Pays> Pays => Set<Pays>();
    public DbSet<Departement> Departements => Set<Departement>();
    public DbSet<Devise> Devises => Set<Devise>();
    public DbSet<Etat> Etats => Set<Etat>();
    public DbSet<TypeTransport> TypeTransports => Set<TypeTransport>();
    public DbSet<TypePartenaire> TypePartenaires => Set<TypePartenaire>();
    public DbSet<Statut> Statuts => Set<Statut>();
    public DbSet<Aeroport> Aeroports => Set<Aeroport>();
    public DbSet<Port> Ports => Set<Port>();
    public DbSet<Corridor> Corridors => Set<Corridor>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<Troncon> Troncons => Set<Troncon>();
    public DbSet<TauxDeChange> TauxDeChanges => Set<TauxDeChange>();
    public DbSet<UniteStatistique> UniteStatistiques => Set<UniteStatistique>();
    public DbSet<Specificite> Specificites => Set<Specificite>();
    public DbSet<RouteNationale> RoutesNationales => Set<RouteNationale>();

    public AssuranceDbContext(
        DbContextOptions<AssuranceDbContext> options,
        ICurrentUserService currentUser) : base(options)
    {
        _currentUser = currentUser;
    }

    public override int SaveChanges()
    {
        ApplyAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditFields()
    {
        var userName = _currentUser.UserName;
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseModel>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.CreerLe == default)
                    {
                        entry.Entity.CreerLe = now;
                    }

                    entry.Entity.CreerPar = userName;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifierLe = now;
                    entry.Entity.ModifierPar = userName;
                    break;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuration des modÃ¨les d'assurance
        ConfigureAssurance(modelBuilder);
        ConfigureAssuranceDeclaration(modelBuilder);
        ConfigureAssuranceLigne(modelBuilder);
        ConfigureVoyage(modelBuilder);
        ConfigurePrime(modelBuilder);
        ConfigureGarantie(modelBuilder);
        ConfigureCotation(modelBuilder);
        ConfigureAvenant(modelBuilder);
        ConfigureHistorique(modelBuilder);
        ConfigureCommentaire(modelBuilder);

        // Configuration des types de transport
        ConfigureAerien(modelBuilder);
        ConfigureMaritime(modelBuilder);
        ConfigureRoutier(modelBuilder);
        ConfigureFluvial(modelBuilder);

        modelBuilder.Entity<Etat>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => x.CodeEcran).IsUnique();
            e.Property(x => x.Libelle).HasMaxLength(150).IsRequired();
            e.Property(x => x.CodeEcran).HasMaxLength(20).IsRequired();
            e.Property(x => x.UsageUI).HasMaxLength(250);
        });

        // RÃ©fÃ©rentiels
        ConfigureTypePartenaire(modelBuilder);
        ConfigurePort(modelBuilder);
        ConfigureTauxDeChange(modelBuilder);
        ConfigureSpecificite(modelBuilder);

        // Configuration VisaAssurance
        ConfigureVisaAssurance(modelBuilder);
        ConfigureDocument(modelBuilder);

        // Configuration SAGA
        ConfigureSaga(modelBuilder);

    }

    private void ConfigureAssurance(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Assurance>(entity =>
        {
            entity.ToTable("Assurances");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.NumeroAFI).IsRequired().HasMaxLength(50);
            entity.HasIndex(a => a.NumeroAFI).IsUnique();
            entity.Property(a => a.NoPolice).HasMaxLength(255);
            entity.HasIndex(a => a.NoPolice)
                  .IsUnique()
                  .HasFilter("[NoPolice] IS NOT NULL");
            entity.Property(a => a.NumeroCert).HasMaxLength(25);
            entity.HasIndex(a => a.NumeroCert)
                  .IsUnique()
                  .HasFilter("[NumeroCert] IS NOT NULL");
            entity.Property(a => a.NoFacture).HasMaxLength(250);
            entity.Property(a => a.Partenaire).HasMaxLength(250);
            entity.Property(a => a.Intermediaire).HasMaxLength(250);
            entity.Property(a => a.TypePartenaire).HasMaxLength(50).HasDefaultValue(TypePartenaireCodes.Assureur);
            entity.Property(a => a.ImportateurNom).IsRequired().HasMaxLength(250);
            entity.Property(a => a.ImportateurNIU).HasMaxLength(25);
            entity.Property(a => a.TypeContrat).IsRequired().HasMaxLength(25);
            entity.Property(a => a.Duree).HasMaxLength(25);
            entity.Property(a => a.DureeJours);
            entity.Property(a => a.Etat).HasColumnName("Statut").HasMaxLength(10).HasDefaultValue("42");
            entity.Property(a => a.OCRE).IsRequired().HasMaxLength(250);
            entity.Property(a => a.PCRE).IsRequired().HasMaxLength(250);
            entity.Property(a => a.ModeDeTransport).IsRequired().HasMaxLength(10);
            entity.HasIndex(a => a.GarantieId);

            entity.HasOne(a => a.Garantie)
                  .WithMany()
                  .HasForeignKey(a => a.GarantieId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Relations
            entity.HasMany(a => a.Primes)
                  .WithOne()
                  .HasForeignKey(p => p.AssuranceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureVoyage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Voyage>(entity =>
        {
            entity.ToTable("Voyages");
            entity.HasKey(v => v.Id);
            entity.Property(v => v.NomTransporteur).IsRequired().HasMaxLength(255);
            entity.Property(v => v.Transit).HasMaxLength(350);
            entity.Property(v => v.PaysProvenance).IsRequired().HasMaxLength(255);
            entity.Property(v => v.PaysDestination).IsRequired().HasMaxLength(255);
            entity.Property(v => v.Designation).HasMaxLength(255);
            entity.Property(v => v.Nature).HasMaxLength(500);
            entity.Property(v => v.Specificites).HasMaxLength(100);
            entity.Property(v => v.Conditionnement).HasMaxLength(500);
            entity.Property(v => v.DescriptionConditionnement).HasMaxLength(500);
            entity.Property(v => v.Devise).HasMaxLength(50);
            entity.Property(v => v.MasseBrute).HasMaxLength(255);
            entity.Property(v => v.UniteStatistique).HasMaxLength(255);
            entity.Property(v => v.Marque).HasMaxLength(255);

            entity.HasOne(v => v.Assurance)
                  .WithOne(a => a.Voyage)
                  .HasForeignKey<Voyage>(v => v.AssuranceId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(v => v.AssuranceId).IsUnique();
        });
    }

    private void ConfigureAssuranceDeclaration(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssuranceDeclaration>(entity =>
        {
            entity.ToTable("AssuranceDeclarations");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.NumeroDI).HasMaxLength(100);
            entity.Property(item => item.PaysEmbarquementCode).IsRequired().HasMaxLength(50);
            entity.Property(item => item.PortEmbarquementCode).IsRequired().HasMaxLength(50);
            entity.HasIndex(item => new { item.AssuranceId, item.DeclarationId }).IsUnique();
            entity.HasIndex(item => item.DeclarationId);

            entity.HasOne(item => item.Assurance)
                .WithMany(assurance => assurance.Declarations)
                .HasForeignKey(item => item.AssuranceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureAssuranceLigne(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssuranceLigne>(entity =>
        {
            entity.ToTable("AssuranceLignes");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.PositionTarifaire).HasMaxLength(11);
            entity.Property(item => item.Designation).IsRequired().HasMaxLength(250);
            entity.Property(item => item.Marque).HasMaxLength(250);
            entity.Property(item => item.Colisage).HasMaxLength(250);
            entity.Property(item => item.Devise).HasMaxLength(20);
            entity.Property(item => item.UniteStatistique).HasMaxLength(50);
            entity.Property(item => item.PaysOrigine).HasMaxLength(50);
            entity.Property(item => item.Quantite).HasPrecision(18, 5);
            entity.Property(item => item.MasseBrute).HasPrecision(18, 5);
            entity.Property(item => item.MasseNette).HasPrecision(18, 5);
            entity.Property(item => item.Volume).HasPrecision(18, 5);
            entity.Property(item => item.PrixUnitaire).HasPrecision(18, 5);
            entity.Property(item => item.ValeurDevise).HasPrecision(18, 5);
            entity.Property(item => item.ValeurXAF).HasPrecision(18, 5);
            entity.HasIndex(item => item.SourceLigneDIId);
            entity.HasIndex(item => new { item.AssuranceId, item.SourceLigneDIId }).IsUnique();

            entity.HasOne(item => item.Assurance)
                .WithMany(assurance => assurance.Lignes)
                .HasForeignKey(item => item.AssuranceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.AssuranceDeclaration)
                .WithMany(declaration => declaration.Lignes)
                .HasForeignKey(item => item.AssuranceDeclarationId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }

    private void ConfigurePrime(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Prime>(entity =>
        {
            entity.ToTable("Primes");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Taux).HasPrecision(18, 4);
            entity.Property(p => p.ValeurFCFA).HasColumnType("decimal(18,2)");
            entity.Property(p => p.ValeurDevise).HasColumnType("decimal(18,2)");
            entity.Property(p => p.PrimeNette).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Accessoires).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Taxe).HasColumnType("decimal(18,2)");
            entity.Property(p => p.PrimeTotale).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Statut).IsRequired();
        });
    }

    private void ConfigureGarantie(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Garantie>(entity =>
        {
            entity.ToTable("Garanties");
            entity.HasKey(g => g.Id);
            entity.Property(g => g.Nom).HasColumnName("NomGarantie").IsRequired().HasMaxLength(255);
            entity.Property(g => g.Taux).HasPrecision(18, 4);
            entity.Property(g => g.Accessoires).HasColumnType("decimal(18,2)");
        });
    }

    private void ConfigureTypePartenaire(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TypePartenaire>(entity =>
        {
            entity.ToTable("TypePartenaires");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Code).IsRequired().HasMaxLength(50);
            entity.Property(t => t.Libelle).IsRequired().HasMaxLength(150);
            entity.Property(t => t.Description).HasMaxLength(500);
            entity.HasIndex(t => t.Code).IsUnique();
        });
    }

    private void ConfigureCotation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cotation>(entity =>
        {
            entity.ToTable("Cotations");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Montant).HasColumnType("decimal(18,2)");
            entity.Property(c => c.Taux).HasMaxLength(255);
            entity.Property(c => c.PrimeNette).HasColumnType("decimal(18,2)");
            entity.Property(c => c.Accessoires).HasColumnType("decimal(18,2)");
            entity.Property(c => c.Taxe).HasColumnType("decimal(18,2)");
            entity.Property(c => c.PrimeTotale).HasColumnType("decimal(18,2)");
        });
    }

    private void ConfigureAvenant(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Avenant>(entity =>
        {
            entity.ToTable("Avenants");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.NoPolice).IsRequired().HasMaxLength(255);
            entity.Property(a => a.NoAvenant).IsRequired().HasMaxLength(255);
            entity.Property(a => a.Statut).IsRequired().HasMaxLength(10);
            entity.Property(a => a.Motif).HasColumnType("nvarchar(max)");
            entity.Property(a => a.Type).IsRequired().HasMaxLength(50);

            entity.HasOne(a => a.Assurance)
                  .WithMany()
                  .HasForeignKey(a => a.AssuranceId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(a => new { a.AssuranceId, a.NoAvenant }).IsUnique();
        });
    }

    private void ConfigureHistorique(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Historique>(entity =>
        {
            entity.ToTable("Historiques");
            entity.HasKey(h => h.Id);
            entity.Property(h => h.CibleEntite).IsRequired().HasMaxLength(30);
            entity.Property(h => h.NomChamp).IsRequired().HasMaxLength(200);
            entity.Property(h => h.ValeurAvant).HasColumnType("nvarchar(max)");
            entity.Property(h => h.ValeurApres).HasColumnType("nvarchar(max)");
            entity.Property(h => h.Commentaire).HasColumnType("nvarchar(max)");

            entity.HasOne(h => h.Avenant)
                  .WithMany(a => a.Historiques)
                  .HasForeignKey(h => h.AvenantId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(h => h.AssuranceId);
            entity.HasIndex(h => h.AvenantId);
        });
    }

    private void ConfigureCommentaire(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Commentaire>(entity =>
        {
            entity.ToTable("Commentaires");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.DocumentId).IsRequired();
            entity.Property(c => c.Motif).IsRequired().HasColumnType("nvarchar(max)");
        });
    }

    private void ConfigureAerien(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Aerien>(entity =>
        {
            entity.ToTable("Aeriens");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.AeroportEmbarquementCode).HasMaxLength(50);
            entity.Property(a => a.AeroportDebarquementCode).HasMaxLength(50);
            entity.Property(a => a.NumeroLTA).HasMaxLength(255);

            entity.HasOne(a => a.Voyage)
                  .WithOne(v => v.Aerien)
                  .HasForeignKey<Aerien>(a => a.VoyageId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(a => a.VoyageId).IsUnique();
        });
    }

    private void ConfigureMaritime(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Maritime>(entity =>
        {
            entity.ToTable("Maritimes");
            entity.HasKey(m => m.Id);
            entity.Property(m => m.PortEmbarquementCode).HasMaxLength(50);
            entity.Property(m => m.PortDebarquementCode).HasMaxLength(50);
            entity.Property(m => m.NumeroBL).HasMaxLength(255);
            entity.Property(m => m.NomNavire).HasMaxLength(255);
            entity.Property(m => m.TypeNavire).HasMaxLength(100);

            entity.HasOne(m => m.Voyage)
                  .WithOne(v => v.Maritime)
                  .HasForeignKey<Maritime>(m => m.VoyageId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(m => m.VoyageId).IsUnique();
        });
    }

    private void ConfigureRoutier(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Routier>(entity =>
        {
            entity.ToTable("Routiers");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.RouteNationaleCode).HasMaxLength(50);
            entity.Property(r => r.NumeroLV).HasMaxLength(255);

            entity.HasOne(r => r.Voyage)
                  .WithOne(v => v.Routier)
                  .HasForeignKey<Routier>(r => r.VoyageId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(r => r.VoyageId).IsUnique();
        });
    }

    private void ConfigureFluvial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Fluvial>(entity =>
        {
            entity.ToTable("Fluviaux");
            entity.HasKey(f => f.Id);
            entity.Property(f => f.PortEmbarquementCode).HasMaxLength(50);
            entity.Property(f => f.PortDebarquementCode).HasMaxLength(50);
            entity.Property(f => f.NomNavire).HasMaxLength(255);
            entity.Property(f => f.TypeNavire).HasMaxLength(100);

            entity.HasOne(f => f.Voyage)
                  .WithOne(v => v.Fluvial)
                  .HasForeignKey<Fluvial>(f => f.VoyageId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(f => f.VoyageId).IsUnique();
        });
    }

    private void ConfigurePort(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Port>(entity =>
        {
            entity.ToTable("Ports");
            entity.Property(p => p.Module).IsRequired().HasMaxLength(50);
        });
    }

    private void ConfigureTauxDeChange(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TauxDeChange>(entity =>
        {
            entity.ToTable("TauxDeChanges");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Source).IsRequired().HasMaxLength(100);
            entity.Property(t => t.Taux).HasPrecision(20, 5);
            entity.Property(t => t.ValideDe).HasMaxLength(50);
            entity.HasOne(t => t.Devise)
                  .WithMany(d => d.TauxDeChanges)
                  .HasForeignKey(t => t.DeviseId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureSpecificite(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Specificite>(entity =>
        {
            entity.ToTable("Specificites");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Nom).IsRequired().HasMaxLength(255);
        });
    }

    private void ConfigureVisaAssurance(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VisaAssurance>(entity =>
        {
            entity.ToTable("VisaAssurances");
            entity.HasKey(v => v.Id);
            entity.Property(v => v.TypePartenaire).IsRequired().HasMaxLength(50);
            entity.Property(v => v.Organisation).IsRequired().HasMaxLength(250);
            entity.Property(v => v.VisaContent).HasColumnType("nvarchar(max)");
            entity.Property(v => v.Message).HasMaxLength(1000);
            entity.Property(v => v.Licence).HasMaxLength(250);
            entity.Property(v => v.DateVisa).HasColumnType("datetime2(0)");
            entity.Property(v => v.VisaOK).IsRequired();
            entity.Property(v => v.Statut).IsRequired().HasMaxLength(50);
            entity.HasOne(v => v.Assurance)
                  .WithMany(a => a.Visas)
                  .HasForeignKey(v => v.AssuranceId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(v => new { v.AssuranceId, v.TypePartenaire, v.Organisation }).IsUnique();
        });
    }

    private void ConfigureDocument(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("Documents");
            entity.HasKey(d => d.Id);
            entity.Property(d => d.TypeDocument).IsRequired().HasMaxLength(50).HasDefaultValue("PIECE_ASSURANCE");
            entity.Property(d => d.Description).IsRequired().HasMaxLength(255);
            entity.Property(d => d.DocumentUrl).IsRequired().HasMaxLength(500);
            entity.Property(d => d.ContentType).HasMaxLength(100);
            entity.Property(d => d.Taille);
            entity.HasOne(d => d.Assurance)
                  .WithMany(a => a.Documents)
                  .HasForeignKey(d => d.AssuranceId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(d => d.AssuranceId);
        });
    }

    private void ConfigureSaga(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssuranceProcessState>(entity =>
        {
            entity.ToTable("AssuranceProcessStates");
            entity.HasKey(x => x.CorrelationId);
            entity.Property(x => x.CurrentState).HasMaxLength(64);
            entity.Property(x => x.AssuranceId);
            entity.Property(x => x.NoPolice).HasMaxLength(255);
            entity.Property(x => x.TypeContrat).HasMaxLength(25);
            entity.Property(x => x.Importateur).HasMaxLength(250);
            entity.Property(x => x.StartedAt);
            entity.Property(x => x.CompletedAt);
            entity.Property(x => x.ErrorMessage).HasMaxLength(1000);
            entity.Property(x => x.ErrorCode).HasMaxLength(50);
            entity.Property(x => x.AssuranceCreated);
            entity.Property(x => x.MarchandisesAdded);
            entity.Property(x => x.PrimeCalculated);
            entity.Property(x => x.GarantiesAssigned);
            entity.Property(x => x.ProcessCompleted);
            entity.Property(x => x.RetryCount);
            entity.Property(x => x.LastRetryAt);
        });
    }
}
