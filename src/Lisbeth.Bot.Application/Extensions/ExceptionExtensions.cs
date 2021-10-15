﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Lisbeth.Bot.Application.Extensions
{
    public static class ExceptionExtensions
    {
        public static string GetFullMessage(this Exception ex)
        {
            return ex.InnerException == null ? ex.Message : ex.Message + " --> " + ex.InnerException.GetFullMessage();
        }

        public static IEnumerable<Exception> GetAllExceptions(this Exception exception)
        {
            yield return exception;

            if (exception is AggregateException aggrEx)
            {
                foreach (Exception innerEx in aggrEx.InnerExceptions.SelectMany(e => e.GetAllExceptions()))
                {
                    yield return innerEx;
                }
            }
            else if (exception.InnerException != null)
            {
                foreach (Exception innerEx in exception.InnerException.GetAllExceptions())
                {
                    yield return innerEx;
                }
            }
        }

        public static string ToFormattedString(this Exception exception)
        {
            var messages = exception.GetAllExceptions()
                .Where(e => !string.IsNullOrWhiteSpace(e.Message))
                .Select(exceptionPart => exceptionPart.Message.Trim() + "\r\n" +
                                         (exceptionPart.StackTrace != null ? exceptionPart.StackTrace.Trim() : ""));

            return string.Join("\r\n\r\n", messages);
        }
    }
}
