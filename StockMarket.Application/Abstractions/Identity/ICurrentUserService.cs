namespace StockMarket.Application.Abstractions.Identity;

public interface ICurrentUserService
{
    Guid? UserId { get; }
}
