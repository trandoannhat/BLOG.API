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

        // 2. Xử lý chữ 'đ' riêng 
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
        text = Regex.Replace(text, @"[^a-z0-9\s-]", "");

        //  [ĐOẠN THÊM MỚI] 4.5. Trảm các "Từ nối" (Stop words) tiếng Việt
        // Lưu ý: Đã bỏ dấu ở trên nên danh sách này viết không dấu
        string[] stopWords = {
            "va", "cua", "cho", "voi", "trong", "o", "nhung", "cac",
            "de", "thi", "ma", "nhu", "nay", "den", "boi", "ve",
            "ra", "lai", "rang", "nao", "duoc", "bi", "lam", "su", "viec", "mot", "nhung"
        };
        // Tạo Regex tìm chính xác các từ đứng độc lập (\b là word boundary)
        string stopWordsPattern = $@"\b({string.Join("|", stopWords)})\b";
        text = Regex.Replace(text, stopWordsPattern, "");
        //  [KẾT THÚC ĐOẠN THÊM MỚI]

        // 5. Chuyển khoảng trắng thành gạch ngang
        text = Regex.Replace(text, @"\s+", "-");

        // 6. QUAN TRỌNG: Gộp nhiều dấu gạch ngang liên tiếp thành 1
        text = Regex.Replace(text, @"-+", "-");

        // 7. Cắt gạch ngang thừa ở đầu và cuối
        return text.Trim('-');
    }
}