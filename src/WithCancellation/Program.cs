using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Console;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss ";
        options.UseUtcTimestamp = true;
        options.ColorBehavior = LoggerColorBehavior.Enabled;
    });
});

builder.Services.TryAddScoped<IHandler, Handler>();
//builder.Services.TryAddSingleton<CancellationMiddleware>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.ConfigObject.DisplayRequestDuration = true;
        options.ConfigObject.TryItOutEnabled = true;
    });
}

app.UseHttpsRedirection();

app.UseMiddleware<CancellationMiddleware>();
app
    .MapGet("/slowtest", async ([FromServices] IHandler handler, CancellationToken stopper) 
            => await handler.Handle(stopper))
.WithName("SlowTest");//helps swagger to see this endpoint

app.Run();

interface IHandler
{
    Task<string> Handle(CancellationToken stopper);
}

internal class Handler : IHandler
{
    private readonly ILogger<Handler> _logger;

    public Handler(ILogger<Handler> logger) => _logger = logger;

    public async Task<string> Handle(CancellationToken stopper)
    {
        var id = Guid.NewGuid();
        _logger.LogInformation($"Starting to do slow work #{id}");
        
        await Task.Delay(10_000, stopper);
        
        var message = $"Finished slow operation #{id}";
        _logger.LogInformation(message);
        return message;
    }
}

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