using System;
using System.Text.RegularExpressions;

namespace CoreLibrary.Utility
{
    public static class NumberUtil
    {
        /// <summary>
        /// Kiểm tra xem chuỗi có phải là một số hợp lệ không (Bao gồm cả số âm và số thập phân).
        /// </summary>
        public static bool IsNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;

            // Dùng double để bao quát cả trường hợp số nguyên lẫn số thập phân
            return double.TryParse(value, out _);
        }

        /// <summary>
        /// Kiểm tra chuỗi có CHỈ CHỨA CÁC CHỮ SỐ (0-9) hay không bằng Regex.
        /// </summary>
        public static bool IsDigitsOnly(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;

            // Kiểm tra từ đầu (^) đến cuối ($) chỉ chứa các ký tự từ 0 đến 9
            return Regex.IsMatch(value, @"^[0-9]+$");
        }

        /// <summary>
        /// Ép kiểu chuỗi sang số nguyên (int). Nếu chuỗi rỗng hoặc nhập bậy, tự động trả về giá trị mặc định.
        /// </summary>
        public static int ToInt(string value, int defaultValue = 0)
        {
            if (string.IsNullOrWhiteSpace(value)) return defaultValue;

            return int.TryParse(value, out int result) ? result : defaultValue;
        }

        /// <summary>
        /// Ép kiểu sang số nguyên lớn (long). Thường dùng cho ID của Database hoặc Unix Timestamp.
        /// </summary>
        public static long ToLong(string value, long defaultValue = 0L)
        {
            if (string.IsNullOrWhiteSpace(value)) return defaultValue;

            return long.TryParse(value, out long result) ? result : defaultValue;
        }

        /// <summary>
        /// Ép kiểu sang số thực (double). Thường dùng cho điểm số, tọa độ...
        /// </summary>
        public static double ToDouble(string value, double defaultValue = 0d)
        {
            if (string.IsNullOrWhiteSpace(value)) return defaultValue;

            return double.TryParse(value, out double result) ? result : defaultValue;
        }

        /// <summary>
        /// CỰC KỲ QUAN TRỌNG: Ép kiểu sang Decimal. 
        /// Bắt buộc dùng cái này cho TIỀN TỆ (Giá khóa học, giỏ hàng...) để không bị sai số làm tròn.
        /// </summary>
        public static decimal ToDecimal(string value, decimal defaultValue = 0m)
        {
            if (string.IsNullOrWhiteSpace(value)) return defaultValue;

            return decimal.TryParse(value, out decimal result) ? result : defaultValue;
        }
    }
}