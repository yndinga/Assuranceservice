using AssuranceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations;

[DbContext(typeof(AssuranceDbContext))]
[Migration("20260625180000_MoveNavireFieldsToMaritimeFluvial")]
public partial class MoveNavireFieldsToMaritimeFluvial : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Maritimes', N'U') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Maritimes') AND name = N'NomNavire')
                ALTER TABLE [dbo].[Maritimes] ADD [NomNavire] nvarchar(255) NULL;
            IF OBJECT_ID(N'dbo.Maritimes', N'U') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Maritimes') AND name = N'TypeNavire')
                ALTER TABLE [dbo].[Maritimes] ADD [TypeNavire] nvarchar(100) NULL;

            IF OBJECT_ID(N'dbo.Fluviaux', N'U') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Fluviaux') AND name = N'NomNavire')
                ALTER TABLE [dbo].[Fluviaux] ADD [NomNavire] nvarchar(255) NULL;
            IF OBJECT_ID(N'dbo.Fluviaux', N'U') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Fluviaux') AND name = N'TypeNavire')
                ALTER TABLE [dbo].[Fluviaux] ADD [TypeNavire] nvarchar(100) NULL;
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Voyages', N'U') IS NOT NULL
               AND OBJECT_ID(N'dbo.Maritimes', N'U') IS NOT NULL
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Voyages') AND name = N'NomNavire')
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Voyages') AND name = N'TypeNavire')
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Voyages') AND name = N'ModeDeTransport')
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Maritimes') AND name = N'NomNavire')
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Maritimes') AND name = N'TypeNavire')
            BEGIN
                EXEC(N'
                UPDATE m
                SET m.[NomNavire] = NULLIF(LTRIM(RTRIM(v.[NomNavire])), ''''),
                    m.[TypeNavire] = NULLIF(LTRIM(RTRIM(v.[TypeNavire])), '''')
                FROM [dbo].[Maritimes] m
                INNER JOIN [dbo].[Voyages] v ON v.[Id] = m.[VoyageId]
                LEFT JOIN [dbo].[Assurances] a ON a.[Id] = v.[AssuranceId]
                WHERE UPPER(LTRIM(RTRIM(COALESCE(NULLIF(v.[ModeDeTransport], ''''), a.[ModeDeTransport], ''MA'')))) = N''MA'';
                ');
            END
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Voyages', N'U') IS NOT NULL
               AND OBJECT_ID(N'dbo.Fluviaux', N'U') IS NOT NULL
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Voyages') AND name = N'NomNavire')
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Voyages') AND name = N'TypeNavire')
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Voyages') AND name = N'ModeDeTransport')
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Fluviaux') AND name = N'NomNavire')
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Fluviaux') AND name = N'TypeNavire')
            BEGIN
                EXEC(N'
                UPDATE f
                SET f.[NomNavire] = NULLIF(LTRIM(RTRIM(v.[NomNavire])), ''''),
                    f.[TypeNavire] = NULLIF(LTRIM(RTRIM(v.[TypeNavire])), '''')
                FROM [dbo].[Fluviaux] f
                INNER JOIN [dbo].[Voyages] v ON v.[Id] = f.[VoyageId]
                LEFT JOIN [dbo].[Assurances] a ON a.[Id] = v.[AssuranceId]
                WHERE UPPER(LTRIM(RTRIM(COALESCE(NULLIF(v.[ModeDeTransport], ''''), a.[ModeDeTransport], ''FL'')))) = N''FL'';
                ');
            END
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Voyages', N'U') IS NOT NULL
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Voyages') AND name = N'NomNavire')
            BEGIN
                DECLARE @NomNavireDefaultConstraint nvarchar(128);
                SELECT @NomNavireDefaultConstraint = dc.[name]
                FROM sys.default_constraints dc
                INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
                WHERE dc.parent_object_id = OBJECT_ID(N'dbo.Voyages')
                  AND c.[name] = N'NomNavire';

                IF @NomNavireDefaultConstraint IS NOT NULL
                    EXEC(N'ALTER TABLE [dbo].[Voyages] DROP CONSTRAINT [' + @NomNavireDefaultConstraint + N']');

                ALTER TABLE [dbo].[Voyages] DROP COLUMN [NomNavire];
            END

            IF OBJECT_ID(N'dbo.Voyages', N'U') IS NOT NULL
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Voyages') AND name = N'TypeNavire')
            BEGIN
                DECLARE @TypeNavireDefaultConstraint nvarchar(128);
                SELECT @TypeNavireDefaultConstraint = dc.[name]
                FROM sys.default_constraints dc
                INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
                WHERE dc.parent_object_id = OBJECT_ID(N'dbo.Voyages')
                  AND c.[name] = N'TypeNavire';

                IF @TypeNavireDefaultConstraint IS NOT NULL
                    EXEC(N'ALTER TABLE [dbo].[Voyages] DROP CONSTRAINT [' + @TypeNavireDefaultConstraint + N']');

                ALTER TABLE [dbo].[Voyages] DROP COLUMN [TypeNavire];
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Voyages', N'U') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Voyages') AND name = N'NomNavire')
                ALTER TABLE [dbo].[Voyages] ADD [NomNavire] nvarchar(255) NULL;
            IF OBJECT_ID(N'dbo.Voyages', N'U') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Voyages') AND name = N'TypeNavire')
                ALTER TABLE [dbo].[Voyages] ADD [TypeNavire] nvarchar(100) NULL;
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Voyages', N'U') IS NOT NULL
               AND OBJECT_ID(N'dbo.Maritimes', N'U') IS NOT NULL
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Voyages') AND name = N'NomNavire')
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Voyages') AND name = N'TypeNavire')
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Maritimes') AND name = N'NomNavire')
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Maritimes') AND name = N'TypeNavire')
            BEGIN
                EXEC(N'
                UPDATE v
                SET v.[NomNavire] = COALESCE(m.[NomNavire], v.[NomNavire]),
                    v.[TypeNavire] = COALESCE(m.[TypeNavire], v.[TypeNavire])
                FROM [dbo].[Voyages] v
                INNER JOIN [dbo].[Maritimes] m ON m.[VoyageId] = v.[Id];
                ');
            END
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Voyages', N'U') IS NOT NULL
               AND OBJECT_ID(N'dbo.Fluviaux', N'U') IS NOT NULL
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Voyages') AND name = N'NomNavire')
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Voyages') AND name = N'TypeNavire')
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Fluviaux') AND name = N'NomNavire')
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Fluviaux') AND name = N'TypeNavire')
            BEGIN
                EXEC(N'
                UPDATE v
                SET v.[NomNavire] = COALESCE(f.[NomNavire], v.[NomNavire]),
                    v.[TypeNavire] = COALESCE(f.[TypeNavire], v.[TypeNavire])
                FROM [dbo].[Voyages] v
                INNER JOIN [dbo].[Fluviaux] f ON f.[VoyageId] = v.[Id];
                ');
            END
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Maritimes', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Maritimes') AND name = N'NomNavire')
                ALTER TABLE [dbo].[Maritimes] DROP COLUMN [NomNavire];
            IF OBJECT_ID(N'dbo.Maritimes', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Maritimes') AND name = N'TypeNavire')
                ALTER TABLE [dbo].[Maritimes] DROP COLUMN [TypeNavire];
            IF OBJECT_ID(N'dbo.Fluviaux', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Fluviaux') AND name = N'NomNavire')
                ALTER TABLE [dbo].[Fluviaux] DROP COLUMN [NomNavire];
            IF OBJECT_ID(N'dbo.Fluviaux', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Fluviaux') AND name = N'TypeNavire')
                ALTER TABLE [dbo].[Fluviaux] DROP COLUMN [TypeNavire];
            """);
    }
}
