using AssuranceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations;

[DbContext(typeof(AssuranceDbContext))]
[Migration("20260625140000_MoveNumeroBLToMaritime")]
public partial class MoveNumeroBLToMaritime : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Maritimes', N'U') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Maritimes') AND name = N'NumeroBL')
                ALTER TABLE [dbo].[Maritimes] ADD [NumeroBL] nvarchar(255) NULL;
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Voyages', N'U') IS NOT NULL
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Voyages') AND name = N'NumeroBL')
               AND OBJECT_ID(N'dbo.Maritimes', N'U') IS NOT NULL
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Maritimes') AND name = N'NumeroBL')
            BEGIN
                EXEC(N'
                UPDATE m
                SET m.[NumeroBL] = NULLIF(LTRIM(RTRIM(v.[NumeroBL])), '''')
                FROM [dbo].[Maritimes] m
                INNER JOIN [dbo].[Voyages] v ON v.[Id] = m.[VoyageId]
                WHERE ISNULL(NULLIF(LTRIM(RTRIM(m.[NumeroBL])), ''''), '''') = ''''
                  AND ISNULL(NULLIF(LTRIM(RTRIM(v.[NumeroBL])), ''''), '''') <> '''';
                ');

                ALTER TABLE [dbo].[Voyages] DROP COLUMN [NumeroBL];
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Voyages', N'U') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Voyages') AND name = N'NumeroBL')
                ALTER TABLE [dbo].[Voyages] ADD [NumeroBL] nvarchar(255) NULL;
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Voyages', N'U') IS NOT NULL
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Voyages') AND name = N'NumeroBL')
               AND OBJECT_ID(N'dbo.Maritimes', N'U') IS NOT NULL
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Maritimes') AND name = N'NumeroBL')
            BEGIN
                EXEC(N'
                UPDATE v
                SET v.[NumeroBL] = m.[NumeroBL]
                FROM [dbo].[Voyages] v
                INNER JOIN [dbo].[Maritimes] m ON m.[VoyageId] = v.[Id]
                WHERE m.[NumeroBL] IS NOT NULL;
                ');

                ALTER TABLE [dbo].[Maritimes] DROP COLUMN [NumeroBL];
            END
            """);
    }
}
