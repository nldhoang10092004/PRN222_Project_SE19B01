using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace WebApplication1.Areas.Admin
{
    /// <summary>
    /// Đăng ký routing cho khu vực quản trị.
    /// URL prefix: /admin/...
    /// </summary>
    public static class AdminAreaRegistration
    {
        public const string Name = "Admin";
        public const string UrlPrefix = "admin";

        public static IEndpointRouteBuilder MapAdminArea(this IEndpointRouteBuilder endpoints)
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
