using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Builders
{
    public interface IHtmlChatBuilder
    {
        Task<string> BuildAsync();
        IHtmlChatBuilder WithCss(string css);
        IHtmlChatBuilder WithJs(string js);
        IHtmlChatBuilder WithChannel(DiscordChannel channel);
        IHtmlChatBuilder WithMessages(List<DiscordMessage> messages);
        IHtmlChatBuilder WithUsers(List<DiscordUser> users);
    }
}
