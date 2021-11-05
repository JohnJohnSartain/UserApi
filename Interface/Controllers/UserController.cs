using AutoWrapper.Wrappers;
using Interface.Controllers.Base;
using Interface.Controllers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SartainStudios.Entities.Entities;
using SartainStudios.SharedModels.Users;
using Services;

namespace Interface.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UserController : BaseController, IDataAccessController<UserModel>
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService) => _userService = userService;

        // GET: /User
        [HttpGet]
        [Authorize(Roles = Role.Service + "," + Role.Administrator)]
        public async Task<ActionResult<IEnumerable<UserModel>>> GetAll() => Ok(await _userService.GetAsync());

        // GET: /User/someid
        [HttpGet("{id}")]
        [Authorize(Roles = Role.Service + "," + Role.User)]
        public async Task<ActionResult<UserModel>> GetById(string id) => Ok(await UserService.GetAsync(id));

        // PUT: /User
        [HttpPut]
        [Authorize(Roles = Role.Service + "," + Role.User + "," + Role.Administrator)]
        public async Task<ApiResponse> Update(UserModel userModel) => throw new NotImplementedException();

        // PATCH: /User
        [HttpPatch]
        [Authorize(Roles = Role.Service + "," + Role.User + "," + Role.Administrator)]
        public async Task<ApiResponse> Patch(UserModel userModel)
        {
            await UserService.UpdateAsync(null, userModel);

            return new ApiResponse("User details were updated", userModel.Id);
        }

        // PATCH: /User/Password
        [HttpPatch("Password")]
        [Authorize(Roles = Role.Service + "," + Role.User + "," + Role.Administrator)]
        public async Task<ApiResponse> ChangePassword(UserModel userModel)
        {
            await UserService.ChangePassword(null, userModel);

            return new ApiResponse("User password was changed", userModel.Id);
        }

        // POST: /User
        [HttpPost]
        [AllowAnonymous]
        public async Task<ApiResponse> Create(UserModel userModel)
        {
            await UserService.CreateAsync(userModel);

            return new ApiResponse("User was created", userModel.Id, 201);
        }

        // DELETE: User/someid
        [HttpDelete("{id}")]
        [Authorize(Roles = Role.Service + "," + Role.Administrator + "," + Role.God)]
        public async Task<ApiResponse> Delete(string id)
        {
            await UserService.DeleteAsync(id);

            return new ApiResponse("User was deleted", id);
        }

        // GET: /User/Count
        [HttpGet("Count")]
        [AllowAnonymous]
        public async Task<ActionResult<int>> GetUserCount() => Ok(await UserService.UserCount());

        // POST: /User/Username/someusername
        [HttpPost("Username/{username}")]
        [Authorize(Roles = Role.Service)]
        public async Task<ActionResult<bool>> UsernameExists(string username) => Ok(await UserService.UsernameExistsAsync(username));

        // POST: /User/Credentials/Valid
        [HttpPost("Credentials/Valid")]
        [Authorize(Roles = Role.Service)]
        public async Task<ActionResult<bool>> CredentialsAreValid(UserModel model) => Ok(await UserService.CredentialsAreValidAsync(model));

        // GET: /User/Username/someusername
        [HttpGet("Username/{username}")]
        [Authorize(Roles = Role.Service)]
        public async Task<ActionResult<string>> GetUserId(string username) => Ok(await UserService.GetUserId(username));

        // GET: /User/Self/Profile
        [HttpGet("Self/Profile")]
        [Authorize(Roles = Role.User + "," + Role.Administrator)]
        public async Task<ActionResult<UserModel>> GetUserProfile() => Ok(await UserService.GetAsync(GetUserId()));
    }
}