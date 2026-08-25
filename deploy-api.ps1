param(
    [string]$DeployPath = "E:\inetpub\PensionAPI",
    [string]$SiteName = "PensionAPI",
    [string]$AppPoolName = "PensionAPI",
    [string]$ProjectPath = "",
    [string]$Configuration = "Release",
    [switch]$SkipBuild,
    [switch]$RestartOnly
)

$ErrorActionPreference = "Stop"
$SolutionRoot = Split-Path -Parent $PSCommandPath
if (-not $ProjectPath) { $ProjectPath = Join-Path $SolutionRoot "PensionMS.API" }

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "  API Deployment Script" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "  Project : $ProjectPath" -ForegroundColor White
Write-Host "  Target  : $DeployPath" -ForegroundColor White
Write-Host "  Site    : $SiteName" -ForegroundColor White

Import-Module WebAdministration -Force

if ($RestartOnly) {
    Write-Host "`n[RestartOnly] Restarting site & AppPool..." -ForegroundColor Yellow
    Start-WebAppPool -Name $AppPoolName -ErrorAction SilentlyContinue
    Start-Website -Name $SiteName -ErrorAction SilentlyContinue
    Write-Host "  [OK] $AppPoolName / $SiteName restarted" -ForegroundColor Green
    exit 0
}

# Backup production configs from project source
Write-Host "`n[1/6] Backing up configs from project..." -ForegroundColor Yellow
$projectSettings = Join-Path $ProjectPath "appsettings.Production.json"
$prodWebConfig = Join-Path $ProjectPath "web.config"
$tempDir = Join-Path $env:TEMP "api-deploy-settings"
New-Item -ItemType Directory -Path $tempDir -Force -ErrorAction SilentlyContinue | Out-Null
$savedSettings = Join-Path $tempDir "appsettings.Production.json"
$savedWebConfig = Join-Path $tempDir "web.config"
if (Test-Path $projectSettings) { Copy-Item $projectSettings $savedSettings -Force; Write-Host "  [OK] appsettings.Production.json backed up" -ForegroundColor Green }
if (Test-Path $prodWebConfig) { Copy-Item $prodWebConfig $savedWebConfig -Force; Write-Host "  [OK] web.config backed up" -ForegroundColor Green }

# Stop AppPool so files are unlocked
Write-Host "`n[2/6] Stopping AppPool..." -ForegroundColor Yellow
try {
    $pool = Get-WebAppPoolState -Name $AppPoolName -ErrorAction SilentlyContinue
    if ($pool.Value -ne 'Stopped') {
        Stop-WebAppPool -Name $AppPoolName -ErrorAction SilentlyContinue
    }
} catch {
    if (Test-Path "IIS:\AppPools\$AppPoolName") {
        Stop-WebAppPool -Name $AppPoolName -ErrorAction SilentlyContinue
    }
}

# Wait for all w3wp processes for this AppPool to fully exit
$maxWait = 30
for ($i = 0; $i -lt $maxWait; $i++) {
    $procs = Get-CimInstance -Namespace "root\WebAdministration" -ClassName "WorkerProcess" -ErrorAction SilentlyContinue |
        Where-Object { $_.AppPoolName -eq $AppPoolName }
    if (-not $procs) { break }
    $procs | ForEach-Object { taskkill /F /PID $_.ProcessId 2>&1 | Out-Null }
    Start-Sleep -Seconds 1
}
Start-Sleep -Seconds 1
Write-Host "  [OK] $AppPoolName stopped" -ForegroundColor Green

# Clean deploy target (preserve uploads and config backups)
Write-Host "`n[3/6] Cleaning deploy target..." -ForegroundColor Yellow
if (Test-Path $DeployPath) {
    Get-ChildItem -Path $DeployPath -Exclude "wwwroot", ".config-backups" | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
    if (Test-Path (Join-Path $DeployPath "wwwroot")) {
        Get-ChildItem (Join-Path $DeployPath "wwwroot") -Exclude "uploads" | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
    }
    Write-Host "  [OK] Old files cleaned (preserved uploads/)" -ForegroundColor Green
} else {
    New-Item -ItemType Directory -Path $DeployPath -Force | Out-Null
    Write-Host "  [OK] Target directory created" -ForegroundColor Green
}

