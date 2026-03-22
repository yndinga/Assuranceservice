-- Restauration de la base MS_ASSURANCE depuis un fichier .bak
-- A EXECUTER DANS SSMS (ou sqlcmd), PAS le fichier .ps1 qui est du PowerShell.
--
-- 1) Remplacez N'C:\backup\MS_ASSURANCE.bak' par le chemin reel du .bak (tel que vu par SQL Server).
-- 2) Si la base MS_ASSURANCE EXISTE deja : executez le bloc "Option A" ci-dessous.
-- 3) Si la base N'EXISTE PAS : executez uniquement le bloc "Option B".

-- ========== Option A : la base existe deja (remplacement) ==========
USE master;
GO
ALTER DATABASE [MS_ASSURANCE] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO
RESTORE DATABASE [MS_ASSURANCE] FROM DISK = N'C:\backup\MS_ASSURANCE.bak' WITH REPLACE, RECOVERY;
GO
ALTER DATABASE [MS_ASSURANCE] SET MULTI_USER;
GO

-- ========== Option B : la base n'existe pas (premiere restauration) ==========
-- Decommentez les 2 lignes ci-dessous et commentez Option A si la base n'existe pas.
/*
USE master;
GO
RESTORE DATABASE [MS_ASSURANCE] FROM DISK = N'C:\backup\MS_ASSURANCE.bak' WITH RECOVERY;
GO
*/
