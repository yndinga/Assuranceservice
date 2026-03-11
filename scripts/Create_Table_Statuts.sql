-- Création de la table Statuts (référentiel) pour MS_ASSURANCE
-- Exécuter ce script si la migration n'a pas été appliquée (ex: dotnet ef database update impossible)

USE [MS_ASSURANCE];
GO

IF OBJECT_ID(N'[dbo].[Statuts]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Statuts](
        [Id] [uniqueidentifier] NOT NULL,
        [Code] [nvarchar](50) NOT NULL,
        [Libelle] [nvarchar](250) NOT NULL,
        [CreerPar] [nvarchar](max) NULL,
        [ModifierPar] [nvarchar](max) NULL,
        [CreerLe] [datetime2](7) NOT NULL,
        [ModifierLe] [datetime2](7) NULL,
        CONSTRAINT [PK_Statuts] PRIMARY KEY CLUSTERED ([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_Statuts_Code] ON [dbo].[Statuts]([Code]);

    PRINT 'Table Statuts créée.';
END
ELSE
    PRINT 'Table Statuts existe déjà.';
GO

-- Optionnel : enregistrer la migration pour éviter qu''EF ne tente de la réappliquer
IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20260228110000_AddStatutsTable')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260228110000_AddStatutsTable', N'8.0.0');
    PRINT 'Migration AddStatutsTable enregistrée.';
END
GO
