using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure;

public static class StringExtensions
{
    public static T ToEnum<T>(this string value)
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }
}