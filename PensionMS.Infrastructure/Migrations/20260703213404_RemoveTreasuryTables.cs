using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PensionMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTreasuryTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TreasuryIncomes");

            migrationBuilder.DropTable(
                name: "TreasuryTransactions");

            migrationBuilder.DropTable(
                name: "Treasuries");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 117);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 118);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1383);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1384);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1385);

            migrationBuilder.DeleteData(
                table: "SystemTables",
                keyColumn: "TableID",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "SystemTables",
                keyColumn: "TableID",
                keyValue: 45);

            migrationBuilder.DropColumn(
                name: "TreasuryAllocated",
                table: "Quarters");

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 21, 34, 0, 953, DateTimeKind.Utc).AddTicks(321));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 21, 34, 0, 953, DateTimeKind.Utc).AddTicks(357));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 21, 34, 0, 953, DateTimeKind.Utc).AddTicks(359));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 21, 34, 0, 953, DateTimeKind.Utc).AddTicks(361));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 21, 34, 0, 953, DateTimeKind.Utc).AddTicks(362));

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1116,
                column: "PermissionId",
                value: 119);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1117,
                column: "PermissionId",
                value: 120);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1118,
                column: "PermissionId",
                value: 121);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1119,
                column: "PermissionId",
                value: 122);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1120,
                column: "PermissionId",
                value: 123);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1121,
                column: "PermissionId",
                value: 124);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1122,
                column: "PermissionId",
                value: 125);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1123,
                column: "PermissionId",
                value: 126);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1124,
                column: "PermissionId",
                value: 127);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1125,
                column: "PermissionId",
                value: 128);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1126,
                column: "PermissionId",
                value: 129);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1127,
                column: "PermissionId",
                value: 130);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1128,
                column: "PermissionId",
                value: 131);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1129,
                column: "PermissionId",
                value: 132);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1130,
                column: "PermissionId",
                value: 133);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1131,
                column: "PermissionId",
                value: 134);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1132,
                column: "PermissionId",
                value: 135);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1133,
                column: "PermissionId",
                value: 136);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1134,
                column: "PermissionId",
                value: 137);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1135,
                column: "PermissionId",
                value: 138);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1136,
                column: "PermissionId",
                value: 139);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1137,
                column: "PermissionId",
                value: 140);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1138,
                column: "PermissionId",
                value: 141);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1139,
                column: "PermissionId",
                value: 142);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1140,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 1, 2 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1141,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 2, 2 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1142,
                column: "PermissionId",
                value: 4);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1143,
                column: "PermissionId",
                value: 5);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1144,
                column: "PermissionId",
                value: 6);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1145,
                column: "PermissionId",
                value: 7);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1146,
                column: "PermissionId",
                value: 9);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1147,
                column: "PermissionId",
                value: 13);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1148,
                column: "PermissionId",
                value: 17);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1149,
                column: "PermissionId",
                value: 18);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1150,
                column: "PermissionId",
                value: 19);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1151,
                column: "PermissionId",
                value: 20);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1152,
                column: "PermissionId",
                value: 21);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1153,
                column: "PermissionId",
                value: 22);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1154,
                column: "PermissionId",
                value: 23);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1155,
                column: "PermissionId",
                value: 49);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1156,
                column: "PermissionId",
                value: 54);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1157,
                column: "PermissionId",
                value: 77);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1158,
                column: "PermissionId",
                value: 24);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1159,
                column: "PermissionId",
                value: 25);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1160,
                column: "PermissionId",
                value: 26);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1161,
                column: "PermissionId",
                value: 27);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1162,
                column: "PermissionId",
                value: 28);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1163,
                column: "PermissionId",
                value: 50);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1164,
                column: "PermissionId",
                value: 55);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1165,
                column: "PermissionId",
                value: 78);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1166,
                column: "PermissionId",
                value: 29);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1167,
                column: "PermissionId",
                value: 30);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1168,
                column: "PermissionId",
                value: 60);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1169,
                column: "PermissionId",
                value: 84);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1170,
                column: "PermissionId",
                value: 85);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1171,
                column: "PermissionId",
                value: 31);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1172,
                column: "PermissionId",
                value: 32);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1173,
                column: "PermissionId",
                value: 52);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1174,
                column: "PermissionId",
                value: 57);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1175,
                column: "PermissionId",
                value: 80);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1176,
                column: "PermissionId",
                value: 33);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1177,
                column: "PermissionId",
                value: 34);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1178,
                column: "PermissionId",
                value: 53);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1179,
                column: "PermissionId",
                value: 56);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1180,
                column: "PermissionId",
                value: 79);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1181,
                column: "PermissionId",
                value: 35);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1182,
                column: "PermissionId",
                value: 36);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1183,
                column: "PermissionId",
                value: 61);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1184,
                column: "PermissionId",
                value: 86);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1185,
                column: "PermissionId",
                value: 87);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1186,
                column: "PermissionId",
                value: 37);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1187,
                column: "PermissionId",
                value: 38);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1188,
                column: "PermissionId",
                value: 51);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1189,
                column: "PermissionId",
                value: 58);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1190,
                column: "PermissionId",
                value: 81);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1191,
                column: "PermissionId",
                value: 39);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1192,
                column: "PermissionId",
                value: 40);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1193,
                column: "PermissionId",
                value: 62);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1194,
                column: "PermissionId",
                value: 90);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1195,
                column: "PermissionId",
                value: 91);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1196,
                column: "PermissionId",
                value: 41);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1197,
                column: "PermissionId",
                value: 42);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1198,
                column: "PermissionId",
                value: 63);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1199,
                column: "PermissionId",
                value: 65);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1200,
                column: "PermissionId",
                value: 83);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1201,
                column: "PermissionId",
                value: 88);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1202,
                column: "PermissionId",
                value: 89);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1203,
                column: "PermissionId",
                value: 46);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1204,
                column: "PermissionId",
                value: 82);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1205,
                column: "PermissionId",
                value: 66);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1206,
                column: "PermissionId",
                value: 43);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1207,
                column: "PermissionId",
                value: 112);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1208,
                column: "PermissionId",
                value: 121);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1209,
                column: "PermissionId",
                value: 122);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1210,
                column: "PermissionId",
                value: 123);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1211,
                column: "PermissionId",
                value: 125);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1212,
                column: "PermissionId",
                value: 126);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1213,
                column: "PermissionId",
                value: 127);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1214,
                column: "PermissionId",
                value: 140);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1215,
                column: "PermissionId",
                value: 142);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1216,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 1, 3 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1217,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 2, 3 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1218,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 4, 3 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1219,
                column: "PermissionId",
                value: 5);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1220,
                column: "PermissionId",
                value: 17);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1221,
                column: "PermissionId",
                value: 18);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1222,
                column: "PermissionId",
                value: 19);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1223,
                column: "PermissionId",
                value: 22);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1224,
                column: "PermissionId",
                value: 23);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1225,
                column: "PermissionId",
                value: 49);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1226,
                column: "PermissionId",
                value: 54);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1227,
                column: "PermissionId",
                value: 24);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1228,
                column: "PermissionId",
                value: 25);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1229,
                column: "PermissionId",
                value: 27);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1230,
                column: "PermissionId",
                value: 28);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1231,
                column: "PermissionId",
                value: 50);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1232,
                column: "PermissionId",
                value: 55);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1233,
                column: "PermissionId",
                value: 29);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1234,
                column: "PermissionId",
                value: 30);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1235,
                column: "PermissionId",
                value: 60);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1236,
                column: "PermissionId",
                value: 84);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1237,
                column: "PermissionId",
                value: 31);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1238,
                column: "PermissionId",
                value: 32);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1239,
                column: "PermissionId",
                value: 52);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1240,
                column: "PermissionId",
                value: 57);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1241,
                column: "PermissionId",
                value: 33);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1242,
                column: "PermissionId",
                value: 34);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1243,
                column: "PermissionId",
                value: 53);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1244,
                column: "PermissionId",
                value: 56);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1245,
                column: "PermissionId",
                value: 35);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1246,
                column: "PermissionId",
                value: 36);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1247,
                column: "PermissionId",
                value: 61);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1248,
                column: "PermissionId",
                value: 86);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1249,
                column: "PermissionId",
                value: 37);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1250,
                column: "PermissionId",
                value: 38);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1251,
                column: "PermissionId",
                value: 51);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1252,
                column: "PermissionId",
                value: 58);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1253,
                column: "PermissionId",
                value: 46);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1254,
                column: "PermissionId",
                value: 47);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1255,
                column: "PermissionId",
                value: 48);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1256,
                column: "PermissionId",
                value: 59);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1257,
                column: "PermissionId",
                value: 43);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1258,
                column: "PermissionId",
                value: 121);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1259,
                column: "PermissionId",
                value: 122);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1260,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 3, 4 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1261,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 4, 4 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1262,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 11, 4 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1263,
                column: "PermissionId",
                value: 12);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1264,
                column: "PermissionId",
                value: 17);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1265,
                column: "PermissionId",
                value: 22);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1266,
                column: "PermissionId",
                value: 49);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1267,
                column: "PermissionId",
                value: 54);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1268,
                column: "PermissionId",
                value: 24);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1269,
                column: "PermissionId",
                value: 27);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1270,
                column: "PermissionId",
                value: 50);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1271,
                column: "PermissionId",
                value: 55);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1272,
                column: "PermissionId",
                value: 29);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1273,
                column: "PermissionId",
                value: 31);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1274,
                column: "PermissionId",
                value: 52);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1275,
                column: "PermissionId",
                value: 57);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1276,
                column: "PermissionId",
                value: 33);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1277,
                column: "PermissionId",
                value: 53);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1278,
                column: "PermissionId",
                value: 56);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1279,
                column: "PermissionId",
                value: 35);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1280,
                column: "PermissionId",
                value: 37);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1281,
                column: "PermissionId",
                value: 38);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1282,
                column: "PermissionId",
                value: 51);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1283,
                column: "PermissionId",
                value: 58);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1284,
                column: "PermissionId",
                value: 39);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1285,
                column: "PermissionId",
                value: 41);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1286,
                column: "PermissionId",
                value: 42);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1287,
                column: "PermissionId",
                value: 63);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1288,
                column: "PermissionId",
                value: 65);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1289,
                column: "PermissionId",
                value: 43);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1290,
                column: "PermissionId",
                value: 44);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1291,
                column: "PermissionId",
                value: 45);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1292,
                column: "PermissionId",
                value: 64);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1293,
                column: "PermissionId",
                value: 88);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1294,
                column: "PermissionId",
                value: 46);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1295,
                column: "PermissionId",
                value: 47);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1296,
                column: "PermissionId",
                value: 48);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1297,
                column: "PermissionId",
                value: 59);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1298,
                column: "PermissionId",
                value: 66);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1299,
                column: "PermissionId",
                value: 67);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1300,
                column: "PermissionId",
                value: 68);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1301,
                column: "PermissionId",
                value: 69);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1302,
                column: "PermissionId",
                value: 70);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1303,
                column: "PermissionId",
                value: 71);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1304,
                column: "PermissionId",
                value: 72);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1305,
                column: "PermissionId",
                value: 92);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1306,
                column: "PermissionId",
                value: 93);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1307,
                column: "PermissionId",
                value: 73);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1308,
                column: "PermissionId",
                value: 75);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1309,
                column: "PermissionId",
                value: 95);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1310,
                column: "PermissionId",
                value: 96);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1311,
                column: "PermissionId",
                value: 98);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1312,
                column: "PermissionId",
                value: 99);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1313,
                column: "PermissionId",
                value: 113);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1314,
                column: "PermissionId",
                value: 114);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1315,
                column: "PermissionId",
                value: 115);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1316,
                column: "PermissionId",
                value: 116);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1317,
                column: "PermissionId",
                value: 117);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1318,
                column: "PermissionId",
                value: 118);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1319,
                column: "PermissionId",
                value: 119);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1320,
                column: "PermissionId",
                value: 120);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1321,
                column: "PermissionId",
                value: 121);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1322,
                column: "PermissionId",
                value: 140);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1323,
                column: "PermissionId",
                value: 141);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1324,
                column: "PermissionId",
                value: 142);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1325,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 4, 5 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1326,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 13, 5 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1327,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 17, 5 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1328,
                column: "PermissionId",
                value: 22);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1329,
                column: "PermissionId",
                value: 24);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1330,
                column: "PermissionId",
                value: 27);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1331,
                column: "PermissionId",
                value: 29);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1332,
                column: "PermissionId",
                value: 31);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1333,
                column: "PermissionId",
                value: 33);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1334,
                column: "PermissionId",
                value: 35);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1335,
                column: "PermissionId",
                value: 37);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1336,
                column: "PermissionId",
                value: 39);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1337,
                column: "PermissionId",
                value: 41);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1338,
                column: "PermissionId",
                value: 43);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1339,
                column: "PermissionId",
                value: 44);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1340,
                column: "PermissionId",
                value: 46);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1341,
                column: "PermissionId",
                value: 66);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1342,
                column: "PermissionId",
                value: 71);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1343,
                column: "PermissionId",
                value: 73);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1344,
                column: "PermissionId",
                value: 113);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1345,
                column: "PermissionId",
                value: 117);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1346,
                column: "PermissionId",
                value: 121);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1347,
                column: "PermissionId",
                value: 125);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1348,
                column: "PermissionId",
                value: 126);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1349,
                column: "PermissionId",
                value: 127);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1350,
                column: "PermissionId",
                value: 140);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1351,
                column: "PermissionId",
                value: 142);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1352,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 5, 6 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1353,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 17, 6 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1354,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 22, 6 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1355,
                column: "PermissionId",
                value: 24);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1356,
                column: "PermissionId",
                value: 27);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1357,
                column: "PermissionId",
                value: 29);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1358,
                column: "PermissionId",
                value: 30);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1359,
                column: "PermissionId",
                value: 60);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1360,
                column: "PermissionId",
                value: 84);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1361,
                column: "PermissionId",
                value: 31);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1362,
                column: "PermissionId",
                value: 32);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1363,
                column: "PermissionId",
                value: 52);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1364,
                column: "PermissionId",
                value: 57);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1365,
                column: "PermissionId",
                value: 33);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1366,
                column: "PermissionId",
                value: 34);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1367,
                column: "PermissionId",
                value: 53);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1368,
                column: "PermissionId",
                value: 56);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1369,
                column: "PermissionId",
                value: 35);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1370,
                column: "PermissionId",
                value: 36);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1371,
                column: "PermissionId",
                value: 61);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1372,
                column: "PermissionId",
                value: 86);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1373,
                column: "PermissionId",
                value: 37);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1374,
                column: "PermissionId",
                value: 43);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1375,
                column: "PermissionId",
                value: 121);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1376,
                column: "PermissionId",
                value: 122);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1377,
                column: "PermissionId",
                value: 123);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1378,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 73, 7 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1379,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 74, 7 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1380,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 95, 7 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1381,
                column: "PermissionId",
                value: 96);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1382,
                column: "PermissionId",
                value: 43);

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 21, 34, 0, 982, DateTimeKind.Utc).AddTicks(9323));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 21, 34, 0, 982, DateTimeKind.Utc).AddTicks(9620));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 21, 34, 0, 982, DateTimeKind.Utc).AddTicks(9623));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 21, 34, 0, 982, DateTimeKind.Utc).AddTicks(9625));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 21, 34, 0, 982, DateTimeKind.Utc).AddTicks(9628));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TreasuryAllocated",
                table: "Quarters",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Treasuries",
                columns: table => new
                {
                    TreasuryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CashBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TotalBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Treasuries", x => x.TreasuryID);
                    table.ForeignKey(
                        name: "FK_Treasuries_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TreasuryIncomes",
                columns: table => new
                {
                    IncomeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuarterID = table.Column<int>(type: "int", nullable: false),
                    TreasuryID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IncomeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IncomeSource = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreasuryIncomes", x => x.IncomeID);
                    table.ForeignKey(
                        name: "FK_TreasuryIncomes_Quarters_QuarterID",
                        column: x => x.QuarterID,
                        principalTable: "Quarters",
                        principalColumn: "QuarterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TreasuryIncomes_Treasuries_TreasuryID",
                        column: x => x.TreasuryID,
                        principalTable: "Treasuries",
                        principalColumn: "TreasuryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TreasuryIncomes_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TreasuryTransactions",
                columns: table => new
                {
                    TransactionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TreasuryID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ChequeImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChequeNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserID = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ReferenceID = table.Column<int>(type: "int", nullable: true),
                    ReferenceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreasuryTransactions", x => x.TransactionID);
                    table.ForeignKey(
                        name: "FK_TreasuryTransactions_Treasuries_TreasuryID",
                        column: x => x.TreasuryID,
                        principalTable: "Treasuries",
                        principalColumn: "TreasuryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TreasuryTransactions_Users_DeletedByUserID",
                        column: x => x.DeletedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 19, 54, 55, 549, DateTimeKind.Utc).AddTicks(6564));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 19, 54, 55, 549, DateTimeKind.Utc).AddTicks(6601));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 19, 54, 55, 549, DateTimeKind.Utc).AddTicks(6603));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 19, 54, 55, 549, DateTimeKind.Utc).AddTicks(6604));

            migrationBuilder.UpdateData(
                table: "AdministrativeOrderTypes",
                keyColumn: "AdministrativeOrderTypeID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 19, 54, 55, 549, DateTimeKind.Utc).AddTicks(6606));

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "PermissionId", "Category", "Code", "Description" },
                values: new object[,]
                {
                    { 117, "Treasury", "Treasury.View", "عرض الخزينة والحركات المالية" },
                    { 118, "Treasury", "Treasury.Manage", "إدارة عمليات الخزينة (إيداع/سحب)" }
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1116,
                column: "PermissionId",
                value: 117);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1117,
                column: "PermissionId",
                value: 118);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1118,
                column: "PermissionId",
                value: 119);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1119,
                column: "PermissionId",
                value: 120);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1120,
                column: "PermissionId",
                value: 121);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1121,
                column: "PermissionId",
                value: 122);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1122,
                column: "PermissionId",
                value: 123);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1123,
                column: "PermissionId",
                value: 124);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1124,
                column: "PermissionId",
                value: 125);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1125,
                column: "PermissionId",
                value: 126);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1126,
                column: "PermissionId",
                value: 127);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1127,
                column: "PermissionId",
                value: 128);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1128,
                column: "PermissionId",
                value: 129);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1129,
                column: "PermissionId",
                value: 130);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1130,
                column: "PermissionId",
                value: 131);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1131,
                column: "PermissionId",
                value: 132);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1132,
                column: "PermissionId",
                value: 133);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1133,
                column: "PermissionId",
                value: 134);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1134,
                column: "PermissionId",
                value: 135);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1135,
                column: "PermissionId",
                value: 136);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1136,
                column: "PermissionId",
                value: 137);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1137,
                column: "PermissionId",
                value: 138);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1138,
                column: "PermissionId",
                value: 139);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1139,
                column: "PermissionId",
                value: 140);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1140,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 141, 1 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1141,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 142, 1 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1142,
                column: "PermissionId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1143,
                column: "PermissionId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1144,
                column: "PermissionId",
                value: 4);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1145,
                column: "PermissionId",
                value: 5);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1146,
                column: "PermissionId",
                value: 6);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1147,
                column: "PermissionId",
                value: 7);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1148,
                column: "PermissionId",
                value: 9);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1149,
                column: "PermissionId",
                value: 13);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1150,
                column: "PermissionId",
                value: 17);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1151,
                column: "PermissionId",
                value: 18);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1152,
                column: "PermissionId",
                value: 19);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1153,
                column: "PermissionId",
                value: 20);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1154,
                column: "PermissionId",
                value: 21);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1155,
                column: "PermissionId",
                value: 22);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1156,
                column: "PermissionId",
                value: 23);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1157,
                column: "PermissionId",
                value: 49);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1158,
                column: "PermissionId",
                value: 54);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1159,
                column: "PermissionId",
                value: 77);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1160,
                column: "PermissionId",
                value: 24);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1161,
                column: "PermissionId",
                value: 25);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1162,
                column: "PermissionId",
                value: 26);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1163,
                column: "PermissionId",
                value: 27);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1164,
                column: "PermissionId",
                value: 28);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1165,
                column: "PermissionId",
                value: 50);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1166,
                column: "PermissionId",
                value: 55);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1167,
                column: "PermissionId",
                value: 78);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1168,
                column: "PermissionId",
                value: 29);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1169,
                column: "PermissionId",
                value: 30);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1170,
                column: "PermissionId",
                value: 60);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1171,
                column: "PermissionId",
                value: 84);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1172,
                column: "PermissionId",
                value: 85);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1173,
                column: "PermissionId",
                value: 31);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1174,
                column: "PermissionId",
                value: 32);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1175,
                column: "PermissionId",
                value: 52);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1176,
                column: "PermissionId",
                value: 57);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1177,
                column: "PermissionId",
                value: 80);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1178,
                column: "PermissionId",
                value: 33);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1179,
                column: "PermissionId",
                value: 34);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1180,
                column: "PermissionId",
                value: 53);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1181,
                column: "PermissionId",
                value: 56);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1182,
                column: "PermissionId",
                value: 79);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1183,
                column: "PermissionId",
                value: 35);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1184,
                column: "PermissionId",
                value: 36);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1185,
                column: "PermissionId",
                value: 61);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1186,
                column: "PermissionId",
                value: 86);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1187,
                column: "PermissionId",
                value: 87);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1188,
                column: "PermissionId",
                value: 37);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1189,
                column: "PermissionId",
                value: 38);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1190,
                column: "PermissionId",
                value: 51);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1191,
                column: "PermissionId",
                value: 58);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1192,
                column: "PermissionId",
                value: 81);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1193,
                column: "PermissionId",
                value: 39);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1194,
                column: "PermissionId",
                value: 40);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1195,
                column: "PermissionId",
                value: 62);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1196,
                column: "PermissionId",
                value: 90);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1197,
                column: "PermissionId",
                value: 91);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1198,
                column: "PermissionId",
                value: 41);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1199,
                column: "PermissionId",
                value: 42);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1200,
                column: "PermissionId",
                value: 63);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1201,
                column: "PermissionId",
                value: 65);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1202,
                column: "PermissionId",
                value: 83);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1203,
                column: "PermissionId",
                value: 88);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1204,
                column: "PermissionId",
                value: 89);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1205,
                column: "PermissionId",
                value: 46);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1206,
                column: "PermissionId",
                value: 82);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1207,
                column: "PermissionId",
                value: 66);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1208,
                column: "PermissionId",
                value: 43);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1209,
                column: "PermissionId",
                value: 112);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1210,
                column: "PermissionId",
                value: 117);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1211,
                column: "PermissionId",
                value: 121);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1212,
                column: "PermissionId",
                value: 122);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1213,
                column: "PermissionId",
                value: 123);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1214,
                column: "PermissionId",
                value: 125);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1215,
                column: "PermissionId",
                value: 126);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1216,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 127, 2 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1217,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 140, 2 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1218,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 142, 2 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1219,
                column: "PermissionId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1220,
                column: "PermissionId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1221,
                column: "PermissionId",
                value: 4);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1222,
                column: "PermissionId",
                value: 5);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1223,
                column: "PermissionId",
                value: 17);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1224,
                column: "PermissionId",
                value: 18);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1225,
                column: "PermissionId",
                value: 19);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1226,
                column: "PermissionId",
                value: 22);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1227,
                column: "PermissionId",
                value: 23);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1228,
                column: "PermissionId",
                value: 49);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1229,
                column: "PermissionId",
                value: 54);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1230,
                column: "PermissionId",
                value: 24);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1231,
                column: "PermissionId",
                value: 25);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1232,
                column: "PermissionId",
                value: 27);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1233,
                column: "PermissionId",
                value: 28);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1234,
                column: "PermissionId",
                value: 50);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1235,
                column: "PermissionId",
                value: 55);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1236,
                column: "PermissionId",
                value: 29);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1237,
                column: "PermissionId",
                value: 30);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1238,
                column: "PermissionId",
                value: 60);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1239,
                column: "PermissionId",
                value: 84);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1240,
                column: "PermissionId",
                value: 31);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1241,
                column: "PermissionId",
                value: 32);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1242,
                column: "PermissionId",
                value: 52);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1243,
                column: "PermissionId",
                value: 57);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1244,
                column: "PermissionId",
                value: 33);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1245,
                column: "PermissionId",
                value: 34);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1246,
                column: "PermissionId",
                value: 53);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1247,
                column: "PermissionId",
                value: 56);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1248,
                column: "PermissionId",
                value: 35);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1249,
                column: "PermissionId",
                value: 36);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1250,
                column: "PermissionId",
                value: 61);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1251,
                column: "PermissionId",
                value: 86);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1252,
                column: "PermissionId",
                value: 37);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1253,
                column: "PermissionId",
                value: 38);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1254,
                column: "PermissionId",
                value: 51);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1255,
                column: "PermissionId",
                value: 58);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1256,
                column: "PermissionId",
                value: 46);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1257,
                column: "PermissionId",
                value: 47);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1258,
                column: "PermissionId",
                value: 48);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1259,
                column: "PermissionId",
                value: 59);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1260,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 43, 3 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1261,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 121, 3 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1262,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 122, 3 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1263,
                column: "PermissionId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1264,
                column: "PermissionId",
                value: 4);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1265,
                column: "PermissionId",
                value: 11);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1266,
                column: "PermissionId",
                value: 12);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1267,
                column: "PermissionId",
                value: 17);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1268,
                column: "PermissionId",
                value: 22);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1269,
                column: "PermissionId",
                value: 49);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1270,
                column: "PermissionId",
                value: 54);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1271,
                column: "PermissionId",
                value: 24);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1272,
                column: "PermissionId",
                value: 27);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1273,
                column: "PermissionId",
                value: 50);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1274,
                column: "PermissionId",
                value: 55);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1275,
                column: "PermissionId",
                value: 29);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1276,
                column: "PermissionId",
                value: 31);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1277,
                column: "PermissionId",
                value: 52);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1278,
                column: "PermissionId",
                value: 57);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1279,
                column: "PermissionId",
                value: 33);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1280,
                column: "PermissionId",
                value: 53);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1281,
                column: "PermissionId",
                value: 56);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1282,
                column: "PermissionId",
                value: 35);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1283,
                column: "PermissionId",
                value: 37);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1284,
                column: "PermissionId",
                value: 38);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1285,
                column: "PermissionId",
                value: 51);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1286,
                column: "PermissionId",
                value: 58);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1287,
                column: "PermissionId",
                value: 39);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1288,
                column: "PermissionId",
                value: 41);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1289,
                column: "PermissionId",
                value: 42);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1290,
                column: "PermissionId",
                value: 63);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1291,
                column: "PermissionId",
                value: 65);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1292,
                column: "PermissionId",
                value: 43);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1293,
                column: "PermissionId",
                value: 44);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1294,
                column: "PermissionId",
                value: 45);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1295,
                column: "PermissionId",
                value: 64);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1296,
                column: "PermissionId",
                value: 88);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1297,
                column: "PermissionId",
                value: 46);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1298,
                column: "PermissionId",
                value: 47);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1299,
                column: "PermissionId",
                value: 48);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1300,
                column: "PermissionId",
                value: 59);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1301,
                column: "PermissionId",
                value: 66);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1302,
                column: "PermissionId",
                value: 67);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1303,
                column: "PermissionId",
                value: 68);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1304,
                column: "PermissionId",
                value: 69);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1305,
                column: "PermissionId",
                value: 70);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1306,
                column: "PermissionId",
                value: 71);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1307,
                column: "PermissionId",
                value: 72);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1308,
                column: "PermissionId",
                value: 92);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1309,
                column: "PermissionId",
                value: 93);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1310,
                column: "PermissionId",
                value: 73);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1311,
                column: "PermissionId",
                value: 75);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1312,
                column: "PermissionId",
                value: 95);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1313,
                column: "PermissionId",
                value: 96);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1314,
                column: "PermissionId",
                value: 98);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1315,
                column: "PermissionId",
                value: 99);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1316,
                column: "PermissionId",
                value: 113);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1317,
                column: "PermissionId",
                value: 114);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1318,
                column: "PermissionId",
                value: 115);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1319,
                column: "PermissionId",
                value: 116);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1320,
                column: "PermissionId",
                value: 117);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1321,
                column: "PermissionId",
                value: 118);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1322,
                column: "PermissionId",
                value: 119);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1323,
                column: "PermissionId",
                value: 120);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1324,
                column: "PermissionId",
                value: 121);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1325,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 140, 4 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1326,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 141, 4 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1327,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 142, 4 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1328,
                column: "PermissionId",
                value: 4);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1329,
                column: "PermissionId",
                value: 13);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1330,
                column: "PermissionId",
                value: 17);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1331,
                column: "PermissionId",
                value: 22);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1332,
                column: "PermissionId",
                value: 24);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1333,
                column: "PermissionId",
                value: 27);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1334,
                column: "PermissionId",
                value: 29);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1335,
                column: "PermissionId",
                value: 31);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1336,
                column: "PermissionId",
                value: 33);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1337,
                column: "PermissionId",
                value: 35);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1338,
                column: "PermissionId",
                value: 37);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1339,
                column: "PermissionId",
                value: 39);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1340,
                column: "PermissionId",
                value: 41);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1341,
                column: "PermissionId",
                value: 43);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1342,
                column: "PermissionId",
                value: 44);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1343,
                column: "PermissionId",
                value: 46);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1344,
                column: "PermissionId",
                value: 66);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1345,
                column: "PermissionId",
                value: 71);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1346,
                column: "PermissionId",
                value: 73);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1347,
                column: "PermissionId",
                value: 113);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1348,
                column: "PermissionId",
                value: 117);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1349,
                column: "PermissionId",
                value: 121);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1350,
                column: "PermissionId",
                value: 125);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1351,
                column: "PermissionId",
                value: 126);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1352,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 127, 5 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1353,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 140, 5 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1354,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 142, 5 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1355,
                column: "PermissionId",
                value: 5);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1356,
                column: "PermissionId",
                value: 17);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1357,
                column: "PermissionId",
                value: 22);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1358,
                column: "PermissionId",
                value: 24);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1359,
                column: "PermissionId",
                value: 27);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1360,
                column: "PermissionId",
                value: 29);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1361,
                column: "PermissionId",
                value: 30);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1362,
                column: "PermissionId",
                value: 60);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1363,
                column: "PermissionId",
                value: 84);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1364,
                column: "PermissionId",
                value: 31);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1365,
                column: "PermissionId",
                value: 32);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1366,
                column: "PermissionId",
                value: 52);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1367,
                column: "PermissionId",
                value: 57);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1368,
                column: "PermissionId",
                value: 33);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1369,
                column: "PermissionId",
                value: 34);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1370,
                column: "PermissionId",
                value: 53);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1371,
                column: "PermissionId",
                value: 56);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1372,
                column: "PermissionId",
                value: 35);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1373,
                column: "PermissionId",
                value: 36);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1374,
                column: "PermissionId",
                value: 61);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1375,
                column: "PermissionId",
                value: 86);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1376,
                column: "PermissionId",
                value: 37);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1377,
                column: "PermissionId",
                value: 43);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1378,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 121, 6 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1379,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 122, 6 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1380,
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { 123, 6 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1381,
                column: "PermissionId",
                value: 73);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 1382,
                column: "PermissionId",
                value: 74);

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "RolePermissionId", "CreatedAt", "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 1383, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 95, 7 },
                    { 1384, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 96, 7 },
                    { 1385, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 43, 7 }
                });

            migrationBuilder.InsertData(
                table: "SystemTables",
                columns: new[] { "TableID", "CreatedAt", "Description", "TableNameAr", "TableNameEn" },
                values: new object[,]
                {
                    { 44, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "الخزائن المالية", "Treasuries" },
                    { 45, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "حركات الخزينة", "TreasuryTransactions" }
                });

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 19, 54, 55, 585, DateTimeKind.Utc).AddTicks(8868));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 19, 54, 55, 585, DateTimeKind.Utc).AddTicks(8906));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 19, 54, 55, 585, DateTimeKind.Utc).AddTicks(8908));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 19, 54, 55, 585, DateTimeKind.Utc).AddTicks(8910));

            migrationBuilder.UpdateData(
                table: "TransactionStatuses",
                keyColumn: "StatusID",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 3, 19, 54, 55, 585, DateTimeKind.Utc).AddTicks(8913));

            migrationBuilder.CreateIndex(
                name: "IX_Treasuries_DeletedByUserID",
                table: "Treasuries",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryIncomes_DeletedByUserID",
                table: "TreasuryIncomes",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryIncomes_QuarterID",
                table: "TreasuryIncomes",
                column: "QuarterID");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryIncomes_TreasuryID",
                table: "TreasuryIncomes",
                column: "TreasuryID");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryTransactions_DeletedByUserID",
                table: "TreasuryTransactions",
                column: "DeletedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryTransactions_TreasuryID",
                table: "TreasuryTransactions",
                column: "TreasuryID");
        }
    }
}
