using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Lisbeth.Bot.DataAccessLayer.Migrations
{
    public partial class rlemenu : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "message_id",
                table: "role_menu");

            migrationBuilder.DropColumn(
                name: "role_emoji_mapping",
                table: "role_menu");

            migrationBuilder.AddColumn<string>(
                name: "custom_select_component_id",
                table: "role_menu",
                type: "varchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "role_menu_option",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_menu_id = table.Column<long>(type: "bigint", nullable: false),
                    RoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    emoji = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    custom_select_option_value_id = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_menu_option", x => x.id);
                    table.ForeignKey(
                        name: "FK_role_menu_option_role_menu_role_menu_id",
                        column: x => x.role_menu_id,
                        principalTable: "role_menu",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_role_menu_option_role_menu_id",
                table: "role_menu_option",
                column: "role_menu_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "role_menu_option");

            migrationBuilder.DropColumn(
                name: "custom_select_component_id",
                table: "role_menu");

            migrationBuilder.AddColumn<long>(
                name: "message_id",
                table: "role_menu",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "role_emoji_mapping",
                table: "role_menu",
                type: "text",
                nullable: true);
        }
    }
}
