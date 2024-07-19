using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace DropboxSignEmbeddedSigning.Extensions;

public static class EnumExtensions
{
    public static string? GetEnumDisplayName(this Enum enumValue)
    {
        return enumValue.GetType()
            .GetMember(enumValue.ToString())
            .FirstOrDefault()
            ?.GetCustomAttribute<DisplayAttribute>()
            ?.Name;
    }
    
    
}