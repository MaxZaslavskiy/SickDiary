using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace SickDiary.Web.Filters;

public class AuthorizeUserAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var userId = context.HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            context.Result = new RedirectToActionResult("Login", "Home", null);
        }

        base.OnActionExecuting(context);
    }
}