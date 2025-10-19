using Mapster;
using Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Services.Mapping
{
    public class MapsterEnumConfig
    {
        public static void RegisterEnumStringConversions(params Assembly[] assembliesWithEnums)
        {
            var cfg = TypeAdapterConfig.GlobalSettings;

            foreach (var asm in assembliesWithEnums)
            {
                var enumTypes = asm.GetTypes().Where(t => t.IsEnum && !t.IsNested);
                foreach (var enumType in enumTypes)
                {
                    // string? -> TEnum?
                    InvokeGeneric(nameof(ConfigStringToNullableEnum), enumType, cfg);
                    // string  -> TEnum  (nếu muốn default khi null/invalid, sửa trong helper)
                    InvokeGeneric(nameof(ConfigStringToEnum), enumType, cfg);

                    // TEnum? -> string?
                    InvokeGeneric(nameof(ConfigNullableEnumToString), enumType, cfg);
                    // TEnum  -> string
                    InvokeGeneric(nameof(ConfigEnumToString), enumType, cfg);
                }
            }
        }

        static void InvokeGeneric(string method, Type enumType, TypeAdapterConfig cfg)
        {
            typeof(MapsterEnumConfig).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(enumType)
                .Invoke(null, new object[] { cfg });
        }

        // ---- per TEnum registrations ----
        static void ConfigStringToEnum<TEnum>(TypeAdapterConfig cfg) where TEnum : struct, Enum
        {
            cfg.NewConfig<string, TEnum>()
               .MapWith(s => EnumConverter.StringToEnum<TEnum>(s).GetValueOrDefault());
        }

        static void ConfigStringToNullableEnum<TEnum>(TypeAdapterConfig cfg) where TEnum : struct, Enum
        {
            cfg.NewConfig<string?, TEnum?>()
               .MapWith(s => EnumConverter.StringToEnum<TEnum>(s));
        }

        static void ConfigEnumToString<TEnum>(TypeAdapterConfig cfg) where TEnum : struct, Enum
        {
            cfg.NewConfig<TEnum, string>()
               .MapWith(e => EnumConverter.EnumToString<TEnum>(e) ?? string.Empty);
        }

        static void ConfigNullableEnumToString<TEnum>(TypeAdapterConfig cfg) where TEnum : struct, Enum
        {
            cfg.NewConfig<TEnum?, string?>()
               .MapWith(e => EnumConverter.EnumToString<TEnum>(e));
        }
    }
}