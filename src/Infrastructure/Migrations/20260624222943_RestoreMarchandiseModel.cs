using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations;

/// <inheritdoc />
public partial class RestoreMarchandiseModel : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'PCRE')
                ALTER TABLE [dbo].[Assurances] ADD [PCRE] nvarchar(250) NOT NULL CONSTRAINT [DF_Assurances_PCRE] DEFAULT '';
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Marchandises', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[Marchandises] (
                    [Id]                uniqueidentifier NOT NULL,
                    [AssuranceId]       uniqueidentifier NOT NULL,
                    [Designation]       nvarchar(255)    NULL,
                    [Nature]            nvarchar(500)    NULL,
                    [Specificites]      nvarchar(100)    NULL,
                    [Conditionnement]   nvarchar(500)    NULL,
                    [Description]       nvarchar(500)    NULL,
                    [Devise]            nvarchar(50)     NULL,
                    [MasseBrute]        nvarchar(255)    NULL,
                    [UniteStatistique]  nvarchar(255)    NULL,
                    [Marque]            nvarchar(255)    NULL,
                    [CreerPar]          nvarchar(max)    NULL,
                    [ModifierPar]       nvarchar(max)    NULL,
                    [CreerLe]           datetime2        NOT NULL,
                    [ModifierLe]        datetime2        NULL,
                    CONSTRAINT [PK_Marchandises] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Marchandises_Assurances_AssuranceId] FOREIGN KEY ([AssuranceId])
                        REFERENCES [dbo].[Assurances] ([Id]) ON DELETE CASCADE
                );
            END
            """);

        migrationBuilder.Sql("""
            IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'Designation')
            BEGIN
                INSERT INTO [dbo].[Marchandises] (
                    [Id], [AssuranceId], [Designation], [Nature], [Specificites], [Conditionnement],
                    [Description], [Devise], [MasseBrute], [UniteStatistique], [Marque],
                    [CreerPar], [ModifierPar], [CreerLe], [ModifierLe]
                )
                SELECT
                    NEWID(),
                    a.[Id],
                    a.[Designation],
                    a.[Nature],
                    a.[Specificites],
                    a.[Conditionnement],
                    a.[Description],
                    a.[Devise],
                    a.[MasseBrute],
                    a.[UniteStatistique],
                    a.[Marque],
                    a.[CreerPar],
                    a.[ModifierPar],
                    ISNULL(a.[CreerLe], SYSUTCDATETIME()),
                    a.[ModifierLe]
                FROM [dbo].[Assurances] a
                WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Marchandises] m WHERE m.[AssuranceId] = a.[Id]);
            END
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Marchandises', N'U') IS NOT NULL
            BEGIN
                ;WITH ranked AS (
                    SELECT [Id], ROW_NUMBER() OVER (PARTITION BY [AssuranceId] ORDER BY [CreerLe] DESC, [Id]) AS rn
                    FROM [dbo].[Marchandises]
                )
                DELETE m
                FROM [dbo].[Marchandises] m
                INNER JOIN ranked r ON r.[Id] = m.[Id]
                WHERE r.rn > 1;
            END
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Marchandises', N'U') IS NOT NULL
               AND EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Marchandises_AssuranceId' AND object_id = OBJECT_ID(N'dbo.Marchandises') AND is_unique = 0)
                DROP INDEX [IX_Marchandises_AssuranceId] ON [dbo].[Marchandises];
            IF OBJECT_ID(N'dbo.Marchandises', N'U') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Marchandises_AssuranceId' AND object_id = OBJECT_ID(N'dbo.Marchandises'))
                CREATE UNIQUE INDEX [IX_Marchandises_AssuranceId] ON [dbo].[Marchandises] ([AssuranceId]);
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'PCRE')
                ALTER TABLE [dbo].[Assurances] DROP COLUMN [PCRE];
            """);
    }
}
