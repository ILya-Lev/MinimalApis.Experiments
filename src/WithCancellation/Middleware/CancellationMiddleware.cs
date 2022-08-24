namespace WithCancellation.Middleware;

public class CancellationMiddleware
{
    private readonly ILogger<CancellationMiddleware> _logger;
    private readonly RequestDelegate _next;

    //if we want to have ctor; we need to provide next delegate here;
    //app.UseMiddleware registers the class, so that we do not need to do it explicitly
    public CancellationMiddleware(ILogger<CancellationMiddleware> logger, RequestDelegate next)
    {
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next.Invoke(context);
        }
        catch (OperationCanceledException)//task cancelled exception derives from this one
        {
            _logger.LogWarning("A task has been cancelled");
            context.Response.StatusCode = 409;//aka client closed request
        }
    }
}