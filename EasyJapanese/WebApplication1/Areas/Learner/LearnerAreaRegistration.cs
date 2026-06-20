using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace WebApplication1.Areas.Learner
{
    public static class LearnerAreaRegistration
    {
        public const string Name = "Learner";
        public const string UrlPrefix = "learn";

        public static IEndpointRouteBuilder MapLearnerArea(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapAreaControllerRoute(
                name: $"{Name}_default",
                areaName: Name,
                pattern: $"{UrlPrefix}/{{controller=Dashboard}}/{{action=Index}}/{{id?}}");

            return endpoints;
        }
    }
}