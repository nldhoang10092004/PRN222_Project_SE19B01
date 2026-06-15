using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace WebApplication1.Areas.Mentor
{
    /// <summary>
    /// Đăng ký routing cho khu vực giảng viên.
    /// URL prefix: /mentor/...
    /// </summary>
    public static class MentorAreaRegistration
    {
        public const string Name = "Mentor";
        public const string UrlPrefix = "mentor";

        public static IEndpointRouteBuilder MapMentorArea(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapAreaControllerRoute(
                name: $"{Name}_default",
                areaName: Name,
                pattern: "{UrlPrefix}/{controller=Dashboard}/{action=Index}/{id?}",
                defaults: new { UrlPrefix = UrlPrefix });

            return endpoints;
        }
    }
}
