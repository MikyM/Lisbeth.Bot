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

namespace MikyM.Common.Utilities.Extensions;

public static class ExceptionExtensions
{
    public static string GetFullMessage(this Exception ex)
    {
        return ex.InnerException is null ? ex.Message : ex.Message + " --> " + ex.InnerException.GetFullMessage();
    }

    public static IEnumerable<Exception> GetAllExceptions(this Exception exception)
    {
        yield return exception;

        if (exception is AggregateException aggrEx)
            foreach (Exception innerEx in aggrEx.InnerExceptions.SelectMany(e => e.GetAllExceptions()))
                yield return innerEx;
        else if (exception.InnerException is not null)
            foreach (Exception innerEx in exception.InnerException.GetAllExceptions())
                yield return innerEx;
    }

    public static string ToFormattedString(this Exception exception)
    {
        var messages = exception.GetAllExceptions()
            .Where(e => !string.IsNullOrWhiteSpace(e.Message))
            .Select(exceptionPart => exceptionPart.Message.Trim() + "\r\n" +
                                     (exceptionPart.StackTrace is not null ? exceptionPart.StackTrace.Trim() : ""));

        return string.Join("\r\n\r\n", messages);
    }
}