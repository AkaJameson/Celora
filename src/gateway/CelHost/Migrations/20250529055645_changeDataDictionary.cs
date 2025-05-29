using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CelHost.Migrations
{
    /// <inheritdoc />
    public partial class changeDataDictionary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SystemDict_typeCode_itemCode",
                table: "SystemDict");

            migrationBuilder.DropColumn(
                name: "itemCode",
                table: "SystemDict");

            migrationBuilder.DropColumn(
                name: "itemDesc",
                table: "SystemDict");

            migrationBuilder.DropColumn(
                name: "itemName",
                table: "SystemDict");

            migrationBuilder.DropColumn(
                name: "itemValue",
                table: "SystemDict");

            migrationBuilder.RenameColumn(
                name: "PkId",
                table: "SystemDict",
                newName: "Id");

            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "SystemDict",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemDict_typeCode_typeName",
                table: "SystemDict",
                columns: new[] { "typeCode", "typeName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SystemDict_typeCode_typeName",
                table: "SystemDict");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "SystemDict");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "SystemDict",
                newName: "PkId");

            migrationBuilder.AddColumn<string>(
                name: "itemCode",
                table: "SystemDict",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "itemDesc",
                table: "SystemDict",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "itemName",
                table: "SystemDict",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "itemValue",
                table: "SystemDict",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SystemDict_typeCode_itemCode",
                table: "SystemDict",
                columns: new[] { "typeCode", "itemCode" },
                unique: true);
        }
    }
}
