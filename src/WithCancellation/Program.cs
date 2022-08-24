using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Console;
using WithCancellation.Middleware;
using WithCancellation.Services;

var builder = WebApplication.CreateBuilder(args);

#region register services
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

builder.Services.TryAddScoped<ISlowOperationModeler, SlowOperationModeler>();
//builder.Services.TryAddSingleton<CancellationMiddleware>();
#endregion register services

var app = builder.Build();

#region configure http pipeline
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
    .MapGet("/slowtest", 
        async ([FromServices] ISlowOperationModeler handler, CancellationToken stopper) 
            => await handler.Handle(stopper))
.WithName("SlowTest");//helps swagger to see this endpoint
#endregion configure http pipeline

app.Run();
