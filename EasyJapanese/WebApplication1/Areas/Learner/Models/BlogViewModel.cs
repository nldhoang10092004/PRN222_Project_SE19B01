using System;
using System.Collections.Generic;
using CoreLibrary.Data.Entities;

namespace CoreWeb.Areas.Learner.Models
{
    public class CommunityIndexViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public int CurrentUserId { get; set; }
        public CommunityPost? FeaturedPost { get; set; }
        public List<CommunityPost> Posts { get; set; } = new List<CommunityPost>();
        public List<CommunityPost> RecentPosts { get; set; } = new List<CommunityPost>();
        public string CurrentCategory { get; set; } = string.Empty;
        public string SearchQuery { get; set; } = string.Empty;
        public string CurrentSort { get; set; } = "newest";
        public int Page { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalCount { get; set; } = 0;
    }

    public class CommunityDetailsViewModel
    {
        public CommunityPost Post { get; set; } = null!;
        public List<CommunityComment> Comments { get; set; } = new List<CommunityComment>();
        public bool IsLikedByCurrentUser { get; set; }
        public int CurrentUserId { get; set; }
    }
}