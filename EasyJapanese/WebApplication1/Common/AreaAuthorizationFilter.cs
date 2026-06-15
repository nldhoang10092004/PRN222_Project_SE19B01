using CoreLibrary.Authentication;
using CoreLibrary.Const;
using CoreLibrary.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApplication1.Common
{
    // Áp dụng cho mọi request: kiểm tra Area của route đang truy cập,
    // so khớp với role trong Session. Thiếu CurrentUser hoặc sai role → redirect /Login?returnUrl=...
    public class AreaAuthorizationFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            var area = httpContext.GetRouteValue("area") as string;

            if (string.IsNullOrEmpty(area))
            {
                return;
            }

            var controller = httpContext.GetRouteValue("controller") as string;
            var action = httpContext.GetRouteValue("action") as string;

            // Cho phép action Index của các LoginController trong Area truy cập không cần auth
            if (controller?.EndsWith("Login", StringComparison.Ordinal) == true &&
                string.Equals(action, "Index", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var currentUser = httpContext.Session.GetObject<CurrentUser>(IAuthenticationService.SessionKeyCurrentUser);

            if (currentUser == null)
            {
                RedirectToLogin(context, httpContext);
                return;
            }

            var allowed = area switch
            {
                "Admin"  => currentUser.Role == RoleConst.ADMIN,
                "Mentor" => currentUser.Role == RoleConst.MENTOR,
                "Learner" => currentUser.Role == RoleConst.STUDENT,
                _ => true
            };

            if (!allowed)
            {
                context.Result = new ForbidResult();
            }
        }

        private static void RedirectToLogin(AuthorizationFilterContext context, HttpContext httpContext)
        {
            var path = httpContext.Request.Path + httpContext.Request.QueryString;
            context.Result = new RedirectToActionResult("Index", "Login", new { returnUrl = path });
        }
    }
}
