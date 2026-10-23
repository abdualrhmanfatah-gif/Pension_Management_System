using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PensionMS.Infrastructure.Data;
using System.Text.RegularExpressions;

namespace PensionMS.IntegrationTests;

public sealed class SqlTestDatabase : IAsyncLifetime
{
    private readonly string _databaseName = "PensionMS_Test_" + Guid.NewGuid().ToString("N");
    private string _connectionString = "";
    private bool _created;

    public PensionDbContext OpenContext(params IInterceptor[] interceptors) => new(new DbContextOptionsBuilder<PensionDbContext>()
        .UseSqlServer(_connectionString, sql => sql.CommandTimeout(120))
        .AddInterceptors(interceptors).Options);

    public async Task InitializeAsync()
    {
        var configured = Environment.GetEnvironmentVariable("PENSIONMS_TEST_SQLSERVER");
        if (string.IsNullOrWhiteSpace(configured))
            throw new InvalidOperationException("حدد PENSIONMS_TEST_SQLSERVER لتشغيل اختبارات SQL المستقلة");
        ValidateName();
        var builder = new SqlConnectionStringBuilder(configured) { InitialCatalog = "master" };
        await using var master = new SqlConnection(builder.ConnectionString);
        await master.OpenAsync();
        await using (var command = new SqlCommand($"CREATE DATABASE [{_databaseName}]", master))
            await command.ExecuteNonQueryAsync();
        _created = true;
        builder.InitialCatalog = _databaseName;
        _connectionString = builder.ConnectionString;
        try
        {
            await using var db = OpenContext();
            // Use the real relational schema. Legacy RolePermissions seed data refers to
            // permissions absent from HasData; authorization tests seed their own roles.
            var model = db.GetService<IDesignTimeModel>().Model;
            var operations = db.GetService<IMigrationsModelDiffer>()
                .GetDifferences(null, model.GetRelationalModel())
                .Where(op => op is not InsertDataOperation { Table: "RolePermissions" })
                .ToList();
            var commands = db.GetService<IMigrationsSqlGenerator>().Generate(operations, model);
            foreach (var command in commands)
                await db.Database.ExecuteSqlRawAsync(command.CommandText);
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }

    public async Task DisposeAsync()
    {
        if (!_created) return;
        ValidateName();
        var builder = new SqlConnectionStringBuilder(_connectionString);
        if (builder.InitialCatalog != _databaseName)
            throw new InvalidOperationException("قاعدة الاختبار لا تطابق الاسم الذي أنشأته التجهيزات");
        await using var db = OpenContext();
        await db.Database.EnsureDeletedAsync();
        _created = false;
    }

    private void ValidateName()
    {
        if (!Regex.IsMatch(_databaseName, "^PensionMS_Test_[a-f0-9]{32}$"))
            throw new InvalidOperationException("اسم قاعدة الاختبار غير صالح");
    }
}
