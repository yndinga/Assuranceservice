-- =============================================================================
-- Supprime la table Voyages et la recrée avec ModuleCode après PaysDestination.
-- ATTENTION : toutes les données Voyages + Aeriens, Maritimes, Routiers, Fluviaux
-- sont supprimées. À exécuter uniquement si vous acceptez cette perte.
-- Exécuter sur la base utilisée par l'API (ex: MS_ASSURANCE).
-- =============================================================================

BEGIN TRANSACTION;

-- 1. Supprimer les FK des tables enfants vers Voyages
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Aeriens_Voyages_VoyageId')
    ALTER TABLE [dbo].[Aeriens] DROP CONSTRAINT [FK_Aeriens_Voyages_VoyageId];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Maritimes_Voyages_VoyageId')
    ALTER TABLE [dbo].[Maritimes] DROP CONSTRAINT [FK_Maritimes_Voyages_VoyageId];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Routiers_Voyages_VoyageId')
    ALTER TABLE [dbo].[Routiers] DROP CONSTRAINT [FK_Routiers_Voyages_VoyageId];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Fluviaux_Voyages_VoyageId')
    ALTER TABLE [dbo].[Fluviaux] DROP CONSTRAINT [FK_Fluviaux_Voyages_VoyageId];

-- 2. Vider les tables enfants (VoyageId ne pointera plus vers des lignes valides)
TRUNCATE TABLE [dbo].[Aeriens];
TRUNCATE TABLE [dbo].[Maritimes];
TRUNCATE TABLE [dbo].[Routiers];
TRUNCATE TABLE [dbo].[Fluviaux];

-- 3. Supprimer la FK Voyages -> Assurances, puis la table Voyages
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Voyages_Assurances_AssuranceId')
    ALTER TABLE [dbo].[Voyages] DROP CONSTRAINT [FK_Voyages_Assurances_AssuranceId];

DROP TABLE IF EXISTS [dbo].[Voyages];

-- 4. Recréer Voyages avec ModuleCode après PaysDestination
CREATE TABLE [dbo].[Voyages] (
    [Id]                uniqueidentifier NOT NULL,
    [AssuranceId]       uniqueidentifier NOT NULL,
    [NomTransporteur]   nvarchar(255)    NOT NULL,
    [NomNavire]         nvarchar(255)    NOT NULL,
    [TypeNavire]        nvarchar(100)    NOT NULL,
    [LieuSejour]        nvarchar(255)    NULL,
    [DureeSejour]       nvarchar(50)     NULL,
    [PaysProvenance]    nvarchar(255)    NOT NULL,
    [PaysDestination]   nvarchar(255)    NOT NULL,
    [ModuleCode]        nvarchar(10)     NOT NULL,
    [CreerPar]          nvarchar(max)    NULL,
    [ModifierPar]       nvarchar(max)    NULL,
    [CreerLe]           datetime2        NOT NULL,
    [ModifierLe]        datetime2        NULL,
    CONSTRAINT [PK_Voyages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Voyages_Assurances_AssuranceId] FOREIGN KEY ([AssuranceId])
        REFERENCES [dbo].[Assurances] ([Id]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [IX_Voyages_AssuranceId] ON [dbo].[Voyages] ([AssuranceId]);

-- 5. Recréer les FK des tables enfants vers Voyages
ALTER TABLE [dbo].[Aeriens] ADD CONSTRAINT [FK_Aeriens_Voyages_VoyageId]
    FOREIGN KEY ([VoyageId]) REFERENCES [dbo].[Voyages] ([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[Maritimes] ADD CONSTRAINT [FK_Maritimes_Voyages_VoyageId]
    FOREIGN KEY ([VoyageId]) REFERENCES [dbo].[Voyages] ([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[Routiers] ADD CONSTRAINT [FK_Routiers_Voyages_VoyageId]
    FOREIGN KEY ([VoyageId]) REFERENCES [dbo].[Voyages] ([Id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[Fluviaux] ADD CONSTRAINT [FK_Fluviaux_Voyages_VoyageId]
    FOREIGN KEY ([VoyageId]) REFERENCES [dbo].[Voyages] ([Id]) ON DELETE CASCADE;

COMMIT TRANSACTION;
PRINT 'Table Voyages recréée avec ModuleCode après PaysDestination.';
GO
