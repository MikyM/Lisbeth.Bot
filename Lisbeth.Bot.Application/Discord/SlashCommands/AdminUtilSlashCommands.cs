using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using JetBrains.Annotations;
using Lisbeth.Bot.DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using MikyM.Common.Application.Interfaces;
using MikyM.Common.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Discord.SlashCommands
{
    [SlashCommandGroup("admin", "Admin util commands")]
    [SlashModuleLifespan(SlashModuleLifespan.Transient)]
    [UsedImplicitly]
    public class AdminUtilSlashCommands : ApplicationCommandModule
    {
        [UsedImplicitly]
        public LisbethBotDbContext _ctx { private get; set; }
        [UsedImplicitly]
        public IReadOnlyService<Audit, LisbethBotDbContext> _service { private get; set; }

        [SlashRequireOwner]
        [SlashCommand("audit", "Gets last 10 audit logs.")]
        public async Task AuditCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var res = await _service.GetBySpecificationsAsync<Audit>();
            string botRes = res.Aggregate("", (current, resp) => current + $"\n Affected columns: {resp.AffectedColumns}, Table: {resp.TableName}, Old: {resp.OldValues}, New: {resp.NewValues}, Type: {resp.Type}");

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(botRes));
        }

        [SlashRequireOwner]
        [SlashCommand("sql", "A command that runs sql query.")]
        public async Task MuteCommand(InteractionContext ctx,
            [Option("query", "Sql query to be executed.")] string query)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var dat = new List<Dictionary<string, string>>();
            var i = 0;

            using var cmd = _ctx.Database.GetDbConnection().CreateCommand();
            await _ctx.Database.OpenConnectionAsync();

            cmd.CommandText = query;
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                var dict = new Dictionary<string, string>();
                for (i = 0; i < rdr.FieldCount; i++)
                    dict[rdr.GetName(i)] = rdr[i] is DBNull ? "<null>" : rdr[i].ToString();
                dat.Add(dict);
            }

            DiscordEmbedBuilder embed = null;
            if (!dat.Any() || !dat.First().Any())
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "Given query produced no results.",
                    Description = string.Concat("Query: ", Formatter.InlineCode(query), "."),
                    Color = new DiscordColor(0x007FFF)
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
                return;
            }

            var d0 = dat.First().Select(xd => xd.Key).OrderByDescending(xs => xs.Length).First().Length + 1;

            embed = new DiscordEmbedBuilder
            {
                Title = string.Concat("Results: ", dat.Count.ToString("#,##0")),
                Description = string.Concat("Showing ", dat.Count > 24 ? "first 24" : "all", " results for query ", Formatter.InlineCode(query), ":"),
                Color = new DiscordColor(0x18315C)
            };
            var adat = dat.Take(24);

            i = 0;
            foreach (var xdat in adat)
            {
                var sb = new StringBuilder();

                foreach (var (k, v) in xdat)
                    sb.Append(k).Append(new string(' ', d0 - k.Length)).Append("| ").AppendLine(v);

                embed.AddField(string.Concat("Result #", i++), Formatter.BlockCode(sb.ToString()), false);
            }

            if (dat.Count > 24)
                embed.AddField("Display incomplete", string.Concat((dat.Count - 24).ToString("#,##0"), " results were omitted."), false);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
        }
    }
}
