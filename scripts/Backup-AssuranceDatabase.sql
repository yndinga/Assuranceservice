-- Sauvegarde de la base MS_ASSURANCE vers un fichier .bak
-- A EXECUTER DANS SSMS (ou sqlcmd), PAS un fichier .ps1.
--
-- Le .bak est cree dans le DOSSIER PAR DEFAUT de SQL Server (pas dans C:\backup).
-- C:\backup n'existe pas par defaut : le script n'utilise pas ce chemin.
-- Apres execution, regardez l'onglet MESSAGES : le chemin exact du fichier s'affiche (PRINT).
-- Dossier typique : C:\Programmes\...\MSSQL16.MSSQLSERVER\MSSQL\Backup\
--
-- Pour utiliser C:\backup : creez le dossier C:\backup, donnez "Modifier" a NT SERVICE\MSSQLSERVER
-- sur ce dossier, puis decommentez le bloc "Option B" ci-dessous et commentez "Option A".

USE master;
GO

-- ========== Option A : dossier par defaut SQL Server (recommandé, pas de C:\backup requis) ==========
DECLARE @BackupDir NVARCHAR(4000);
CREATE TABLE #reg (Value NVARCHAR(100), Data NVARCHAR(4000));
INSERT INTO #reg (Value, Data)
EXEC master.dbo.xp_instance_regread
    N'HKEY_LOCAL_MACHINE',
    N'Software\Microsoft\MSSQLServer\MSSQLServer',
    N'BackupDirectory';
SELECT @BackupDir = Data FROM #reg;
DROP TABLE #reg;

DECLARE @BackupPath NVARCHAR(512) = @BackupDir + N'\MS_ASSURANCE_' + CONVERT(NVARCHAR(20), GETDATE(), 112) + REPLACE(CONVERT(NVARCHAR(12), GETDATE(), 114), ':', '') + N'.bak';

-- ========== Option B : utiliser C:\backup (decommenter si vous avez cree C:\backup et donne les droits au service SQL) ==========
-- DECLARE @BackupPath NVARCHAR(512) = N'C:\backup\MS_ASSURANCE_' + CONVERT(NVARCHAR(20), GETDATE(), 112) + REPLACE(CONVERT(NVARCHAR(12), GETDATE(), 114), ':', '') + N'.bak';
-- (commentez tout le bloc @BackupDir / #reg ci-dessus si vous utilisez Option B)

PRINT N'Backup vers : ' + @BackupPath;

BACKUP DATABASE [MS_ASSURANCE] TO DISK = @BackupPath
WITH NOFORMAT, NOINIT, SKIP, REWIND, NOUNLOAD, STATS = 10;
GO
