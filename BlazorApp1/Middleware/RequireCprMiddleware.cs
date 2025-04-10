using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Data;
using System.Linq;

namespace BlazorApp1.Middleware
{
    public class RequireCprMiddleware
    {
        private readonly RequestDelegate _next;

        public RequireCprMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, TodoDbContext db)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                // Exclude paths that do not require a CPR check (e.g., CPR entry page, logout, account management).
                if (!context.Request.Path.StartsWithSegments("/Cpr") &&
                    !context.Request.Path.StartsWithSegments("/Account"))
                {
                    // Check if the session flag "CPRVerified" is set to "true"
                    var cprVerified = context.Session.GetString("CPRVerified");
                    if (cprVerified != "true")
                    {
                        // Redirect the user to the CPR page if CPR is not verified for this login.
                        context.Response.Redirect("/Cpr");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
    
}