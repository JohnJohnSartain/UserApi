using Microsoft.AspNetCore.Mvc;
using SartainStudios.Log;
using SartainStudios.Token;
using Services;

namespace Interface.Controllers.Base
{
    public class BaseController : ControllerBase
    {
        private ILog _log;
        private IToken _tokenService;
        private IUserService _userService;

        protected IUserService UserService => _userService ??= HttpContext?.RequestServices.GetService<IUserService>();
        private IToken TokenService => _tokenService ??= HttpContext?.RequestServices.GetService<IToken>();
        
        protected ILog Log =>
            _log ??= HttpContext?.RequestServices.GetService<ILog>();

        protected string GetUserId() =>
            TokenService.GetUserId(HttpContext.Request.Headers["Authorization"]);

        protected bool IsUserLeastPrivileged() =>
            TokenService.IsUserLeastPrivileged(HttpContext.Request.Headers["Authorization"]);
    }
}