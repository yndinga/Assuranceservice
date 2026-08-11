using AssuranceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations;

[DbContext(typeof(AssuranceDbContext))]
[Migration("20260609210000_RenameModuleToTypeTransportReferentiel")]
public partial class RenameModuleToTypeTransportReferentiel : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.TypeTransports', N'U') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.TypeTransports') AND name = N'Code')
            BEGIN
                DROP TABLE [TypeTransports];
            END
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Modules', N'U') IS NOT NULL
               AND OBJECT_ID(N'dbo.TypeTransports', N'U') IS NULL
            BEGIN
                EXEC sp_rename N'dbo.Modules', N'TypeTransports';
            END
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.RoutesNationales', N'U') IS NULL
            BEGIN
                CREATE TABLE [RoutesNationales] (
                    [Id] uniqueidentifier NOT NULL,
                    [Code] nvarchar(20) NOT NULL,
                    [Nom] nvarchar(100) NOT NULL,
                    [Actif] bit NOT NULL,
                    [CreerPar] nvarchar(max) NULL,
                    [ModifierPar] nvarchar(max) NULL,
                    [CreerLe] datetime2 NOT NULL,
                    [ModifierLe] datetime2 NULL,
                    CONSTRAINT [PK_RoutesNationales] PRIMARY KEY ([Id])
                );
            END
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "RoutesNationales");

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.TypeTransports', N'U') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.TypeTransports') AND name = N'Module')
            BEGIN
                EXEC sp_rename N'dbo.TypeTransports', N'Modules';
            END
            """);
    }
}
