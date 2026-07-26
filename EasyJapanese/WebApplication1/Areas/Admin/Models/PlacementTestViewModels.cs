using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Areas.Admin.Models
{
    public class CreateQuestionViewModel
    {
        public int TestId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung câu hỏi")]
        [Display(Name = "Nội dung câu hỏi")]
        public string QuestionText { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn loại câu hỏi")]
        [Display(Name = "Loại câu hỏi")]
        public string QuestionType { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập điểm")]
        [Range(1, 100, ErrorMessage = "Điểm phải từ 1 đến 100")]
        [Display(Name = "Điểm")]
        public int Points { get; set; }

        [Display(Name = "Thứ tự")]
        public int SortOrder { get; set; }

        [Required(ErrorMessage = "Vui lòng thêm ít nhất 2 đáp án")]
        [MinLength(2, ErrorMessage = "Cần ít nhất 2 đáp án")]
        public List<AnswerOptionDto> AnswerOptions { get; set; } = new();
    }

    public class EditQuestionViewModel
    {
        public int QuestionId { get; set; }
        public int TestId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung câu hỏi")]
        [Display(Name = "Nội dung câu hỏi")]
        public string QuestionText { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn loại câu hỏi")]
        [Display(Name = "Loại câu hỏi")]
        public string QuestionType { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập điểm")]
        [Range(1, 100, ErrorMessage = "Điểm phải từ 1 đến 100")]
        [Display(Name = "Điểm")]
        public int Points { get; set; }

        [Display(Name = "Thứ tự")]
        public int SortOrder { get; set; }

        [Required(ErrorMessage = "Vui lòng thêm ít nhất 2 đáp án")]
        [MinLength(2, ErrorMessage = "Cần ít nhất 2 đáp án")]
        public List<AnswerOptionDto> AnswerOptions { get; set; } = new();
    }

    public class AnswerOptionDto
    {
        public int? OptionId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung đáp án")]
        [Display(Name = "Nội dung đáp án")]
        public string AnswerText { get; set; } = null!;

        [Display(Name = "Đáp án đúng")]
        public bool IsCorrect { get; set; }
    }
}
