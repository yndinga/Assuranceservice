using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlignAssuranceFunctionalModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VisaAssurances_AssuranceId",
                table: "VisaAssurances");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateVisa",
                table: "VisaAssurances",
                type: "datetime2(0)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Licence",
                table: "VisaAssurances",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "VisaAssurances",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeDocument",
                table: "Documents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "PIECE_ASSURANCE");

            migrationBuilder.AddColumn<int>(
                name: "DureeJours",
                table: "Assurances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroAFI",
                table: "Assurances",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE [dbo].[Assurances]
                SET [NumeroAFI] = UPPER(CONCAT(N'AFI-', REPLACE(CONVERT(nvarchar(36), [Id]), N'-', N'')))
                WHERE [NumeroAFI] IS NULL OR LTRIM(RTRIM([NumeroAFI])) = N'';

                UPDATE [dbo].[Assurances]
                SET [DureeJours] = TRY_CONVERT(int, NULLIF(LTRIM(RTRIM([Duree])), N''))
                WHERE [DureeJours] IS NULL
                  AND TRY_CONVERT(int, NULLIF(LTRIM(RTRIM([Duree])), N'')) > 0;

                UPDATE [dbo].[Documents]
                SET [TypeDocument] = N'FACTURE_DI'
                WHERE [Description] LIKE N'Facture DI %';

                UPDATE [dbo].[VisaAssurances]
                SET [DateVisa] = COALESCE([ModifierLe], [CreerLe])
                WHERE [VisaOK] = 1 AND [DateVisa] IS NULL;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroAFI",
                table: "Assurances",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VisaAssurances_AssuranceId_TypePartenaire_Organisation",
                table: "VisaAssurances",
                columns: new[] { "AssuranceId", "TypePartenaire", "Organisation" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assurances_NumeroAFI",
                table: "Assurances",
                column: "NumeroAFI",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VisaAssurances_AssuranceId_TypePartenaire_Organisation",
                table: "VisaAssurances");

            migrationBuilder.DropIndex(
                name: "IX_Assurances_NumeroAFI",
                table: "Assurances");

            migrationBuilder.DropColumn(
                name: "DateVisa",
                table: "VisaAssurances");

            migrationBuilder.DropColumn(
                name: "Licence",
                table: "VisaAssurances");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "VisaAssurances");

            migrationBuilder.DropColumn(
                name: "TypeDocument",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "DureeJours",
                table: "Assurances");

            migrationBuilder.DropColumn(
                name: "NumeroAFI",
                table: "Assurances");

            migrationBuilder.CreateIndex(
                name: "IX_VisaAssurances_AssuranceId",
                table: "VisaAssurances",
                column: "AssuranceId",
                unique: true);
        }
    }
}