# Build & Publish
if (-not $SkipBuild) {
    Write-Host "`n[4/6] Publishing API..." -ForegroundColor Yellow
    try {
        Push-Location $SolutionRoot
        & dotnet publish $ProjectPath -c $Configuration --self-contained false -o $DeployPath 2>&1
        if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }
        Write-Host "  [OK] Publish completed" -ForegroundColor Green
    } finally { Pop-Location }
} else {
    Write-Host "`n[4/6] Skipping publish (-SkipBuild)" -ForegroundColor Yellow
}

# Restore configs from project source (always use source, not backup)
Write-Host "`n[5/6] Restoring configs..." -ForegroundColor Yellow
if (Test-Path $savedSettings) {
    Copy-Item $savedSettings (Join-Path $DeployPath "appsettings.Production.json") -Force
    Write-Host "  [OK] appsettings.Production.json restored" -ForegroundColor Green
}
if (Test-Path $savedWebConfig) {
    Copy-Item $savedWebConfig (Join-Path $DeployPath "web.config") -Force
    Write-Host "  [OK] web.config restored" -ForegroundColor Green
}

# Ensure upload directories exist with proper permissions
Write-Host "`n[6/6] Creating upload directories..." -ForegroundColor Yellow
$uploadDirs = "wwwroot\uploads\photos", "wwwroot\uploads\documents" | ForEach-Object { Join-Path $DeployPath $_ }
foreach ($dir in $uploadDirs) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }
    $poolIdentity = "IIS AppPool\$AppPoolName"
    try {
        $acl = Get-Acl -LiteralPath $dir
        $rule = New-Object System.Security.AccessControl.FileSystemAccessRule($poolIdentity, "Modify", "ContainerInherit,ObjectInherit", "None", "Allow")
        $acl.AddAccessRule($rule)
        Set-Acl -LiteralPath $dir $acl
    } catch {
        Write-Warning "  Could not set permissions for $dir : $_"
    }
    Write-Host "  [OK] $dir" -ForegroundColor Green
}
Write-Host "  [OK] Upload directories ready" -ForegroundColor Green

# Start AppPool & Site
Write-Host "`n--- Starting services ---" -ForegroundColor Cyan
Start-WebAppPool -Name $AppPoolName -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1
Start-Website -Name $SiteName -ErrorAction SilentlyContinue
Write-Host "  [OK] $SiteName started" -ForegroundColor Green

# Verify
Write-Host "`n--- Verification ---" -ForegroundColor Cyan
Start-Sleep -Seconds 3
$endpoints = @(
    @{ Name = "API login (400 expected)"; Url = "http://192.168.1.10:5141/api/auth/login"; Method = "POST" }
    @{ Name = "Swagger"; Url = "http://192.168.1.10:5141/swagger/v1/swagger.json"; Method = "GET" }
)

foreach ($ep in $endpoints) {
    try {
        if ($ep.Method -eq "GET") {
            $resp = Invoke-WebRequest -Uri $ep.Url -UseBasicParsing -TimeoutSec 10
        } else {
            $resp = Invoke-WebRequest -Uri $ep.Url -Method POST -Body '{"username":"","password":""}' -ContentType "application/json" -UseBasicParsing -TimeoutSec 10
        }
        Write-Host "  $($ep.Name): $($resp.StatusCode)" -ForegroundColor Green
    } catch {
        $code = $_.Exception.Response.StatusCode.value__
        if ($code -eq 400 -or $code -eq 401) {
            Write-Host "  $($ep.Name): $code (expected)" -ForegroundColor Green
        } else {
            Write-Host "  $($ep.Name): $_" -ForegroundColor Yellow
        }
    }
}

Write-Host "`nDone! API deployed to $DeployPath" -ForegroundColor Green
