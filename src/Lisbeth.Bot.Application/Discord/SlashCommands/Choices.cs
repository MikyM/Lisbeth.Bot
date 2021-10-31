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

using DSharpPlus.SlashCommands;

namespace Lisbeth.Bot.Application.Discord.SlashCommands
{
    public enum BanActionType
    {
        [ChoiceName("add")] Add,
        [ChoiceName("remove")] Remove,
        [ChoiceName("get")] Get
    }

    public enum MuteActionType
    {
        [ChoiceName("add")] Add,
        [ChoiceName("remove")] Remove,
        [ChoiceName("get")] Get
    }

    public enum PruneActionType
    {
        [ChoiceName("add")] Add,
        [ChoiceName("remove")] Remove,
        [ChoiceName("get")] Get
    }

    public enum TicketActionType
    {
        [ChoiceName("add")] Add,
        [ChoiceName("remove")] Remove
    }

    public enum ReminderActionType
    {
        [ChoiceName("add")] Add,
        [ChoiceName("reschedule")] Reschedule,
        [ChoiceName("remove")] Remove
    }

    public enum ReminderType
    {
        Single,
        Recurring
    }

    public enum TagActionType
    {
        [ChoiceName("get")] Get,
        [ChoiceName("add")] Add,
        [ChoiceName("edit")] Edit,
        [ChoiceName("configure-embed")] ConfigureEmbed,
        [ChoiceName("remove")] Remove
    }

    public enum ModuleActionType
    {
        [ChoiceName("enable")] Enable,
        [ChoiceName("repair")] Repair,
        [ChoiceName("edit")] Edit,
        [ChoiceName("disable")] Disable
    }
}