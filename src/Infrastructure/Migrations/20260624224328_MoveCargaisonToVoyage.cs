using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations;

/// <inheritdoc />
public partial class MoveCargaisonToVoyage : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var col in new (string Name, string Type)[]
        {
            ("Designation", "nvarchar(255)"),
            ("Nature", "nvarchar(500)"),
            ("Specificites", "nvarchar(100)"),
            ("Conditionnement", "nvarchar(500)"),
            ("Description", "nvarchar(500)"),
            ("Devise", "nvarchar(50)"),
            ("MasseBrute", "nvarchar(255)"),
            ("UniteStatistique", "nvarchar(255)"),
            ("Marque", "nvarchar(255)")
        })
        {
            migrationBuilder.Sql($"""
                IF OBJECT_ID(N'dbo.Voyages', N'U') IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Voyages') AND name = N'{col.Name}')
                    ALTER TABLE [dbo].[Voyages] ADD [{col.Name}] {col.Type} NULL;
                """);
        }

        migrationBuilder.Sql("""
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'ModeDeTransport')
                ALTER TABLE [dbo].[Assurances] ADD [ModeDeTransport] nvarchar(10) NOT NULL CONSTRAINT [DF_Assurances_ModeDeTransport] DEFAULT '';
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Marchandises', N'U') IS NOT NULL
               AND OBJECT_ID(N'dbo.Voyages', N'U') IS NOT NULL
            BEGIN
                UPDATE v
                SET
                    v.[Designation] = COALESCE(v.[Designation], m.[Designation]),
                    v.[Nature] = COALESCE(v.[Nature], m.[Nature]),
                    v.[Specificites] = COALESCE(v.[Specificites], m.[Specificites]),
                    v.[Conditionnement] = COALESCE(v.[Conditionnement], m.[Conditionnement]),
                    v.[Description] = COALESCE(v.[Description], m.[Description]),
                    v.[Devise] = COALESCE(v.[Devise], m.[Devise]),
                    v.[MasseBrute] = COALESCE(v.[MasseBrute], m.[MasseBrute]),
                    v.[UniteStatistique] = COALESCE(v.[UniteStatistique], m.[UniteStatistique]),
                    v.[Marque] = COALESCE(v.[Marque], m.[Marque])
                FROM [dbo].[Voyages] v
                INNER JOIN [dbo].[Marchandises] m ON m.[AssuranceId] = v.[AssuranceId];
            END
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Voyages', N'U') IS NOT NULL
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'Designation')
            BEGIN
                UPDATE v
                SET
                    v.[Designation] = COALESCE(v.[Designation], a.[Designation]),
                    v.[Nature] = COALESCE(v.[Nature], a.[Nature]),
                    v.[Specificites] = COALESCE(v.[Specificites], a.[Specificites]),
                    v.[Conditionnement] = COALESCE(v.[Conditionnement], a.[Conditionnement]),
                    v.[Description] = COALESCE(v.[Description], a.[Description]),
                    v.[Devise] = COALESCE(v.[Devise], a.[Devise]),
                    v.[MasseBrute] = COALESCE(v.[MasseBrute], a.[MasseBrute]),
                    v.[UniteStatistique] = COALESCE(v.[UniteStatistique], a.[UniteStatistique]),
                    v.[Marque] = COALESCE(v.[Marque], a.[Marque])
                FROM [dbo].[Voyages] v
                INNER JOIN [dbo].[Assurances] a ON a.[Id] = v.[AssuranceId];
            END
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Voyages', N'U') IS NOT NULL
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Voyages') AND name = N'ModuleCode')
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'ModeDeTransport')
            BEGIN
                UPDATE a
                SET a.[ModeDeTransport] = UPPER(LEFT(ISNULL(NULLIF(LTRIM(RTRIM(v.[ModuleCode])), ''), ''), 10))
                FROM [dbo].[Assurances] a
                INNER JOIN [dbo].[Voyages] v ON v.[AssuranceId] = a.[Id]
                WHERE ISNULL(NULLIF(LTRIM(RTRIM(a.[ModeDeTransport])), ''), '') = '';
            END
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Marchandises', N'U') IS NOT NULL
                DROP TABLE [dbo].[Marchandises];
            """);

        foreach (var col in new[] { "Designation", "Nature", "Specificites", "Conditionnement", "Description", "Devise", "MasseBrute", "UniteStatistique", "Marque" })
        {
            migrationBuilder.Sql($"""
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'{col}')
                    ALTER TABLE [dbo].[Assurances] DROP COLUMN [{col}];
                """);
        }
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'ModeDeTransport')
                ALTER TABLE [dbo].[Assurances] DROP COLUMN [ModeDeTransport];
            """);
    }
}
