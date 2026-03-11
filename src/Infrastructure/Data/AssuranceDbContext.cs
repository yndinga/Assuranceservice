using AssuranceService.Domain.Models;
using AssuranceService.Domain.Models.Referentiel;
using Microsoft.EntityFrameworkCore;
using AssuranceService.Application.Sagas;

namespace AssuranceService.Infrastructure.Data;

public class AssuranceDbContext : DbContext
{
    // Modèles d'assurance
    public DbSet<Assurance> Assurances => Set<Assurance>();
    public DbSet<Marchandise> Marchandises => Set<Marchandise>();
    public DbSet<Prime> Primes => Set<Prime>();
    public DbSet<Garantie> Garanties => Set<Garantie>();
    public DbSet<Cotation> Cotations => Set<Cotation>();
    public DbSet<Voyage> Voyages => Set<Voyage>();
    public DbSet<VisaAssurance> VisaAssurances => Set<VisaAssurance>();
    public DbSet<Document> Documents => Set<Document>();

    // Types de transport (anciens modèles - à migrer vers Voyage)
    public DbSet<Aerien> Aeriens => Set<Aerien>();
    public DbSet<Maritime> Maritimes => Set<Maritime>();
    public DbSet<Routier> Routiers => Set<Routier>();
    public DbSet<Fluvial> Fluviaux => Set<Fluvial>();

    // SAGA State
    public DbSet<AssuranceProcessState> AssuranceProcessStates => Set<AssuranceProcessState>();

    // Référentiel (même structure que DeclarationImportationService)
    public DbSet<Pays> Pays => Set<Pays>();
    public DbSet<Departement> Departements => Set<Departement>();
    public DbSet<Devise> Devises => Set<Devise>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Statut> Statuts => Set<Statut>();
    public DbSet<Aeroport> Aeroports => Set<Aeroport>();
    public DbSet<Port> Ports => Set<Port>();
    public DbSet<Corridor> Corridors => Set<Corridor>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<Troncon> Troncons => Set<Troncon>();
    public DbSet<TauxDeChange> TauxDeChanges => Set<TauxDeChange>();
    public DbSet<UniteStatistique> UniteStatistiques => Set<UniteStatistique>();
    public DbSet<Specificite> Specificites => Set<Specificite>();
    public DbSet<TypeTransport> TypeTransports => Set<TypeTransport>();

    public AssuranceDbContext(DbContextOptions<AssuranceDbContext> options) : base(options) 
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuration des modèles d'assurance
        ConfigureAssurance(modelBuilder);
        ConfigureMarchandise(modelBuilder);
        ConfigurePrime(modelBuilder);
        ConfigureGarantie(modelBuilder);
        ConfigureCotation(modelBuilder);

        // Configuration des types de transport
        ConfigureAerien(modelBuilder);
        ConfigureMaritime(modelBuilder);
        ConfigureRoutier(modelBuilder);
        ConfigureFluvial(modelBuilder);

        // Référentiels
        ConfigurePort(modelBuilder);
        ConfigureTauxDeChange(modelBuilder);
        ConfigureSpecificite(modelBuilder);
        ConfigureTypeTransport(modelBuilder);

