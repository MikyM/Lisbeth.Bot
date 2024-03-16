using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Lisbeth.Bot.DataAccessLayer.Migrations
{
    public partial class init : Migration
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
                    author = table.Column<string>(type: "varchar(256)", maxLength: 200, nullable: true),
                    author_url = table.Column<string>(type: "varchar(1000)", maxLength: 200, nullable: true),
                    footer = table.Column<string>(type: "varchar(2048)", maxLength: 200, nullable: true),
                    image_url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    footer_image_url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    author_image_url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    description = table.Column<string>(type: "varchar(4096)", nullable: true),
                    thumbnail = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    thumbnail_height = table.Column<int>(type: "integer", nullable: false),
                    thumbnail_width = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    creator_id = table.Column<long>(type: "bigint", nullable: false),
                    last_edit_by_id = table.Column<long>(type: "bigint", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp", nullable: true),
                    hex_color = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
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
                    guild_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    guild_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    user_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                name: "moderation_config",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    member_events_log_channel_id = table.Column<long>(type: "bigint", nullable: false),
                    message_deleted_events_log_channel_id = table.Column<long>(type: "bigint", nullable: false),
                    message_updated_events_log_channel_id = table.Column<long>(type: "bigint", nullable: false),
                    moderation_log_channel_id = table.Column<long>(type: "bigint", nullable: false),
                    mute_role_id = table.Column<long>(type: "bigint", nullable: false),
                    member_welcome_message = table.Column<string>(type: "text", nullable: true),
                    member_welcome_embed_config_id = table.Column<long>(type: "bigint", nullable: true),
                    guild_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_moderation_config", x => x.id);
                    table.ForeignKey(
                        name: "FK_moderation_config_embed_config_member_welcome_embed_config_~",
                        column: x => x.member_welcome_embed_config_id,
                        principalTable: "embed_config",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_moderation_config_guild_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guild",
                        principalColumn: "guild_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mute",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guild_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    user_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    cron_expression = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    hangfire_id = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true),
                    bigint = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    text = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    is_guild_reminder = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false),
                    embed_config_id = table.Column<long>(type: "bigint", nullable: true),
                    creator_id = table.Column<long>(type: "bigint", nullable: false),
                    lasted_edit_by_id = table.Column<long>(type: "bigint", nullable: false),
                    guild_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recurring_reminder", x => x.id);
                    table.ForeignKey(
                        name: "FK_recurring_reminder_embed_config_embed_config_id",
                        column: x => x.embed_config_id,
                        principalTable: "embed_config",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_recurring_reminder_guild_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guild",
                        principalColumn: "guild_id");
                });

            migrationBuilder.CreateTable(
                name: "reminder",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    set_for = table.Column<DateTime>(type: "timestamp", nullable: false),
                    text = table.Column<string>(type: "text", nullable: true),
                    hangfire_id = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true),
                    bigint = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    is_guild_reminder = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false),
                    embed_config_id = table.Column<long>(type: "bigint", nullable: true),
                    creator_id = table.Column<long>(type: "bigint", nullable: false),
                    lasted_edit_by_id = table.Column<long>(type: "bigint", nullable: false),
                    guild_id = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reminder", x => x.id);
                    table.ForeignKey(
                        name: "FK_reminder_embed_config_embed_config_id",
                        column: x => x.embed_config_id,
                        principalTable: "embed_config",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_reminder_guild_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guild",
                        principalColumn: "guild_id");
                });

            migrationBuilder.CreateTable(
                name: "role_menu",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    text = table.Column<string>(type: "text", nullable: true),
                    custom_select_component_id = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    custom_button_id = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false),
                    embed_config_id = table.Column<long>(type: "bigint", nullable: true),
                    creator_id = table.Column<long>(type: "bigint", nullable: false),
                    lasted_edit_by_id = table.Column<long>(type: "bigint", nullable: false),
                    guild_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_menu", x => x.id);
                    table.ForeignKey(
                        name: "FK_role_menu_embed_config_embed_config_id",
                        column: x => x.embed_config_id,
                        principalTable: "embed_config",
                        principalColumn: "id");
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
                    text = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false),
                    embed_config_id = table.Column<long>(type: "bigint", nullable: true),
                    creator_id = table.Column<long>(type: "bigint", nullable: false),
                    lasted_edit_by_id = table.Column<long>(type: "bigint", nullable: false),
                    guild_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tag", x => x.id);
                    table.ForeignKey(
                        name: "FK_tag_embed_config_embed_config_id",
                        column: x => x.embed_config_id,
                        principalTable: "embed_config",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_tag_guild_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guild",
                        principalColumn: "guild_id");
                });

            migrationBuilder.CreateTable(
                name: "ticket",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                name: "ticketing_config",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    log_channel_id = table.Column<long>(type: "bigint", nullable: false),
                    last_ticket_id = table.Column<long>(type: "bigint", nullable: false),
                    closed_category_id = table.Column<long>(type: "bigint", nullable: false),
                    opened_category_id = table.Column<long>(type: "bigint", nullable: false),
                    clean_after = table.Column<long>(type: "bigint", nullable: true),
                    close_after = table.Column<long>(type: "bigint", nullable: true),
                    opened_name_prefix = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    closed_name_prefix = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    welcome_message_description = table.Column<string>(type: "text", nullable: false),
                    welcome_embed_config_id = table.Column<long>(type: "bigint", nullable: true),
                    center_message_description = table.Column<string>(type: "text", nullable: false),
                    center_embed_config_id = table.Column<long>(type: "bigint", nullable: true),
                    guild_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ticketing_config", x => x.id);
                    table.ForeignKey(
                        name: "FK_ticketing_config_embed_config_center_embed_config_id",
                        column: x => x.center_embed_config_id,
                        principalTable: "embed_config",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ticketing_config_embed_config_welcome_embed_config_id",
                        column: x => x.welcome_embed_config_id,
                        principalTable: "embed_config",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ticketing_config_guild_guild_id",
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

            migrationBuilder.CreateTable(
                name: "role_menu_option",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_menu_id = table.Column<long>(type: "bigint", nullable: false),
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    emoji = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
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
                name: "IX_ban_guild_id",
                table: "ban",
                column: "guild_id");

            migrationBuilder.CreateIndex(
                name: "IX_guild_guild_id",
                table: "guild",
                column: "guild_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_guild_server_booster_user_id",
                table: "guild_server_booster",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_moderation_config_guild_id",
                table: "moderation_config",
                column: "guild_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_moderation_config_member_welcome_embed_config_id",
                table: "moderation_config",
                column: "member_welcome_embed_config_id",
                unique: true);

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
                name: "IX_role_menu_option_role_menu_id",
                table: "role_menu_option",
                column: "role_menu_id");

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
                name: "IX_ticket_guild_id",
                table: "ticket",
                column: "guild_id");

            migrationBuilder.CreateIndex(
                name: "IX_ticketing_config_center_embed_config_id",
                table: "ticketing_config",
                column: "center_embed_config_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ticketing_config_guild_id",
                table: "ticketing_config",
                column: "guild_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ticketing_config_welcome_embed_config_id",
                table: "ticketing_config",
                column: "welcome_embed_config_id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "ban");

            migrationBuilder.DropTable(
                name: "guild_server_booster");

            migrationBuilder.DropTable(
                name: "moderation_config");

            migrationBuilder.DropTable(
                name: "mute");

            migrationBuilder.DropTable(
                name: "prune");

            migrationBuilder.DropTable(
                name: "recurring_reminder");

            migrationBuilder.DropTable(
                name: "reminder");

            migrationBuilder.DropTable(
                name: "role_menu_option");

            migrationBuilder.DropTable(
                name: "tag");

            migrationBuilder.DropTable(
                name: "ticket");

            migrationBuilder.DropTable(
                name: "ticketing_config");

            migrationBuilder.DropTable(
                name: "server_booster");

            migrationBuilder.DropTable(
                name: "role_menu");

            migrationBuilder.DropTable(
                name: "embed_config");

            migrationBuilder.DropTable(
                name: "guild");
        }
    }
}
