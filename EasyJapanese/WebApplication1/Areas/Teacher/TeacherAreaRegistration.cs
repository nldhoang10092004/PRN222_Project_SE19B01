using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace WebApplication1.Areas.Teacher
{
    /// <summary>
    /// Đăng ký routing cho khu vực giáo viên.
    /// URL prefix: /teacher/...
    /// </summary>
    public static class TeacherAreaRegistration
    {
        public const string Name = "Teacher";
        public const string UrlPrefix = "teacher";

        public static IEndpointRouteBuilder MapTeacherArea(this IEndpointRouteBuilder endpoints)
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
