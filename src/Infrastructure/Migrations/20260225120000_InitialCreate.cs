using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ========== Référentiels (sans FK ou FK vers référentiels) ==========
            migrationBuilder.CreateTable(
                name: "Garanties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomGarantie = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Taux = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    Accessoires = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Actif = table.Column<bool>(type: "bit", nullable: false),
                    CreerPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreerLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifierLe = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_Garanties", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Pays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Actif = table.Column<bool>(type: "bit", nullable: false),
                    CreerPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreerLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifierLe = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_Pays", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Devises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Actif = table.Column<bool>(type: "bit", nullable: false),
                    CreerPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreerLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifierLe = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_Devises", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Actif = table.Column<bool>(type: "bit", nullable: false),
                    CreerPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreerLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifierLe = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_Modules", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Corridors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Actif = table.Column<bool>(type: "bit", nullable: false),
                    CreerPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreerLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifierLe = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_Corridors", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Actif = table.Column<bool>(type: "bit", nullable: false),
                    CreerPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreerLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifierLe = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_Routes", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Aeroports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaysId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Actif = table.Column<bool>(type: "bit", nullable: false),
                    CreerPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreerLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifierLe = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aeroports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Aeroports_Pays_PaysId",
                        column: x => x.PaysId,
                        principalTable: "Pays",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Ports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaysId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Module = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Actif = table.Column<bool>(type: "bit", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    CreerPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreerLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifierLe = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ports_Pays_PaysId",
                        column: x => x.PaysId,
                        principalTable: "Pays",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TauxDeChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Taux = table.Column<decimal>(type: "decimal(20,5)", nullable: false),
                    ValideDe = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Actif = table.Column<bool>(type: "bit", nullable: false),
                    CreerPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreerLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifierLe = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TauxDeChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TauxDeChanges_Devises_DeviseId",
                        column: x => x.DeviseId,
                        principalTable: "Devises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Troncons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CorridorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RouteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Actif = table.Column<bool>(type: "bit", nullable: false),
                    CreerPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreerLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifierLe = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Troncons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Troncons_Corridors_CorridorId",
                        column: x => x.CorridorId,
                        principalTable: "Corridors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Troncons_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Departements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ordre = table.Column<int>(type: "int", nullable: false),
                    Actif = table.Column<bool>(type: "bit", nullable: false),
                    CreerPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreerLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifierLe = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_Departements", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Cotations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Montant = table.Column<double>(type: "float", nullable: false),
                    Taux = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PrimeNette = table.Column<double>(type: "float", nullable: true),
                    Accessoires = table.Column<double>(type: "float", nullable: true),
                    Taxe = table.Column<double>(type: "float", nullable: true),
                    PrimeTotale = table.Column<double>(type: "float", nullable: true),
                    CreerPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreerLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifierLe = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_Cotations", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Policies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Premium = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Policies", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_Customers", x => x.Id));

            // ========== Assurances (FK Garanties) ==========
            migrationBuilder.CreateTable(
                name: "Assurances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NoPolice = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    NumeroCert = table.Column<string>(type: "nvarchar(250)", maxLength: 25, nullable: true),
                    ImportateurNom = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ImportateurNIU = table.Column<string>(type: "nvarchar(250)", maxLength: 25, nullable: false),
                    DateDebut = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TypeContrat = table.Column<string>(type: "nvarchar(250)", maxLength: 25, nullable: false),
                    Duree = table.Column<string>(type: "nvarchar(250)", maxLength: 25, nullable: true),
                    Statut = table.Column<string>(type: "nvarchar(250)", maxLength: 25, nullable: false),
                    GarantieId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssureurId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IntermediaireId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OCRE = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Module = table.Column<string>(type: "nvarchar(250)", maxLength: 25, nullable: false),
                    CreerPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreerLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifierLe = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assurances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assurances_Garanties_GarantieId",
                        column: x => x.GarantieId,
                        principalTable: "Garanties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Marchandises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssuranceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Nature = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Specificites = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Conditionnement = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ValeurFCFA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValeurDevise = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Devise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MasseBrute = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    UniteStatistique = table.Column<string>(type: "nvarchar(255)", maxLength: 50, nullable: false),
                    Marque = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreerPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreerLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifierLe = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marchandises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Marchandises_Assurances_AssuranceId",
                        column: x => x.AssuranceId,
                        principalTable: "Assurances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Primes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Taux = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ValeurFCFA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValeurDevise = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrimeNette = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Accessoires = table.Column<double>(type: "float", nullable: true),
                    Taxe = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PrimeTotale = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AssuranceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreerPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreerLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifierLe = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Primes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Primes_Assurances_AssuranceId",
                        column: x => x.AssuranceId,
                        principalTable: "Assurances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Voyages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssuranceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomTransporteur = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NomNavire = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TypeNavire = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LieuSejour = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DureeSejour = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PaysProvenance = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PaysDestination = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreerPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreerLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifierLe = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Voyages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Voyages_Assurances_AssuranceId",
                        column: x => x.AssuranceId,
                        principalTable: "Assurances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisaAssurances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssuranceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganisationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisaOK = table.Column<bool>(type: "bit", nullable: false),
                    VisaContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreerPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreerLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifierLe = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisaAssurances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisaAssurances_Assurances_AssuranceId",
                        column: x => x.AssuranceId,
                        principalTable: "Assurances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Aeriens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AeroportEmbarquement = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AeroportDebarquement = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    VoyageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreerPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreerLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifierLe = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aeriens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Aeriens_Voyages_VoyageId",
                        column: x => x.VoyageId,
                        principalTable: "Voyages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Maritimes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PortEmbarquementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PortDebarquementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoyageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreerPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreerLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifierLe = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maritimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Maritimes_Ports_PortDebarquementId",
                        column: x => x.PortDebarquementId,
                        principalTable: "Ports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Maritimes_Ports_PortEmbarquementId",
                        column: x => x.PortEmbarquementId,
                        principalTable: "Ports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Maritimes_Voyages_VoyageId",
                        column: x => x.VoyageId,
                        principalTable: "Voyages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Routiers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RouteNationale = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    VoyageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreerPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreerLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifierLe = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Routiers_Voyages_VoyageId",
                        column: x => x.VoyageId,
                        principalTable: "Voyages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fluviaux",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PortEmbarquementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PortDebarquementId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VoyageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreerPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreerLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifierLe = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fluviaux", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fluviaux_Ports_PortDebarquementId",
                        column: x => x.PortDebarquementId,
                        principalTable: "Ports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fluviaux_Ports_PortEmbarquementId",
                        column: x => x.PortEmbarquementId,
                        principalTable: "Ports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fluviaux_Voyages_VoyageId",
                        column: x => x.VoyageId,
                        principalTable: "Voyages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssuranceProcessStates",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentState = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AssuranceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NoPolice = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TypeContrat = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Importateur = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ErrorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AssuranceCreated = table.Column<bool>(type: "bit", nullable: false),
                    MarchandisesAdded = table.Column<bool>(type: "bit", nullable: false),
                    PrimeCalculated = table.Column<bool>(type: "bit", nullable: false),
                    GarantiesAssigned = table.Column<bool>(type: "bit", nullable: false),
                    ProcessCompleted = table.Column<bool>(type: "bit", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    LastRetryAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_AssuranceProcessStates", x => x.CorrelationId));
            // Index
            migrationBuilder.CreateIndex(name: "IX_Assurances_GarantieId", table: "Assurances", column: "GarantieId");
            migrationBuilder.Sql("CREATE UNIQUE INDEX [IX_Assurances_NoPolice] ON [Assurances] ([NoPolice]) WHERE [NoPolice] IS NOT NULL;");
            migrationBuilder.Sql("CREATE UNIQUE INDEX [IX_Assurances_NumeroCert] ON [Assurances] ([NumeroCert]) WHERE [NumeroCert] IS NOT NULL;");
            migrationBuilder.CreateIndex(name: "IX_Marchandises_AssuranceId", table: "Marchandises", column: "AssuranceId");
            migrationBuilder.CreateIndex(name: "IX_Primes_AssuranceId", table: "Primes", column: "AssuranceId");
            migrationBuilder.CreateIndex(name: "IX_Voyages_AssuranceId", table: "Voyages", column: "AssuranceId", unique: true);
            migrationBuilder.CreateIndex(name: "IX_VisaAssurances_AssuranceId", table: "VisaAssurances", column: "AssuranceId");
            migrationBuilder.CreateIndex(name: "IX_Aeriens_VoyageId", table: "Aeriens", column: "VoyageId", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Maritimes_PortDebarquementId", table: "Maritimes", column: "PortDebarquementId");
            migrationBuilder.CreateIndex(name: "IX_Maritimes_PortEmbarquementId", table: "Maritimes", column: "PortEmbarquementId");
            migrationBuilder.CreateIndex(name: "IX_Maritimes_VoyageId", table: "Maritimes", column: "VoyageId", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Routiers_VoyageId", table: "Routiers", column: "VoyageId", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Fluviaux_PortDebarquementId", table: "Fluviaux", column: "PortDebarquementId");
            migrationBuilder.CreateIndex(name: "IX_Fluviaux_PortEmbarquementId", table: "Fluviaux", column: "PortEmbarquementId");
            migrationBuilder.CreateIndex(name: "IX_Fluviaux_VoyageId", table: "Fluviaux", column: "VoyageId", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Aeroports_PaysId", table: "Aeroports", column: "PaysId");
            migrationBuilder.CreateIndex(name: "IX_Ports_PaysId", table: "Ports", column: "PaysId");
            migrationBuilder.CreateIndex(name: "IX_TauxDeChanges_DeviseId", table: "TauxDeChanges", column: "DeviseId");
            migrationBuilder.CreateIndex(name: "IX_Troncons_CorridorId", table: "Troncons", column: "CorridorId");
            migrationBuilder.CreateIndex(name: "IX_Troncons_RouteId", table: "Troncons", column: "RouteId");
            migrationBuilder.CreateIndex(name: "IX_Policies_Number", table: "Policies", column: "Number", unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Aeriens");
            migrationBuilder.DropTable(name: "Fluviaux");
            migrationBuilder.DropTable(name: "Maritimes");
            migrationBuilder.DropTable(name: "Routiers");
            migrationBuilder.DropTable(name: "VisaAssurances");
            migrationBuilder.DropTable(name: "Voyages");
            migrationBuilder.DropTable(name: "Primes");
            migrationBuilder.DropTable(name: "Marchandises");
            migrationBuilder.DropTable(name: "AssuranceProcessStates");
            migrationBuilder.DropTable(name: "Assurances");
            migrationBuilder.DropTable(name: "Cotations");
            migrationBuilder.DropTable(name: "Policies");
            migrationBuilder.DropTable(name: "Customers");
            migrationBuilder.DropTable(name: "Troncons");
            migrationBuilder.DropTable(name: "TauxDeChanges");
            migrationBuilder.DropTable(name: "Ports");
            migrationBuilder.DropTable(name: "Aeroports");
            migrationBuilder.DropTable(name: "Departements");
            migrationBuilder.DropTable(name: "Garanties");
            migrationBuilder.DropTable(name: "Corridors");
            migrationBuilder.DropTable(name: "Routes");
            migrationBuilder.DropTable(name: "Devises");
            migrationBuilder.DropTable(name: "Modules");
            migrationBuilder.DropTable(name: "Pays");
        }
    }
}
