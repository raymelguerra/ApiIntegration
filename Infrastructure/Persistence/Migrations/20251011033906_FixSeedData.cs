using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "main_app",
                table: "SyncSchedules",
                keyColumn: "Id",
                keyValue: new Guid("22032dc9-7cef-43c3-a005-7b5e746a89e7"));

            migrationBuilder.DeleteData(
                schema: "main_app",
                table: "SyncSchedules",
                keyColumn: "Id",
                keyValue: new Guid("7969ba0a-af1c-477d-9a18-459a97454a7f"));

            migrationBuilder.DeleteData(
                schema: "main_app",
                table: "SyncSchedules",
                keyColumn: "Id",
                keyValue: new Guid("a4d347ab-c00e-40d5-bf89-8ea930eb981c"));

            migrationBuilder.DeleteData(
                schema: "main_app",
                table: "SyncSchedules",
                keyColumn: "Id",
                keyValue: new Guid("cc4d4742-978c-43bd-8303-fa0e5992003c"));

            migrationBuilder.DeleteData(
                schema: "main_app",
                table: "SyncSchedules",
                keyColumn: "Id",
                keyValue: new Guid("f0092452-d3da-4034-82c0-42e60c841d85"));

            migrationBuilder.InsertData(
                schema: "main_app",
                table: "SyncSchedules",
                columns: new[] { "Id", "CronExpression", "Enabled", "JobKey", "LastModifiedUtc" },
                values: new object[,]
                {
                    { new Guid("03f7acfd-bb4c-42e6-b92c-41f12526ce34"), "0 0 0 * * ?", true, "UpdateStockPhotoValuations", new DateTime(2024, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("279517a1-718e-4e70-b7fc-ce9dca2172f9"), "0 0 0 * * ?", true, "UpdateMerchandiseEntry", new DateTime(2024, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("4b18a191-d5bc-4d29-80cf-384d7b1279fa"), "0 0 0 * * ?", true, "UpdateProviders", new DateTime(2024, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("6f194b41-15de-49dd-9c57-4389b357d80e"), "0 0 0 * * ?", true, "UpdateWarehouses", new DateTime(2024, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a46428f0-f01d-4f86-8c18-11515dc9bbed"), "0 0 0 * * ?", true, "UpdateMaterials", new DateTime(2024, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "main_app",
                table: "SyncSchedules",
                keyColumn: "Id",
                keyValue: new Guid("03f7acfd-bb4c-42e6-b92c-41f12526ce34"));

            migrationBuilder.DeleteData(
                schema: "main_app",
                table: "SyncSchedules",
                keyColumn: "Id",
                keyValue: new Guid("279517a1-718e-4e70-b7fc-ce9dca2172f9"));

            migrationBuilder.DeleteData(
                schema: "main_app",
                table: "SyncSchedules",
                keyColumn: "Id",
                keyValue: new Guid("4b18a191-d5bc-4d29-80cf-384d7b1279fa"));

            migrationBuilder.DeleteData(
                schema: "main_app",
                table: "SyncSchedules",
                keyColumn: "Id",
                keyValue: new Guid("6f194b41-15de-49dd-9c57-4389b357d80e"));

            migrationBuilder.DeleteData(
                schema: "main_app",
                table: "SyncSchedules",
                keyColumn: "Id",
                keyValue: new Guid("a46428f0-f01d-4f86-8c18-11515dc9bbed"));

            migrationBuilder.InsertData(
                schema: "main_app",
                table: "SyncSchedules",
                columns: new[] { "Id", "CronExpression", "Enabled", "JobKey", "LastModifiedUtc" },
                values: new object[,]
                {
                    { new Guid("22032dc9-7cef-43c3-a005-7b5e746a89e7"), "0 0 0 * * ?", true, "UpdateProviders", new DateTime(2025, 10, 5, 4, 53, 8, 173, DateTimeKind.Utc).AddTicks(5640) },
                    { new Guid("7969ba0a-af1c-477d-9a18-459a97454a7f"), "0 0 0 * * ?", true, "UpdateWarehouses", new DateTime(2025, 10, 5, 4, 53, 8, 173, DateTimeKind.Utc).AddTicks(5780) },
                    { new Guid("a4d347ab-c00e-40d5-bf89-8ea930eb981c"), "0 0 0 * * ?", true, "UpdateStockPhotoValuations", new DateTime(2025, 10, 5, 4, 53, 8, 173, DateTimeKind.Utc).AddTicks(5780) },
                    { new Guid("cc4d4742-978c-43bd-8303-fa0e5992003c"), "0 0 0 * * ?", true, "UpdateMaterials", new DateTime(2025, 10, 5, 4, 53, 8, 173, DateTimeKind.Utc).AddTicks(5770) },
                    { new Guid("f0092452-d3da-4034-82c0-42e60c841d85"), "0 0 0 * * ?", true, "UpdateMerchandiseEntry", new DateTime(2025, 10, 5, 4, 53, 8, 173, DateTimeKind.Utc).AddTicks(5780) }
                });
        }
    }
}
