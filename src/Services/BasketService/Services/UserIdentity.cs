namespace BasketService.Services;

public class UserIdentity : IUserIdentity
{
    private IHttpContextAccessor _context;

    public UserIdentity(IHttpContextAccessor context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    public string GetUserIdentity()
    {
        return _context.HttpContext.User.FindFirst("sub").Value;
    }
}