using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CelHost.Migrations
{
    /// <inheritdoc />
    public partial class blogMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hosts",
                table: "Cluster");

            migrationBuilder.DropColumn(
                name: "Methods",
                table: "Cluster");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Cluster");

            migrationBuilder.AddColumn<string>(
                name: "LoadBalancingPolicy",
                table: "Cluster",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "SystemDict",
                columns: table => new
                {
                    PkId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    typeCode = table.Column<string>(type: "TEXT", nullable: false),
                    typeName = table.Column<string>(type: "TEXT", nullable: false),
                    itemCode = table.Column<string>(type: "TEXT", nullable: false),
                    itemName = table.Column<string>(type: "TEXT", nullable: false),
                    itemValue = table.Column<string>(type: "TEXT", nullable: false),
                    itemDesc = table.Column<string>(type: "TEXT", nullable: true),
                    remark = table.Column<string>(type: "TEXT", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    order = table.Column<int>(type: "INTEGER", nullable: false),
                    superTypeCode = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemDict", x => x.PkId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemDict_typeCode_itemCode",
                table: "SystemDict",
                columns: new[] { "typeCode", "itemCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemDict");

            migrationBuilder.DropColumn(
                name: "LoadBalancingPolicy",
                table: "Cluster");

            migrationBuilder.AddColumn<string>(
                name: "Hosts",
                table: "Cluster",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Methods",
                table: "Cluster",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Cluster",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
