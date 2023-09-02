using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Lisbeth.Bot.Application.Discord.ChatExport;

public class MarkdownParser
{
    private readonly IDiscordService _discord;

    public MarkdownParser(string html, List<DiscordUser> users, DiscordGuild guild, IDiscordService discord)
    {
        Users ??= users ?? throw new ArgumentNullException(nameof(users));
        Guild ??= guild ?? throw new ArgumentNullException(nameof(guild));
        DiscordHtml ??= html ?? throw new ArgumentNullException(nameof(html));
        _discord = discord;
    }

    public string DiscordHtml { get; }
    private List<DiscordUser> Users { get; }
    private DiscordGuild Guild { get; }

    public async Task<string> GetParsedContentAsync()
    {
        var result = DiscordHtml;
        result = await ParseMentionsAsync(result);
        result = ParseCustomEmojis(result);
        result = ParseNewLines(result);
        result = ParseDiscordTextMarkdown(result);

        return result;
    }

    private async Task<string> ParseMentionsAsync(string result)
    {
        result = await ParseUsersMentionsAsync(result);
        result = ParseRoleMentions(result);
        result = ParseChannelMentions(result);

        return result;
    }

    private async Task<string> ParseUsersMentionsAsync(string result)
    {
        //replace user mentions
        var userMatches = Regex.Matches(result, @"(?<=\<@!|\<@)[0-9]{17,18}(?=\>)");
        foreach (var userMatch in userMatches)
        {
            var user = Users.FirstOrDefault(x => x.Id == ulong.Parse(userMatch.ToString() ?? "0"));
            if (user is not null)
                result = result.Replace($"<@!{user.Id}>", $"<span class=\"user-mention\">@{user.Username}</span>");
            else
                try
                {
                    user = await _discord.Client.GetUserAsync(ulong.Parse(userMatch.ToString() ?? "0"));
                    result = result.Replace($"<@!{user.Id}>",
                        $"<span class=\"user-mention\">@{user.Username}</span>");
                }
                catch
                {
                    result = result.Replace($"<@!{userMatch}>", "<span class=\"user-mention\">@DeletedUser</span>");
                }
        }

        return result;
    }

    private string ParseRoleMentions(string result)
    {
        //replace role mentions
        var roleMatches = Regex.Matches(result, @"(?<=\<@&)[0-9]{17,18}(?=\>)");
        foreach (var roleMatch in roleMatches)
        {
            var role = Guild.Roles
                .FirstOrDefault(x => x.Value.Id == ulong.Parse(roleMatch.ToString() ?? "0"))
                .Value;
            if (role is not null)
                result = result.Replace($"<@!{role.Id}>", $"<span class=\"user-mention\">@{role.Name}</span>");
            else
                result = result.Replace($"<@&{roleMatch}>", "<span class=\"role-mention\">@DeletedRole</span>");
        }

        //replace @everyone and @here with spans for css
        result = result.Replace("@everyone", "<span class=\"mention\">@everyone</span>");
        result = result.Replace("@here", "<span class=\"mention\">@here</span>");

        return result;
    }

    private string ParseChannelMentions(string result)
    {
        //replace channel mentions
        var channelMatches = Regex.Matches(result, @"(?<=\<#)[0-9]{17,18}(?=\>)");
        foreach (var channelMatch in channelMatches)
            try
            {
                var channel = Guild.GetChannel(ulong.Parse(channelMatch.ToString() ?? "0"));
                result = result.Replace($"<#{channel.Id}>",
                    $"<span class=\"channel-mention\">#{channel.Name}</span>");
            }
            catch
            {
                result = result.Replace($"<#{channelMatch}>",
                    "<span class=\"channel-mention\">#DeletedChannel</span>");
            }

        return result;
    }

    private string ParseCustomEmojis(string result)
    {
        //replace custom emojis with grabbed image from discord based on id
        result = Regex.Replace(result, @"(<a?:).+?(>)", x =>
        {
            var id = Regex.Match(x.Value.Split(':').Last(), @"\d+").Value;
            if (x.Value.Split(':').First().Replace("<", "") == "a")
                //if animated
                return $"<img class=\"emoji\" src=\"https://cdn.discordapp.com/emojis/{id}.gif?v=1\">";
            return $"<img class=\"emoji\" src=\"https://cdn.discordapp.com/emojis/{id}.png?v=1\">";
        });

        return result;
    }

    private string ParseNewLines(string result)
    {
        //fix new lines
        return result.Replace("\n", "<br/>");
    }

    private string ParseDiscordTextMarkdown(string result)
    {
        //text formatting
        var boldItalicMatches = Regex.Matches(result, @"(?<=\*\*\*).+?(?=\*\*\*)");
        foreach (var boldItalicMatch in boldItalicMatches)
            result = result.Replace($"***{boldItalicMatch}***",
                $"<span style=\"font-style: italic; font-weight: bold;\">{boldItalicMatch}</span>");
        var boldMatches = Regex.Matches(result, @"(?<=\*\*).+?(?=\*\*)");
        foreach (var boldMatch in boldMatches)
            result = result.Replace($"**{boldMatch}**", $"<span style=\"font-weight: bold;\">{boldMatch}</span>");
        var underscoreMatches = Regex.Matches(result, @"(?<=__).+?(?=__)");
        foreach (var underscoreMatch in underscoreMatches)
            result = result.Replace($"__{underscoreMatch}__",
                $"<span style=\"text-decoration: underline;\">{underscoreMatch}</span>");
        var italicMatches = Regex.Matches(result, @"(?<=\*).+?(?=\*)|(?<=_).+?(?=_)");
        foreach (var italicMatch in italicMatches)
        {
            result = result.Replace($"*{italicMatch}*",
                $"<span style=\"font-style: italic;\">{italicMatch}</span>");
            result = result.Replace($"_{italicMatch}_",
                $"<span style=\"font-style: italic;\">{italicMatch}</span>");
        }

        var codeMatches = Regex.Matches(result, @"(?<=```).+?(?=```)");
        foreach (var codeMatch in codeMatches)
            result = result.Replace($"```{codeMatch}```", $"<code>{codeMatch}</code>");
        codeMatches = Regex.Matches(result, @"(?<=``).+?(?=``)");
        foreach (var codeMatch in codeMatches)
            result = result.Replace($"``{codeMatch}``", $"<code>{codeMatch}</code>");
        codeMatches = Regex.Matches(result, @"(?<=`).+?(?=`)");
        foreach (var codeMatch in codeMatches)
            result = result.Replace($"`{codeMatch}`", $"<code>{codeMatch}</code>");

        return result;
    }
}
