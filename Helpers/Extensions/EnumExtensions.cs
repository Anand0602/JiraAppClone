using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace JiraApp.Helpers.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var displayAttribute = enumValue.GetType()
                .GetMember(enumValue.ToString())
                .FirstOrDefault()
                ?.GetCustomAttribute<DisplayAttribute>();

            return displayAttribute?.Name ?? enumValue.ToString();
        }

        public static string GetDescription(this Enum enumValue)
        {
            var displayAttribute = enumValue.GetType()
                .GetMember(enumValue.ToString())
                .FirstOrDefault()
                ?.GetCustomAttribute<DisplayAttribute>();

            return displayAttribute?.Description ?? enumValue.ToString();
        }

        public static T ToEnum<T>(this string value) where T : struct
        {
            if (string.IsNullOrEmpty(value))
            {
                return default;
            }

            return Enum.TryParse<T>(value, true, out var result) ? result : default;
        }
    }
}