        // Configuration Voyage et VisaAssurance
        ConfigureVoyage(modelBuilder);
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
            entity.Property(a => a.NoPolice).HasMaxLength(255);
            entity.HasIndex(a => a.NoPolice)
                  .IsUnique()
                  .HasFilter("[NoPolice] IS NOT NULL");
            entity.Property(a => a.NumeroCert).HasMaxLength(25);
            entity.HasIndex(a => a.NumeroCert)
                  .IsUnique()
                  .HasFilter("[NumeroCert] IS NOT NULL");
            entity.Property(a => a.ImportateurNom).IsRequired().HasMaxLength(250);
            entity.Property(a => a.ImportateurNIU).HasMaxLength(25);
            entity.Property(a => a.TypeContrat).IsRequired().HasMaxLength(25);
            entity.Property(a => a.Duree).HasMaxLength(25);
            entity.Property(a => a.Statut).HasMaxLength(10).HasDefaultValue("10");
            entity.Property(a => a.Module).IsRequired().HasMaxLength(250);
            entity.HasOne(a => a.Garantie)
                  .WithMany()
                  .HasForeignKey(a => a.GarantieId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(a => a.OCRE).IsRequired().HasMaxLength(250);

            // Relations
            entity.HasMany(a => a.Marchandises)
                  .WithOne(m => m.Assurance)
                  .HasForeignKey(m => m.AssuranceId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(a => a.Primes)
                  .WithOne()
                  .HasForeignKey(p => p.AssuranceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureMarchandise(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Marchandise>(entity =>
        {
            entity.ToTable("Marchandises");
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Designation).IsRequired().HasMaxLength(255);
            entity.Property(m => m.Nature).HasMaxLength(500);
            entity.Property(m => m.Specificites).HasMaxLength(100);
            entity.Property(m => m.Conditionnement).IsRequired().HasMaxLength(500);
            entity.Property(m => m.Description).HasMaxLength(500);
            entity.Property(m => m.ValeurFCFA).HasColumnType("decimal(18,2)");
            entity.Property(m => m.ValeurDevise).HasColumnType("decimal(18,2)");
            entity.Property(m => m.Devise).HasMaxLength(50);
            entity.Property(m => m.UniteStatistique).HasMaxLength(50);
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

    private void ConfigureAerien(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Aerien>(entity =>
        {
            entity.ToTable("Aeriens");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.AeroportEmbarquement).IsRequired().HasMaxLength(255);
            entity.Property(a => a.AeroportDebarquement).HasMaxLength(255);

            entity.HasOne(a => a.Voyage)
                  .WithOne(v => v.Aerien)
                  .HasForeignKey<Aerien>(a => a.VoyageId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureMaritime(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Maritime>(entity =>
        {
            entity.ToTable("Maritimes");
            entity.HasKey(m => m.Id);

            entity.HasOne(m => m.PortEmbarquement)
                  .WithMany()
                  .HasForeignKey(m => m.PortEmbarquementId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.PortDebarquement)
                  .WithMany()
                  .HasForeignKey(m => m.PortDebarquementId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.Voyage)
                  .WithOne(v => v.Maritime)
                  .HasForeignKey<Maritime>(m => m.VoyageId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureRoutier(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Routier>(entity =>
        {
            entity.ToTable("Routiers");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.RouteNationale).IsRequired().HasMaxLength(255);

            entity.HasOne(r => r.Voyage)
                  .WithOne(v => v.Routier)
                  .HasForeignKey<Routier>(r => r.VoyageId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureFluvial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Fluvial>(entity =>
        {
            entity.ToTable("Fluviaux");
            entity.HasKey(f => f.Id);

            entity.HasOne(f => f.PortEmbarquement)
                  .WithMany()
                  .HasForeignKey(f => f.PortEmbarquementId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(f => f.PortDebarquement)
                  .WithMany()
                  .HasForeignKey(f => f.PortDebarquementId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(f => f.Voyage)
                  .WithOne(v => v.Fluvial)
                  .HasForeignKey<Fluvial>(f => f.VoyageId)
                  .OnDelete(DeleteBehavior.Cascade);
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

    private void ConfigureTypeTransport(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TypeTransport>(entity =>
        {
            entity.ToTable("TypeTransports");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Nom).IsRequired().HasMaxLength(255);
            entity.Property(t => t.Module).HasMaxLength(50);
        });
    }

    private void ConfigureVoyage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Voyage>(entity =>
        {
            entity.ToTable("Voyages");
            entity.HasKey(v => v.Id);
            entity.Property(v => v.NomTransporteur).HasMaxLength(255);
            entity.Property(v => v.NomNavire).HasMaxLength(255);
            entity.Property(v => v.TypeNavire).HasMaxLength(100);
            entity.Property(v => v.PaysProvenance).HasMaxLength(255);
            entity.Property(v => v.PaysDestination).HasMaxLength(255);
            entity.Property(v => v.LieuSejour).HasMaxLength(255);
            entity.Property(v => v.DureeSejour).HasMaxLength(50);
            entity.Property(v => v.ModuleCode).IsRequired().HasMaxLength(10);

            entity.HasOne(v => v.Assurance)
                  .WithOne(a => a.Voyage)
                  .HasForeignKey<Voyage>(v => v.AssuranceId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(v => v.AssuranceId).IsUnique();
        });
    }

    private void ConfigureVisaAssurance(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VisaAssurance>(entity =>
        {
            entity.ToTable("VisaAssurances");
            entity.HasKey(v => v.Id);
            entity.Property(v => v.VisaContent).HasColumnType("nvarchar(max)");
            entity.Property(v => v.VisaOK).IsRequired();
            entity.Property(v => v.OrganisationId).IsRequired();
            entity.HasOne(v => v.Assurance)
                  .WithMany(a => a.Visas)
                  .HasForeignKey(v => v.AssuranceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureDocument(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("Documents");
            entity.HasKey(d => d.Id);
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
