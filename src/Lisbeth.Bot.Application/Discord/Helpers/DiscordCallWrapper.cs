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

namespace Lisbeth.Bot.Application.Discord.Helpers;

public static class DiscordCallWrapper
{
    public static async Task<T?> WrapDiscordApiCallNullable<T>(Func<Task<T>> func)
    {
        try
        {
            return await func.Invoke();
        }
        catch
        {
            return default;
        }
    }

    public static async Task<Result<T>> WrapDiscordApiCall<T>(Func<Task<T>> func)
    {
        try
        {
            return Result<T>.FromSuccess(await func.Invoke());
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case DSharpPlus.Exceptions.NotFoundException:
                    return new DiscordNotFoundError();
                case DSharpPlus.Exceptions.UnauthorizedException:
                    return new DiscordNotAuthorizedError();
                case DSharpPlus.Exceptions.BadRequestException:
                case DSharpPlus.Exceptions.RateLimitException:
                case DSharpPlus.Exceptions.RequestSizeException:
                case DSharpPlus.Exceptions.ServerErrorException:
                    return new DiscordError();
                default:
                    throw;
            }
        }
    }
}