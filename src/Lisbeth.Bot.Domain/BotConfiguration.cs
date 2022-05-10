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

using System;

// ReSharper disable ConstantNullCoalescingCondition

namespace Lisbeth.Bot.Domain;

public class BotConfiguration
{
    public string? VimeoApiKey { get; private set; }
    public string? ImgurApiKey { get; private set; }
    public string? LisbethBotToken { get; private set; }
    public string? EmbedHexColor { get; private set; }
    public ulong TestGuildId { get; private set; }
    public bool GlobalRegister { get; private set; }
    public string[] Shorteners { get; private set; } = Array.Empty<string>();
    public string ChatExportCss { get; private set; } = string.Empty;
    public string ChatExportJs { get; private set; } = string.Empty;

    public void SetShorteners(string[] shorteners)
        => Shorteners = shorteners;
    
    public void SetChatExportCss(string css)
        => ChatExportCss = css;
    
    public void SetChatExportJs(string js)
        => ChatExportJs = js;
}
