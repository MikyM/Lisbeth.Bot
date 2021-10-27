using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Lisbeth.Bot.DataAccessLayer.Migrations
{
    public partial class Fiixxy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "footer",
                table: "embed_config",
                type: "varchar(2048)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "embed_config",
                type: "varchar(4096)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "author_url",
                table: "embed_config",
                type: "varchar(1000)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "author",
                table: "embed_config",
                type: "varchar(256)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "thumbnail",
                table: "embed_config",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "thumbnail_height",
                table: "embed_config",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "thumbnail_width",
                table: "embed_config",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "timestamp",
                table: "embed_config",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "title",
                table: "embed_config",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "thumbnail",
                table: "embed_config");

            migrationBuilder.DropColumn(
                name: "thumbnail_height",
                table: "embed_config");

            migrationBuilder.DropColumn(
                name: "thumbnail_width",
                table: "embed_config");

            migrationBuilder.DropColumn(
                name: "timestamp",
                table: "embed_config");

            migrationBuilder.DropColumn(
                name: "title",
                table: "embed_config");

            migrationBuilder.AlterColumn<string>(
                name: "footer",
                table: "embed_config",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(2048)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "embed_config",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(4096)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "author_url",
                table: "embed_config",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "author",
                table: "embed_config",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
