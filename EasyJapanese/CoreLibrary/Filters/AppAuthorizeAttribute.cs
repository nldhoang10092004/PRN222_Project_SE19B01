using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using CoreLibrary.Authentication;
using CoreLibrary.Utility;
using CoreLibrary.Const;
using System.Linq;

namespace CoreLibrary.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class AppAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _requiredRole;

        public AppAuthorizeAttribute(string requiredRole = null)
        {
            _requiredRole = requiredRole;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.Session.GetObject<CurrentUser>(IAuthenticationService.SessionKeyCurrentUser);
            
            if (user == null)
            {
                // Not logged in
                context.Result = new RedirectResult("/Login");
                return;
            }

            if (!string.IsNullOrEmpty(_requiredRole))
            {
                var allowedRoles = _requiredRole.Split(',').Select(r => r.Trim()).ToList();
                // Admin can access everything, or check if user role is in the allowed list
                if (user.Role != RoleConst.ADMIN && !allowedRoles.Contains(user.Role))
                {
                    // Wrong role
                    context.Result = new RedirectResult("/Home/Error?code=403");
                }
            }
        }
    }
}
