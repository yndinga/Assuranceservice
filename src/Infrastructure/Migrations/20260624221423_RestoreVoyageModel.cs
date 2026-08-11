using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations;

/// <inheritdoc />
public partial class RestoreVoyageModel : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Voyages', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[Voyages] (
                    [Id]              uniqueidentifier NOT NULL,
                    [AssuranceId]     uniqueidentifier NOT NULL,
                    [ModuleCode]      nvarchar(10)     NOT NULL CONSTRAINT [DF_Voyages_ModuleCode] DEFAULT 'MA',
                    [NomTransporteur] nvarchar(255)    NOT NULL CONSTRAINT [DF_Voyages_NomTransporteur] DEFAULT '',
                    [NomNavire]       nvarchar(255)    NOT NULL CONSTRAINT [DF_Voyages_NomNavire] DEFAULT '',
                    [TypeNavire]      nvarchar(100)    NOT NULL CONSTRAINT [DF_Voyages_TypeNavire] DEFAULT '',
                    [LieuSejour]      nvarchar(255)    NULL,
                    [DureeSejour]     nvarchar(50)     NULL,
                    [PaysProvenance]  nvarchar(255)    NOT NULL CONSTRAINT [DF_Voyages_PaysProvenance] DEFAULT '',
                    [PaysDestination] nvarchar(255)    NOT NULL CONSTRAINT [DF_Voyages_PaysDestination] DEFAULT '',
                    [CreerPar]        nvarchar(max)    NULL,
                    [ModifierPar]     nvarchar(max)    NULL,
                    [CreerLe]         datetime2        NOT NULL,
                    [ModifierLe]      datetime2        NULL,
                    CONSTRAINT [PK_Voyages] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Voyages_Assurances_AssuranceId] FOREIGN KEY ([AssuranceId])
                        REFERENCES [dbo].[Assurances] ([Id]) ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX [IX_Voyages_AssuranceId] ON [dbo].[Voyages] ([AssuranceId]);
            END
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Voyages', N'U') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Voyages') AND name = N'ModuleCode')
            BEGIN
                ALTER TABLE [dbo].[Voyages] ADD [ModuleCode] nvarchar(10) NOT NULL CONSTRAINT [DF_Voyages_ModuleCode_Legacy] DEFAULT 'MA';
            END
            """);

        migrationBuilder.Sql("""
            IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'Module')
            BEGIN
                INSERT INTO [dbo].[Voyages] (
                    [Id], [AssuranceId], [ModuleCode], [NomTransporteur], [NomNavire], [TypeNavire],
                    [LieuSejour], [DureeSejour], [PaysProvenance], [PaysDestination],
                    [CreerPar], [ModifierPar], [CreerLe], [ModifierLe]
                )
                SELECT
                    NEWID(),
                    a.[Id],
                    UPPER(LEFT(ISNULL(NULLIF(LTRIM(RTRIM(a.[Module])), ''), 'MA'), 10)),
                    ISNULL(a.[NomTransporteur], ''),
                    ISNULL(a.[NomNavire], ''),
                    ISNULL(a.[TypeNavire], ''),
                    a.[LieuSejour],
                    a.[DureeSejour],
                    ISNULL(a.[PaysProvenance], ''),
                    ISNULL(a.[PaysDestination], ''),
                    a.[CreerPar],
                    a.[ModifierPar],
                    ISNULL(a.[CreerLe], SYSUTCDATETIME()),
                    a.[ModifierLe]
                FROM [dbo].[Assurances] a
                WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Voyages] v WHERE v.[AssuranceId] = a.[Id]);
            END
            """);

        MigrateTransportTable(migrationBuilder, "Aeriens", "FK_Aeriens_Assurances_AssuranceId", "FK_Aeriens_Voyages_VoyageId", "IX_Aeriens_AssuranceId", "IX_Aeriens_VoyageId");
        MigrateTransportTable(migrationBuilder, "Maritimes", "FK_Maritimes_Assurances_AssuranceId", "FK_Maritimes_Voyages_VoyageId", "IX_Maritimes_AssuranceId", "IX_Maritimes_VoyageId");
        MigrateTransportTable(migrationBuilder, "Routiers", "FK_Routiers_Assurances_AssuranceId", "FK_Routiers_Voyages_VoyageId", "IX_Routiers_AssuranceId", "IX_Routiers_VoyageId");
        MigrateTransportTable(migrationBuilder, "Fluviaux", "FK_Fluviaux_Assurances_AssuranceId", "FK_Fluviaux_Voyages_VoyageId", "IX_Fluviaux_AssuranceId", "IX_Fluviaux_VoyageId");

        migrationBuilder.Sql("""
            IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'Module')
                ALTER TABLE [dbo].[Assurances] DROP COLUMN [Module];
            IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'NomTransporteur')
                ALTER TABLE [dbo].[Assurances] DROP COLUMN [NomTransporteur];
            IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'NomNavire')
                ALTER TABLE [dbo].[Assurances] DROP COLUMN [NomNavire];
            IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'TypeNavire')
                ALTER TABLE [dbo].[Assurances] DROP COLUMN [TypeNavire];
            IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'LieuSejour')
                ALTER TABLE [dbo].[Assurances] DROP COLUMN [LieuSejour];
            IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'DureeSejour')
                ALTER TABLE [dbo].[Assurances] DROP COLUMN [DureeSejour];
            IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'PaysProvenance')
                ALTER TABLE [dbo].[Assurances] DROP COLUMN [PaysProvenance];
            IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'PaysDestination')
                ALTER TABLE [dbo].[Assurances] DROP COLUMN [PaysDestination];
            """);
    }

    private static void MigrateTransportTable(
        MigrationBuilder migrationBuilder,
        string table,
        string oldFkName,
        string newFkName,
        string oldIndexName,
        string newIndexName)
    {
        migrationBuilder.Sql($"""
            IF OBJECT_ID(N'dbo.{table}', N'U') IS NOT NULL
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.{table}') AND name = N'AssuranceId')
               AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.{table}') AND name = N'VoyageId')
            BEGIN
                ALTER TABLE [dbo].[{table}] ADD [VoyageId] uniqueidentifier NULL;
            END
            """);

        migrationBuilder.Sql($"""
            IF OBJECT_ID(N'dbo.{table}', N'U') IS NOT NULL
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.{table}') AND name = N'VoyageId')
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.{table}') AND name = N'AssuranceId')
            BEGIN
                UPDATE t
                SET t.[VoyageId] = v.[Id]
                FROM [dbo].[{table}] t
                INNER JOIN [dbo].[Voyages] v ON v.[AssuranceId] = t.[AssuranceId]
                WHERE t.[VoyageId] IS NULL;
            END
            """);

        migrationBuilder.Sql($"""
            IF OBJECT_ID(N'dbo.{table}', N'U') IS NOT NULL
               AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'{oldFkName}')
                ALTER TABLE [dbo].[{table}] DROP CONSTRAINT [{oldFkName}];
            IF OBJECT_ID(N'dbo.{table}', N'U') IS NOT NULL
               AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(N'dbo.{table}')
                          AND referenced_object_id = OBJECT_ID(N'dbo.Assurances'))
            BEGIN
                DECLARE @fk_{table} nvarchar(256);
                SELECT TOP 1 @fk_{table} = fk.name
                FROM sys.foreign_keys fk
                INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
                INNER JOIN sys.columns c ON fkc.parent_column_id = c.column_id AND fkc.parent_object_id = c.object_id
                WHERE fk.parent_object_id = OBJECT_ID(N'dbo.{table}')
                  AND fk.referenced_object_id = OBJECT_ID(N'dbo.Assurances')
                  AND c.name = N'AssuranceId';
                IF @fk_{table} IS NOT NULL
                    EXEC(N'ALTER TABLE [dbo].[{table}] DROP CONSTRAINT [' + @fk_{table} + ']');
            END
            """);

        migrationBuilder.Sql($"""
            IF OBJECT_ID(N'dbo.{table}', N'U') IS NOT NULL
               AND EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'{oldIndexName}' AND object_id = OBJECT_ID(N'dbo.{table}'))
                DROP INDEX [{oldIndexName}] ON [dbo].[{table}];
            IF OBJECT_ID(N'dbo.{table}', N'U') IS NOT NULL
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.{table}') AND name = N'AssuranceId')
            BEGIN
                ALTER TABLE [dbo].[{table}] DROP COLUMN [AssuranceId];
            END
            """);

        migrationBuilder.Sql($"""
            IF OBJECT_ID(N'dbo.{table}', N'U') IS NOT NULL
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.{table}') AND name = N'VoyageId')
               AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'{newFkName}')
            BEGIN
                ALTER TABLE [dbo].[{table}] ALTER COLUMN [VoyageId] uniqueidentifier NOT NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'{newIndexName}' AND object_id = OBJECT_ID(N'dbo.{table}'))
                    CREATE UNIQUE INDEX [{newIndexName}] ON [dbo].[{table}] ([VoyageId]);
                ALTER TABLE [dbo].[{table}] ADD CONSTRAINT [{newFkName}]
                    FOREIGN KEY ([VoyageId]) REFERENCES [dbo].[Voyages] ([Id]) ON DELETE CASCADE;
            END
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'Module')
                ALTER TABLE [dbo].[Assurances] ADD [Module] nvarchar(250) NOT NULL CONSTRAINT [DF_Assurances_Module] DEFAULT '';
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'NomTransporteur')
                ALTER TABLE [dbo].[Assurances] ADD [NomTransporteur] nvarchar(255) NULL;
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'NomNavire')
                ALTER TABLE [dbo].[Assurances] ADD [NomNavire] nvarchar(255) NULL;
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'TypeNavire')
                ALTER TABLE [dbo].[Assurances] ADD [TypeNavire] nvarchar(100) NULL;
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'LieuSejour')
                ALTER TABLE [dbo].[Assurances] ADD [LieuSejour] nvarchar(255) NULL;
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'DureeSejour')
                ALTER TABLE [dbo].[Assurances] ADD [DureeSejour] nvarchar(50) NULL;
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'PaysProvenance')
                ALTER TABLE [dbo].[Assurances] ADD [PaysProvenance] nvarchar(255) NULL;
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'PaysDestination')
                ALTER TABLE [dbo].[Assurances] ADD [PaysDestination] nvarchar(255) NULL;
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Voyages', N'U') IS NOT NULL
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'Module')
            BEGIN
                UPDATE a
                SET
                    a.[Module] = v.[ModuleCode],
                    a.[NomTransporteur] = v.[NomTransporteur],
                    a.[NomNavire] = v.[NomNavire],
                    a.[TypeNavire] = v.[TypeNavire],
                    a.[LieuSejour] = v.[LieuSejour],
                    a.[DureeSejour] = v.[DureeSejour],
                    a.[PaysProvenance] = v.[PaysProvenance],
                    a.[PaysDestination] = v.[PaysDestination]
                FROM [dbo].[Assurances] a
                INNER JOIN [dbo].[Voyages] v ON v.[AssuranceId] = a.[Id];
            END
            """);
    }
}
