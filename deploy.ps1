param(
    [string]$ApiSiteName = "PensionAPI",
    [string]$UiSiteName = "PensionUI",
    [string]$ApiPort = "5141",
    [string]$UiPort = "80",
    [string]$ApiDeployPath = "E:\inetpub\PensionAPI",
    [string]$UiDeployPath = "E:\inetpub\PensionUI",
    [string]$CitizenDeployPath = "E:\inetpub\PensionCitizenAPI",
    [string]$LogPath = "E:\logs\pension.api",
    [switch]$SkipIISConfig,
    [switch]$SkipDbReset
)

$ErrorActionPreference = "Stop"
$SolutionRoot = Split-Path -Parent $PSCommandPath
$ApiProject = Join-Path $SolutionRoot "PensionMS.API"
$UiProject = Join-Path $SolutionRoot "ui"

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "Pension Management System - Deployment" -ForegroundColor Cyan
Write-Host "Architecture: API (IIS:$ApiPort) + UI (IIS:$UiPort)" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan

# ========== Phase 1: Prerequisites ==========
Write-Host "`n[Phase 1/7] Prerequisites..." -ForegroundColor Yellow
try {
    Import-Module WebAdministration -ErrorAction Stop
    $module = Get-WebGlobalModule -Name "AspNetCoreModuleV2" -ErrorAction SilentlyContinue
    if (-not $module) { Write-Host "  [WARN] ANCM not found. Run: Enable-WindowsOptionalFeature -Online -FeatureName IIS-ASPNETCore" -ForegroundColor Yellow }
    else { Write-Host "  [OK] ANCM found" -ForegroundColor Green }

    $rewrite = Get-WebGlobalModule -Name "RewriteModule" -ErrorAction SilentlyContinue
    if (-not $rewrite) { Write-Host "  [WARN] URL Rewrite not found" -ForegroundColor Yellow }
    else { Write-Host "  [OK] URL Rewrite found" -ForegroundColor Green }
}
catch { Write-Host "  [WARN] IIS module check failed: $_" -ForegroundColor Yellow }

# ========== Phase 2: Cleanup (except DB) ==========
Write-Host "`n[Phase 2/7] Cleanup..." -ForegroundColor Yellow
try {
    # Stop old sites
    Get-Website | Where-Object { $_.Name -in @("Default Web Site", $ApiSiteName, $UiSiteName) } | ForEach-Object {
        Stop-Website -Name $_.Name
        Write-Host "  [OK] Stopped site: $($_.Name)" -ForegroundColor Green
    }
    # Remove old Scheduled Task
    Unregister-ScheduledTask -TaskName "PensionManagementAPI" -Confirm:$false -ErrorAction SilentlyContinue
    Write-Host "  [OK] Removed old Scheduled Task" -ForegroundColor Green
    # Remove old portproxy
    netsh interface portproxy delete v4tov4 listenport=80 2>$null
    Write-Host "  [OK] Removed old portproxy" -ForegroundColor Green
}
catch { Write-Host "  [WARN] Cleanup issue: $_" -ForegroundColor Yellow }

# ========== Phase 3: Database ==========
if (-not $SkipDbReset) {
    Write-Host "`n[Phase 3/7] Database preparation..." -ForegroundColor Yellow
    Push-Location $ApiProject
    try {
        try { $efVersion = dotnet ef --version 2>$null } catch { }
        if (-not $efVersion) { dotnet tool install --global dotnet-ef }
        dotnet ef migrations script --idempotent -o "$SolutionRoot\deploy_script.sql" --project "..\PensionMS.Infrastructure"
        Write-Host "  [OK] Migration script generated" -ForegroundColor Green
        Write-Host "  -> Run manually: sqlcmd -S .\SQLEXPRESS -i deploy_script.sql" -ForegroundColor Gray
    }
    finally { Pop-Location }
}

# ========== Phase 4: Build API ==========
Write-Host "`n[Phase 4/7] Building API..." -ForegroundColor Yellow
Push-Location $ApiProject
try {
    if (Test-Path "publish") { Remove-Item -Recurse -Force "publish" }
    dotnet publish -c Release -o publish --self-contained false
    Write-Host "  [OK] API published" -ForegroundColor Green
}
finally { Pop-Location }

# ========== Phase 5: Build UI ==========
Write-Host "`n[Phase 5/7] Building UI..." -ForegroundColor Yellow
Push-Location $UiProject
try {
    # Kill any lingering node/esbuild processes that lock node_modules
    Get-Process -Name node, esbuild -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2

    if (Test-Path "node_modules") { Remove-Item -Recurse -Force "node_modules" -ErrorAction SilentlyContinue }
    npm ci
    npx tsc -b; if ($LASTEXITCODE -eq 0) { npx vite build }
    Write-Host "  [OK] UI built" -ForegroundColor Green
}
finally { Pop-Location }

# ========== Phase 6: Deploy to IIS ==========
Write-Host "`n[Phase 6/7] Deploying to IIS..." -ForegroundColor Yellow

# Prepare directories
@($ApiDeployPath, $UiDeployPath, $LogPath, "$ApiDeployPath\wwwroot\uploads") | ForEach-Object {
    New-Item -ItemType Directory -Path $_ -Force | Out-Null
}

# Copy API
Copy-Item -Recurse "$ApiProject\publish\*" $ApiDeployPath -Force
Write-Host "  [OK] API copied to $ApiDeployPath" -ForegroundColor Green

