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

using MikyM.Common.DataAccessLayer.Specifications.Exceptions;
using System.Collections.Generic;
using System.Linq.Expressions;
using MikyM.Common.DataAccessLayer.Specifications.Expressions;

namespace MikyM.Common.DataAccessLayer.Specifications.Extensions;

public static class SearchExtension
{
    public static bool Like(this string input, string pattern)
    {
        try
        {
            return SqlLike(input, pattern);
        }
        catch (Exception)
        {
            throw new InvalidSearchPatternException(pattern);
        }
    }

    /// <summary>
    /// Filters <paramref name="source"/> by applying an 'SQL LIKE' operation to it.
    /// </summary>
    /// <typeparam name="T">The type being queried against.</typeparam>
    /// <param name="source">The sequence of <typeparamref name="T"/></param>
    /// <param name="criterias">
    /// <list type="bullet">
    ///     <item>Selector, the property to apply the SQL LIKE against.</item>
    ///     <item>SearchTerm, the value to use for the SQL LIKE.</item>
    /// </list>
    /// </param>
    /// <returns></returns>
    public static IQueryable<T> Search<T>(this IQueryable<T> source, IEnumerable<SearchExpressionInfo<T>> criterias)
    {
        Expression? expr = null;
        var parameter = Expression.Parameter(typeof(T), "x");

        foreach (var criteria in criterias)
        {
            if (string.IsNullOrEmpty(criteria.SearchTerm)) continue;

            var functions = Expression.Property(null, typeof(EF).GetProperty(nameof(EF.Functions)));
            var like = typeof(DbFunctionsExtensions).GetMethod(nameof(DbFunctionsExtensions.Like),
                new Type[] { functions.Type, typeof(string), typeof(string) });

            var propertySelector =
                ParameterReplacerVisitor.Replace(criteria.Selector, criteria.Selector.Parameters[0], parameter);

            var likeExpression = Expression.Call(null, like, functions, (propertySelector as LambdaExpression)?.Body,
                Expression.Constant(criteria.SearchTerm));

            expr = expr == null ? (Expression)likeExpression : Expression.OrElse(expr, likeExpression);
        }

        return expr == null ? source : source.Where(Expression.Lambda<Func<T, bool>>(expr, parameter));
    }

    // This C# implementation of SQL Like operator is based on the following SO post https://stackoverflow.com/a/8583383/10577116
    // It covers almost all of the scenarios, and it's faster than regex based implementations.
    // It may fail/throw in some very specific and edge cases, hence, wrap it in try/catch.
    private static bool SqlLike(string str, string pattern)
    {
        bool isMatch = true,
            isWildCardOn = false,
            isCharWildCardOn = false,
            isCharSetOn = false,
            isNotCharSetOn = false,
            endOfPattern = false;
        int lastWildCard = -1;
        int patternIndex = 0;
        List<char> set = new();
        char p = '\0';

        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            endOfPattern = (patternIndex >= pattern.Length);
            if (!endOfPattern)
            {
                p = pattern[patternIndex];

                if (!isWildCardOn && p == '%')
                {
                    lastWildCard = patternIndex;
                    isWildCardOn = true;
                    while (patternIndex < pattern.Length &&
                        pattern[patternIndex] == '%')
                    {
                        patternIndex++;
                    }
                    if (patternIndex >= pattern.Length) p = '\0';
                    else p = pattern[patternIndex];
                }
                else if (p == '_')
                {
                    isCharWildCardOn = true;
                    patternIndex++;
                }
                else if (p == '[')
                {
                    if (pattern[++patternIndex] == '^')
                    {
                        isNotCharSetOn = true;
                        patternIndex++;
                    }
                    else isCharSetOn = true;

                    set.Clear();
                    if (pattern[patternIndex + 1] == '-' && pattern[patternIndex + 3] == ']')
                    {
                        char start = char.ToUpper(pattern[patternIndex]);
                        patternIndex += 2;
                        char end = char.ToUpper(pattern[patternIndex]);
                        if (start <= end)
                        {
                            for (char ci = start; ci <= end; ci++)
                            {
                                set.Add(ci);
                            }
                        }
                        patternIndex++;
                    }

                    while (patternIndex < pattern.Length &&
                        pattern[patternIndex] != ']')
                    {
                        set.Add(pattern[patternIndex]);
                        patternIndex++;
                    }
                    patternIndex++;
                }
            }

            if (isWildCardOn)
            {
                if (char.ToUpper(c) == char.ToUpper(p))
                {
                    isWildCardOn = false;
                    patternIndex++;
                }
            }
            else if (isCharWildCardOn)
            {
                isCharWildCardOn = false;
            }
            else if (isCharSetOn || isNotCharSetOn)
            {
                bool charMatch = (set.Contains(char.ToUpper(c)));
                if ((isNotCharSetOn && charMatch) || (isCharSetOn && !charMatch))
                {
                    if (lastWildCard >= 0) patternIndex = lastWildCard;
                    else
                    {
                        isMatch = false;
                        break;
                    }
                }
                isNotCharSetOn = isCharSetOn = false;
            }
            else
            {
                if (char.ToUpper(c) == char.ToUpper(p))
                {
                    patternIndex++;
                }
                else
                {
                    if (lastWildCard >= 0) patternIndex = lastWildCard;
                    else
                    {
                        isMatch = false;
                        break;
                    }
                }
            }
        }
        endOfPattern = (patternIndex >= pattern.Length);

        if (isMatch && !endOfPattern)
        {
            bool isOnlyWildCards = true;
            for (int i = patternIndex; i < pattern.Length; i++)
            {
                if (pattern[i] != '%')
                {
                    isOnlyWildCards = false;
                    break;
                }
            }
            if (isOnlyWildCards) endOfPattern = true;
        }
        return isMatch && endOfPattern;
    }
}
