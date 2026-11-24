using StockMarket.Application.Abstractions.Identity;
using System.Security.Claims;

namespace StockMarket.Web.Services;

public class HttpContextCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;
    public Guid? UserId
    {
        get
        {
            var value = _accessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }

    public HttpContextCurrentUserService(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }
}
