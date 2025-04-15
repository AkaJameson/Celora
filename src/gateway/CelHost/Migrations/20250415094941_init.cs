using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CelHost.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlocklistRecord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BlockIp = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    BlockReason = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    BlockCount = table.Column<int>(type: "INTEGER", nullable: false),
                    EffectiveTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpireTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsPermanent = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastViolationTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlocklistRecord", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cascade",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cascade", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HealthCheckOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ActiveInterval = table.Column<int>(type: "INTEGER", nullable: false),
                    ActiveTimeout = table.Column<int>(type: "INTEGER", nullable: false),
                    ActivePath = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthCheckOptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RateLimitPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PolicyName = table.Column<string>(type: "TEXT", nullable: false),
                    PermitLimit = table.Column<int>(type: "INTEGER", nullable: false),
                    Window = table.Column<int>(type: "INTEGER", nullable: false),
                    QueueProcessingOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    QueueLimit = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RateLimitPolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Account = table.Column<string>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    LockEnable = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsLock = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cluster",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RouteId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Path = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Hosts = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    RateLimitPolicyName = table.Column<string>(type: "TEXT", nullable: false),
                    Methods = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    HealthCheck = table.Column<bool>(type: "INTEGER", nullable: false),
                    HealthCheckId = table.Column<int>(type: "INTEGER", nullable: false),
                    CheckOptionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cluster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cluster_HealthCheckOptions_CheckOptionId",
                        column: x => x.CheckOptionId,
                        principalTable: "HealthCheckOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cluster_HealthCheckOptions_HealthCheckId",
                        column: x => x.HealthCheckId,
                        principalTable: "HealthCheckOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClusterNode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClusterId = table.Column<int>(type: "INTEGER", nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastHealthCheck = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClusterNode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClusterNode_Cluster_ClusterId",
                        column: x => x.ClusterId,
                        principalTable: "Cluster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "Host_Block_Ip",
                table: "BlocklistRecord",
                column: "BlockIp",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cluster_CheckOptionId",
                table: "Cluster",
                column: "CheckOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Cluster_HealthCheckId",
                table: "Cluster",
                column: "HealthCheckId");

            migrationBuilder.CreateIndex(
                name: "IX_ClusterNode_ClusterId",
                table: "ClusterNode",
                column: "ClusterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlocklistRecord");

            migrationBuilder.DropTable(
                name: "Cascade");

            migrationBuilder.DropTable(
                name: "ClusterNode");

            migrationBuilder.DropTable(
                name: "RateLimitPolicies");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Cluster");

            migrationBuilder.DropTable(
                name: "HealthCheckOptions");
        }
    }
}
