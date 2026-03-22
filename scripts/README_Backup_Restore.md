# Sauvegarde et restauration des données (vers Portainer)

Procédure simple pour sauvegarder la base **MS_ASSURANCE** (SQL Server) et la restaurer côté Portainer.

---

## 1. Sauvegarder les données (côté source)

**Méthode rapide dans SSMS :** ouvrez `Backup-AssuranceDatabase.sql`, modifiez le chemin `N'C:\backup\MS_ASSURANCE.bak'` (dossier existant, droits en écriture pour SQL Server), puis exécutez (F5). Le fichier `.bak` sera créé à cet emplacement.

---

### Option A : SQL Server dans Docker

1. Démarrez SQL Server avec un volume monté pour les backups :
   ```powershell
   docker run -d --name sqlserver -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=VotreMotDePasse" -p 1420:1433 -v "${PWD}/backup:/var/opt/mssql/backup" mcr.microsoft.com/mssql/server:2022-latest
   ```

2. Depuis la racine du projet (ou `scripts`) :
   ```powershell
   cd D:\dev_netcore\microservice\AssuranceService\scripts
   .\Backup-AssuranceDatabase.ps1 -ConnectionString "Server=localhost,1420;Database=MS_ASSURANCE;User Id=sa;Password=VotreMotDePasse;TrustServerCertificate=True;" -BackupPathOnServer "/var/opt/mssql/backup/MS_ASSURANCE.bak"
   ```

3. Le fichier `.bak` apparaît dans le dossier `backup` sur votre machine. **Copiez ce fichier** vers le serveur où tourne Portainer (ou vers un partage accessible par le serveur SQL cible).

### Option B : SQL Server installé sur la machine (Windows)

1. Créez un dossier pour les backups (ex. `C:\backup`) et donnez les droits en écriture au compte du service SQL Server.

2. Lancez le backup :
   ```powershell
   .\Backup-AssuranceDatabase.ps1 -ConnectionString "Server=localhost;Database=MS_ASSURANCE;User Id=sa;Password=VotreMotDePasse;TrustServerCertificate=True;" -BackupPathOnServer "C:\backup\MS_ASSURANCE.bak"
   ```

3. Copiez `C:\backup\MS_ASSURANCE.bak` vers le serveur Portainer (ou vers le serveur SQL utilisé par Portainer).

### Option C : SQL Server déjà sur le serveur Portainer (192.168.3.178)

Si vous faites le backup **depuis** la machine qui héberge déjà SQL (ex. 192.168.3.178), connectez-vous à cette machine et exécutez le script avec la chaîne de connexion locale, en indiquant un chemin où SQL Server peut écrire (ex. `C:\backup\MS_ASSURANCE.bak` ou `/var/opt/mssql/backup/...` selon l’OS). Puis utilisez ce même fichier pour la restauration (voir ci‑dessous).

---

## 2. Restaurer les données (côté Portainer)

Sur la machine où SQL Server est accessible par Portainer (ou sur un conteneur SQL avec volume) :

1. Placez le fichier `.bak` dans un emplacement accessible par SQL Server :
   - **SQL dans Docker** : montez un volume (ex. `./backup:/var/opt/mssql/backup`) et mettez le `.bak` dans `./backup` sur l’hôte.
   - **SQL sur Windows** : copiez le `.bak` dans un dossier (ex. `C:\backup`).

2. Restauration :
   - **Dans SSMS** : ouvrez `Restore-AssuranceDatabase.sql`, modifiez le chemin du `.bak` (N'C:\backup\MS_ASSURANCE.bak'), puis exécutez le script (F5). Ne pas ouvrir le fichier `.ps1` dans SSMS.
   - **Dans PowerShell** (sur une machine où le module SQL est disponible) :
   ```powershell
   .\Restore-AssuranceDatabase.ps1 -BackupPathOnServer "C:\backup\MS_ASSURANCE.bak" -ConnectionString "Server=192.168.3.178,1434;User Id=sa;Password=MonMotDePasseFort!;TrustServerCertificate=True;"
   ```
   Ou si SQL est dans Docker avec volume `/var/opt/mssql/backup` :
   ```powershell
   .\Restore-AssuranceDatabase.ps1 -BackupPathOnServer "/var/opt/mssql/backup/MS_ASSURANCE.bak" -ConnectionString "Server=192.168.3.178,1434;User Id=sa;Password=MonMotDePasseFort!;TrustServerCertificate=True;"
   ```

3. Vérifiez que la stack Portainer (ou le service) utilise la même chaîne de connexion (`SQL_CONNECTION_STRING` ou `ConnectionStrings__AssuranceConnection`) pointant vers cette instance et la base `MS_ASSURANCE`.

---

## 3. Résumé du flux

| Étape | Où | Action |
|--------|-----|--------|
| 1 | Machine source | Lancer `Backup-AssuranceDatabase.ps1` → un fichier `.bak` est créé |
| 2 | Transfert | Copier le `.bak` vers le serveur Portainer (ou le serveur SQL cible) |
| 3 | Serveur Portainer / SQL | Placer le `.bak` dans un chemin lisible par SQL (volume ou dossier local) |
| 4 | Serveur Portainer / SQL | Lancer `Restore-AssuranceDatabase.ps1` avec ce chemin et la connexion cible |
| 5 | Portainer | Démarrer ou redémarrer la stack AssuranceService (elle utilisera la base restaurée) |

---

## 4. Paramètres des scripts

**Backup-AssuranceDatabase.ps1**

- `-ConnectionString` : chaîne de connexion vers l’instance SQL source (catalogue `master` utilisé pour exécuter le backup).
- `-BackupPathOnServer` : chemin **côté SQL Server** où écrire le `.bak` (obligatoire si pas de valeur par défaut adaptée).
- `-OutputFolder` : dossier hôte pour le nom du fichier par défaut (utilisé seulement pour afficher le chemin si volume Docker monté).

**Restore-AssuranceDatabase.ps1**

- `-BackupPathOnServer` : chemin **côté SQL Server** du fichier `.bak` (obligatoire).
- `-ConnectionString` : chaîne de connexion vers l’instance SQL cible.
- `-DatabaseName` : nom de la base à restaurer (défaut : `MS_ASSURANCE`).
