using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssuranceDeclarationsAndLines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Statut",
                table: "Assurances",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "42",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldDefaultValue: "10");

            migrationBuilder.CreateTable(
                name: "AssuranceDeclarations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssuranceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeclarationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroDI = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaysEmbarquementCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PortEmbarquementCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreerPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreerLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifierLe = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssuranceDeclarations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssuranceDeclarations_Assurances_AssuranceId",
                        column: x => x.AssuranceId,
                        principalTable: "Assurances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssuranceLignes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssuranceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssuranceDeclarationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeclarationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceCommandeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceLigneDIId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NoLigne = table.Column<int>(type: "int", nullable: false),
                    PositionTarifaire = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Marque = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Colisage = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Quantite = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: true),
                    MasseBrute = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: true),
                    MasseNette = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: true),
                    Volume = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: true),
                    PrixUnitaire = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: true),
                    ValeurDevise = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: true),
                    ValeurXAF = table.Column<decimal>(type: "decimal(18,5)", precision: 18, scale: 5, nullable: true),
                    Devise = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UniteStatistique = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PaysOrigine = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EstPartielle = table.Column<bool>(type: "bit", nullable: false),
                    CreerPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreerLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifierLe = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssuranceLignes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssuranceLignes_AssuranceDeclarations_AssuranceDeclarationId",
                        column: x => x.AssuranceDeclarationId,
                        principalTable: "AssuranceDeclarations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssuranceLignes_Assurances_AssuranceId",
                        column: x => x.AssuranceId,
                        principalTable: "Assurances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssuranceDeclarations_AssuranceId_DeclarationId",
                table: "AssuranceDeclarations",
                columns: new[] { "AssuranceId", "DeclarationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssuranceDeclarations_DeclarationId",
                table: "AssuranceDeclarations",
                column: "DeclarationId");

            migrationBuilder.CreateIndex(
                name: "IX_AssuranceLignes_AssuranceDeclarationId",
                table: "AssuranceLignes",
                column: "AssuranceDeclarationId");

            migrationBuilder.CreateIndex(
                name: "IX_AssuranceLignes_AssuranceId_SourceLigneDIId",
                table: "AssuranceLignes",
                columns: new[] { "AssuranceId", "SourceLigneDIId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssuranceLignes_SourceLigneDIId",
                table: "AssuranceLignes",
                column: "SourceLigneDIId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssuranceLignes");

            migrationBuilder.DropTable(
                name: "AssuranceDeclarations");

            migrationBuilder.AlterColumn<string>(
                name: "Statut",
                table: "Assurances",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "10",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldDefaultValue: "42");
        }
    }
}
