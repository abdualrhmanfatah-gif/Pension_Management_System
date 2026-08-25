param(
    [string]$ConnectionString = "Server=.\SQLEXPRESS;Database=PensionManagementDB;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30"
)

Write-Host "=== إعادة تهيئة قاعدة البيانات ===" -ForegroundColor Cyan

# 1. Drop database using sqlcmd (more reliable than dotnet ef)
Write-Host "[1/4] حذف قاعدة البيانات..." -ForegroundColor Yellow
try {
    $conn = New-Object System.Data.SqlClient.SqlConnection $ConnectionString
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "DROP DATABASE PensionManagementDB"
    $cmd.ExecuteNonQuery() | Out-Null
    $conn.Close()
    Write-Host "  ✅ تم حذف قاعدة البيانات" -ForegroundColor Green
} catch {
    if ($_.Exception.Message -match "Cannot open database") {
        Write-Host "  ⚠️ قاعدة البيانات غير موجودة أصلاً" -ForegroundColor Yellow
    } else {
        Write-Host "  ❌ خطأ: $_" -ForegroundColor Red
        exit 1
    }
}

# 2. Apply migrations (creates database + tables + HasData)
Write-Host "[2/4] تطبيق الميغريشن..." -ForegroundColor Yellow
dotnet ef database update --project "..\PensionMS.Infrastructure\PensionMS.Infrastructure.csproj" --startup-project "." --connection $ConnectionString 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "  ❌ فشل تطبيق الميغريشن" -ForegroundColor Red
    exit 1
}
Write-Host "  ✅ تم تطبيق الميغريشن" -ForegroundColor Green

# 3. Ensure IIS APPPOOL\PensionAPI has database access
Write-Host "[3/4] التأكد من صلاحية IIS APPPOOL\PensionAPI..." -ForegroundColor Yellow
try {
    $conn = New-Object System.Data.SqlClient.SqlConnection $ConnectionString
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = @"
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = 'IIS APPPOOL\PensionAPI')
    CREATE LOGIN [IIS APPPOOL\PensionAPI] FROM WINDOWS;
IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = 'IIS APPPOOL\PensionAPI')
BEGIN
    CREATE USER [IIS APPPOOL\PensionAPI] FOR LOGIN [IIS APPPOOL\PensionAPI];
    ALTER ROLE db_owner ADD MEMBER [IIS APPPOOL\PensionAPI];
END
"@
    $cmd.ExecuteNonQuery() | Out-Null
    $conn.Close()
    Write-Host "  ✅ تم التأكد من صلاحية IIS APPPOOL\PensionAPI" -ForegroundColor Green
} catch {
    Write-Host "  ❌ خطأ: $_" -ForegroundColor Red
}

# 4. Restart API app pool to trigger seeding
Write-Host "[4/4] إعادة تشغيل PensionAPI..." -ForegroundColor Yellow
try {
    Restart-WebAppPool -Name "PensionAPI" -ErrorAction Stop
    Write-Host "  ✅ تم إعادة التشغيل" -ForegroundColor Green
} catch {
    Write-Host "  ⚠️ تعذر إعادة التشغيل (قد تحتاج لتشغيل السكربت كمسؤول)" -ForegroundColor Yellow
}

Write-Host "`n=== اكتملت إعادة التهيئة ===" -ForegroundColor Cyan
Write-Host "انتظر 10 ثوانٍ ثم استخدم API" -ForegroundColor Gray
