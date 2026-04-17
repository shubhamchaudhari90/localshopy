using localshopyNew.Data;
using localshopyNew.Models;

namespace localshopyNew.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDBContext db)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Save to DB
                var log = new ErrorLog
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    Path = context.Request.Path,
                    CreatedAt = DateTime.UtcNow
                };

                db.ErrorLogs.Add(log);
                await db.SaveChangesAsync();

                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Internal Server Error");
            }
        }
    }
}
