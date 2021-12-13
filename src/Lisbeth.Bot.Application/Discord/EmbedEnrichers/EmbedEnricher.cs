// This file is part of Lisbeth.Bot project
//
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

using MikyM.Discord.EmbedBuilders.Enrichers;
using MikyM.Discord.EmbedBuilders.Enums;

namespace Lisbeth.Bot.Application.Discord.EmbedEnrichers;

public abstract class EmbedEnricher<TPrimaryEnricher> : EmbedEnricherBase<TPrimaryEnricher> where TPrimaryEnricher : class
{
    protected EmbedEnricher(TPrimaryEnricher entity) : base(entity){}

    protected (string Name, string PastTense) GetUnderlyingNameAndPastTense()
    {
        var partial = this.GetModerationTypeAndPastTense();
        return (partial.Moderation.ToString(), partial.PastTense);
    }

    protected (DiscordModeration Moderation, string PastTense) GetModerationTypeAndPastTense()
    {
        string type = this.PrimaryEnricher.GetType().Name;
        if (type.Contains("ban", StringComparison.InvariantCultureIgnoreCase) && type.Contains("revoke", StringComparison.InvariantCultureIgnoreCase)) return (DiscordModeration.Unban, "Unbanned");
        if (type.Contains("mute", StringComparison.InvariantCultureIgnoreCase) && type.Contains("revoke", StringComparison.InvariantCultureIgnoreCase)) return (DiscordModeration.Unmute, "Unmuted");
        if (type.Contains("ban", StringComparison.InvariantCultureIgnoreCase)) return (DiscordModeration.Ban, "Banned");
        if (type.Contains("mute", StringComparison.InvariantCultureIgnoreCase)) return (DiscordModeration.Mute, "Muted");
        if (type.Contains("prune", StringComparison.InvariantCultureIgnoreCase)) return (DiscordModeration.Prune, "Pruned");
        if (type.Contains("identity", StringComparison.InvariantCultureIgnoreCase)) return (DiscordModeration.Id, "Checked identity of");

        throw new NotSupportedException();
    }
}
