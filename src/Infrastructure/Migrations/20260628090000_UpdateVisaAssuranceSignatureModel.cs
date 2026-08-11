using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations;

/// <inheritdoc />
public partial class UpdateVisaAssuranceSignatureModel : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.VisaAssurances', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[VisaAssurances] (
                    [Id] uniqueidentifier NOT NULL,
                    [AssuranceId] uniqueidentifier NOT NULL,
                    [TypePartenaire] nvarchar(50) NOT NULL,
                    [Organisation] nvarchar(250) NOT NULL,
                    [VisaOK] bit NOT NULL,
                    [VisaContent] nvarchar(max) NULL,
                    [Statut] nvarchar(50) NOT NULL,
                    [CreerPar] nvarchar(max) NULL,
                    [ModifierPar] nvarchar(max) NULL,
                    [CreerLe] datetime2 NOT NULL,
                    [ModifierLe] datetime2 NULL,
                    CONSTRAINT [PK_VisaAssurances] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_VisaAssurances_Assurances_AssuranceId] FOREIGN KEY ([AssuranceId]) REFERENCES [dbo].[Assurances] ([Id]) ON DELETE CASCADE
                );

                CREATE UNIQUE INDEX [IX_VisaAssurances_AssuranceId] ON [dbo].[VisaAssurances] ([AssuranceId]);
            END
            ELSE
            BEGIN
                IF COL_LENGTH(N'dbo.VisaAssurances', N'TypePartenaire') IS NULL
                    ALTER TABLE [dbo].[VisaAssurances] ADD [TypePartenaire] nvarchar(50) NOT NULL CONSTRAINT [DF_VisaAssurances_TypePartenaire] DEFAULT N'ASSUREUR';

                IF COL_LENGTH(N'dbo.VisaAssurances', N'Organisation') IS NULL
                    ALTER TABLE [dbo].[VisaAssurances] ADD [Organisation] nvarchar(250) NOT NULL CONSTRAINT [DF_VisaAssurances_Organisation] DEFAULT N'';

                IF COL_LENGTH(N'dbo.VisaAssurances', N'Statut') IS NULL
                    ALTER TABLE [dbo].[VisaAssurances] ADD [Statut] nvarchar(50) NOT NULL CONSTRAINT [DF_VisaAssurances_Statut] DEFAULT N'EN_ATTENTE';

                IF COL_LENGTH(N'dbo.VisaAssurances', N'OrganisationId') IS NOT NULL
                    UPDATE [dbo].[VisaAssurances]
                    SET [Organisation] = CONVERT(nvarchar(36), [OrganisationId])
                    WHERE NULLIF([Organisation], N'') IS NULL;

                IF COL_LENGTH(N'dbo.VisaAssurances', N'OrganisationId') IS NOT NULL
                BEGIN
                    DECLARE @constraintName sysname;
                    SELECT @constraintName = dc.name
                    FROM sys.default_constraints dc
                    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
                    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.VisaAssurances')
                      AND c.name = N'OrganisationId';

                    IF @constraintName IS NOT NULL
                        EXEC(N'ALTER TABLE [dbo].[VisaAssurances] DROP CONSTRAINT [' + @constraintName + N']');

                    ALTER TABLE [dbo].[VisaAssurances] DROP COLUMN [OrganisationId];
                END

                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_VisaAssurances_AssuranceId'
                      AND object_id = OBJECT_ID(N'dbo.VisaAssurances'))
                    CREATE UNIQUE INDEX [IX_VisaAssurances_AssuranceId] ON [dbo].[VisaAssurances] ([AssuranceId]);
            END
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.VisaAssurances', N'U') IS NOT NULL
            BEGIN
                IF COL_LENGTH(N'dbo.VisaAssurances', N'OrganisationId') IS NULL
                    ALTER TABLE [dbo].[VisaAssurances] ADD [OrganisationId] uniqueidentifier NOT NULL CONSTRAINT [DF_VisaAssurances_OrganisationId] DEFAULT '00000000-0000-0000-0000-000000000000';

                IF COL_LENGTH(N'dbo.VisaAssurances', N'TypePartenaire') IS NOT NULL
                    ALTER TABLE [dbo].[VisaAssurances] DROP COLUMN [TypePartenaire];

                IF COL_LENGTH(N'dbo.VisaAssurances', N'Organisation') IS NOT NULL
                    ALTER TABLE [dbo].[VisaAssurances] DROP COLUMN [Organisation];

                IF COL_LENGTH(N'dbo.VisaAssurances', N'Statut') IS NOT NULL
                    ALTER TABLE [dbo].[VisaAssurances] DROP COLUMN [Statut];
            END
            """);
    }
}
