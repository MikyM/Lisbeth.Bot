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

using System.Diagnostics.CodeAnalysis;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Domain.DTOs.Request.Base;

namespace Lisbeth.Bot.Application.Discord.Services;

[Service]
[RegisterAs(typeof(IDiscordGuildRequestDataProvider))]
[Lifetime(Lifetime.InstancePerDependency)]
[UsedImplicitly]
public class DiscordGuildRequestDataProvider : IDiscordGuildRequestDataProvider
{
    private DiscordGuild? _discordGuild;
    private DiscordMember? _requestingMember;

    public InteractionContext? InteractionContext { get; set; }
    public DiscordInteraction? DiscordInteraction { get; set; }
    public IBaseModAuthReq? BaseDto { get; set; }

    [MemberNotNullWhen(true, nameof(_discordGuild), nameof(_requestingMember))]
    public bool IsInitialized
        => _discordGuild is not null && _requestingMember is not null;
    public DiscordGuild DiscordGuild
        => _discordGuild ?? InteractionContext?.Guild ?? DiscordInteraction?.Guild ?? throw new InvalidOperationException();

    public DiscordMember RequestingMember
        => _requestingMember ?? InteractionContext?.Member ?? DiscordInteraction?.User as DiscordMember ?? throw new InvalidOperationException();


    private readonly IDiscordService _discord;

    public DiscordGuildRequestDataProvider(IDiscordService discord)
    {
        _discord = discord;
    }

    public async Task<Result> InitializeAsync(IBaseModAuthReq baseDto)
        => await Initialize(baseDto, null, null);

    public async Task<Result> InitializeAsync(IBaseModAuthReq baseDto, InteractionContext? ctx)
        => await Initialize(baseDto, null, ctx);

    public async Task<Result> InitializeAsync(IBaseModAuthReq baseDto, DiscordInteraction? interaction)
        => await Initialize(baseDto, interaction, null);

    private async Task<Result> Initialize(IBaseModAuthReq baseDto, DiscordInteraction? interaction, InteractionContext? ctx)
    {
        BaseDto = baseDto ?? throw new ArgumentNullException(nameof(baseDto));
        InteractionContext = ctx;
        DiscordInteraction = interaction;

        try
        {
            _discordGuild = DiscordInteraction?.Guild ?? InteractionContext?.Guild;

            if (_discordGuild is null && !_discord.Client.Guilds.TryGetValue(baseDto.GuildId, out _discordGuild))
                return new DiscordNotFoundError(DiscordEntity.Guild);

            _requestingMember = InteractionContext?.Member ?? DiscordInteraction?.User as DiscordMember;

            if (_requestingMember is null)
            {
                _requestingMember = await _discordGuild.GetMemberAsync(baseDto.RequestedOnBehalfOfId);

                if (_requestingMember is null)
                    return new DiscordNotFoundError(DiscordEntity.Member);
            }

        }
        catch (Exception ex)
        {
            return GetErrorFromDiscordException(ex);
        }
        
        return Result.FromSuccess();
    }


    public async Task<Result<DiscordMember>> GetMemberAsync(ulong userId)
    {
        if (!IsInitialized)
            throw new InvalidOperationException();

        DiscordMember? member;
        try
        {
            member = await DiscordGuild.GetMemberAsync(userId);

            if (member is null)
                return new DiscordNotFoundError(DiscordEntity.Member);
        }
        catch (Exception ex)
        {
            return GetErrorFromDiscordException(ex, DiscordEntity.Member);
        }

        return member;
    }

    public async Task<Result<DiscordMember>> GetFirstResolvedMemberOrAsync(ulong userId)
    {
        if (!IsInitialized)
            throw new InvalidOperationException();

        DiscordMember? member;
        try
        {
            member = InteractionContext?.ResolvedUserMentions?.ElementAtOrDefault(0) as DiscordMember ??
                     await DiscordGuild.GetMemberAsync(userId);

            if (member is null)
                return new DiscordNotFoundError(DiscordEntity.Member);
        }
        catch (Exception ex)
        {
            return GetErrorFromDiscordException(ex, DiscordEntity.Member);
        }

        return member;
    }

    public Task<Result<DiscordRole>> GetRoleAsync(ulong roleId)
    {
        if (!IsInitialized)
            throw new InvalidOperationException();

        DiscordRole? role;
        try
        {
            role = DiscordGuild.GetRole(roleId);

            if (role is null)
                return Task.FromResult<Result<DiscordRole>>(new DiscordNotFoundError(DiscordEntity.Role));
        }
        catch (Exception ex)
        {
            return Task.FromResult<Result<DiscordRole>>(GetErrorFromDiscordException(ex, DiscordEntity.Role));
        }

        return Task.FromResult<Result<DiscordRole>>(role);
    }

    public Task<Result<DiscordRole>> GetFirstResolvedRoleOrAsync(ulong roleId)
    {
        if (!IsInitialized)
            throw new InvalidOperationException();

        DiscordRole? role;
        try
        {
            role = InteractionContext?.ResolvedRoleMentions?.ElementAtOrDefault(0) ??
                   DiscordGuild.GetRole(roleId);

            if (role is null)
                return Task.FromResult<Result<DiscordRole>>(new DiscordNotFoundError(DiscordEntity.Role));
        }
        catch (Exception ex)
        {
            return Task.FromResult<Result<DiscordRole>>(GetErrorFromDiscordException(ex, DiscordEntity.Role));
        }

        return Task.FromResult<Result<DiscordRole>>(role);
    }

