using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniteStatistiqueToMarchandises : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'[dbo].[Marchandises]')
                    AND name = 'UniteStatistique'
                )
                BEGIN
                    ALTER TABLE [dbo].[Marchandises]
                    ADD [UniteStatistique] nvarchar(50) NOT NULL DEFAULT '';
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'[dbo].[Marchandises]')
                    AND name = 'UniteStatistique'
                )
                BEGIN
                    ALTER TABLE [dbo].[Marchandises] DROP COLUMN [UniteStatistique];
                END
            ");
        }
    }
}
