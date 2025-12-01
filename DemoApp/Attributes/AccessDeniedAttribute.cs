using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DemoApp.Attributes
{
    public class AccessDeniedAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang login
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = context.HttpContext.Request.Path });
            }
            else
            {
                // Nếu đã đăng nhập nhưng không có quyền, hiển thị trang Access Denied
                context.Result = new ViewResult { ViewName = "AccessDenied" };
            }
        }
    }
}
