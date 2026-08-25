param(
    [string]$UiDeployPath = "E:\inetpub\PensionUI",
    [string]$UiSiteName = "PensionUI",
    [string]$UiProjectPath = "",
    [switch]$SkipBuild,
    [switch]$RestartOnly
)

$ErrorActionPreference = "Stop"
$SolutionRoot = Split-Path -Parent $PSCommandPath
if (-not $UiProjectPath) { $UiProjectPath = Join-Path $SolutionRoot "ui" }

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "  UI Deployment Script" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan

# Show current API URL from .env.production
$envFile = Join-Path $UiProjectPath ".env.production"
if (Test-Path $envFile) {
    $apiUrl = Get-Content $envFile -Raw
    Write-Host "  API URL: $apiUrl" -ForegroundColor White
} else {
    Write-Host "  [WARN] .env.production not found" -ForegroundColor Yellow
}

if ($RestartOnly) {
    Write-Host "`n[RestartOnly] Restarting site..." -ForegroundColor Yellow
    Import-Module WebAdministration -Force
    Restart-WebItem "IIS:\Sites\$UiSiteName" -ErrorAction SilentlyContinue
    Write-Host "  [OK] $UiSiteName restarted" -ForegroundColor Green
    exit 0
}

# Build
if (-not $SkipBuild) {
    Write-Host "`n[1/3] Building UI..." -ForegroundColor Yellow
    Push-Location $UiProjectPath
    try {
        & npm run build 2>&1
        if ($LASTEXITCODE -ne 0) { throw "npm build failed" }
        Write-Host "  [OK] Build completed" -ForegroundColor Green
    } finally { Pop-Location }
} else {
    Write-Host "`n[1/3] Skipping build (-SkipBuild)" -ForegroundColor Yellow
}

# Deploy
Write-Host "`n[2/3] Deploying to $UiDeployPath ..." -ForegroundColor Yellow

# Ensure target exists
if (-not (Test-Path $UiDeployPath)) {
    New-Item -ItemType Directory -Path $UiDeployPath -Force | Out-Null
}

# Clean old assets (prevents stale chunk file issues)
$assetsDir = Join-Path $UiDeployPath "assets"
if (Test-Path $assetsDir) {
    Remove-Item -Path "$assetsDir\*" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "  [OK] Old assets cleaned" -ForegroundColor Green
}

# Backup existing web.config
$backupWebConfig = $null
$targetWebConfig = Join-Path $UiDeployPath "web.config"
if (Test-Path $targetWebConfig) {
    $backupDir = Join-Path $UiDeployPath ".webconfig-backups"
    New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $backupFile = Join-Path $backupDir "web.config.$timestamp"
    Copy-Item $targetWebConfig $backupFile
    $backupWebConfig = $backupFile
    Write-Host "  [OK] Existing web.config backed up to $backupFile" -ForegroundColor Green
}

# Copy dist files (copy each top-level entry individually to avoid
# Copy-Item -Recurse glob failures on transiently-locked/renamed chunk files)
$distPath = Join-Path $UiProjectPath "dist"
if (Test-Path $distPath) {
    $copied = 0
    Get-ChildItem -Path $distPath -Force | ForEach-Object {
        try {
            Copy-Item -Path $_.FullName -Destination $UiDeployPath -Recurse -Force
            $copied++
        } catch {
            Write-Host "  [WARN] Could not copy $($_.Name): $($_.Exception.Message)" -ForegroundColor Yellow
        }
    }
    Write-Host "  [OK] dist files copied ($copied entries)" -ForegroundColor Green
} else {
    throw "dist folder not found at $distPath. Run npm run build first."
}

# Ensure web.config exists — always use the source project's web.config
$sourceWebConfig = Join-Path $UiProjectPath "web.config"
if (Test-Path $sourceWebConfig) {
    Copy-Item $sourceWebConfig $targetWebConfig -Force
    Write-Host "  [OK] web.config copied from project source" -ForegroundColor Green
} elseif ($backupWebConfig -and -not (Test-Path $targetWebConfig)) {
    Copy-Item $backupWebConfig $targetWebConfig
    Write-Host "  [OK] web.config restored from backup" -ForegroundColor Green
} elseif (-not (Test-Path $targetWebConfig)) {
    Write-Host "  [WARN] No web.config source found!" -ForegroundColor Yellow
}

# Restart site
Write-Host "`n[3/3] Restarting IIS site..." -ForegroundColor Yellow
Import-Module WebAdministration -Force
$site = Get-Website -Name $UiSiteName -ErrorAction SilentlyContinue
if (-not $site) {
    Write-Host "  [WARN] Site $UiSiteName not found, creating it..." -ForegroundColor Yellow
    New-Website -Name $UiSiteName -Port 80 -PhysicalPath $UiDeployPath -ErrorAction SilentlyContinue
}
Start-Website -Name $UiSiteName -ErrorAction SilentlyContinue
Restart-WebItem "IIS:\Sites\$UiSiteName" -ErrorAction SilentlyContinue
Write-Host "  [OK] $UiSiteName restarted" -ForegroundColor Green

# Verify
Write-Host "`n--- Verification ---" -ForegroundColor Cyan
Start-Sleep -Seconds 2
try {
    $resp = Invoke-WebRequest -Uri "http://localhost" -UseBasicParsing -TimeoutSec 10
    Write-Host "  UI: $($resp.StatusCode) ✅" -ForegroundColor Green
} catch { Write-Host "  UI: $_" -ForegroundColor Yellow }

try {
    $resp = Invoke-WebRequest -Uri "http://localhost/login" -UseBasicParsing -TimeoutSec 10
    Write-Host "  SPA routing: $($resp.StatusCode) ✅" -ForegroundColor Green
} catch { Write-Host "  SPA routing: $_" -ForegroundColor Yellow }

Write-Host "`nDone! Open http://pension.local or http://192.168.1.10" -ForegroundColor Green
