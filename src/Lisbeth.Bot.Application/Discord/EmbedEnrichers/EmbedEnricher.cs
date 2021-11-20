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

namespace Lisbeth.Bot.Application.Discord.EmbedEnrichers;

public abstract class EmbedEnricher<TEntity> : MikyM.Discord.EmbedBuilders.Enrichers.EmbedEnricher<TEntity> where TEntity : class
{
    protected string HexColor { get; }

    protected EmbedEnricher(TEntity entity, long? caseId = null, string hexColor = "#26296e") : base(entity, caseId)
    {
        this.HexColor = hexColor;
    }

    protected (string Name, string PastTense) GetUnderlyingNameAndPastTense(object req)
    {
        string type = req.GetType().Name;
        if (type.Contains("Ban")) return ("Ban", "Banned");
        if (type.Contains("Mute")) return ("Mute", "Muted");

        throw new NotSupportedException();
    }
}