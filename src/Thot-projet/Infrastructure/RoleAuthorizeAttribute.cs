using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Thot_projet.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RoleAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly string[] _roles;

        public RoleAuthorizeAttribute(params string[] roles)
        {
            _roles = roles ?? new string[0];
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.Request.IsAuthenticated) 
                
                return false;

            var role = Convert.ToString(httpContext.Session["UserRole"] ?? "");






            if (string.IsNullOrWhiteSpace(role)) return false;

            // Si no se especifican roles, basta con estar autenticado
            if (_roles.Length == 0) return true;

            return _roles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
        }




        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsAuthenticated)
                base.HandleUnauthorizedRequest(filterContext);
            else
                filterContext.Result = new RedirectResult("~/Auth/Login");
        }
    }
}
