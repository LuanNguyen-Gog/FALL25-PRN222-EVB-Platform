using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Helpers
{
    public static class EnumConverter
    {
        // enum -> "active" (lowercase) | null
        public static string? EnumToString<TEnum>(TEnum? value)
            where TEnum : struct, Enum
            => value.HasValue ? value.Value.ToString().ToLowerInvariant() : null;

        // "active" -> enum | defaultValue
        public static TEnum? StringToEnum<TEnum>(string? input, TEnum? defaultValue = null)
            where TEnum : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(input)) return defaultValue;

            var norm = input.Trim()
                            .Replace(" ", "_")
                            .Replace("-", "_")
                            .ToLowerInvariant();

            // alias chung (tuỳ bạn mở rộng)
            if (norm is "enabled") norm = "active";
            if (norm is "disabled" or "deactivated") norm = "inactive";

            foreach (var name in Enum.GetNames(typeof(TEnum)))
                if (string.Equals(name, norm, StringComparison.OrdinalIgnoreCase))
                    return Enum.Parse<TEnum>(name, ignoreCase: true);

            return defaultValue;
        }
    }
}
