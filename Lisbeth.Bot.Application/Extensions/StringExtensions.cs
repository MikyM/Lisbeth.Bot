using System;
using System.Linq;

namespace Lisbeth.Bot.Application.Extensions
{
    public static class StringExtensions
    {
        public static (DateTime? FinalDateFromToday, TimeSpan Duration) ToDateTimeDuration(this string input)
        {
            TimeSpan tmsp = new TimeSpan();
            DateTime? result;

            if (Int32.TryParse(input, out int inputInMinutes))
            {
                if (inputInMinutes > 44640)
                {
                    inputInMinutes = 44640;
                }
                tmsp = TimeSpan.FromMinutes(inputInMinutes);
                result = DateTime.Now.Add(tmsp);
            }
            else if (input.ToLower().Contains("perm"))
            {
                result = DateTime.MaxValue.Date;
            }
            else
            {
                int parsedInput = 0;
                char inputType = 'x';

                if (!Char.IsDigit(input.First()))
                {
                    return (null, tmsp);
                }

                foreach (char c in input)
                {
                    if (!Char.IsDigit(c))
                    {
                        inputType = Char.ToLower(c);
                        if (inputType == 'm' || inputType == 'd' || inputType == 'w' || inputType == 'y' || inputType == 'h')
                        {
                            parsedInput = Int32.Parse(input.Substring(0, input.IndexOf(c)));
                            break;
                        }
                        else
                        {
                            return (null, tmsp);
                        }
                    }
                }

                switch (inputType)
                {
                    case 'h':
                        if (parsedInput > 8784)
                        {
                            return (null, tmsp);
                        }
                        tmsp = TimeSpan.FromHours(parsedInput);
                        break;

                    case 'd':
                        if (parsedInput > 366)
                        {
                            return (null, tmsp);
                        }
                        tmsp = TimeSpan.FromDays(parsedInput);
                        break;

                    case 'w':
                        if (parsedInput > 53)
                        {
                            return (null, tmsp);
                        }
                        tmsp = TimeSpan.FromDays(parsedInput * 7);
                        break;

                    case 'm':
                        if (parsedInput > 12)
                        {
                            return (null, tmsp);
                        }
                        tmsp = TimeSpan.FromDays(parsedInput * 31);
                        break;

                    case 'y':
                        if (parsedInput > 1)
                        {
                            return (null, tmsp);
                        }
                        tmsp = TimeSpan.FromDays(parsedInput * 365);
                        break;

                    default:
                        return (null, tmsp);
                }

                result = DateTime.Now.Add(tmsp);
            }

            return (result, tmsp);
        }
    }
}
