using BCrypt.Net;

namespace CoreLibrary.Security
{
    public static class PasswordUtil
    {
        /// <summary>
        /// Băm mật khẩu người dùng trước khi lưu vào Database
        /// </summary>
        public static string HashPassword(string plainPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword)) return string.Empty;

            // Mặc định BCrypt.Net-Next dùng work factor = 11, cực kỳ an toàn và tối ưu tốc độ
            return BCrypt.Net.BCrypt.HashPassword(plainPassword);
        }

        /// <summary>
        /// So sánh mật khẩu người dùng nhập vào với chuỗi Hash lấy từ Database
        /// </summary>
        public static bool VerifyPassword(string plainPassword, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword) || string.IsNullOrWhiteSpace(hashedPassword))
                return false;

            return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
        }
    }
}