# Deployment Guide

## Pension Management System — IIS Deployment

---

## 1. Architecture

```
┌─────────────────────────────────────────┐
│           Windows Server (IIS)           │
│                                          │
│  ┌──────────────┐  ┌──────────────┐     │
│  │  PensionAPI   │  │  PensionUI   │     │
│  │  Port: 5141   │  │  Port: 80    │     │
│  │  (ASP.NET Core│  │  (Static    │     │
│  │   via ANCM)   │  │   Files)    │     │
│  └──────┬───────┘  └──────────────┘     │
│         │                                │
│         ▼                                │
│  ┌──────────────┐                       │
│  │  SQL Server   │                       │
│  │  Express/Std  │                       │
│  └──────────────┘                       │
└─────────────────────────────────────────┘
```

---

## 2. Prerequisites

### 2.1 Server Requirements

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| OS | Windows Server 2016 | Windows Server 2022 |
| RAM | 4 GB | 8 GB |
| CPU | 2 cores | 4 cores |
| Disk | 20 GB | 50 GB SSD |
| .NET Runtime | .NET 8 Hosting Bundle | Latest |

### 2.2 Required Software

1. **IIS** with ASP.NET Core Module (ANCM)
2. **URL Rewrite** module
3. **SQL Server** Express (dev) or Standard (prod)
4. **.NET 8 Hosting Bundle** (includes ANCM)
5. **Node.js 20+** (for building UI)
6. **PowerShell 5.1+** (for deployment scripts)

### 2.3 Install ANCM

```powershell
# Install .NET 8 Hosting Bundle (includes ANCM)
# Download from: https://dotnet.microsoft.com/download/dotnet/8.0
# Select "Hosting Bundle" installer

# Verify installation
Get-WebGlobalModule -Name "AspNetCoreModuleV2"
```

### 2.4 Install URL Rewrite

```powershell
# Download from IIS Gallery or install via Web Platform Installer
# Verify
Get-WebGlobalModule -Name "RewriteModule"
```

---

## 3. Deployment Scripts

### 3.1 Full Deployment (`deploy.ps1`)

Deploys both API and UI to IIS.

```powershell
.\deploy.ps1                          # Full deploy with defaults
.\deploy.ps1 -SkipDbReset             # Skip database migration
.\deploy.ps1 -SkipIISConfig           # Skip IIS site setup
.\deploy.ps1 -ApiPort 5141 -UiPort 80 # Custom ports
```

**Parameters:**
| Parameter | Default | Description |
|-----------|---------|-------------|
| `ApiSiteName` | `PensionAPI` | IIS site name for API |
| `UiSiteName` | `PensionUI` | IIS site name for UI |
| `ApiPort` | `5141` | API HTTP port |
| `UiPort` | `80` | UI HTTP port |
| `ApiDeployPath` | `E:\inetpub\PensionAPI` | API deployment path |
| `UiDeployPath` | `E:\inetpub\PensionUI` | UI deployment path |
| `LogPath` | `E:\logs\pension.api` | Log directory |
| `SkipIISConfig` | `$false` | Skip IIS configuration |
| `SkipDbReset` | `$false` | Skip DB migration |

**What it does (7 phases):**
1. **Prerequisites**: Check ANCM and URL Rewrite
2. **Cleanup**: Stop old sites, remove scheduled tasks
3. **Database**: Generate EF migration script
4. **Build API**: `dotnet publish -c Release`
5. **Build UI**: `npm ci && npm run build`
6. **Deploy to IIS**: Copy files, set permissions
7. **IIS Sites**: Create/configure sites, verify

### 3.2 API-Only Deployment (`deploy-api.ps1`)

```powershell
.\deploy-api.ps1                      # Full API deploy
.\deploy-api.ps1 -SkipBuild           # Skip build, just deploy
.\deploy-api.ps1 -RestartOnly         # Just restart IIS site
```

**What it does:**
1. Backs up production configs
2. Stops AppPool
3. Cleans deploy target (preserves uploads/)
4. Publishes API
5. Restores configs
6. Creates upload directories with permissions
7. Starts AppPool & site
8. Verifies endpoints

### 3.3 UI-Only Deployment (`deploy-ui.ps1`)

```powershell
.\deploy-ui.ps1                       # Full UI deploy
.\deploy-ui.ps1 -SkipBuild            # Skip build, just deploy
.\deploy-ui.ps1 -RestartOnly          # Just restart IIS site
```

**What it does:**
1. Builds UI (or skips)
2. Cleans old assets (prevents stale chunks)
3. Backs up web.config
4. Copies dist files
5. Ensures web.config exists
6. Restarts IIS site
7. Verifies

---

## 4. Database Setup

### 4.1 Generate Migration Script

```bash
cd PensionMS.API
dotnet ef migrations script --idempotent -o deploy_script.sql --project ..\PensionMS.Infrastructure
```

### 4.2 Apply Migration

```bash
sqlcmd -S .\SQLEXPRESS -i deploy_script.sql
```

### 4.3 Connection String

Located in `PensionMS.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=PensionManagementDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
  }
}
```

For production, update in `appsettings.Production.json`.

---

## 5. IIS Configuration

### 5.1 AppPool Settings

| Setting | Value |
|---------|-------|
| Name | `PensionAPI` |
| Managed Runtime | `No Managed Code` |
| Start Mode | `AlwaysRunning` |
| Idle Timeout | `00:00:00` (no timeout) |
| Recycling Regular Interval | `00:00:00` (no recycling) |

### 5.2 API Site

