using DSharpPlus.Entities;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.ChatExport.Builders;
using Lisbeth.Bot.Application.Services.Interfaces;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Discord.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lisbeth.Bot.Application.Discord.ChatExport.Models;
using Lisbeth.Bot.Domain.DTOs.Request;
using MikyM.Common.DataAccessLayer.Specifications;

namespace Lisbeth.Bot.Application.Discord.ChatExport
{
    [UsedImplicitly]
    public class DiscordChatExportService : IDiscordChatExportService
    {
        private readonly IDiscordService _discord;
        private readonly IGuildService _guildService;
        private readonly ITicketService _ticketService;

        public DiscordChatExportService(IDiscordService discord, IGuildService guildService, ITicketService ticketService)
        {
            _discord = discord;
            _guildService = guildService;
            _ticketService = ticketService;
        }

        public async Task<DiscordEmbed> ExportToHtmlAsync(TicketExportReqDto req, DiscordUser triggerUser = null)
        {
            if (req is null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            var resGuild = await _guildService.GetBySpecificationsAsync<Guild>(new Specifications<Guild>(x => x.GuildId == req.GuildId && !x.IsDisabled));

            var guildCfg = resGuild.FirstOrDefault();

            if (guildCfg is null)
            {
                throw new ArgumentException("Guild doesn't exist in database.");
            }

            Ticket ticket;

            if (req.TicketId is not null)
            {
                var resTicket = await _ticketService.GetBySpecificationsAsync<Ticket>(new Specifications<Ticket>(x => x.Id == req.TicketId));
                ticket = resTicket.FirstOrDefault();
            }
            else
            {
                var resTicket = await _ticketService.GetBySpecificationsAsync<Ticket>(new Specifications<Ticket>(x => x.GuildId == req.GuildId && x.UserId == req.OwnerId));
                ticket = resTicket.FirstOrDefault();
            }

            DiscordUser owner;
            DiscordChannel ticketLogChannel;
            DiscordChannel channel;
            DiscordGuild guild;

            if (ticket is null)
            {
                throw new ArgumentException("Ticket doesn't exist in database.");
            }

            if (guildCfg.TicketLogChannelId is null)
            {
                throw new ArgumentException("Guild doesn't have ticketing log channel set.");
            }
            try
            {
                guild = await _discord.Client.GetGuildAsync(guildCfg.GuildId);
            }
            catch (Exception)
            {
                throw new ArgumentException($"Guild with Id: {guildCfg.GuildId} doesn't exist.");
            }

            try
            {
                ticketLogChannel = await _discord.Client.GetChannelAsync(guildCfg.TicketLogChannelId.Value);
            }
            catch (Exception)
            {
                throw new ArgumentException($"Channel with Id: {guildCfg.TicketLogChannelId} doesn't exist.");
            }

            try
            {
                owner = await _discord.Client.GetUserAsync(req.OwnerId);
            }
            catch (Exception)
            {
                throw new ArgumentException($"User with Id: {req.OwnerId} doesn't exist.");
            }

            try
            {
                channel = await _discord.Client.GetChannelAsync(req.ChannelId);
            }
            catch (Exception)
            {
                throw new ArgumentException($"Channel with Id: {req.ChannelId} doesn't exist.");
            }

            await Task.Delay(500);
            List<DiscordUser> users = new();
            List<DiscordMessage> messages = new();

            messages.AddRange(await channel.GetMessagesAsync());

            var embed = new DiscordEmbedBuilder();

            while (true)
            {
                await Task.Delay(1000);
                var newMessages = await channel.GetMessagesBeforeAsync(messages.Last().Id);
                if (newMessages.Count == 0)
                {
                    break;
                }
                messages.AddRange(newMessages);
            }

            messages.Reverse();

            foreach (var msg in messages.Where(msg => !users.Contains(msg.Author)))
            {
                users.Add(msg.Author);
            }

            string path = Path.Combine(Directory.GetCurrentDirectory(), "ChatExport", "ChatExport.css");
            string css;
            if (File.Exists(path))
            {
                using StreamReader streamReader = new StreamReader(path, Encoding.UTF8);
                css = await streamReader.ReadToEndAsync();
            }
            else
            {
                throw new Exception($"CSS file was not found at {path}");
            }

            path = Path.Combine(Directory.GetCurrentDirectory(), "ChatExport", "ChatExport.js");
            string js;
            if (File.Exists(path))
            {
                using StreamReader streamReader = new StreamReader(path, Encoding.UTF8);
                js = await streamReader.ReadToEndAsync();
            }
            else
            {
                throw new Exception($"JS file was not found at {path}");
            }

            var htmlChatBuilder = new HtmlChatBuilder();
            htmlChatBuilder.WithChannel(channel).WithUsers(users).WithMessages(messages).WithCss(css).WithJs(js);
            string html = await htmlChatBuilder.BuildAsync();

            var parser = new MarkdownParser(html, users, guild, _discord);
            html = await parser.GetParsedContentAsync();


            string usersString = users.Aggregate("", (current, user) => current + $"{user.Mention}\n");

            var embedBuilder = new DiscordEmbedBuilder();

            embedBuilder.WithFooter(triggerUser is null
                ? "This transcript has been automatically saved"
                : $"This transcript was saved by {triggerUser.Username}#{triggerUser.Discriminator}");

            embedBuilder.WithAuthor($"Transcript | {owner.Username}#{owner.Discriminator}", null, owner.AvatarUrl);
            embedBuilder.AddField("Ticket Owner", owner.Mention, true);
            embedBuilder.AddField("Ticket Name", $"ticket-{Regex.Replace(channel.Name, @"[^\d]", "")}", true);
            embedBuilder.AddField("Channel", channel.Mention, true);
            embedBuilder.AddField("Users in transcript", usersString);
            embedBuilder.WithColor(new DiscordColor(0x18315C));

            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(html));
            DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder();

            messageBuilder.WithFile($"transcript-{channel.Name}.html", ms);
            messageBuilder.WithEmbed(embedBuilder.Build());

            Log.Logger.Information(triggerUser is null
                ? $"Automatically saved transcript of {channel.Name}"
                : $"User {triggerUser.Username}#{triggerUser.Discriminator} with ID: {triggerUser.Id} saved transcript of {channel.Name}");

            await ticketLogChannel.SendMessageAsync(messageBuilder);

            return embed;
        }
    }
}
