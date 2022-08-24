namespace WithCancellation.Entities;

public record StopOrder
{
    public required string Token { get; init; }
    public required decimal StopPrice { get; init; }
    public required decimal LimitPrice { get; init; }
    public required decimal Volume { get; init; }

    //[SetsRequiredMembers]
    //public StopOrder(string token, decimal allAtOnce)
    //{
    //    Token = token;
    //    StopPrice = allAtOnce;
    //    LimitPrice = allAtOnce;
    //    Volume = allAtOnce;
    //}
}