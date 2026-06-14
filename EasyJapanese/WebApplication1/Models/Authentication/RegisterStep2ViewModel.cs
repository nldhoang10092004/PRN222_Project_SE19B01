using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.Authentication
{
    public class RegisterStep2ViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mã OTP.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP gồm 6 chữ số.")]
        [RegularExpression("^[0-9]{6}$", ErrorMessage = "Mã OTP chỉ gồm chữ số.")]
        public string Otp { get; set; } = string.Empty;
    }
}