    public Task<Result<DiscordChannel>> GetChannelAsync(ulong channelId)
    {
        if (!IsInitialized)
            throw new InvalidOperationException();

        DiscordChannel? channel;
        try
        {
            channel = DiscordGuild.GetChannel(channelId);

            if (channel is null)
                return Task.FromResult<Result<DiscordChannel>>(new DiscordNotFoundError(DiscordEntity.Role));
        }
        catch (Exception ex)
        {
            return Task.FromResult<Result<DiscordChannel>>(GetErrorFromDiscordException(ex, DiscordEntity.Role));
        }

        return Task.FromResult<Result<DiscordChannel>>(channel);
    }

    public Task<Result<DiscordChannel>> GetFirstResolvedChannelOrAsync(ulong channelId)
    {
        if (!IsInitialized)
            throw new InvalidOperationException();

        DiscordChannel? channel;
        try
        {
            channel = InteractionContext?.ResolvedChannelMentions?.ElementAtOrDefault(0) ??
                      DiscordGuild.GetChannel(channelId);

            if (channel is null)
                return Task.FromResult<Result<DiscordChannel>>(new DiscordNotFoundError(DiscordEntity.Channel));
        }
        catch (Exception ex)
        {
            return Task.FromResult<Result<DiscordChannel>>(GetErrorFromDiscordException(ex, DiscordEntity.Channel));
        }

        return Task.FromResult<Result<DiscordChannel>>(channel);
    }

    public async Task<Result<(SnowflakeObject Snowflake, DiscordEntity Type)>> GetFirstResolvedSnowflakeOrAsync(ulong id, DiscordEntity? type = null)
    {
        if (!IsInitialized)
            throw new InvalidOperationException();

        SnowflakeObject? snowflake;
        DiscordEntity resultType = DiscordEntity.Guild;
        try
        {
            snowflake = InteractionContext?.ResolvedChannelMentions?.ElementAtOrDefault(0);
            if (snowflake is not null)
            {
                return (snowflake, DiscordEntity.Channel);
            }

            snowflake = InteractionContext?.ResolvedRoleMentions?.ElementAtOrDefault(0);
            if (snowflake is not null)
            {
                return (snowflake, DiscordEntity.Role);
            }

            snowflake = InteractionContext?.ResolvedUserMentions?.ElementAtOrDefault(0) as DiscordMember;
            if (snowflake is not null)
            {
                return (snowflake, DiscordEntity.Member);
            }

            if (snowflake is null)
            {
                switch (type)
                {
                    case DiscordEntity.Guild:
                        return new NotSupportedException();
                    case DiscordEntity.Channel:
                        snowflake = _discordGuild.GetChannel(id);
                        resultType = DiscordEntity.Channel;
                        break;
                    case DiscordEntity.Member:
                        snowflake = await _discordGuild.GetMemberAsync(id);
                        resultType = DiscordEntity.Member;
                        break;
                    case DiscordEntity.User:
                        snowflake = await _discordGuild.GetMemberAsync(id);
                        resultType = DiscordEntity.Member;
                        break;
                    case DiscordEntity.Role:
                        snowflake = _discordGuild.GetRole(id);
                        resultType = DiscordEntity.Role;
                        break;
                    case DiscordEntity.Message:
                        return new NotSupportedException();
                    case null:
                        return new DiscordNotFoundError();
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }

            if (snowflake is null)
                return new DiscordNotFoundError(DiscordEntity.Channel);
        }
        catch (Exception ex)
        {
            return GetErrorFromDiscordException(ex, DiscordEntity.Channel);
        }

        return (snowflake, resultType);
    }

    public async Task<Result<(SnowflakeObject Snowflake, DiscordEntity Type)>> GetFirstResolvedRoleOrMemberOrAsync(ulong id)
    {
        if (!IsInitialized)
            throw new InvalidOperationException();

        SnowflakeObject? snowflake;
        DiscordEntity resultType = DiscordEntity.Guild;
        try
        {
            snowflake = InteractionContext?.ResolvedRoleMentions?.ElementAtOrDefault(0);
            if (snowflake is not null)
            {
                return (snowflake, DiscordEntity.Role);
            }

            snowflake = InteractionContext?.ResolvedUserMentions?.ElementAtOrDefault(0) as DiscordMember;
            if (snowflake is not null)
            {
                return (snowflake, DiscordEntity.Member);
            }

            if (snowflake is null)
            {
                try
                {
                    snowflake = _discordGuild.GetRole(id);
                    resultType = DiscordEntity.Role;
                }
                catch
                {
                    // ignored
                }

                if (snowflake is null)
                {
                    snowflake = await _discordGuild.GetMemberAsync(id);
                    resultType = DiscordEntity.Member;
                }
                
            }

            if (snowflake is null)
                return new DiscordNotFoundError();
        }
        catch (Exception ex)
        {
            return GetErrorFromDiscordException(ex, DiscordEntity.Channel);
        }

        return (snowflake, resultType);
    }

    public Task<Result<DiscordMember>> GetOwnerAsync()
    {
        if (!IsInitialized)
            throw new InvalidOperationException();

        return Task.FromResult<Result<DiscordMember>>(DiscordGuild.Owner);
    }

    private static ResultError GetErrorFromDiscordException(Exception ex, DiscordEntity? entity = null)
    {
        switch (ex)
        {
            case NotFoundException:
                return entity.HasValue ? new DiscordNotFoundError(entity.Value) : new DiscordNotFoundError();
            case UnauthorizedException:
                return new DiscordNotAuthorizedError();
            case BadRequestException:
            case RateLimitException:
            case RequestSizeException:
            case ServerErrorException:
                return new DiscordError();
            default:
                return new DiscordError();
        }
    }
}
