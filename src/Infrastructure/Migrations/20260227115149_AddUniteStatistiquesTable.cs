using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniteStatistiquesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Supprimer les FK et index seulement s'ils existent
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Aeroports_Pays_PaysId' AND parent_object_id = OBJECT_ID(N'Aeroports'))
                    ALTER TABLE [Aeroports] DROP CONSTRAINT [FK_Aeroports_Pays_PaysId];
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ports_Pays_PaysId' AND parent_object_id = OBJECT_ID(N'Ports'))
                    ALTER TABLE [Ports] DROP CONSTRAINT [FK_Ports_Pays_PaysId];
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TauxDeChanges_Devises_DeviseId' AND parent_object_id = OBJECT_ID(N'TauxDeChanges'))
                    ALTER TABLE [TauxDeChanges] DROP CONSTRAINT [FK_TauxDeChanges_Devises_DeviseId];
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Troncons_Corridors_CorridorId' AND parent_object_id = OBJECT_ID(N'Troncons'))
                    ALTER TABLE [Troncons] DROP CONSTRAINT [FK_Troncons_Corridors_CorridorId];
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Troncons_Routes_RouteId' AND parent_object_id = OBJECT_ID(N'Troncons'))
                    ALTER TABLE [Troncons] DROP CONSTRAINT [FK_Troncons_Routes_RouteId];

                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Troncons_CorridorId' AND object_id = OBJECT_ID(N'Troncons'))
                    DROP INDEX [IX_Troncons_CorridorId] ON [Troncons];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Troncons_RouteId' AND object_id = OBJECT_ID(N'Troncons'))
                    DROP INDEX [IX_Troncons_RouteId] ON [Troncons];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TauxDeChanges_DeviseId' AND object_id = OBJECT_ID(N'TauxDeChanges'))
                    DROP INDEX [IX_TauxDeChanges_DeviseId] ON [TauxDeChanges];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ports_PaysId' AND object_id = OBJECT_ID(N'Ports'))
                    DROP INDEX [IX_Ports_PaysId] ON [Ports];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Aeroports_PaysId' AND object_id = OBJECT_ID(N'Aeroports'))
                    DROP INDEX [IX_Aeroports_PaysId] ON [Aeroports];
            ");

            // ALTER uniquement si la table existe (évite erreur sur base vide où InitialCreate n'est pas dans la chaîne EF)
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[Primes]', N'U') IS NOT NULL
                BEGIN
                    DECLARE @dcPrimes sysname;
                    SELECT @dcPrimes = [d].[name]
                    FROM [sys].[default_constraints] [d]
                    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Primes]') AND [c].[name] = N'Accessoires');
                    IF @dcPrimes IS NOT NULL EXEC(N'ALTER TABLE [Primes] DROP CONSTRAINT [' + @dcPrimes + '];');
                    IF EXISTS (
                        SELECT 1 FROM sys.columns
                        WHERE object_id = OBJECT_ID(N'[Primes]') AND name = N'Accessoires'
                        AND system_type_id = TYPE_ID(N'float'))
                        ALTER TABLE [Primes] ALTER COLUMN [Accessoires] decimal(18,2) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'Policies', N'U') IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Policies') AND name = N'CustomerId')
                        ALTER TABLE [Policies] ADD [CustomerId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Policies') AND name = N'EndDate')
                        ALTER TABLE [Policies] ADD [EndDate] datetime2 NOT NULL DEFAULT '0001-01-01';
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Policies') AND name = N'StartDate')
                        ALTER TABLE [Policies] ADD [StartDate] datetime2 NOT NULL DEFAULT '0001-01-01';
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Policies') AND name = N'Status')
                        ALTER TABLE [Policies] ADD [Status] int NOT NULL DEFAULT 0;
                END
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[Cotations]', N'U') IS NOT NULL
                BEGIN
                    DECLARE @dc sysname;

                    SELECT @dc = [d].[name] FROM [sys].[default_constraints] [d] INNER JOIN [sys].[columns] [c]
                        ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                    WHERE [d].[parent_object_id] = OBJECT_ID(N'[Cotations]') AND [c].[name] = N'Taxe';
                    IF @dc IS NOT NULL EXEC(N'ALTER TABLE [Cotations] DROP CONSTRAINT [' + @dc + '];');
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Cotations]') AND name = N'Taxe' AND system_type_id = TYPE_ID(N'float'))
                        ALTER TABLE [Cotations] ALTER COLUMN [Taxe] decimal(18,2) NULL;

                    SELECT @dc = [d].[name] FROM [sys].[default_constraints] [d] INNER JOIN [sys].[columns] [c]
                        ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                    WHERE [d].[parent_object_id] = OBJECT_ID(N'[Cotations]') AND [c].[name] = N'PrimeTotale';
                    IF @dc IS NOT NULL EXEC(N'ALTER TABLE [Cotations] DROP CONSTRAINT [' + @dc + '];');
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Cotations]') AND name = N'PrimeTotale' AND system_type_id = TYPE_ID(N'float'))
                        ALTER TABLE [Cotations] ALTER COLUMN [PrimeTotale] decimal(18,2) NULL;

                    SELECT @dc = [d].[name] FROM [sys].[default_constraints] [d] INNER JOIN [sys].[columns] [c]
                        ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                    WHERE [d].[parent_object_id] = OBJECT_ID(N'[Cotations]') AND [c].[name] = N'PrimeNette';
                    IF @dc IS NOT NULL EXEC(N'ALTER TABLE [Cotations] DROP CONSTRAINT [' + @dc + '];');
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Cotations]') AND name = N'PrimeNette' AND system_type_id = TYPE_ID(N'float'))
                        ALTER TABLE [Cotations] ALTER COLUMN [PrimeNette] decimal(18,2) NULL;

                    SELECT @dc = [d].[name] FROM [sys].[default_constraints] [d] INNER JOIN [sys].[columns] [c]
                        ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                    WHERE [d].[parent_object_id] = OBJECT_ID(N'[Cotations]') AND [c].[name] = N'Montant';
                    IF @dc IS NOT NULL EXEC(N'ALTER TABLE [Cotations] DROP CONSTRAINT [' + @dc + '];');
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Cotations]') AND name = N'Montant' AND system_type_id = TYPE_ID(N'float'))
                        ALTER TABLE [Cotations] ALTER COLUMN [Montant] decimal(18,2) NOT NULL;

                    SELECT @dc = [d].[name] FROM [sys].[default_constraints] [d] INNER JOIN [sys].[columns] [c]
                        ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                    WHERE [d].[parent_object_id] = OBJECT_ID(N'[Cotations]') AND [c].[name] = N'Accessoires';
                    IF @dc IS NOT NULL EXEC(N'ALTER TABLE [Cotations] DROP CONSTRAINT [' + @dc + '];');
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Cotations]') AND name = N'Accessoires' AND system_type_id = TYPE_ID(N'float'))
                        ALTER TABLE [Cotations] ALTER COLUMN [Accessoires] decimal(18,2) NULL;

                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Cotations') AND name = N'ContratId')
                        ALTER TABLE [Cotations] ADD [ContratId] decimal(20,0) NOT NULL DEFAULT 0;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Cotations') AND name = N'PartenaireId')
                        ALTER TABLE [Cotations] ADD [PartenaireId] decimal(20,0) NOT NULL DEFAULT 0;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Cotations') AND name = N'StatutId')
                        ALTER TABLE [Cotations] ADD [StatutId] decimal(20,0) NOT NULL DEFAULT 0;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Cotations') AND name = N'UserId')
                        ALTER TABLE [Cotations] ADD [UserId] decimal(20,0) NOT NULL DEFAULT 0;
                END
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[Assurances]', N'U') IS NOT NULL
                BEGIN
                    ALTER TABLE [Assurances] ALTER COLUMN [TypeContrat] nvarchar(250) NOT NULL;
                    ALTER TABLE [Assurances] ALTER COLUMN [Statut] nvarchar(250) NOT NULL;
                    ALTER TABLE [Assurances] ALTER COLUMN [NumeroCert] nvarchar(250) NULL;
                    ALTER TABLE [Assurances] ALTER COLUMN [ImportateurNIU] nvarchar(250) NOT NULL;
                    ALTER TABLE [Assurances] ALTER COLUMN [Duree] nvarchar(250) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[UniteStatistiques]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [UniteStatistiques] (
                        [Id] uniqueidentifier NOT NULL,
                        [Code] nvarchar(20) NOT NULL,
                        [Nom] nvarchar(100) NOT NULL,
                        [Actif] bit NOT NULL,
                        [CreerPar] nvarchar(max) NULL,
                        [ModifierPar] nvarchar(max) NULL,
                        [CreerLe] datetime2 NOT NULL,
                        [ModifierLe] datetime2 NULL,
                        CONSTRAINT [PK_UniteStatistiques] PRIMARY KEY ([Id])
                    );
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF OBJECT_ID(N'[UniteStatistiques]', N'U') IS NOT NULL DROP TABLE [UniteStatistiques];");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'Policies', N'U') IS NOT NULL
                BEGIN
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Policies') AND name = N'CustomerId')
                        ALTER TABLE [Policies] DROP COLUMN [CustomerId];
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Policies') AND name = N'EndDate')
                        ALTER TABLE [Policies] DROP COLUMN [EndDate];
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Policies') AND name = N'StartDate')
                        ALTER TABLE [Policies] DROP COLUMN [StartDate];
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Policies') AND name = N'Status')
                        ALTER TABLE [Policies] DROP COLUMN [Status];
                END
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[Cotations]', N'U') IS NOT NULL
                BEGIN
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Cotations') AND name = N'UserId')
                        ALTER TABLE [Cotations] DROP COLUMN [UserId];
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Cotations') AND name = N'StatutId')
                        ALTER TABLE [Cotations] DROP COLUMN [StatutId];
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Cotations') AND name = N'PartenaireId')
                        ALTER TABLE [Cotations] DROP COLUMN [PartenaireId];
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Cotations') AND name = N'ContratId')
                        ALTER TABLE [Cotations] DROP COLUMN [ContratId];
                END
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[Primes]', N'U') IS NOT NULL
                BEGIN
                    DECLARE @dcP sysname;
                    SELECT @dcP = [d].[name] FROM [sys].[default_constraints] [d] INNER JOIN [sys].[columns] [c]
                        ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                    WHERE [d].[parent_object_id] = OBJECT_ID(N'[Primes]') AND [c].[name] = N'Accessoires';
                    IF @dcP IS NOT NULL EXEC(N'ALTER TABLE [Primes] DROP CONSTRAINT [' + @dcP + '];');
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Primes]') AND name = N'Accessoires' AND system_type_id = TYPE_ID(N'decimal'))
                        ALTER TABLE [Primes] ALTER COLUMN [Accessoires] float NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[Cotations]', N'U') IS NOT NULL
                BEGIN
                    DECLARE @dcC sysname;
                    SELECT @dcC = [d].[name] FROM [sys].[default_constraints] [d] INNER JOIN [sys].[columns] [c]
                        ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                    WHERE [d].[parent_object_id] = OBJECT_ID(N'[Cotations]') AND [c].[name] = N'Taxe';
                    IF @dcC IS NOT NULL EXEC(N'ALTER TABLE [Cotations] DROP CONSTRAINT [' + @dcC + '];');
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Cotations]') AND name = N'Taxe' AND system_type_id = TYPE_ID(N'decimal'))
                        ALTER TABLE [Cotations] ALTER COLUMN [Taxe] float NULL;
                    SELECT @dcC = [d].[name] FROM [sys].[default_constraints] [d] INNER JOIN [sys].[columns] [c]
                        ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                    WHERE [d].[parent_object_id] = OBJECT_ID(N'[Cotations]') AND [c].[name] = N'PrimeTotale';
                    IF @dcC IS NOT NULL EXEC(N'ALTER TABLE [Cotations] DROP CONSTRAINT [' + @dcC + '];');
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Cotations]') AND name = N'PrimeTotale' AND system_type_id = TYPE_ID(N'decimal'))
                        ALTER TABLE [Cotations] ALTER COLUMN [PrimeTotale] float NULL;
                    SELECT @dcC = [d].[name] FROM [sys].[default_constraints] [d] INNER JOIN [sys].[columns] [c]
                        ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                    WHERE [d].[parent_object_id] = OBJECT_ID(N'[Cotations]') AND [c].[name] = N'PrimeNette';
                    IF @dcC IS NOT NULL EXEC(N'ALTER TABLE [Cotations] DROP CONSTRAINT [' + @dcC + '];');
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Cotations]') AND name = N'PrimeNette' AND system_type_id = TYPE_ID(N'decimal'))
                        ALTER TABLE [Cotations] ALTER COLUMN [PrimeNette] float NULL;
                    SELECT @dcC = [d].[name] FROM [sys].[default_constraints] [d] INNER JOIN [sys].[columns] [c]
                        ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                    WHERE [d].[parent_object_id] = OBJECT_ID(N'[Cotations]') AND [c].[name] = N'Montant';
                    IF @dcC IS NOT NULL EXEC(N'ALTER TABLE [Cotations] DROP CONSTRAINT [' + @dcC + '];');
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Cotations]') AND name = N'Montant' AND system_type_id = TYPE_ID(N'decimal'))
                        ALTER TABLE [Cotations] ALTER COLUMN [Montant] float NOT NULL;
                    SELECT @dcC = [d].[name] FROM [sys].[default_constraints] [d] INNER JOIN [sys].[columns] [c]
                        ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                    WHERE [d].[parent_object_id] = OBJECT_ID(N'[Cotations]') AND [c].[name] = N'Accessoires';
                    IF @dcC IS NOT NULL EXEC(N'ALTER TABLE [Cotations] DROP CONSTRAINT [' + @dcC + '];');
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Cotations]') AND name = N'Accessoires' AND system_type_id = TYPE_ID(N'decimal'))
                        ALTER TABLE [Cotations] ALTER COLUMN [Accessoires] float NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[Assurances]', N'U') IS NOT NULL
                BEGIN
                    ALTER TABLE [Assurances] ALTER COLUMN [TypeContrat] nvarchar(25) NOT NULL;
                    ALTER TABLE [Assurances] ALTER COLUMN [Statut] nvarchar(25) NOT NULL;
                    ALTER TABLE [Assurances] ALTER COLUMN [NumeroCert] nvarchar(25) NULL;
                    ALTER TABLE [Assurances] ALTER COLUMN [ImportateurNIU] nvarchar(25) NOT NULL;
                    ALTER TABLE [Assurances] ALTER COLUMN [Duree] nvarchar(25) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'Troncons', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Troncons_CorridorId' AND object_id = OBJECT_ID(N'Troncons'))
                    CREATE INDEX [IX_Troncons_CorridorId] ON [Troncons] ([CorridorId]);
                IF OBJECT_ID(N'Troncons', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Troncons_RouteId' AND object_id = OBJECT_ID(N'Troncons'))
                    CREATE INDEX [IX_Troncons_RouteId] ON [Troncons] ([RouteId]);
                IF OBJECT_ID(N'TauxDeChanges', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TauxDeChanges_DeviseId' AND object_id = OBJECT_ID(N'TauxDeChanges'))
                    CREATE INDEX [IX_TauxDeChanges_DeviseId] ON [TauxDeChanges] ([DeviseId]);
                IF OBJECT_ID(N'Ports', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ports_PaysId' AND object_id = OBJECT_ID(N'Ports'))
                    CREATE INDEX [IX_Ports_PaysId] ON [Ports] ([PaysId]);
                IF OBJECT_ID(N'Aeroports', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Aeroports_PaysId' AND object_id = OBJECT_ID(N'Aeroports'))
                    CREATE INDEX [IX_Aeroports_PaysId] ON [Aeroports] ([PaysId]);
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'Aeroports', N'U') IS NOT NULL AND OBJECT_ID(N'Pays', N'U') IS NOT NULL
                    AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Aeroports_Pays_PaysId')
                    ALTER TABLE [Aeroports] ADD CONSTRAINT [FK_Aeroports_Pays_PaysId] FOREIGN KEY ([PaysId]) REFERENCES [Pays] ([Id]);
                IF OBJECT_ID(N'Ports', N'U') IS NOT NULL AND OBJECT_ID(N'Pays', N'U') IS NOT NULL
                    AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ports_Pays_PaysId')
                    ALTER TABLE [Ports] ADD CONSTRAINT [FK_Ports_Pays_PaysId] FOREIGN KEY ([PaysId]) REFERENCES [Pays] ([Id]);
                IF OBJECT_ID(N'TauxDeChanges', N'U') IS NOT NULL AND OBJECT_ID(N'Devises', N'U') IS NOT NULL
                    AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TauxDeChanges_Devises_DeviseId')
                    ALTER TABLE [TauxDeChanges] ADD CONSTRAINT [FK_TauxDeChanges_Devises_DeviseId] FOREIGN KEY ([DeviseId]) REFERENCES [Devises] ([Id]) ON DELETE CASCADE;
                IF OBJECT_ID(N'Troncons', N'U') IS NOT NULL AND OBJECT_ID(N'Corridors', N'U') IS NOT NULL
                    AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Troncons_Corridors_CorridorId')
                    ALTER TABLE [Troncons] ADD CONSTRAINT [FK_Troncons_Corridors_CorridorId] FOREIGN KEY ([CorridorId]) REFERENCES [Corridors] ([Id]) ON DELETE CASCADE;
                IF OBJECT_ID(N'Troncons', N'U') IS NOT NULL AND OBJECT_ID(N'Routes', N'U') IS NOT NULL
                    AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Troncons_Routes_RouteId')
                    ALTER TABLE [Troncons] ADD CONSTRAINT [FK_Troncons_Routes_RouteId] FOREIGN KEY ([RouteId]) REFERENCES [Routes] ([Id]) ON DELETE CASCADE;
            ");
        }
    }
}
