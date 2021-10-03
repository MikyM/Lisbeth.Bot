using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Discord.ChatExport.Builders
{
    public interface IAsyncHtmlBuilder
    {
        Task<string> BuildAsync();
    }
}
