using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Lisbeth.Bot.DataAccessLayer.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_log",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    table_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    old_values = table.Column<string>(type: "text", nullable: true),
                    new_values = table.Column<string>(type: "text", nullable: false),
                    affected_columns = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    primary_key = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_log", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "embed_config",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    author = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    footer = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    image_url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    footer_image_url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    author_image_url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    fields = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_embed_config", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "guild",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guild_id = table.Column<long>(type: "bigint", nullable: false),
                    inviter_id = table.Column<long>(type: "bigint", nullable: false),
                    reminder_channel_id = table.Column<long>(type: "bigint", nullable: true),
                    embed_hex_color = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild", x => x.id);
                    table.UniqueConstraint("AK_guild_guild_id", x => x.guild_id);
                });

            migrationBuilder.CreateTable(
                name: "server_booster",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guild_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    boosting_since = table.Column<DateTime>(type: "timestamp", nullable: false),
                    boost_count = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_server_booster", x => x.id);
                    table.UniqueConstraint("AK_server_booster_user_id", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "ban",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guild_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    lifted_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    applied_until = table.Column<DateTime>(type: "timestamp", nullable: false),
                    applied_by_id = table.Column<long>(type: "bigint", nullable: false),
                    lifted_by_id = table.Column<long>(type: "bigint", nullable: false),
                    reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ban", x => x.id);
                    table.ForeignKey(
                        name: "FK_ban_guild_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guild",
                        principalColumn: "guild_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "guild_moderation_config",
                columns: table => new
                {
                    guild_id = table.Column<long>(type: "bigint", nullable: false),
                    member_events_log_channel_id = table.Column<long>(type: "bigint", nullable: true),
                    message_deleted_events_log_channel_id = table.Column<long>(type: "bigint", nullable: true),
                    message_updated_events_log_channel_id = table.Column<long>(type: "bigint", nullable: true),
                    mute_role_id = table.Column<long>(type: "bigint", nullable: false),
                    member_welcome_message = table.Column<string>(type: "text", nullable: true),
                    member_welcome_message_title = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false),
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild_moderation_config", x => x.guild_id);
                    table.ForeignKey(
                        name: "FK_guild_moderation_config_guild_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guild",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "guild_ticketing_config",
                columns: table => new
                {
                    guild_id = table.Column<long>(type: "bigint", nullable: false),
                    log_channel_id = table.Column<long>(type: "bigint", nullable: true),
                    last_ticket_id = table.Column<long>(type: "bigint", nullable: false),
                    closed_category_id = table.Column<long>(type: "bigint", nullable: false),
                    opened_category_id = table.Column<long>(type: "bigint", nullable: false),
                    clean_after = table.Column<long>(type: "bigint", nullable: true),
                    close_after = table.Column<long>(type: "bigint", nullable: true),
                    opened_name_prefix = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    closed_name_prefix = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    ticket_welcome_message_description = table.Column<string>(type: "text", nullable: true),
                    ticket_welcome_message_fields = table.Column<string>(type: "text", nullable: true),
                    ticket_center_message_description = table.Column<string>(type: "text", nullable: true),
                    ticket_center_message_fields = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false),
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild_ticketing_config", x => x.guild_id);
                    table.ForeignKey(
                        name: "FK_guild_ticketing_config_guild_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guild",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mute",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guild_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    lifted_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    applied_until = table.Column<DateTime>(type: "timestamp", nullable: false),
                    applied_by_id = table.Column<long>(type: "bigint", nullable: false),
                    lifted_by_id = table.Column<long>(type: "bigint", nullable: false),
                    reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mute", x => x.id);
                    table.ForeignKey(
                        name: "FK_mute_guild_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guild",
                        principalColumn: "guild_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prune",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    moderator_id = table.Column<long>(type: "bigint", nullable: false),
                    channel_id = table.Column<long>(type: "bigint", nullable: false),
                    messages = table.Column<string>(type: "text", nullable: true),
                    count = table.Column<int>(type: "int", nullable: false),
                    guild_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prune", x => x.id);
                    table.ForeignKey(
                        name: "FK_prune_guild_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guild",
                        principalColumn: "guild_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recurring_reminder",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    cron_expression = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    guild_id = table.Column<long>(type: "bigint", nullable: true),
                    embed_config_id = table.Column<long>(type: "bigint", nullable: true),
                    text = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recurring_reminder", x => x.id);
                    table.ForeignKey(
                        name: "FK_recurring_reminder_embed_config_embed_config_id",
                        column: x => x.embed_config_id,
                        principalTable: "embed_config",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_recurring_reminder_guild_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guild",
                        principalColumn: "guild_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reminder",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    set_for_date = table.Column<DateTime>(type: "timestamp", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    guild_id = table.Column<long>(type: "bigint", nullable: true),
                    embed_config_id = table.Column<long>(type: "bigint", nullable: true),
                    text = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reminder", x => x.id);
                    table.ForeignKey(
                        name: "FK_reminder_embed_config_embed_config_id",
                        column: x => x.embed_config_id,
                        principalTable: "embed_config",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reminder_guild_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guild",
                        principalColumn: "guild_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "role_menu",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    guild_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    message_id = table.Column<long>(type: "bigint", nullable: false),
                    text = table.Column<string>(type: "text", nullable: true),
                    embed_config_id = table.Column<long>(type: "bigint", nullable: false),
                    role_emoji_mapping = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_menu", x => x.id);
                    table.ForeignKey(
                        name: "FK_role_menu_embed_config_embed_config_id",
                        column: x => x.embed_config_id,
                        principalTable: "embed_config",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_role_menu_guild_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guild",
                        principalColumn: "guild_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tag",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    guild_id = table.Column<long>(type: "bigint", nullable: false),
                    text = table.Column<string>(type: "text", nullable: true),
                    embed_config_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tag", x => x.id);
                    table.ForeignKey(
                        name: "FK_tag_embed_config_embed_config_id",
                        column: x => x.embed_config_id,
                        principalTable: "embed_config",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tag_guild_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guild",
                        principalColumn: "guild_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ticket",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    channel_id = table.Column<long>(type: "bigint", nullable: false),
                    guild_specific_id = table.Column<long>(type: "bigint", nullable: false),
                    reopened_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    closed_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    closed_by_id = table.Column<long>(type: "bigint", nullable: true),
                    reopened_by_id = table.Column<long>(type: "bigint", nullable: true),
                    message_open_id = table.Column<long>(type: "bigint", nullable: false),
                    message_close_id = table.Column<long>(type: "bigint", nullable: true),
                    message_reopen_id = table.Column<long>(type: "bigint", nullable: true),
                    added_users = table.Column<string>(type: "text", nullable: true),
                    added_roles = table.Column<string>(type: "text", nullable: true),
                    is_private = table.Column<bool>(type: "boolean", nullable: false),
                    guild_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ticket", x => x.id);
                    table.ForeignKey(
                        name: "FK_ticket_guild_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guild",
                        principalColumn: "guild_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "guild_server_booster",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    guild_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild_server_booster", x => new { x.guild_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_guild_server_booster_guild_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guild",
                        principalColumn: "guild_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_guild_server_booster_server_booster_user_id",
                        column: x => x.user_id,
                        principalTable: "server_booster",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ban_guild_id",
                table: "ban",
                column: "guild_id");

            migrationBuilder.CreateIndex(
                name: "IX_guild_server_booster_user_id",
                table: "guild_server_booster",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_mute_guild_id",
                table: "mute",
                column: "guild_id");

            migrationBuilder.CreateIndex(
                name: "IX_prune_guild_id",
                table: "prune",
                column: "guild_id");

            migrationBuilder.CreateIndex(
                name: "IX_recurring_reminder_embed_config_id",
                table: "recurring_reminder",
                column: "embed_config_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recurring_reminder_guild_id",
                table: "recurring_reminder",
                column: "guild_id");

            migrationBuilder.CreateIndex(
                name: "IX_reminder_embed_config_id",
                table: "reminder",
                column: "embed_config_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reminder_guild_id",
                table: "reminder",
                column: "guild_id");

            migrationBuilder.CreateIndex(
                name: "IX_role_menu_embed_config_id",
                table: "role_menu",
                column: "embed_config_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_role_menu_guild_id",
                table: "role_menu",
                column: "guild_id");

            migrationBuilder.CreateIndex(
                name: "IX_tag_embed_config_id",
                table: "tag",
                column: "embed_config_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tag_guild_id",
                table: "tag",
                column: "guild_id");

            migrationBuilder.CreateIndex(
                name: "IX_tag_name",
                table: "tag",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ticket_guild_id",
                table: "ticket",
                column: "guild_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "ban");

            migrationBuilder.DropTable(
                name: "guild_moderation_config");

            migrationBuilder.DropTable(
                name: "guild_server_booster");

            migrationBuilder.DropTable(
                name: "guild_ticketing_config");

            migrationBuilder.DropTable(
                name: "mute");

            migrationBuilder.DropTable(
                name: "prune");

            migrationBuilder.DropTable(
                name: "recurring_reminder");

            migrationBuilder.DropTable(
                name: "reminder");

            migrationBuilder.DropTable(
                name: "role_menu");

            migrationBuilder.DropTable(
                name: "tag");

            migrationBuilder.DropTable(
                name: "ticket");

            migrationBuilder.DropTable(
                name: "server_booster");

            migrationBuilder.DropTable(
                name: "embed_config");

            migrationBuilder.DropTable(
                name: "guild");
        }
    }
}