| Setting | Value |
|---------|-------|
| Name | `PensionAPI` |
| Port | `5141` |
| Physical Path | `E:\inetpub\PensionAPI` |
| AppPool | `PensionAPI` |

### 5.3 UI Site

| Setting | Value |
|---------|-------|
| Name | `PensionUI` |
| Port | `80` |
| Physical Path | `E:\inetpub\PensionUI` |
| AppPool | `DefaultAppPool` |

### 5.4 web.config (API)

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet"
                  arguments=".\PensionMS.API.dll"
                  stdoutLogEnabled="false"
                  stdoutLogFile=".\logs\stdout"
                  hostingModel="inprocess" />
    </system.webServer>
  </location>
</configuration>
```

### 5.5 web.config (UI - SPA)

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="SPA" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
          </conditions>
          <action type="Rewrite" url="/" />
        </rule>
      </rules>
    </rewrite>
  </system.webServer>
</configuration>
```

---

## 6. Environment Configuration

### 6.1 API Environment

| File | Purpose |
|------|---------|
| `appsettings.json` | Base configuration |
| `appsettings.Production.json` | Production overrides |

**Key Settings:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=PROD_SERVER;Database=PensionManagementDB;User=sa;Password=...;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "SecretKey": "YOUR_SECRET_KEY_HERE",
    "Issuer": "PensionMS",
    "Audience": "PensionMS",
    "ExpirationInMinutes": 60
  },
  "EncryptionKey": "YOUR_32_CHAR_ENCRYPTION_KEY",
  "FileStorage": {
    "BasePath": "E:\\inetpub\\PensionAPI\\wwwroot\\uploads"
  }
}
```

### 6.2 Frontend Environment

| File | Purpose |
|------|---------|
| `.env` | Development (`localhost:5001`) |
| `.env.production` | Production API URL |

**.env.production:**
```
VITE_API_URL=http://192.168.1.10:5141/api
```

---

## 7. File Storage

### 7.1 Directory Structure

```
E:\inetpub\PensionAPI\wwwroot\uploads\
├── photos\          # Retiree/EP photos
├── documents\       # Uploaded documents
└── exports\         # Generated reports
```

### 7.2 Permissions

IIS AppPool identity needs `Modify` permission on uploads directory:

```powershell
$uploadsPath = "E:\inetpub\PensionAPI\wwwroot\uploads"
$identity = "IIS APPPOOL\PensionAPI"
$acl = Get-Acl $uploadsPath
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    $identity, "Modify", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.AddAccessRule($rule)
Set-Acl $uploadsPath $acl
```

---

## 8. Troubleshooting

### 8.1 Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| 502.5 Process Exit | Wrong .NET version | Install correct .NET Hosting Bundle |
| 500.19 Config Error | Missing ANCM | Install .NET Hosting Bundle |
| 404 on API | Wrong port/site | Check IIS site binding |
| 404 on SPA routes | Missing URL Rewrite | Install URL Rewrite module |
| Upload fails | Permissions | Grant Modify to AppPool identity |
| DB connection error | Wrong server | Check connection string |
| JWT errors | Wrong secret key | Verify key in appsettings.Production.json |

### 8.2 Logs

| Log | Location |
|-----|----------|
| API stdout | `E:\inetpub\PensionAPI\logs\stdout` |
| IIS logs | `E:\inetpub\logs\LogFiles` |
| Windows Event | Event Viewer → Windows Logs → Application |
| Application logs | `E:\logs\pension.api` |

### 8.3 Verification Commands

```powershell
# Test API
Invoke-WebRequest -Uri "http://localhost:5141/api/auth/login" -Method POST -ContentType "application/json" -Body '{"username":"admin","password":"Admin@123"}'

# Test UI
Invoke-WebRequest -Uri "http://localhost" -UseBasicParsing

# Test SPA routing
Invoke-WebRequest -Uri "http://localhost/login" -UseBasicParsing

# Check IIS sites
Get-Website
Get-WebAppPoolState -Name "PensionAPI"

# Check SQL Server
sqlcmd -S .\SQLEXPRESS -Q "SELECT name FROM sys.databases WHERE name='PensionManagementDB'"
```

---

## 9. Security Checklist

- [ ] Change default admin password
- [ ] Use strong JWT secret key (32+ chars)
- [ ] Use strong encryption key (32 chars)
- [ ] Enable HTTPS (install SSL certificate)
- [ ] Restrict SQL Server to local access only
- [ ] Set proper file system permissions
- [ ] Remove Swagger from production
- [ ] Enable request logging
- [ ] Configure CORS for production domain only
- [ ] Regular database backups
- [ ] Monitor Windows Event Logs

---

## 10. Backup Strategy

### 10.1 Database Backup

```sql
-- Full backup
BACKUP DATABASE PensionManagementDB TO DISK = 'E:\Backups\PensionDB_Full.bak' WITH INIT;

-- Scheduled via SQL Server Agent: Daily full, hourly tlog
```

### 10.2 Application Backup

```powershell
# Backup deployment
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
Compress-Archive -Path "E:\inetpub\PensionAPI" -DestinationPath "E:\Backups\API_$timestamp.zip"
Compress-Archive -Path "E:\inetpub\PensionUI" -DestinationPath "E:\Backups\UI_$timestamp.zip"
```

### 10.3 File Storage Backup

```powershell
# Backup uploaded files
Compress-Archive -Path "E:\inetpub\PensionAPI\wwwroot\uploads" -DestinationPath "E:\Backups\Uploads_$timestamp.zip"
```
