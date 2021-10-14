using Consumables;
using Microsoft.Extensions.Configuration;
using SartainStudios.Cryptography;
using SartainStudios.DatabaseInteraction.Consumables;
using SartainStudios.Entities.Entities;
using SartainStudios.SharedModels.Users;

namespace Services;

public interface IUserService : IBaseNonSpecificUserConsumable<UserModel>
{
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> CredentialsAreValidAsync(UserModel userModel);
    Task<int> UserCount();
    Task<string> GetUserId(string username);
    Task ChangePassword(string _, UserModel userModel);
}

public class UserService : IUserService
{
    private readonly IConfiguration _configuration;
    private readonly IHash _hash;
    private readonly IUserConsumable _userConsumable;

    public UserService(IUserConsumable userConsumable, IHash hash, IConfiguration configuration)
    {
        _userConsumable = userConsumable;
        _hash = hash;
        _configuration = configuration;
    }

    public async Task<bool> UsernameExistsAsync(string username) =>
        (await GetAsync()).Any(x => x.Username != null && x.Username.Equals(username));

    public async Task<bool> CredentialsAreValidAsync(UserModel userCredentials)
    {
        var userModels = await GetAsync();

        userCredentials.Password = _hash.GenerateHash(userCredentials.Password);

        return userModels.Any(userModel =>
            CredentialsMatch(userCredentials.Username, userModel.Username)
            && CredentialsMatch(userCredentials.Password, userModel.Password));
    }

    public async Task<int> UserCount() => (await GetAsync()).Count();

    public async Task<string> GetUserId(string username)
    {
        var userModels = await GetAsync();

        if (await UsernameExistsAsync(username))
        {
            var userModel = userModels.Where(x => x.Username != null && x.Username.Equals(username))
                .Select(x => x)
                .First();
            return userModel.Id;
        }

        throw new KeyNotFoundException($"User with username: {username} not found");
    }

    public async Task<IEnumerable<UserModel>> GetAsync() => (await _userConsumable.GetAsync()).ToList();

    public async Task<UserModel> GetAsync(string userId) => await _userConsumable.GetAsync(userId);

    public async Task UpdateAsync(string _, UserModel userModel)
    {
        var originalUserModel = await GetAsync(userModel.Id);

        if (!userModel.Username.ToLower().Equals((originalUserModel).Username.ToLower()))
            await ValidateUsername(userModel);
        await ValidatePassword(userModel);

        userModel.Password = originalUserModel.Password;

        await _userConsumable.UpdateAsync(userModel.Id, userModel);
    }

    public async Task ChangePassword(string _, UserModel userModel)
    {
        var originalUserModel = await GetAsync(userModel.Id);

        await ValidatePassword(userModel);

        originalUserModel.Password = HashPassword(userModel.Password);

        await _userConsumable.UpdateAsync(userModel.Id, originalUserModel);
    }

    public async Task<string> CreateAsync(UserModel userModel)
    {
        await ValidateUsername(userModel);
        await ValidatePassword(userModel);

        userModel.Password = HashPassword(userModel.Password);
        userModel.Created = DateTime.UtcNow;
        userModel.Roles = new string[] { Role.User };
        userModel.ProfilePhoto = !string.IsNullOrEmpty(userModel.ProfilePhoto)
            ? userModel.ProfilePhoto
            : GetDefaultProfilePhoto();

        return await _userConsumable.CreateAsync(userModel);
    }

    public async Task DeleteAsync(string id) => await _userConsumable.DeleteAsync(id);

    private static bool CredentialsMatch(string credential1, string credential2) =>
        credential1 != null && credential2 != null && credential1.Equals(credential2);

    private async Task ValidateUsername(UserModel userModel)
    {
        if (await UsernameExistsAsync(userModel.Username))
            throw new ArgumentException("Username already in use", nameof(userModel.Username));
    }

    private async Task ValidatePassword(UserModel userModel)
    {
        if (userModel.Password.ToLower().Contains(userModel.Username.ToLower()))
            throw new ArgumentException("Password contains Username", nameof(userModel.Password));
    }

    private string HashPassword(string password) => _hash.GenerateHash(password);

    private string GetDefaultProfilePhoto() => _configuration.GetSection("Assets:DefaultProfilePhoto").Value;
}