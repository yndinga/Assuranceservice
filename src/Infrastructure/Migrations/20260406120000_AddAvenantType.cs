using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations
{
    public partial class AddAvenantType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ajout colonne Type (sans DEFAULT au final) tout en rétro-remplissant l'existant.
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Avenants]', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.Avenants', N'Type') IS NULL
    BEGIN
        ALTER TABLE [dbo].[Avenants] ADD [Type] nvarchar(50) NULL;
        UPDATE [dbo].[Avenants] SET [Type] = N'MODIFICATION' WHERE [Type] IS NULL;
        ALTER TABLE [dbo].[Avenants] ALTER COLUMN [Type] nvarchar(50) NOT NULL;
    END
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Avenants]', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.Avenants', N'Type') IS NOT NULL
    BEGIN
        ALTER TABLE [dbo].[Avenants] DROP COLUMN [Type];
    END
END
");
        }
    }
}

