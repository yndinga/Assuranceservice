-- Insertion des statuts référentiel (assurance)
-- Base : MS_ASSURANCE. Exécuter après création de la table Statuts.
-- Id fixes pour reproductibilité ; ré-exécutable (MERGE sur Code).

USE [MS_ASSURANCE];
GO

SET NOCOUNT ON;

;MERGE [dbo].[Statuts] AS t
USING (VALUES
    (N'10', N'Elaboré'),
    (N'11', N'Visa demandé'),
    (N'12', N'Modification soumise'),
    (N'13', N'Validé'),
    (N'14', N'Modification demandée'),
    (N'15', N'Attente Validation'),
    (N'16', N'Payé'),
    (N'17', N'Approuvé')
) AS s ([Code], [Libelle])
ON t.[Code] = s.[Code]
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id], [Code], [Libelle], [CreerLe])
    VALUES (NEWID(), s.[Code], s.[Libelle], GETUTCDATE())
WHEN MATCHED AND t.[Libelle] <> s.[Libelle] THEN
    UPDATE SET t.[Libelle] = s.[Libelle], t.[ModifierLe] = GETUTCDATE();

PRINT 'Statuts référentiel : merge terminé.';
GO
