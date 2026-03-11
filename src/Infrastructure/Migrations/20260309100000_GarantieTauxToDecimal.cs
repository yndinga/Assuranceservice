using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GarantieTauxToDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Garanties]') AND name = 'Taux')
                BEGIN
                    ALTER TABLE [dbo].[Garanties] ADD [TauxNew] decimal(18,4) NULL;

                    UPDATE [dbo].[Garanties]
                    SET [TauxNew] = TRY_CAST(REPLACE(LTRIM(RTRIM(ISNULL([Taux], ''))), ',', '.') AS decimal(18,4))
                    WHERE LTRIM(RTRIM(ISNULL([Taux], ''))) <> '';

                    ALTER TABLE [dbo].[Garanties] DROP COLUMN [Taux];

                    EXEC sp_rename 'dbo.Garanties.TauxNew', 'Taux', 'COLUMN';
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Garanties]') AND name = 'Taux')
                BEGIN
                    ALTER TABLE [dbo].[Garanties] ADD [TauxStr] nvarchar(25) NULL;

                    UPDATE [dbo].[Garanties]
                    SET [TauxStr] = CAST([Taux] AS nvarchar(25))
                    WHERE [Taux] IS NOT NULL;

                    ALTER TABLE [dbo].[Garanties] DROP COLUMN [Taux];

                    EXEC sp_rename 'dbo.Garanties.TauxStr', 'Taux', 'COLUMN';
                END
            ");
        }
    }
}
