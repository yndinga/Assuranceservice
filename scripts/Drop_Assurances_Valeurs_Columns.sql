USE [MS_ASSURANCE];
GO

IF OBJECT_ID(N'[dbo].[Assurances]', N'U') IS NULL
BEGIN
    PRINT 'Table [dbo].[Assurances] introuvable.';
    RETURN;
END
GO

IF COL_LENGTH(N'[dbo].[Assurances]', N'ValeurFCFA') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[Assurances] DROP COLUMN [ValeurFCFA];
    PRINT 'Colonne [ValeurFCFA] supprimée de [Assurances].';
END
ELSE
BEGIN
    PRINT 'Colonne [ValeurFCFA] déjà absente.';
END
GO

IF COL_LENGTH(N'[dbo].[Assurances]', N'ValeurDevise') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[Assurances] DROP COLUMN [ValeurDevise];
    PRINT 'Colonne [ValeurDevise] supprimée de [Assurances].';
END
ELSE
BEGIN
    PRINT 'Colonne [ValeurDevise] déjà absente.';
END
GO
