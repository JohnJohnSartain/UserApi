using Microsoft.AspNetCore.Mvc;
using SartainStudios.Token;
using Services;

namespace Interface.Controllers.Base
{
    public class BaseController : ControllerBase
    {
        private IToken _tokenService;
        private IUserService _userService;

        protected IUserService UserService => _userService ??= HttpContext?.RequestServices.GetService<IUserService>();
        private IToken TokenService => _tokenService ??= HttpContext?.RequestServices.GetService<IToken>();
 
        protected string GetUserId() =>
            TokenService.GetUserId(HttpContext.Request.Headers["Authorization"]);

        protected bool IsUserLeastPrivileged() =>
            TokenService.IsUserLeastPrivileged(HttpContext.Request.Headers["Authorization"]);
    }
}