namespace WebApplication1.Areas.Learner.Models
{
    public class FlashcardSetListItemViewModel
    {
        public int FlashcardSetId { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int TotalCount { get; set; }   
        public int DueCount { get; set; }    
        public bool Started { get; set; }     
    }

    public class FlashcardSetDetailViewModel
    {
        public int FlashcardSetId { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public List<FlashcardPreviewItem> PreviewCards { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class FlashcardPreviewItem
    {
        public string FrontText { get; set; } = "";
        public string BackText { get; set; } = "";
    }

    public class FlashcardCardViewModel
    {
        public int FlashcardId { get; set; }
        public string FrontText { get; set; } = "";
        public string BackText { get; set; } = "";
        public string? ImageUrl { get; set; }
    }

    public class FlashcardReviewPageViewModel
    {
        public int FlashcardSetId { get; set; }
        public string SetTitle { get; set; } = "";
        public int DueCount { get; set; }
        public int TotalCount { get; set; }
    }

    public class FlashcardAnswerRequest
    {
        public int FlashcardId { get; set; }
        public int Quality { get; set; } // 0=Quên, 3=Khó, 4=Tốt, 5=Dễ
    }
}