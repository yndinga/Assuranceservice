<#
.SYNOPSIS
    Restaure la base MS_ASSURANCE a partir d'un fichier .bak (cote Portainer ou autre serveur SQL).
.DESCRIPTION
    IMPORTANT : Ce script doit etre execute dans POWERSHELL (invite de commandes PowerShell),
    pas dans SSMS ni avec sqlcmd. Sinon utilisez Restore-AssuranceDatabase.sql dans SSMS.
    Utilisez ce script sur la machine ou le conteneur ou SQL Server a acces au fichier .bak.
.EXAMPLE
    Dans PowerShell : .\Restore-AssuranceDatabase.ps1 -BackupPathOnServer "C:\backup\MS_ASSURANCE.bak" -ConnectionString "Server=192.168.3.178,1434;User Id=sa;Password=...;TrustServerCertificate=True;"
#>
param(
    [Parameter(Mandatory = $true)]
    [string] $BackupPathOnServer,
    [Parameter(Mandatory = $false)]
    [string] $ConnectionString = "Server=localhost,1420;User Id=sa;Password=DevStrongPwd@123;TrustServerCertificate=True;",
    [Parameter(Mandatory = $false)]
    [string] $DatabaseName = "MS_ASSURANCE",
    [Parameter(Mandatory = $false)]
    [ValidateSet("auto", "microsoft", "system")]
    [string] $SqlClient = "auto"
)

$ErrorActionPreference = "Stop"

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

        $clean = [regex]::Replace($cs, '(?i)(^|;)\s*(database|initial\s+catalog|initialcatalog)\s*=\s*[^;]*', '$1')
        $clean = [regex]::Replace($clean, ';{2,}', ';').Trim()
        $clean = $clean.TrimEnd(';')
        if ([string]::IsNullOrWhiteSpace($clean)) { return "Database=master" }
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

    $masterCs = Get-MasterConnectionString -cs $connectionString

    if ($clientToUse -eq "microsoft") {
        if (-not ("Microsoft.Data.SqlClient.SqlConnection" -as [type])) {
            Add-Type -AssemblyName "Microsoft.Data.SqlClient" -ErrorAction Stop | Out-Null
        }
        return New-Object Microsoft.Data.SqlClient.SqlConnection($masterCs)
    }

    return New-Object System.Data.SqlClient.SqlConnection($masterCs)
}

try {
    $conn = New-DbConnection -connectionString $ConnectionString -client $SqlClient
    $conn.Open()
    try {
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = "SELECT 1 FROM sys.databases WHERE name = N'$($DatabaseName.Replace("'", "''"))'"
        $exists = $cmd.ExecuteScalar()
        $pathEscaped = $BackupPathOnServer.Replace("'", "''")
        if ($exists) {
            $cmd.CommandText = "ALTER DATABASE [$DatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
                "RESTORE DATABASE [$DatabaseName] FROM DISK = N'$pathEscaped' WITH REPLACE, RECOVERY; " +
                "ALTER DATABASE [$DatabaseName] SET MULTI_USER;"
        } else {
            $cmd.CommandText = "RESTORE DATABASE [$DatabaseName] FROM DISK = N'$pathEscaped' WITH RECOVERY"
        }
        $cmd.CommandTimeout = 600
        $cmd.ExecuteNonQuery() | Out-Null
        Write-Host "Restauration reussie: $DatabaseName"
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
    Write-Error "Erreur restauration: $msg"
    exit 1
}
