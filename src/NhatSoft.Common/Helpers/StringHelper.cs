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

        // 1. Chuyển thành chữ thường
        text = text.ToLowerInvariant();

        // 2. Xử lý chữ 'đ' riêng (Trước khi Normalize để tránh lỗi sót font)
        // Vì đã ToLower nên chỉ cần replace 'đ', không cần 'Đ'
        text = text.Replace("đ", "d");

        // 3. Bỏ dấu (Normalize)
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

        // 4. Giữ lại: Chữ thường, số, khoảng trắng (\s), gạch ngang (-)
        // Loại bỏ các ký tự đặc biệt (@, #, $, %, ^, &...)
        text = Regex.Replace(text, @"[^a-z0-9\s-]", "");

        // 5. Chuyển khoảng trắng thành gạch ngang
        text = Regex.Replace(text, @"\s+", "-");

        // 6. QUAN TRỌNG: Gộp nhiều dấu gạch ngang liên tiếp thành 1
        // Ví dụ: "a---b" -> "a-b"
        text = Regex.Replace(text, @"-+", "-");

        // 7. Cắt gạch ngang thừa ở đầu và cuối
        return text.Trim('-');
    }
}