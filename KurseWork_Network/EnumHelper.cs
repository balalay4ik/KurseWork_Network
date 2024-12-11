using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

public static class EnumHelper
{
    /// <summary>
    /// Отримати опис елемента enum через атрибут Description.
    /// Якщо атрибут відсутній, повертає строкову назву елемента.
    /// </summary>
    public static string GetDescription<TEnum>(TEnum value) where TEnum : Enum
    {
        var field = typeof(TEnum).GetField(value.ToString());
        var attribute = field.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? FormatEnumName(value.ToString());
    }

    /// <summary>
    /// Створити словник Enum -> Опис (з використанням атрибуту Description).
    /// Якщо атрибут відсутній, використовується форматоване ім'я.
    /// </summary>
    public static Dictionary<TEnum, string> GetDescriptions<TEnum>() where TEnum : Enum
    {
        return Enum.GetValues(typeof(TEnum))
                   .Cast<TEnum>()
                   .ToDictionary(
                       value => value,
                       value => GetDescription(value)
                   );
    }

    /// <summary>
    /// Створити словник Enum -> Текстове представлення (без атрибутів).
    /// </summary>
    public static Dictionary<TEnum, string> GetEnumTextMappings<TEnum>() where TEnum : Enum
    {
        return Enum.GetValues(typeof(TEnum))
                   .Cast<TEnum>()
                   .ToDictionary(
                       value => value,
                       value => FormatEnumName(value.ToString())
                   );
    }

    /// <summary>
    /// Форматувати ім'я елемента enum для зручного відображення.
    /// </summary>
    private static string FormatEnumName(string name)
    {
        return string.Concat(name.Select((c, i) => i > 0 && char.IsUpper(c) ? " " + c : c.ToString()));
    }
}

