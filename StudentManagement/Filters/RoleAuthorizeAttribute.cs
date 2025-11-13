using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace StudentManagement.Filters
{
    // Custom role-based authorization attribute
    public class RoleAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;

        public RoleAuthorizeAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            var userRole = httpContext.Session.GetString("UserRole");

            if (userRole == null || !_roles.Contains(userRole))
            {
                // Redirect unauthorized users to Login page
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }
        }
    }
}
