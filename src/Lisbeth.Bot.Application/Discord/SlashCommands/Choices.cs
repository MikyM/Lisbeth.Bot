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

namespace Lisbeth.Bot.Application.Discord.SlashCommands;

public enum BanActionType
{
    [ChoiceName("ban")] Ban,
    [ChoiceName("unban")] Remove,
    [ChoiceName("get")] Get
}

public enum MuteActionType
{
    [ChoiceName("mute")] Mute,
    [ChoiceName("unmute")] Remove,
    [ChoiceName("get")] Get
}

public enum TicketCenterActionType
{
    [ChoiceName("get")] Get,
    [ChoiceName("configure-embed")] ConfigureEmbed,
    [ChoiceName("send")] Send
}

public enum PruneActionType
{
    [ChoiceName("prune")] Prune,
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
    [ChoiceName("set")] Set,
    [ChoiceName("reschedule")] Reschedule,
    [ChoiceName("configure-embed")] ConfigureEmbed,
    [ChoiceName("remove")] Disable
}

public enum ChannelMessageFormatActionType
{
    [ChoiceName("create")] Create,
    [ChoiceName("edit")] Edit,
    [ChoiceName("get")] Get,
    [ChoiceName("disable")] Disable,
    [ChoiceName("enable")] Enable,
    [ChoiceName("verify")] Verify
}

public enum TagActionType
{
    [ChoiceName("get")] Get,
    [ChoiceName("list")] List,
    [ChoiceName("send")] Send,
    [ChoiceName("create")] Create,
    [ChoiceName("add-permission-for")] AddPermissionFor,
    [ChoiceName("revoke-permission-for")] RevokePermissionFor,
    [ChoiceName("edit")] Edit,
    [ChoiceName("configure-embed")] ConfigureEmbed,
    [ChoiceName("disable")] Disable
}

public enum RoleMenuActionType
{
    [ChoiceName("get")] Get,
    [ChoiceName("send")] Send,
    [ChoiceName("create")] Create,
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