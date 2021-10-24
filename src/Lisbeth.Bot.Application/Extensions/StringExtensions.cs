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

using System;
using System.Linq;

namespace Lisbeth.Bot.Application.Extensions
{
    public static class StringExtensions
    {
        public static (DateTimeOffset? FinalDateFromToday, TimeSpan Duration) ToDateTimeOffsetDuration(this string input)
        {

            if (input is null) throw new ArgumentNullException(nameof(input));

            TimeSpan tmsp = new TimeSpan();
            DateTimeOffset? result;

            if (int.TryParse(input, out int inputInMinutes))
            {
                if (inputInMinutes > 44640) inputInMinutes = 44640;
                tmsp = TimeSpan.FromMinutes(inputInMinutes);
                result = DateTimeOffset.UtcNow.Add(tmsp);
            }
            else if (input.Contains("perm", StringComparison.InvariantCultureIgnoreCase))
            {
                result = DateTimeOffset.MaxValue;
            }
            else
            {
                int parsedInput = 0;
                char inputType = 'x';

                if (!char.IsDigit(input.First())) return (null, tmsp);

                foreach (var c in input.Where(c => !char.IsDigit(c)))
                {
                    inputType = char.ToLower(c);
                    if (inputType is not ('m' or 'd' or 'w' or 'y' or 'h')) return (null, tmsp);
                    parsedInput = int.Parse(input[..input.IndexOf(c)]);
                    break;
                }

                switch (inputType)
                {
                    case 'h':
                        if (parsedInput > 8784) return (null, tmsp);
                        tmsp = TimeSpan.FromHours(parsedInput);
                        break;

                    case 'd':
                        if (parsedInput > 366) return (null, tmsp);
                        tmsp = TimeSpan.FromDays(parsedInput);
                        break;

                    case 'w':
                        if (parsedInput > 53) return (null, tmsp);
                        tmsp = TimeSpan.FromDays(parsedInput * 7);
                        break;

                    case 'm':
                        if (parsedInput > 12) return (null, tmsp);
                        tmsp = TimeSpan.FromDays(parsedInput * 31);
                        break;

                    case 'y':
                        if (parsedInput > 1) return (null, tmsp);
                        tmsp = TimeSpan.FromDays(parsedInput * 365);
                        break;

                    default:
                        return (null, tmsp);
                }

                result = DateTimeOffset.UtcNow.Add(tmsp);
            }

            return (result, tmsp);
        }
    }
}