# Copy UI
Copy-Item -Recurse "$UiProject\dist\*" $UiDeployPath -Force
Write-Host "  [OK] UI copied to $UiDeployPath" -ForegroundColor Green

# Permissions for uploads
$uploadsPath = "$ApiDeployPath\wwwroot\uploads"
try {
    $identity = "IIS APPPOOL\$ApiSiteName"
    $acl = Get-Acl $uploadsPath
    $rule = New-Object System.Security.AccessControl.FileSystemAccessRule(
        $identity, "Modify", "ContainerInherit,ObjectInherit", "None", "Allow")
    $acl.AddAccessRule($rule)
    Set-Acl $uploadsPath $acl
    Write-Host "  [OK] Upload folder permissions set" -ForegroundColor Green
}
catch { Write-Host "  [WARN] Permission error: $_" -ForegroundColor Yellow }

# ========== Phase 7: IIS Sites ==========
if (-not $SkipIISConfig) {
    Write-Host "`n[Phase 7/7] Configuring IIS sites..." -ForegroundColor Yellow
    Import-Module WebAdministration -ErrorAction Stop

    # --- API Site ---
    $pool = $null
    try { $pool = Get-WebAppPoolState -Name $ApiSiteName -ErrorAction Stop } catch { }
    if (-not $pool) { New-WebAppPool -Name $ApiSiteName -Force }
    Set-ItemProperty "IIS:\AppPools\$ApiSiteName" -Name "managedRuntimeVersion" -Value ""
    Set-ItemProperty "IIS:\AppPools\$ApiSiteName" -Name "startMode" -Value "AlwaysRunning"
    Set-ItemProperty "IIS:\AppPools\$ApiSiteName" -Name "processModel.idleTimeout" -Value ([TimeSpan]::Zero)
    Set-ItemProperty "IIS:\AppPools\$ApiSiteName" -Name "recycling.periodicRestart.time" -Value ([TimeSpan]::Zero)
    Write-Host "  [OK] AppPool $ApiSiteName configured" -ForegroundColor Green

    $site = $null
    try { $site = Get-Website -Name $ApiSiteName -ErrorAction Stop } catch { }
    if (-not $site) {
        New-Website -Name $ApiSiteName -Port $ApiPort -PhysicalPath $ApiDeployPath -ApplicationPool $ApiSiteName
    } else {
        Set-ItemProperty "IIS:\Sites\$ApiSiteName" -Name "physicalPath" -Value $ApiDeployPath
        Set-ItemProperty "IIS:\Sites\$ApiSiteName" -Name "applicationPool" -Value $ApiSiteName
    }
    Start-Website -Name $ApiSiteName
    Write-Host "  [OK] $ApiSiteName (port $ApiPort)" -ForegroundColor Green

    # --- UI Site ---
    # UI doesn't need a special AppPool; use DefaultAppPool
    $uiSite = $null
    try { $uiSite = Get-Website -Name $UiSiteName -ErrorAction Stop } catch { }
    if (-not $uiSite) {
        New-Website -Name $UiSiteName -Port $UiPort -PhysicalPath $UiDeployPath
    } else {
        Set-ItemProperty "IIS:\Sites\$UiSiteName" -Name "physicalPath" -Value $UiDeployPath
    }
    Start-Website -Name $UiSiteName
    Write-Host "  [OK] $UiSiteName (port $UiPort)" -ForegroundColor Green

    # Stop old Default Web Site if still running
    $defaultSite = $null
    try { $defaultSite = Get-Website -Name "Default Web Site" -ErrorAction Stop } catch { }
    if ($defaultSite -and $defaultSite.State -eq "Started") {
        Stop-Website -Name "Default Web Site"
        Write-Host "  [OK] Stopped Default Web Site" -ForegroundColor Green
    }

    # --- Verification ---
    Write-Host "`n--- Verification ---" -ForegroundColor Cyan
    Start-Sleep -Seconds 3

    # Test API
    try {
        $resp = Invoke-WebRequest -Uri "http://localhost:$ApiPort/api/auth/login" -Method POST `
            -ContentType "application/json" -Body '{"username":"admin","password":"Admin@123"}' -UseBasicParsing -TimeoutSec 30
        Write-Host "  [OK] API ($ApiPort): $($resp.StatusCode)" -ForegroundColor Green
    } catch { Write-Host "  [WARN] API ($ApiPort): $_" -ForegroundColor Yellow }

    # Test UI
    try {
        $resp = Invoke-WebRequest -Uri "http://localhost:$UiPort" -UseBasicParsing -TimeoutSec 10
        Write-Host "  [OK] UI ($UiPort): $($resp.StatusCode)" -ForegroundColor Green
    } catch { Write-Host "  [WARN] UI ($UiPort): $_" -ForegroundColor Yellow }

    # Test SPA routing
    try {
        $resp = Invoke-WebRequest -Uri "http://localhost:$UiPort/login" -UseBasicParsing -TimeoutSec 10
        Write-Host "  [OK] SPA routing: $($resp.StatusCode)" -ForegroundColor Green
    } catch { Write-Host "  [WARN] SPA routing: $_" -ForegroundColor Yellow }
}

Write-Host ""
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "Deployment complete!" -ForegroundColor Green
Write-Host "  API: http://localhost:$ApiPort" -ForegroundColor White
Write-Host "  UI:  http://localhost:$UiPort" -ForegroundColor White
Write-Host "===============================================" -ForegroundColor Cyan
