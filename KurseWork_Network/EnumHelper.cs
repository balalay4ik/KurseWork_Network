using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

public static class EnumHelper
{
    /// <summary>
    /// Получить описание элемента enum через атрибут Description.
    /// Если атрибут отсутствует, возвращает строковое имя элемента.
    /// </summary>
    public static string GetDescription<TEnum>(TEnum value) where TEnum : Enum
    {
        var field = typeof(TEnum).GetField(value.ToString());
        var attribute = field.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? FormatEnumName(value.ToString());
    }

    /// <summary>
    /// Создать словарь Enum -> Описание (с использованием атрибута Description).
    /// Если атрибут отсутствует, используется форматированное имя.
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
    /// Создать словарь Enum -> Текстовое представление (без атрибутов).
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
    /// Форматировать имя элемента enum для удобочитаемого отображения.
    /// </summary>
    private static string FormatEnumName(string name)
    {
        return string.Concat(name.Select((c, i) => i > 0 && char.IsUpper(c) ? " " + c : c.ToString()));
    }
}
