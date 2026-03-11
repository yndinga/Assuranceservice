<#
.SYNOPSIS
    Insère les ports dans [dbo].[Ports] en résolvant PaysId (GUID) à partir du code pays
    via new_pays.csv (Id;Code;Nom;...). Lit ports.csv (PaysCode;Code;AncienPaysId;Nom;Actif).

.PARAMETER PortsCsvPath
    Chemin du CSV des ports (defaut: ports.csv). Format sans en-tete: PaysCode;Code;AncienPaysId;Nom;Actif

.PARAMETER PaysCsvPath
    Chemin du CSV des pays avec Id (GUID) et Code (defaut: new_pays.csv). En-tete: Id;Code;Nom;...

.PARAMETER ConnectionString
    Chaine de connexion SQL Server (MS_ASSURANCE).

.PARAMETER TruncateFirst
    Si $true, vide la table Ports avant insertion (defaut: $false).

.EXAMPLE
    .\Import-PortsWithPaysId.ps1 -PortsCsvPath ".\ports.csv" -PaysCsvPath ".\new_pays.csv" -ConnectionString "Server=localhost,1420;Database=MS_ASSURANCE;User Id=sa;Password=xxx;TrustServerCertificate=True"
#>
param(
    [string] $PortsCsvPath = ".\ports.csv",
    [string] $PaysCsvPath = ".\new_pays.csv",
    [Parameter(Mandatory = $true)]
    [string] $ConnectionString,
    [bool] $TruncateFirst = $false
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $PortsCsvPath)) { Write-Error "Fichier introuvable : $PortsCsvPath" }
if (-not (Test-Path $PaysCsvPath)) { Write-Error "Fichier introuvable : $PaysCsvPath" }

# --- Charger Pays: Code -> Id (Guid) ---
$paysRaw = Get-Content $PaysCsvPath -Encoding UTF8 -Raw
if ($paysRaw) { $paysRaw = $paysRaw.TrimStart([char]0xFEFF) }  # BOM UTF-8
$paysLines = $paysRaw -split "`r?`n"
$sep = if ($paysLines[0] -match ";") { ";" } else { "," }
$paysByCode = @{}
for ($i = 0; $i -lt $paysLines.Count; $i++) {
    $first = ($paysLines[$i] -replace '"', '').Trim()
    if ($i -eq 0 -and ($first -match "^(Id|Code)" -or $first.StartsWith("Id"))) { continue }
    $p = $paysLines[$i] -split $sep
    if ($p.Count -ge 2) {
        $idStr = ($p[0] -replace '"', '').Trim()
        $code = ($p[1] -replace '"', '').Trim().ToUpperInvariant()
        $guidOut = [Guid]::Empty
        if ($code -and [Guid]::TryParse($idStr, [ref]$guidOut)) {
            $paysByCode[$code] = $guidOut
        }
    }
}
Write-Host "Pays charges : $($paysByCode.Count) (Code -> Id)"

# --- Lire ports.csv : PaysCode;Code;AncienPaysId;Nom;Actif (sans en-tete) ---
$portLines = Get-Content $PortsCsvPath -Encoding UTF8
$toInsert = @()
$skipped = 0
foreach ($line in $portLines) {
    $line = $line.Trim()
    if (-not $line) { continue }
    $cols = $line -split ";"
    if ($cols.Count -lt 5) { $skipped++; continue }
    $paysCode = ($cols[0] -replace '"', '').Trim().ToUpperInvariant()
    $portCode = ($cols[1] -replace '"', '').Trim()
    $nom = ($cols[3] -replace '"', '').Trim()
    $actif = ($cols[4] -replace '"', '').Trim()
    if (-not $portCode -or -not $nom) { $skipped++; continue }
    $paysId = $paysByCode[$paysCode]
    if (-not $paysId) {
        Write-Warning "Code pays inconnu : $paysCode (port $portCode) - ignore."
        $skipped++
        continue
    }
    $toInsert += [PSCustomObject]@{
        PaysId   = $paysId
        Code     = $portCode
        Nom      = $nom
        Actif    = ($actif -eq "1" -or $actif -eq "true" -or $actif -eq "oui")
    }
}
Write-Host "Ports a inserer : $($toInsert.Count) (ignores : $skipped)"

if ($toInsert.Count -eq 0) {
    Write-Warning "Aucun port a inserer."
    exit 0
}

# --- Connexion et insertion ---
$conn = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
$conn.Open()
try {
    if ($TruncateFirst) {
        Write-Host "Vidage de la table Ports..."
        $truncCmd = $conn.CreateCommand()
        $truncCmd.CommandText = "TRUNCATE TABLE dbo.Ports"
        $truncCmd.ExecuteNonQuery() | Out-Null
    }

    $insertSql = @"
INSERT INTO dbo.Ports (Id, Code, Nom, PaysId, Module, Actif, Type, CreerPar, ModifierPar, CreerLe, ModifierLe)
VALUES (@Id, @Code, @Nom, @PaysId, @Module, @Actif, @Type, @CreerPar, @ModifierPar, GETUTCDATE(), GETUTCDATE())
"@
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $insertSql
    [void]$cmd.Parameters.Add("@Id", [System.Data.SqlDbType]::UniqueIdentifier)
    [void]$cmd.Parameters.Add("@Code", [System.Data.SqlDbType]::NVarChar, 50)
    [void]$cmd.Parameters.Add("@Nom", [System.Data.SqlDbType]::NVarChar, 255)
    [void]$cmd.Parameters.Add("@PaysId", [System.Data.SqlDbType]::UniqueIdentifier)
    [void]$cmd.Parameters.Add("@Module", [System.Data.SqlDbType]::NVarChar, 50)
    [void]$cmd.Parameters.Add("@Actif", [System.Data.SqlDbType]::Bit)
    [void]$cmd.Parameters.Add("@Type", [System.Data.SqlDbType]::NVarChar, 2)
    [void]$cmd.Parameters.Add("@CreerPar", [System.Data.SqlDbType]::NVarChar, 100)
    [void]$cmd.Parameters.Add("@ModifierPar", [System.Data.SqlDbType]::NVarChar, 100)

    $count = 0
    foreach ($row in $toInsert) {
        $paysGuid = [Guid]::Empty
        if (-not $row.PaysId -or -not [Guid]::TryParse([string]$row.PaysId, [ref]$paysGuid)) {
            Write-Warning "PaysId invalide pour port $($row.Code) - ignore."
            continue
        }
        $cmd.Parameters["@Id"].Value = [Guid]::NewGuid()
        $cmd.Parameters["@Code"].Value = $row.Code
        $cmd.Parameters["@Nom"].Value = $row.Nom
        $cmd.Parameters["@PaysId"].Value = $paysGuid
        $cmd.Parameters["@Module"].Value = ""
        $cmd.Parameters["@Actif"].Value = $row.Actif
        $cmd.Parameters["@Type"].Value = [DBNull]::Value
        $cmd.Parameters["@CreerPar"].Value = "SYSTEM"
        $cmd.Parameters["@ModifierPar"].Value = "SYSTEM"
        $cmd.ExecuteNonQuery() | Out-Null
        $count++
        if ($count % 5000 -eq 0) { Write-Host "  $count..." }
    }
    Write-Host "OK : $count port(s) insere(s) dans Ports (PaysId = GUID depuis new_pays.csv)."
} finally {
    $conn.Close()
    $conn.Dispose()
}
