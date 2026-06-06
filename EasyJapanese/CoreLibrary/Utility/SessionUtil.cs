using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace CoreLibrary.Utility
{
    // Phải là static class để dùng Extension Method
    public static class SessionUtil
    {
        /// <summary>
        /// Chuyển Object thành chuỗi JSON và lưu vào Session
        /// </summary>
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            session.SetString(key, JsonSerializer.Serialize(value, options));
        }

        /// <summary>
        /// Lấy chuỗi JSON từ Session và ép kiểu ngược lại thành Object
        /// </summary>
        public static T? GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }

        /// <summary>
        /// Xóa một key cụ thể khỏi Session
        /// </summary>
        public static void Remove(this ISession session, string key)
        {
            session.Remove(key);
        }

        /// <summary>
        /// Xóa toàn bộ dữ liệu trong Session, 
        /// NGOẠI TRỪ các key bắt đầu bằng ký tự "_" (Dành cho System config)
        /// </summary>
        public static void RemoveAll(this ISession session)
        {
            // BẮT BUỘC phải ép sang .ToList() để tạo một bản sao danh sách key.
            // Nếu không có .ToList(), khi ông vừa duyệt (foreach) vừa xóa (Remove), C# sẽ báo lỗi Collection Was Modified.
            var keys = session.Keys.ToList();

            foreach (var key in keys)
            {
                if (!key.StartsWith("_"))
                {
                    session.Remove(key);
                }
            }
        }
    }
}