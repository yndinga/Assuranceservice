using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations
{
    [Migration("20260716123000_AddAssurancePartnerCodes")]
    public partial class AddAssurancePartnerCodes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Assurances', N'Partenaire') IS NULL
                    ALTER TABLE [dbo].[Assurances] ADD [Partenaire] nvarchar(250) NULL;

                IF COL_LENGTH(N'dbo.Assurances', N'Intermediaire') IS NULL
                    ALTER TABLE [dbo].[Assurances] ADD [Intermediaire] nvarchar(250) NULL;

                IF COL_LENGTH(N'dbo.Assurances', N'TypePartenaire') IS NULL
                    ALTER TABLE [dbo].[Assurances] ADD [TypePartenaire] nvarchar(50) NOT NULL CONSTRAINT [DF_Assurances_TypePartenaire] DEFAULT N'ASSUREUR';
            """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Assurances', N'TypePartenaire') IS NOT NULL
                BEGIN
                    IF OBJECT_ID(N'dbo.DF_Assurances_TypePartenaire', N'D') IS NOT NULL
                        ALTER TABLE [dbo].[Assurances] DROP CONSTRAINT [DF_Assurances_TypePartenaire];

                    ALTER TABLE [dbo].[Assurances] DROP COLUMN [TypePartenaire];
                END

                IF COL_LENGTH(N'dbo.Assurances', N'Intermediaire') IS NOT NULL
                    ALTER TABLE [dbo].[Assurances] DROP COLUMN [Intermediaire];

                IF COL_LENGTH(N'dbo.Assurances', N'Partenaire') IS NOT NULL
                    ALTER TABLE [dbo].[Assurances] DROP COLUMN [Partenaire];
            """);
        }
    }
}
