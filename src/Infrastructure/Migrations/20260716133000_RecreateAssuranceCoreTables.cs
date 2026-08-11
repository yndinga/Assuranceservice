using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations
{
    [Migration("20260716133000_RecreateAssuranceCoreTables")]
    public partial class RecreateAssuranceCoreTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DECLARE @dropFkSql nvarchar(max) = N'';

                SELECT @dropFkSql +=
                    N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(parent.schema_id)) + N'.' + QUOTENAME(parent.name) +
                    N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';'
                FROM sys.foreign_keys fk
                INNER JOIN sys.objects parent ON parent.object_id = fk.parent_object_id
                WHERE fk.referenced_object_id IN (
                    OBJECT_ID(N'dbo.Assurances'),
                    OBJECT_ID(N'dbo.Voyages'),
                    OBJECT_ID(N'dbo.Documents'),
                    OBJECT_ID(N'dbo.Avenants')
                )
                OR fk.parent_object_id IN (
                    OBJECT_ID(N'dbo.Assurances'),
                    OBJECT_ID(N'dbo.Voyages'),
                    OBJECT_ID(N'dbo.Primes'),
                    OBJECT_ID(N'dbo.Maritimes'),
                    OBJECT_ID(N'dbo.Aeriens'),
                    OBJECT_ID(N'dbo.Routiers'),
                    OBJECT_ID(N'dbo.Fluviaux'),
                    OBJECT_ID(N'dbo.VisaAssurances'),
                    OBJECT_ID(N'dbo.Documents'),
                    OBJECT_ID(N'dbo.Avenants'),
                    OBJECT_ID(N'dbo.Historiques')
                );

                IF @dropFkSql <> N''
                    EXEC sp_executesql @dropFkSql;

                IF OBJECT_ID(N'dbo.Commentaires', N'U') IS NOT NULL
                   AND OBJECT_ID(N'dbo.Documents', N'U') IS NOT NULL
                    DELETE c
                    FROM [dbo].[Commentaires] c
                    INNER JOIN [dbo].[Documents] d ON d.[Id] = c.[DocumentId];

                IF OBJECT_ID(N'dbo.Documents', N'U') IS NOT NULL
                    DELETE FROM [dbo].[Documents];

                IF OBJECT_ID(N'dbo.Historiques', N'U') IS NOT NULL
                    DELETE FROM [dbo].[Historiques];

                IF OBJECT_ID(N'dbo.Avenants', N'U') IS NOT NULL
                    DELETE FROM [dbo].[Avenants];

                IF OBJECT_ID(N'dbo.AssuranceProcessStates', N'U') IS NOT NULL
                    DELETE FROM [dbo].[AssuranceProcessStates];

                DROP TABLE IF EXISTS [dbo].[Aeriens];
                DROP TABLE IF EXISTS [dbo].[Maritimes];
                DROP TABLE IF EXISTS [dbo].[Routiers];
                DROP TABLE IF EXISTS [dbo].[Fluviaux];
                DROP TABLE IF EXISTS [dbo].[VisaAssurances];
                DROP TABLE IF EXISTS [dbo].[Primes];
                DROP TABLE IF EXISTS [dbo].[Voyages];
                DROP TABLE IF EXISTS [dbo].[Assurances];

                CREATE TABLE [dbo].[Assurances] (
                    [Id] uniqueidentifier NOT NULL,
                    [NoPolice] nvarchar(255) NULL,
                    [NumeroCert] nvarchar(25) NULL,
                    [NoFacture] nvarchar(250) NULL,
                    [Partenaire] nvarchar(250) NULL,
                    [Intermediaire] nvarchar(250) NULL,
                    [TypePartenaire] nvarchar(50) NOT NULL CONSTRAINT [DF_Assurances_TypePartenaire] DEFAULT N'ASSUREUR',
                    [ImportateurNom] nvarchar(250) NOT NULL,
                    [ImportateurNIU] nvarchar(25) NOT NULL,
                    [DateDebut] datetime2 NULL,
                    [DateFin] datetime2 NULL,
                    [TypeContrat] nvarchar(25) NOT NULL,
                    [Duree] nvarchar(25) NULL,
                    [GarantieId] uniqueidentifier NULL,
                    [OCRE] nvarchar(250) NOT NULL,
                    [PCRE] nvarchar(250) NOT NULL,
                    [ModeDeTransport] nvarchar(10) NOT NULL,
                    [Statut] nvarchar(10) NOT NULL CONSTRAINT [DF_Assurances_Statut] DEFAULT N'10',
                    [CreerPar] nvarchar(max) NULL,
                    [ModifierPar] nvarchar(max) NULL,
                    [CreerLe] datetime2 NOT NULL,
                    [ModifierLe] datetime2 NULL,
                    CONSTRAINT [PK_Assurances] PRIMARY KEY ([Id])
                );

                IF OBJECT_ID(N'dbo.Garanties', N'U') IS NOT NULL
                    ALTER TABLE [dbo].[Assurances] WITH CHECK
                    ADD CONSTRAINT [FK_Assurances_Garanties_GarantieId]
                    FOREIGN KEY ([GarantieId]) REFERENCES [dbo].[Garanties] ([Id]);

                CREATE UNIQUE INDEX [IX_Assurances_NoPolice] ON [dbo].[Assurances] ([NoPolice]) WHERE [NoPolice] IS NOT NULL;
                CREATE UNIQUE INDEX [IX_Assurances_NumeroCert] ON [dbo].[Assurances] ([NumeroCert]) WHERE [NumeroCert] IS NOT NULL;
                CREATE INDEX [IX_Assurances_GarantieId] ON [dbo].[Assurances] ([GarantieId]);
                CREATE INDEX [IX_Assurances_Partenaire] ON [dbo].[Assurances] ([Partenaire]);
                CREATE INDEX [IX_Assurances_Intermediaire] ON [dbo].[Assurances] ([Intermediaire]);

                CREATE TABLE [dbo].[Voyages] (
                    [Id] uniqueidentifier NOT NULL,
                    [AssuranceId] uniqueidentifier NOT NULL,
                    [NomTransporteur] nvarchar(255) NOT NULL,
                    [LieuSejour] nvarchar(255) NULL,
                    [DureeSejour] nvarchar(50) NULL,
                    [PaysProvenance] nvarchar(255) NOT NULL,
                    [PaysDestination] nvarchar(255) NOT NULL,
                    [Designation] nvarchar(255) NOT NULL,
                    [Nature] nvarchar(500) NULL,
                    [Specificites] nvarchar(100) NULL,
                    [Conditionnement] nvarchar(500) NULL,
                    [DescriptionConditionnement] nvarchar(500) NULL,
                    [Devise] nvarchar(50) NULL,
                    [MasseBrute] nvarchar(255) NULL,
                    [UniteStatistique] nvarchar(255) NULL,
                    [Marque] nvarchar(255) NULL,
                    [CreerPar] nvarchar(max) NULL,
                    [ModifierPar] nvarchar(max) NULL,
                    [CreerLe] datetime2 NOT NULL,
                    [ModifierLe] datetime2 NULL,
                    CONSTRAINT [PK_Voyages] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Voyages_Assurances_AssuranceId] FOREIGN KEY ([AssuranceId]) REFERENCES [dbo].[Assurances] ([Id]) ON DELETE CASCADE
                );

                CREATE UNIQUE INDEX [IX_Voyages_AssuranceId] ON [dbo].[Voyages] ([AssuranceId]);

                CREATE TABLE [dbo].[Primes] (
                    [Id] uniqueidentifier NOT NULL,
                    [AssuranceId] uniqueidentifier NOT NULL,
                    [Taux] decimal(18,4) NULL,
                    [ValeurDevise] decimal(18,2) NOT NULL,
                    [ValeurFCFA] decimal(18,2) NOT NULL,
                    [PrimeNette] decimal(18,2) NULL,
                    [Accessoires] decimal(18,2) NULL,
                    [Taxe] decimal(18,2) NULL,
                    [PrimeTotale] decimal(18,2) NULL,
                    [Statut] nvarchar(max) NOT NULL,
                    [CreerPar] nvarchar(max) NULL,
                    [ModifierPar] nvarchar(max) NULL,
                    [CreerLe] datetime2 NOT NULL,
                    [ModifierLe] datetime2 NULL,
                    CONSTRAINT [PK_Primes] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Primes_Assurances_AssuranceId] FOREIGN KEY ([AssuranceId]) REFERENCES [dbo].[Assurances] ([Id]) ON DELETE CASCADE
                );

                CREATE INDEX [IX_Primes_AssuranceId] ON [dbo].[Primes] ([AssuranceId]);

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

                CREATE TABLE [dbo].[Maritimes] (
                    [Id] uniqueidentifier NOT NULL,
                    [VoyageId] uniqueidentifier NOT NULL,
                    [PortEmbarquementCode] nvarchar(50) NULL,
                    [PortDebarquementCode] nvarchar(50) NULL,
                    [NumeroBL] nvarchar(255) NULL,
                    [NomNavire] nvarchar(255) NULL,
                    [TypeNavire] nvarchar(100) NULL,
                    [CreerPar] nvarchar(max) NULL,
                    [ModifierPar] nvarchar(max) NULL,
                    [CreerLe] datetime2 NOT NULL,
                    [ModifierLe] datetime2 NULL,
                    CONSTRAINT [PK_Maritimes] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Maritimes_Voyages_VoyageId] FOREIGN KEY ([VoyageId]) REFERENCES [dbo].[Voyages] ([Id]) ON DELETE CASCADE
                );

                CREATE UNIQUE INDEX [IX_Maritimes_VoyageId] ON [dbo].[Maritimes] ([VoyageId]);

                CREATE TABLE [dbo].[Fluviaux] (
                    [Id] uniqueidentifier NOT NULL,
                    [VoyageId] uniqueidentifier NOT NULL,
                    [PortEmbarquementCode] nvarchar(50) NULL,
                    [PortDebarquementCode] nvarchar(50) NULL,
                    [NomNavire] nvarchar(255) NULL,
                    [TypeNavire] nvarchar(100) NULL,
                    [CreerPar] nvarchar(max) NULL,
                    [ModifierPar] nvarchar(max) NULL,
                    [CreerLe] datetime2 NOT NULL,
                    [ModifierLe] datetime2 NULL,
                    CONSTRAINT [PK_Fluviaux] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Fluviaux_Voyages_VoyageId] FOREIGN KEY ([VoyageId]) REFERENCES [dbo].[Voyages] ([Id]) ON DELETE CASCADE
                );

                CREATE UNIQUE INDEX [IX_Fluviaux_VoyageId] ON [dbo].[Fluviaux] ([VoyageId]);

                CREATE TABLE [dbo].[Aeriens] (
                    [Id] uniqueidentifier NOT NULL,
                    [VoyageId] uniqueidentifier NOT NULL,
                    [AeroportEmbarquementCode] nvarchar(50) NULL,
                    [AeroportDebarquementCode] nvarchar(50) NULL,
                    [NumeroLTA] nvarchar(255) NULL,
                    [CreerPar] nvarchar(max) NULL,
                    [ModifierPar] nvarchar(max) NULL,
                    [CreerLe] datetime2 NOT NULL,
                    [ModifierLe] datetime2 NULL,
                    CONSTRAINT [PK_Aeriens] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Aeriens_Voyages_VoyageId] FOREIGN KEY ([VoyageId]) REFERENCES [dbo].[Voyages] ([Id]) ON DELETE CASCADE
                );

                CREATE UNIQUE INDEX [IX_Aeriens_VoyageId] ON [dbo].[Aeriens] ([VoyageId]);

                CREATE TABLE [dbo].[Routiers] (
                    [Id] uniqueidentifier NOT NULL,
                    [VoyageId] uniqueidentifier NOT NULL,
                    [RouteNationaleCode] nvarchar(50) NULL,
                    [NumeroLV] nvarchar(255) NULL,
                    [CreerPar] nvarchar(max) NULL,
                    [ModifierPar] nvarchar(max) NULL,
                    [CreerLe] datetime2 NOT NULL,
                    [ModifierLe] datetime2 NULL,
                    CONSTRAINT [PK_Routiers] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Routiers_Voyages_VoyageId] FOREIGN KEY ([VoyageId]) REFERENCES [dbo].[Voyages] ([Id]) ON DELETE CASCADE
                );

                CREATE UNIQUE INDEX [IX_Routiers_VoyageId] ON [dbo].[Routiers] ([VoyageId]);

                IF OBJECT_ID(N'dbo.Documents', N'U') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[Documents] WITH CHECK
                    ADD CONSTRAINT [FK_Documents_Assurances_AssuranceId]
                    FOREIGN KEY ([AssuranceId]) REFERENCES [dbo].[Assurances] ([Id]) ON DELETE CASCADE;
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Documents_AssuranceId' AND object_id = OBJECT_ID(N'dbo.Documents'))
                        CREATE INDEX [IX_Documents_AssuranceId] ON [dbo].[Documents] ([AssuranceId]);
                END

                IF OBJECT_ID(N'dbo.Avenants', N'U') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[Avenants] WITH CHECK
                    ADD CONSTRAINT [FK_Avenants_Assurances_AssuranceId]
                    FOREIGN KEY ([AssuranceId]) REFERENCES [dbo].[Assurances] ([Id]) ON DELETE CASCADE;
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Avenants_AssuranceId_NoAvenant' AND object_id = OBJECT_ID(N'dbo.Avenants'))
                        CREATE UNIQUE INDEX [IX_Avenants_AssuranceId_NoAvenant] ON [dbo].[Avenants] ([AssuranceId], [NoAvenant]);
                END
            """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                THROW 51000, 'RecreateAssuranceCoreTables is destructive and cannot be reverted automatically.', 1;
            """);
        }
    }
}
