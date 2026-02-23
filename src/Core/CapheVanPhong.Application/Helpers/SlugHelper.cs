using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CapheVanPhong.Application.Helpers;

public static class SlugHelper
{
    private static readonly Dictionary<string, string> VietnameseMap = new()
    {
        { "à", "a" }, { "á", "a" }, { "ả", "a" }, { "ã", "a" }, { "ạ", "a" },
        { "ă", "a" }, { "ằ", "a" }, { "ắ", "a" }, { "ẳ", "a" }, { "ẵ", "a" }, { "ặ", "a" },
        { "â", "a" }, { "ầ", "a" }, { "ấ", "a" }, { "ẩ", "a" }, { "ẫ", "a" }, { "ậ", "a" },
        { "è", "e" }, { "é", "e" }, { "ẻ", "e" }, { "ẽ", "e" }, { "ẹ", "e" },
        { "ê", "e" }, { "ề", "e" }, { "ế", "e" }, { "ể", "e" }, { "ễ", "e" }, { "ệ", "e" },
        { "ì", "i" }, { "í", "i" }, { "ỉ", "i" }, { "ĩ", "i" }, { "ị", "i" },
        { "ò", "o" }, { "ó", "o" }, { "ỏ", "o" }, { "õ", "o" }, { "ọ", "o" },
        { "ô", "o" }, { "ồ", "o" }, { "ố", "o" }, { "ổ", "o" }, { "ỗ", "o" }, { "ộ", "o" },
        { "ơ", "o" }, { "ờ", "o" }, { "ớ", "o" }, { "ở", "o" }, { "ỡ", "o" }, { "ợ", "o" },
        { "ù", "u" }, { "ú", "u" }, { "ủ", "u" }, { "ũ", "u" }, { "ụ", "u" },
        { "ư", "u" }, { "ừ", "u" }, { "ứ", "u" }, { "ử", "u" }, { "ữ", "u" }, { "ự", "u" },
        { "ỳ", "y" }, { "ý", "y" }, { "ỷ", "y" }, { "ỹ", "y" }, { "ỵ", "y" },
        { "đ", "d" },
        // Uppercase
        { "À", "a" }, { "Á", "a" }, { "Ả", "a" }, { "Ã", "a" }, { "Ạ", "a" },
        { "Ă", "a" }, { "Ằ", "a" }, { "Ắ", "a" }, { "Ẳ", "a" }, { "Ẵ", "a" }, { "Ặ", "a" },
        { "Â", "a" }, { "Ầ", "a" }, { "Ấ", "a" }, { "Ẩ", "a" }, { "Ẫ", "a" }, { "Ậ", "a" },
        { "È", "e" }, { "É", "e" }, { "Ẻ", "e" }, { "Ẽ", "e" }, { "Ẹ", "e" },
        { "Ê", "e" }, { "Ề", "e" }, { "Ế", "e" }, { "Ể", "e" }, { "Ễ", "e" }, { "Ệ", "e" },
        { "Ì", "i" }, { "Í", "i" }, { "Ỉ", "i" }, { "Ĩ", "i" }, { "Ị", "i" },
        { "Ò", "o" }, { "Ó", "o" }, { "Ỏ", "o" }, { "Õ", "o" }, { "Ọ", "o" },
        { "Ô", "o" }, { "Ồ", "o" }, { "Ố", "o" }, { "Ổ", "o" }, { "Ỗ", "o" }, { "Ộ", "o" },
        { "Ơ", "o" }, { "Ờ", "o" }, { "Ớ", "o" }, { "Ở", "o" }, { "Ỡ", "o" }, { "Ợ", "o" },
        { "Ù", "u" }, { "Ú", "u" }, { "Ủ", "u" }, { "Ũ", "u" }, { "Ụ", "u" },
        { "Ư", "u" }, { "Ừ", "u" }, { "Ứ", "u" }, { "Ử", "u" }, { "Ữ", "u" }, { "Ự", "u" },
        { "Ỳ", "y" }, { "Ý", "y" }, { "Ỷ", "y" }, { "Ỹ", "y" }, { "Ỵ", "y" },
        { "Đ", "d" }
    };

    /// <summary>
    /// Converts a Vietnamese string to a URL-friendly slug.
    /// Example: "Máy Pha Cà Phê" → "may-pha-ca-phe"
    /// </summary>
    public static string Generate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var sb = new StringBuilder(input);

        // Replace Vietnamese characters
        foreach (var (key, value) in VietnameseMap)
        {
            sb.Replace(key, value);
        }

        var result = sb.ToString().ToLowerInvariant();

        // Replace spaces and hyphens chains with a single hyphen
        result = Regex.Replace(result, @"[^a-z0-9\s-]", "");
        result = Regex.Replace(result, @"[\s]+", "-");
        result = Regex.Replace(result, @"-+", "-");
        result = result.Trim('-');

        return result;
    }
}
