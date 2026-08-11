using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations;

[Migration("20260730190000_AlignOfficialEtatStructure")]
public partial class AlignOfficialEtatStructure : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
IF COL_LENGTH(N'dbo.Etats', N'Code') IS NULL
BEGIN
    ALTER TABLE [dbo].[Etats] ADD [Code] int NOT NULL CONSTRAINT [DF_Etats_Code] DEFAULT (0);
END;

IF COL_LENGTH(N'dbo.Etats', N'UsageUI') IS NULL
BEGIN
    ALTER TABLE [dbo].[Etats] ADD [UsageUI] nvarchar(250) NOT NULL CONSTRAINT [DF_Etats_UsageUI] DEFAULT (N'');
END;

UPDATE [dbo].[Etats]
SET [Code] = TRY_CONVERT(int, [CodeEcran])
WHERE [Code] = 0 AND TRY_CONVERT(int, [CodeEcran]) IS NOT NULL;

MERGE [dbo].[Etats] AS [target]
USING (VALUES
    (42, N'Elaboré', N'Demande en cours de saisie', N'E', N'Badge brouillon/éditable'),
    (79, N'Visas demandés', N'Demande soumise aux signataires', N'VD', N'Liste des dossiers à signer'),
    (50, N'Ouvert', N'Demande signée/validée', N'O', N'Dossiers utilisables par l''étape suivante'),
    (66, N'Modification demandée', N'Correction demandée par un signataire', N'MD', N'Dossiers retournés au déclarant'),
    (68, N'Modification soumise', N'Correction renvoyée au circuit de signature', N'MS', N'Dossiers modifiés en attente'),
    (80, N'Visas refusés', N'Demande refusée', N'VR', N'Dossiers refusés'),
    (55, N'Refusé', N'Refus d''un amendement ou avenant', N'REF', N'Demande secondaire refusée, parent conservé'),
    (51, N'Annulé', N'Demande annulée', N'DA', N'Dossiers annulés'),
    (52, N'Clôturé', N'Processus fermé', N'CL', N'Dossiers clos'),
    (53, N'Prorogé', N'Validité prolongée', N'DP', N'Dossiers/prorogations'),
    (54, N'Modifié', N'Dossier ou contrat modifié', N'DM', N'Historique modification'),
    (57, N'En cours d''amendement', N'Déclaration ouverte avec amendement actif', N'ECA', N'Déclaration bloquée/encadrée par un amendement'),
    (58, N'Avenant en cours', N'Assurance ouverte avec avenant actif', N'AEC', N'Assurance bloquée/encadrée par un avenant'),
    (40, N'Approuvé', N'Amendement approuvé', N'AP', N'Demande d''amendement approuvée'),
    (45, N'Contrôlé', N'Certificat d''origine approuvé', N'CO', N'Certificat d''origine approuvé')
) AS [source] ([Code], [Libelle], [Description], [CodeEcran], [UsageUI])
ON [target].[Code] = [source].[Code]
WHEN MATCHED THEN
    UPDATE SET
        [target].[Libelle] = [source].[Libelle],
        [target].[Description] = [source].[Description],
        [target].[CodeEcran] = [source].[CodeEcran],
        [target].[UsageUI] = [source].[UsageUI],
        [target].[Actif] = CAST(1 AS bit),
        [target].[ModifierLe] = SYSUTCDATETIME(),
        [target].[ModifierPar] = N'System'
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id], [Code], [Libelle], [Description], [CodeEcran], [UsageUI], [Actif], [CreerLe], [CreerPar], [ModifierLe], [ModifierPar])
    VALUES (NEWID(), [source].[Code], [source].[Libelle], [source].[Description], [source].[CodeEcran], [source].[UsageUI], CAST(1 AS bit), SYSUTCDATETIME(), N'System', NULL, NULL);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Etats_Code' AND object_id = OBJECT_ID(N'dbo.Etats'))
BEGIN
    CREATE UNIQUE INDEX [IX_Etats_Code] ON [dbo].[Etats] ([Code]);
END;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Etats_Code' AND object_id = OBJECT_ID(N'dbo.Etats'))
BEGIN
    DROP INDEX [IX_Etats_Code] ON [dbo].[Etats];
END;

IF COL_LENGTH(N'dbo.Etats', N'UsageUI') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[Etats] DROP CONSTRAINT IF EXISTS [DF_Etats_UsageUI];
    ALTER TABLE [dbo].[Etats] DROP COLUMN [UsageUI];
END;

IF COL_LENGTH(N'dbo.Etats', N'Code') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[Etats] DROP CONSTRAINT IF EXISTS [DF_Etats_Code];
    ALTER TABLE [dbo].[Etats] DROP COLUMN [Code];
END;
""");
    }
}
