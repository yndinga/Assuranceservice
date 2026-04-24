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
    [string] $OutputFolder = ".\backup",
    [Parameter(Mandatory = $false)]
    [ValidateSet("auto", "microsoft", "system")]
    [string] $SqlClient = "auto"
)

$ErrorActionPreference = "Stop"
$DatabaseName = "MS_ASSURANCE"

function New-DbConnection {
    param(
        [Parameter(Mandatory = $true)]
        [string] $connectionString,
        [Parameter(Mandatory = $true)]
        [ValidateSet("auto", "microsoft", "system")]
        [string] $client
    )

    function Get-MasterConnectionString {
        param([Parameter(Mandatory = $true)][string] $cs)

        # Remove any existing database/catalog keywords, then force master.
        # Handles: Database=..., Initial Catalog=..., InitialCatalog=...
        $clean = [regex]::Replace($cs, '(?i)(^|;)\s*(database|initial\s+catalog|initialcatalog)\s*=\s*[^;]*', '$1')
        $clean = [regex]::Replace($clean, ';{2,}', ';').Trim()
        $clean = $clean.TrimEnd(';')
        if ([string]::IsNullOrWhiteSpace($clean)) {
            return "Database=master"
        }
        return "$clean;Database=master"
    }

    $clientToUse = $client
    if ($clientToUse -eq "auto") {
        $clientToUse = "microsoft"
        try {
            Add-Type -AssemblyName "Microsoft.Data.SqlClient" -ErrorAction Stop | Out-Null
        } catch {
            $clientToUse = "system"
        }
    }

    if ($clientToUse -eq "microsoft") {
        if (-not ("Microsoft.Data.SqlClient.SqlConnection" -as [type])) {
            Add-Type -AssemblyName "Microsoft.Data.SqlClient" -ErrorAction Stop | Out-Null
        }
        $masterCs = Get-MasterConnectionString -cs $connectionString
        return New-Object Microsoft.Data.SqlClient.SqlConnection($masterCs)
    }

    $masterCs = Get-MasterConnectionString -cs $connectionString
    return New-Object System.Data.SqlClient.SqlConnection($masterCs)
}

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
    $conn = New-DbConnection -connectionString $ConnectionString -client $SqlClient
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
    $msg = $_.Exception.Message
    $inner = $_.Exception.InnerException
    while ($inner) {
        $msg += " | Inner: " + $inner.Message
        $inner = $inner.InnerException
    }
    Write-Error "Erreur backup: $msg"
    exit 1
}
