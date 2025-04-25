using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CelHost.Migrations
{
    /// <inheritdoc />
    public partial class AddKeyIv : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Users",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "IV",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ClusterNode",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IV",
                table: "Cascade",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "Cascade",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IV",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Key",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ClusterNode");

            migrationBuilder.DropColumn(
                name: "IV",
                table: "Cascade");

            migrationBuilder.DropColumn(
                name: "Key",
                table: "Cascade");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
