// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2018-2021 Emzi0767
// Copyright (C) 2021 Krzysztof Kupisz - MikyM
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

using Autofac;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Lisbeth.Bot.Application.Discord.ChatExport;
using Lisbeth.Bot.DataAccessLayer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using MikyM.Common.Domain.Entities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using Lisbeth.Bot.Application.Discord.Exceptions;
using Lisbeth.Bot.Application.Discord.SlashCommands.Base;
using Lisbeth.Bot.Domain;
using Microsoft.Extensions.Options;
using MikyM.Discord.Extensions.BaseExtensions;

namespace Lisbeth.Bot.Application.Discord.SlashCommands;

[SlashCommandGroup("owner", "Owner util commands", false)]
[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
[UsedImplicitly]
public class OwnerUtilSlashCommands : ExtendedApplicationCommandModule
{
    private readonly LisbethBotDbContext _ctx;
    private readonly IReadOnlyDataService<AuditLog, LisbethBotDbContext> _dataService;
    private readonly IDiscordGuildService _discordGuildService;
    private readonly IOptions<BotOptions> _options;

    public OwnerUtilSlashCommands(LisbethBotDbContext ctx, IReadOnlyDataService<AuditLog, LisbethBotDbContext> dataService,
        IDiscordGuildService discordGuildService, IOptions<BotOptions> options)
    {
        _ctx = ctx;
        _dataService = dataService;
        _discordGuildService = discordGuildService;
        _options = options;
    }

    [UsedImplicitly]
    [SlashRequireOwner]
    [SlashCommand("re-register-commands", "A command that allows re-registering bot slashies", false)]
    public async Task RegisterSlashiesCommand(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral(true));

        var res = await _discordGuildService.PrepareSlashPermissionsAsync(ctx.Client.Guilds.Values);

        if (!res.IsSuccess)
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().AddEmbed(GetUnsuccessfulResultEmbed(res, ctx.Client)));
        else
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(GetSuccessfulActionEmbed(ctx.Client)));
    }

    [UsedImplicitly]
    [SlashRequireOwner]
    [SlashCommand("audit", "Gets last 10 audit logs.", false)]
    public async Task AuditCommand(InteractionContext ctx,
        [Option("ephemeral", "Whether response should be eph")] string shouldEph = "true")
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral(bool.Parse(shouldEph)));

        if (!ctx.User.IsBotOwner(ctx.Client))
            throw new DiscordNotAuthorizedException();

        var res = await this._dataService!.GetAllAsync<AuditLog>();

        if (!res.IsDefined()) throw new InvalidOperationException();

        string botRes = res.Entity.Aggregate("",
            (current, resp) =>
                current +
                $"\n Affected columns: {resp.AffectedColumns}, Table: {resp.TableName}, Old: {resp.OldValues}, New: {resp.NewValues}, Type: {resp.Type}");

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(botRes));
    }

    [UsedImplicitly]
    [SlashRequireOwner]
    [SlashCommand("sql", "A command that runs sql query.", false)]
    public async Task SqlCommand(InteractionContext ctx,
        [Option("query", "Sql query to be executed.")] string query,
        [Option("ephemeral", "Whether response should be eph")] string shouldEph = "true")
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral(bool.Parse(shouldEph)));

        if (!ctx.User.IsBotOwner(ctx.Client))
            throw new DiscordNotAuthorizedException();

        var dat = new List<Dictionary<string, string?>>();
        int i;

        using var cmd = this._ctx!.Database.GetDbConnection().CreateCommand();
        await this._ctx.Database.OpenConnectionAsync();

        cmd.CommandText = query;
        using var rdr = await cmd.ExecuteReaderAsync();
        while (await rdr.ReadAsync())
        {
            var dict = new Dictionary<string, string?>();
            for (i = 0; i < rdr.FieldCount; i++)
                dict[rdr.GetName(i)] = rdr[i] is DBNull ? "<null>" : rdr[i].ToString();
            dat.Add(dict);
        }

        DiscordEmbedBuilder embed;
        if (!dat.Any() || !dat.First().Any())
        {
            embed = new DiscordEmbedBuilder
            {
                Title = "Given query produced no results.",
                Description = string.Concat("Query: ", Formatter.InlineCode(query), "."),
                Color = new DiscordColor(_options.Value.EmbedHexColor)
            };
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
            return;
        }

        var d0 = dat.First().Select(xd => xd.Key).OrderByDescending(xs => xs.Length).First().Length + 1;

        embed = new DiscordEmbedBuilder
        {
            Title = string.Concat("Results: ", dat.Count.ToString("#,##0")),
            Description = string.Concat("Showing ", dat.Count > 24 ? "first 24" : "all", " results for query ",
                Formatter.InlineCode(query), ":"),
            Color = new DiscordColor(_options.Value.EmbedHexColor)
        };
        var adat = dat.Take(24);

        i = 0;
        foreach (var xdat in adat)
        {
            var sb = new StringBuilder();

            foreach (var (k, v) in xdat)
                sb.Append(k).Append(new string(' ', d0 - k.Length)).Append("| ").AppendLine(v);

            embed.AddField(string.Concat("Result #", i++), Formatter.BlockCode(sb.ToString()));
        }

        if (dat.Count > 24)
            embed.AddField("Display incomplete",
                string.Concat((dat.Count - 24).ToString("#,##0"), " results were omitted."));

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
    }

    [UsedImplicitly]
    [SlashRequireOwner]
    [SlashCommand("eval", "Evaluate a piece of C# code.", false)]
    public async Task EvalCommand(InteractionContext ctx, [Option("code", "Code to evaluate.")] string code,
        [Option("ephemeral", "Whether response should be eph")]
        string shouldEph = "true")
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral(bool.Parse(shouldEph)));

        if (!ctx.User.IsBotOwner(ctx.Client))
            throw new DiscordNotAuthorizedException();

        var cs1 = code.IndexOf("```", StringComparison.Ordinal) + 3;
        var cs2 = code.LastIndexOf("```", StringComparison.Ordinal);

        if (cs1 == -1 || cs2 == -1)
            throw new ArgumentException("You need to wrap the code into a code block.", nameof(code));

        code = code[cs1..cs2];

        var embed = new DiscordEmbedBuilder { Title = "Evaluating...", Color = new DiscordColor(_options.Value.EmbedHexColor) };
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));

        var globals = new EvaluationEnvironment(ctx);
        var sopts = ScriptOptions.Default
            .WithImports("System", "System.Collections.Generic", "System.Diagnostics", "System.Linq",
                "System.Net.Http", "System.Net.Http.Headers", "System.Reflection", "System.Text",
                "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.CommandsNext", "DSharpPlus.Entities",
                "DSharpPlus.EventArgs", "DSharpPlus.Exceptions", "Lisbeth.Bot.Application.Services",
                "Lisbeth.Bot.Application.Discord.Services", "Lisbeth.Bot.Application.Extensions")
            .WithReferences(AppDomain.CurrentDomain.GetAssemblies()
                .Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

        var sw1 = Stopwatch.StartNew();
        var cs = CSharpScript.Create(code, sopts, typeof(EvaluationEnvironment));
        var csc = cs.Compile();
        sw1.Stop();

        if (csc.Any(xd => xd.Severity == DiagnosticSeverity.Error))
        {
            embed = new DiscordEmbedBuilder
            {
                Title = "Compilation failed",
                Description =
                    string.Concat("Compilation failed after ", sw1.ElapsedMilliseconds.ToString("#,##0"),
                        "ms with ", csc.Length.ToString("#,##0"), " errors."),
                Color = new DiscordColor(0xD091B2)
            };
            foreach (var xd in csc.Take(3))
            {
                var ls = xd.Location.GetLineSpan();
                embed.AddField(
                    string.Concat("Error at ", ls.StartLinePosition.Line.ToString("#,##0"), ", ",
                        ls.StartLinePosition.Character.ToString("#,##0")), Formatter.InlineCode(xd.GetMessage()));
            }

            if (csc.Length > 3)
                embed.AddField("Some errors ommitted",
                    string.Concat((csc.Length - 3).ToString("#,##0"), " more errors not displayed"));

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
            return;
        }

        Exception rex;
        ScriptState<object>? css = null;
        var sw2 = Stopwatch.StartNew();
        try
        {
            css = await cs.RunAsync(globals);
            rex = css.Exception;
        }
        catch (Exception ex)
        {
            rex = ex;
        }

        sw2.Stop();

        if (rex is not null)
        {
            embed = new DiscordEmbedBuilder
            {
                Title = "Execution failed",
                Description =
                    string.Concat("Execution failed after ", sw2.ElapsedMilliseconds.ToString("#,##0"),
                        "ms with `", rex.GetType(), ": ", rex.Message, "`."),
                Color = new DiscordColor(0xD091B2)
            };
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
            return;
        }

        // execution succeeded
        embed = new DiscordEmbedBuilder { Title = "Evaluation successful", Color = new DiscordColor(_options.Value.EmbedHexColor) };
        embed.WithDescription(code);

        embed.AddField("Result", css?.ReturnValue is not null ? css.ReturnValue.ToString() : "No value returned")
            .AddField("Compilation time", string.Concat(sw1.ElapsedMilliseconds.ToString("#,##0"), "ms"), true)
            .AddField("Execution time", string.Concat(sw2.ElapsedMilliseconds.ToString("#,##0"), "ms"), true);

        if (css?.ReturnValue is not null) embed.AddField("Return type", css.ReturnValue.GetType().ToString(), true);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
    }
}

public sealed class EvaluationEnvironment
{
    public EvaluationEnvironment(InteractionContext ctx)
    {
        Context = ctx;
    }

    public InteractionContext Context { get; }

    public DiscordInteraction Interaction => Context.Interaction;
    public DiscordChannel Channel => Context.Channel;
    public DiscordGuild Guild => Context.Guild;
    public DiscordUser User => Context.User;
    public DiscordMember Member => Context.Member;
    public DiscordClient Client => Context.Client;
    public static HttpClient Http => ChatExportHttpClientFactory.Build();
}
