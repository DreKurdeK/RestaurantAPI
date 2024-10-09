using System.Diagnostics;

namespace RestaurantAPI.Middleware
{
    public class RequestTimeMiddleware(ILogger<RequestTimeMiddleware> logger) : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var stopwatch = Stopwatch.StartNew();

            await next.Invoke(context);

            stopwatch.Stop();

            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            if (elapsedMilliseconds / 1000 > 4)
            {
                var message = $"Request [{context.Request.Method}] at {context.Request.Path} took {elapsedMilliseconds} ms";
                logger.LogInformation(message);
            }
        }
    }
}
