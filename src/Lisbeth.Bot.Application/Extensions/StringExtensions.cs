﻿// This file is part of Lisbeth.Bot project
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

namespace Lisbeth.Bot.Application.Extensions;

public static class StringExtensions
{
    public static bool IsDigitsOnly(this string input)
    {
        return input.All(c => c is >= '0' and <= '9');
    }
    
    public static string? Truncate(this string? value, int maxLength, string truncationSuffix = "…")
    {
        return value?.Length > maxLength
            ? string.Concat(value.AsSpan(0, maxLength), truncationSuffix)
            : value;
    }
    
    public static bool TryParseToDurationAndNextOccurrence(this string input, [NotNullWhen(true)] out DateTime? occurrence,
        [NotNullWhen(true)] out TimeSpan? duration)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            occurrence = null;
            duration = null;
            return false;
        }

        TimeSpan tmsp = new ();
        DateTime result;

        if (int.TryParse(input, out var inputInMinutes))
        {
            if (inputInMinutes > 44640) inputInMinutes = 44640;
            tmsp = TimeSpan.FromMinutes(inputInMinutes);
            result = DateTime.UtcNow.Add(tmsp);
        }
        else if (input.Contains("perm", StringComparison.InvariantCultureIgnoreCase))
        {
            result = DateTime.MaxValue;
        }
        else
        {
            var parsedInput = 0;
            var inputType = 'x';

            if (!char.IsDigit(input.First()))
            {
                occurrence = null;
                duration = null;
                return false;
            }

            foreach (var c in input.Where(c => !char.IsDigit(c)))
            {
                inputType = char.ToLower(c);
                if (inputType is not ('m' or 'd' or 'w' or 'y' or 'h'))
                {
                    occurrence = null;
                    duration = null;
                    return false;
                }

                parsedInput = int.Parse(input[..input.IndexOf(c)]);
                break;
            }

            switch (inputType)
            {
                case 'h':
                    if (parsedInput > 8784)
                    {
                        occurrence = null;
                        duration = null;
                        return false;
                    }

                    tmsp = TimeSpan.FromHours(parsedInput);
                    break;

                case 'd':
                    if (parsedInput > 366)
                    {
                        occurrence = null;
                        duration = null;
                        return false;
                    }

                    tmsp = TimeSpan.FromDays(parsedInput);
                    break;

                case 'w':
                    if (parsedInput > 53)
                    {
                        occurrence = null;
                        duration = null;
                        return false;
                    }

                    tmsp = TimeSpan.FromDays(parsedInput * 7);
                    break;

                case 'm':
                    tmsp = TimeSpan.FromMinutes(parsedInput);
                    break;

                case 'y':
                    if (parsedInput > 1)
                    {
                        occurrence = null;
                        duration = null;
                        return false;
                    }

                    tmsp = TimeSpan.FromDays(parsedInput * 365);
                    break;

                default:
                    throw new ArgumentException();
            }

            result = DateTime.UtcNow.Add(tmsp);
        }

        occurrence = result;
        duration = tmsp;

        return true;
    }

    /// <summary>
    ///     Takes a substring between two anchor strings (or the end of the string if that anchor  is null)
    /// </summary>
    /// <param name="value">a string</param>
    /// <param name="from">an optional string to search after</param>
    /// <param name="until">an optional string to search before</param>
    /// <param name="comparison">an optional comparison for the search</param>
    /// <returns>a substring based on the search</returns>
    public static string GetStringBetween(this string value, string? from = null, string? until = null,
        StringComparison comparison = StringComparison.InvariantCulture)
    {
        var fromLength = (from ?? string.Empty).Length;
        var startIndex = !string.IsNullOrEmpty(from)
            ? value.IndexOf(from, comparison) + fromLength
            : 0;

        if (startIndex < fromLength)
            return ""; //{ throw new ArgumentException("from: Failed to find an instance of the first anchor"); }

        var endIndex = !string.IsNullOrEmpty(until)
            ? value.IndexOf(until, startIndex, comparison)
            : value.Length;

        return endIndex < 0 ? "" : value.Substring(startIndex, endIndex - startIndex);
    }
}
