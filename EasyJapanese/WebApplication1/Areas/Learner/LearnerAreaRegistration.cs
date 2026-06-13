using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace WebApplication1.Areas.Learner
{
    /// <summary>
    /// Đăng ký routing cho khu vực học viên (sau khi đăng nhập).
    /// URL prefix: /learn/...
    /// </summary>
    public static class LearnerAreaRegistration
    {
        public const string Name = "Learner";
        public const string UrlPrefix = "learn";

        public static IEndpointRouteBuilder MapLearnerArea(this IEndpointRouteBuilder endpoints)
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
