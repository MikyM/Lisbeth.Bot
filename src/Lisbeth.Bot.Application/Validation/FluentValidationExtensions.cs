using FluentValidation;
using System;

namespace Lisbeth.Bot.Application.Validation
{
    public static class FluentValidationExtensions
    {
        public static IRuleBuilderOptions<T, TProperty> DependentRules<T, TProperty>(
            this IRuleBuilderOptions<T, TProperty> currentRule,
            Action<IRuleBuilderOptions<T, TProperty>> action)
        {
            return currentRule.DependentRules(() => action(currentRule));
        }
    }
}
