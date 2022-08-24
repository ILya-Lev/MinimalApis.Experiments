using WithCancellation.Entities;

namespace WithCancellation.Services;

interface ISlowOperationModeler
{
    Task<StopOrder> Handle(CancellationToken stopper);
}

internal class SlowOperationModeler : ISlowOperationModeler
{
    private readonly ILogger<SlowOperationModeler> _logger;

    public SlowOperationModeler(ILogger<SlowOperationModeler> logger) => _logger = logger;

    public async Task<StopOrder> Handle(CancellationToken stopper)
    {
        var id = Guid.NewGuid();
        _logger.LogInformation($"Starting to do slow work #{id}");
        await Task.Delay(10_000, stopper);
        _logger.LogInformation($"Finished slow operation #{id}");

        return new StopOrder()
        {
            Token = "AAPL",
            StopPrice = 387,
            LimitPrice = 388,
            Volume = 10
        };
    }
}