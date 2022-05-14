// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021-2022 Krzysztof Kupisz - MikyM
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using MikyM.Common.EfCore.DataAccessLayer;

namespace Lisbeth.Bot.DataAccessLayer;

public sealed class LisbethBotDbContext : AuditableDbContext
{
    public LisbethBotDbContext(DbContextOptions<LisbethBotDbContext> options) : base(options)
    {
    }

    public DbSet<Guild> Guilds { get; set; } = null!;
    public DbSet<Mute> Mutes { get; set; } = null!;
    public DbSet<Ticket> Tickets { get; set; } = null!;
    public DbSet<Ban> Bans { get; set; } = null!;
    public DbSet<ServerBooster> ServerBoosters { get; set; } = null!;
    public DbSet<Prune> Prunes { get; set; } = null!;
    public DbSet<TicketingConfig> TicketingConfigs { get; set; } = null!;
    public DbSet<ModerationConfig> ModerationConfigs { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<Reminder> Reminders { get; set; } = null!;
    public DbSet<RoleMenu> RoleMenus { get; set; } = null!;
    public DbSet<EmbedConfig> EmbedConfigs { get; set; } = null!;
    public DbSet<ChannelMessageFormat> ChannelMessageFormats { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LisbethBotDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}