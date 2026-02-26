-- ============================================================
-- Migration : RefactorTransportToVoyage
-- Objectif  :
--   1. Renommer AssuranceId → VoyageId dans Aeriens, Fluviaux, Maritimes, Routiers
--   2. Supprimer les anciennes colonnes de transport de la table Voyages
--   3. Ajouter LieuSejour et DureeSejour dans la table Voyages
--   4. Ajouter les index unique sur Assurances (NoPolice, NumeroCert)
--   5. Recréer les FK vers Voyages
-- ============================================================
BEGIN TRANSACTION;

-- 1. Supprimer les anciennes FK (Aeriens, Fluviaux, Maritimes, Routiers → Assurances)
ALTER TABLE [Aeriens]  DROP CONSTRAINT IF EXISTS [FK_Aeriens_Assurances_AssuranceId];
ALTER TABLE [Fluviaux] DROP CONSTRAINT IF EXISTS [FK_Fluviaux_Assurances_AssuranceId];
ALTER TABLE [Maritimes] DROP CONSTRAINT IF EXISTS [FK_Maritimes_Assurances_AssuranceId];
ALTER TABLE [Routiers] DROP CONSTRAINT IF EXISTS [FK_Routiers_Assurances_AssuranceId];

-- 2. Supprimer les anciennes colonnes de transport dans Voyages
ALTER TABLE [Voyages] DROP COLUMN IF EXISTS [AeroportDebarquement];
ALTER TABLE [Voyages] DROP COLUMN IF EXISTS [AeroportEmbarquement];
ALTER TABLE [Voyages] DROP COLUMN IF EXISTS [FleuveDebarquement];
ALTER TABLE [Voyages] DROP COLUMN IF EXISTS [FleuveEmbarquement];
ALTER TABLE [Voyages] DROP COLUMN IF EXISTS [PortDebarquement];
ALTER TABLE [Voyages] DROP COLUMN IF EXISTS [PortEmbarquement];
ALTER TABLE [Voyages] DROP COLUMN IF EXISTS [RouteNationale];

-- 3. Renommer AssuranceId → VoyageId dans les tables de transport
-- (supprimer les index existants avant renommage)
DROP INDEX IF EXISTS [IX_Routiers_AssuranceId]  ON [Routiers];
DROP INDEX IF EXISTS [IX_Maritimes_AssuranceId] ON [Maritimes];
DROP INDEX IF EXISTS [IX_Fluviaux_AssuranceId]  ON [Fluviaux];
DROP INDEX IF EXISTS [IX_Aeriens_AssuranceId]   ON [Aeriens];

EXEC sp_rename 'Routiers.AssuranceId',  'VoyageId', 'COLUMN';
EXEC sp_rename 'Maritimes.AssuranceId', 'VoyageId', 'COLUMN';
EXEC sp_rename 'Fluviaux.AssuranceId',  'VoyageId', 'COLUMN';
EXEC sp_rename 'Aeriens.AssuranceId',   'VoyageId', 'COLUMN';

-- 4. Recréer les index après renommage
CREATE INDEX [IX_Routiers_VoyageId]  ON [Routiers]  ([VoyageId]);
CREATE INDEX [IX_Maritimes_VoyageId] ON [Maritimes] ([VoyageId]);
CREATE INDEX [IX_Fluviaux_VoyageId]  ON [Fluviaux]  ([VoyageId]);
CREATE INDEX [IX_Aeriens_VoyageId]   ON [Aeriens]   ([VoyageId]);

-- 5. Ajouter LieuSejour et DureeSejour dans Voyages
ALTER TABLE [Voyages] ADD [LieuSejour]  NVARCHAR(255) NULL;
ALTER TABLE [Voyages] ADD [DureeSejour] NVARCHAR(50)  NULL;

-- 6. Rendre FleuveEmbarquement NOT NULL (Fluviaux)
ALTER TABLE [Fluviaux] ALTER COLUMN [FleuveEmbarquement] NVARCHAR(255) NOT NULL;

-- 7. Rendre AeroportEmbarquement NOT NULL (Aeriens)
ALTER TABLE [Aeriens] ALTER COLUMN [AeroportEmbarquement] NVARCHAR(255) NOT NULL;

-- 8. Index unique sur Assurances
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Assurances_NoPolice' AND object_id = OBJECT_ID('Assurances'))
    CREATE UNIQUE INDEX [IX_Assurances_NoPolice]   ON [Assurances] ([NoPolice])   WHERE [NoPolice] IS NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Assurances_NumeroCert' AND object_id = OBJECT_ID('Assurances'))
    CREATE UNIQUE INDEX [IX_Assurances_NumeroCert] ON [Assurances] ([NumeroCert]) WHERE [NumeroCert] IS NOT NULL;

-- 9. Recréer les FK vers Voyages
ALTER TABLE [Aeriens]  ADD CONSTRAINT [FK_Aeriens_Voyages_VoyageId]
    FOREIGN KEY ([VoyageId]) REFERENCES [Voyages]([Id]) ON DELETE CASCADE;

ALTER TABLE [Fluviaux] ADD CONSTRAINT [FK_Fluviaux_Voyages_VoyageId]
    FOREIGN KEY ([VoyageId]) REFERENCES [Voyages]([Id]) ON DELETE CASCADE;

ALTER TABLE [Maritimes] ADD CONSTRAINT [FK_Maritimes_Voyages_VoyageId]
    FOREIGN KEY ([VoyageId]) REFERENCES [Voyages]([Id]) ON DELETE CASCADE;

ALTER TABLE [Routiers] ADD CONSTRAINT [FK_Routiers_Voyages_VoyageId]
    FOREIGN KEY ([VoyageId]) REFERENCES [Voyages]([Id]) ON DELETE CASCADE;

-- 10. Marquer la migration comme appliquée dans __EFMigrationsHistory
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260219183451_RefactorTransportToVoyage', N'8.0.0');

COMMIT;
