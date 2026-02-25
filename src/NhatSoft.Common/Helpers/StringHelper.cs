using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace NhatSoft.Common.Helpers;

public static class StringHelper
{
    public static string ToSlug(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // 1. Chuyển về chữ thường
        text = text.ToLowerInvariant();

        // 2. Loại bỏ dấu tiếng Việt (Normalize)
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        text = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

        // 3. Thay thế đ -> d
        text = Regex.Replace(text, "[đĐ]", "d");

        // 4. Xóa các ký tự đặc biệt, giữ lại chữ cái, số và dấu gạch ngang
        text = Regex.Replace(text, @"[^a-z0-9\s-]", "");

        // 5. Thay khoảng trắng bằng dấu gạch ngang
        text = Regex.Replace(text, @"\s+", "-");

        // 6. Xóa các dấu gạch ngang thừa ở đầu/cuối
        return text.Trim('-');
    }
}