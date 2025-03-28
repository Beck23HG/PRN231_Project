namespace FeedbackTeacher.Middleware
{
    public class AddTokenFromSessionMiddleware
    {
        private readonly RequestDelegate _next;

        public AddTokenFromSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Lấy token từ session
            string token = context.Session.GetString("Token");

            if (!string.IsNullOrEmpty(token))
            {
                // Thêm token vào header Authorization
                context.Request.Headers["Authorization"] = $"Bearer {token}";
            }

            await _next(context);
        }
    }

    public static class AddTokenFromSessionMiddlewareExtensions
    {
        public static IApplicationBuilder UseAddTokenFromSession(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AddTokenFromSessionMiddleware>();
        }
    }
}
