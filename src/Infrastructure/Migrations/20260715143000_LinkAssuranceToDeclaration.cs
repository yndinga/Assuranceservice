using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations
{
    /// <inheritdoc />
    [Migration("20260715143000_LinkAssuranceToDeclaration")]
    public partial class LinkAssuranceToDeclaration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
IF COL_LENGTH(N'dbo.Assurances', N'NoFacture') IS NULL
    ALTER TABLE [dbo].[Assurances] ADD [NoFacture] nvarchar(250) NULL;
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
IF COL_LENGTH(N'dbo.Assurances', N'NoFacture') IS NOT NULL
    ALTER TABLE [dbo].[Assurances] DROP COLUMN [NoFacture];
""");
        }
    }
}
