using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisbeth.Bot.DataAccessLayer.Migrations
{
    public partial class ef6postgrestimea : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "timestamptz",
                table: "embed_config",
                newName: "Timestamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "embed_config",
                newName: "timestamptz");
        }
    }
}
