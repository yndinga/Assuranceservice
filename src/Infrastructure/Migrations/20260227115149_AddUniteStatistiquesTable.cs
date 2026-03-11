using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniteStatistiquesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Supprimer les FK et index seulement s'ils existent (base peut avoir été créée par EnsureCreated avec un schéma légèrement différent)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Aeroports_Pays_PaysId' AND parent_object_id = OBJECT_ID(N'Aeroports'))
                    ALTER TABLE [Aeroports] DROP CONSTRAINT [FK_Aeroports_Pays_PaysId];
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ports_Pays_PaysId' AND parent_object_id = OBJECT_ID(N'Ports'))
                    ALTER TABLE [Ports] DROP CONSTRAINT [FK_Ports_Pays_PaysId];
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TauxDeChanges_Devises_DeviseId' AND parent_object_id = OBJECT_ID(N'TauxDeChanges'))
                    ALTER TABLE [TauxDeChanges] DROP CONSTRAINT [FK_TauxDeChanges_Devises_DeviseId];
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Troncons_Corridors_CorridorId' AND parent_object_id = OBJECT_ID(N'Troncons'))
                    ALTER TABLE [Troncons] DROP CONSTRAINT [FK_Troncons_Corridors_CorridorId];
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Troncons_Routes_RouteId' AND parent_object_id = OBJECT_ID(N'Troncons'))
                    ALTER TABLE [Troncons] DROP CONSTRAINT [FK_Troncons_Routes_RouteId];

                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Troncons_CorridorId' AND object_id = OBJECT_ID(N'Troncons'))
                    DROP INDEX [IX_Troncons_CorridorId] ON [Troncons];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Troncons_RouteId' AND object_id = OBJECT_ID(N'Troncons'))
                    DROP INDEX [IX_Troncons_RouteId] ON [Troncons];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TauxDeChanges_DeviseId' AND object_id = OBJECT_ID(N'TauxDeChanges'))
                    DROP INDEX [IX_TauxDeChanges_DeviseId] ON [TauxDeChanges];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ports_PaysId' AND object_id = OBJECT_ID(N'Ports'))
                    DROP INDEX [IX_Ports_PaysId] ON [Ports];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Aeroports_PaysId' AND object_id = OBJECT_ID(N'Aeroports'))
                    DROP INDEX [IX_Aeroports_PaysId] ON [Aeroports];
            ");

            migrationBuilder.AlterColumn<decimal>(
                name: "Accessoires",
                table: "Primes",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            // Ajouter les colonnes Policies seulement si elles n'existent pas (évite doublon si table déjà à jour)
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'Policies', N'U') IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Policies') AND name = N'CustomerId')
                        ALTER TABLE [Policies] ADD [CustomerId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Policies') AND name = N'EndDate')
                        ALTER TABLE [Policies] ADD [EndDate] datetime2 NOT NULL DEFAULT '0001-01-01';
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Policies') AND name = N'StartDate')
                        ALTER TABLE [Policies] ADD [StartDate] datetime2 NOT NULL DEFAULT '0001-01-01';
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Policies') AND name = N'Status')
                        ALTER TABLE [Policies] ADD [Status] int NOT NULL DEFAULT 0;
                END
            ");

            migrationBuilder.AlterColumn<decimal>(
                name: "Taxe",
                table: "Cotations",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PrimeTotale",
                table: "Cotations",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PrimeNette",
                table: "Cotations",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Montant",
                table: "Cotations",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<decimal>(
                name: "Accessoires",
                table: "Cotations",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            // Ajouter les colonnes Cotations seulement si elles n'existent pas
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'Cotations', N'U') IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Cotations') AND name = N'ContratId')
                        ALTER TABLE [Cotations] ADD [ContratId] decimal(20,0) NOT NULL DEFAULT 0;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Cotations') AND name = N'PartenaireId')
                        ALTER TABLE [Cotations] ADD [PartenaireId] decimal(20,0) NOT NULL DEFAULT 0;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Cotations') AND name = N'StatutId')
                        ALTER TABLE [Cotations] ADD [StatutId] decimal(20,0) NOT NULL DEFAULT 0;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Cotations') AND name = N'UserId')
                        ALTER TABLE [Cotations] ADD [UserId] decimal(20,0) NOT NULL DEFAULT 0;
                END
            ");

            migrationBuilder.AlterColumn<string>(
                name: "TypeContrat",
                table: "Assurances",
                type: "nvarchar(250)",
                maxLength: 25,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(25)",
                oldMaxLength: 25);

            migrationBuilder.AlterColumn<string>(
                name: "Statut",
                table: "Assurances",
                type: "nvarchar(250)",
                maxLength: 25,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(25)",
                oldMaxLength: 25);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroCert",
                table: "Assurances",
                type: "nvarchar(250)",
                maxLength: 25,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(25)",
                oldMaxLength: 25,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImportateurNIU",
                table: "Assurances",
                type: "nvarchar(250)",
                maxLength: 25,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(25)",
                oldMaxLength: 25);

            migrationBuilder.AlterColumn<string>(
                name: "Duree",
                table: "Assurances",
                type: "nvarchar(250)",
                maxLength: 25,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(25)",
                oldMaxLength: 25,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "UniteStatistiques",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Actif = table.Column<bool>(type: "bit", nullable: false),
                    CreerPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreerLe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifierLe = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_UniteStatistiques", x => x.Id));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "UniteStatistiques");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "ContratId",
                table: "Cotations");

            migrationBuilder.DropColumn(
                name: "PartenaireId",
                table: "Cotations");

            migrationBuilder.DropColumn(
                name: "StatutId",
                table: "Cotations");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Cotations");

            migrationBuilder.AlterColumn<double>(
                name: "Accessoires",
                table: "Primes",
                type: "float",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Taxe",
                table: "Cotations",
                type: "float",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "PrimeTotale",
                table: "Cotations",
                type: "float",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "PrimeNette",
                table: "Cotations",
                type: "float",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Montant",
                table: "Cotations",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<double>(
                name: "Accessoires",
                table: "Cotations",
                type: "float",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TypeContrat",
                table: "Assurances",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 25);

            migrationBuilder.AlterColumn<string>(
                name: "Statut",
                table: "Assurances",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 25);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroCert",
                table: "Assurances",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 25,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImportateurNIU",
                table: "Assurances",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 25);

            migrationBuilder.AlterColumn<string>(
                name: "Duree",
                table: "Assurances",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 25,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Troncons_CorridorId",
                table: "Troncons",
                column: "CorridorId");

            migrationBuilder.CreateIndex(
                name: "IX_Troncons_RouteId",
                table: "Troncons",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_TauxDeChanges_DeviseId",
                table: "TauxDeChanges",
                column: "DeviseId");

            migrationBuilder.CreateIndex(
                name: "IX_Ports_PaysId",
                table: "Ports",
                column: "PaysId");

            migrationBuilder.CreateIndex(
                name: "IX_Aeroports_PaysId",
                table: "Aeroports",
                column: "PaysId");

            migrationBuilder.AddForeignKey(
                name: "FK_Aeroports_Pays_PaysId",
                table: "Aeroports",
                column: "PaysId",
                principalTable: "Pays",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ports_Pays_PaysId",
                table: "Ports",
                column: "PaysId",
                principalTable: "Pays",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TauxDeChanges_Devises_DeviseId",
                table: "TauxDeChanges",
                column: "DeviseId",
                principalTable: "Devises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Troncons_Corridors_CorridorId",
                table: "Troncons",
                column: "CorridorId",
                principalTable: "Corridors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Troncons_Routes_RouteId",
                table: "Troncons",
                column: "RouteId",
                principalTable: "Routes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
