using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CelHost.Migrations
{
    /// <inheritdoc />
    public partial class deletesomeProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cluster_HealthCheckOptions_HealthCheckId",
                table: "Cluster");

            migrationBuilder.DropColumn(
                name: "superTypeCode",
                table: "SystemDict");

            migrationBuilder.DropColumn(
                name: "LastHealthCheck",
                table: "ClusterNode");

            migrationBuilder.RenameColumn(
                name: "ActiveTimeout",
                table: "HealthCheckOptions",
                newName: "Timeout");

            migrationBuilder.RenameColumn(
                name: "ActiveInterval",
                table: "HealthCheckOptions",
                newName: "Interval");

            migrationBuilder.AlterColumn<int>(
                name: "HealthCheckId",
                table: "Cluster",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "ClusterName",
                table: "Cluster",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Cluster_HealthCheckOptions_HealthCheckId",
                table: "Cluster",
                column: "HealthCheckId",
                principalTable: "HealthCheckOptions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cluster_HealthCheckOptions_HealthCheckId",
                table: "Cluster");

            migrationBuilder.DropColumn(
                name: "ClusterName",
                table: "Cluster");

            migrationBuilder.RenameColumn(
                name: "Timeout",
                table: "HealthCheckOptions",
                newName: "ActiveTimeout");

            migrationBuilder.RenameColumn(
                name: "Interval",
                table: "HealthCheckOptions",
                newName: "ActiveInterval");

            migrationBuilder.AddColumn<string>(
                name: "superTypeCode",
                table: "SystemDict",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastHealthCheck",
                table: "ClusterNode",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "HealthCheckId",
                table: "Cluster",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cluster_HealthCheckOptions_HealthCheckId",
                table: "Cluster",
                column: "HealthCheckId",
                principalTable: "HealthCheckOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
