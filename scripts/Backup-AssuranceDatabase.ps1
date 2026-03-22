<#
.SYNOPSIS
    Sauvegarde la base MS_ASSURANCE (SQL Server) pour transfert vers Portainer.
.DESCRIPTION
    Crée un fichier .bak que vous pouvez copier sur le serveur Portainer puis restaurer.
    Si SQL Server tourne dans Docker, montez un volume (ex. .\backup:/var/opt/mssql/backup)
    et indiquez -BackupPathOnServer avec le chemin vu par le conteneur.
.EXAMPLE
    .\Backup-AssuranceDatabase.ps1 -ConnectionString "Server=localhost,1420;User Id=sa;Password=DevStrongPwd@123;TrustServerCertificate=True;"
.EXAMPLE
    # SQL Server dans Docker avec volume monté sur ./backup
    .\Backup-AssuranceDatabase.ps1 -BackupPathOnServer "/var/opt/mssql/backup/MS_ASSURANCE.bak"
#>
param(
    [Parameter(Mandatory = $false)]
    [string] $ConnectionString = "Server=localhost,1420;Database=MS_ASSURANCE;User Id=sa;Password=DevStrongPwd@123;TrustServerCertificate=True;",
    [Parameter(Mandatory = $false)]
    [string] $BackupPathOnServer = "",
    [Parameter(Mandatory = $false)]
    [string] $OutputFolder = ".\backup"
)

$ErrorActionPreference = "Stop"
$DatabaseName = "MS_ASSURANCE"

# Chemin du backup côté serveur SQL (où SQL Server écrit le .bak)
if ([string]::IsNullOrWhiteSpace($BackupPathOnServer)) {
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $localBakName = "${DatabaseName}_${timestamp}.bak"
    # Par défaut : on suppose un volume monté en ./backup côté conteneur en /var/opt/mssql/backup
    if (-not (Test-Path $OutputFolder)) { New-Item -ItemType Directory -Path $OutputFolder -Force | Out-Null }
    $BackupPathOnServer = "/var/opt/mssql/backup/$localBakName"
    $hostBackupPath = Join-Path $OutputFolder $localBakName
    Write-Host "Backup path on SQL Server (container): $BackupPathOnServer"
    Write-Host "If using Docker, mount host folder: -v ${PWD}/backup:/var/opt/mssql/backup"
} else {
    $hostBackupPath = $null
}

try {
    $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder($ConnectionString)
    $builder.InitialCatalog = "master"
    $conn = New-Object System.Data.SqlClient.SqlConnection($builder.ConnectionString)
    $conn.Open()
    try {
        $pathEscaped = $BackupPathOnServer.Replace("'", "''")
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = "BACKUP DATABASE [$DatabaseName] TO DISK = N'$pathEscaped' WITH NOFORMAT, NOINIT, SKIP, REWIND, NOUNLOAD, STATS = 10"
        $cmd.CommandTimeout = 600
        $cmd.ExecuteNonQuery() | Out-Null
        Write-Host "Backup reussi: $BackupPathOnServer"
        if ($hostBackupPath) {
            Write-Host "Si SQL Server est dans Docker avec volume -v `"${OutputFolder}:/var/opt/mssql/backup`", le fichier est dans: $hostBackupPath"
            Write-Host "Copiez ce fichier vers le serveur Portainer puis utilisez Restore-AssuranceDatabase.ps1"
        }
    } finally {
        $conn.Close()
    }
} catch {
    Write-Error "Erreur backup: $_"
    exit 1
}
