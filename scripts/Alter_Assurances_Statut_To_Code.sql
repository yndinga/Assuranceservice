-- Modifier la table Assurances : Statut = code (nvarchar 10), défaut '10' (Elaboré)
-- Exécuter sur MS_ASSURANCE après avoir inséré le référentiel Statuts (10=Elaboré, 11=Visa demandé, ...)

USE [MS_ASSURANCE];
GO

-- Supprimer la FK StatutId si elle existe (nom généré par EF)
DECLARE @fk nvarchar(128);
SELECT @fk = name FROM sys.foreign_keys 
WHERE parent_object_id = OBJECT_ID(N'[dbo].[Assurances]') AND name LIKE N'FK_%Statut%';
IF @fk IS NOT NULL
BEGIN
    EXEC('ALTER TABLE [dbo].[Assurances] DROP CONSTRAINT [' + @fk + ']');
    PRINT 'Contrainte FK Statut supprimée.';
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Assurances]') AND name = N'StatutId')
BEGIN
    ALTER TABLE [dbo].[Assurances] DROP COLUMN [StatutId];
    PRINT 'Colonne StatutId supprimée.';
END
GO

-- Ajouter la colonne Statut (code) si elle n'existe pas
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Assurances]') AND name = N'Statut')
BEGIN
    ALTER TABLE [dbo].[Assurances] ADD [Statut] nvarchar(10) NOT NULL DEFAULT N'10';
    PRINT 'Colonne Statut (code) ajoutée, défaut 10 (Elaboré).';
END
ELSE
    PRINT 'Colonne Statut existe déjà.';
GO
