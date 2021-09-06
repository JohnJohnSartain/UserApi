using Consumables;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using Sartain_Studios_Common.SharedEntities;
using SartainStudios.Cryptography;
using Services;
using SharedModels;

namespace ServicesTests;

public class UserServiceTests
{
    private const string SampleId1 = "5eba08740bdc1e00945702411";
    private const string SampleId2 = "8aty08740bdc2e00945706666";

    private static readonly UserModel UserModel1 = new() { Id = SampleId1 };
    private static readonly UserModel UserModel2 = new() { Id = SampleId2 };
    private static readonly List<UserModel> UserModels1 = new() { UserModel1, UserModel2 };

    private static readonly List<UserModel> UserModels2 = new()
    {
        new UserModel { Username = "Jim" },
        new UserModel { Id = SampleId1, Username = "John", Password = "Hashed JohnsPassword" }
    };

    private readonly Mock<IConfiguration> _configurationMock = new();
    private readonly Mock<IConfigurationSection> _configurationSectionMock = new();

    private Mock<IHash> _hasherMock;

    private Mock<IUserConsumable> _userConsumableMock;

    private UserService _userService;

    public object Times { get; private set; }

    [SetUp]
    public void Setup()
    {
        _configurationSectionMock.SetupGet(x => x.Value).Returns("defaultProfilePhoto");
        _configurationMock.Setup(a =>
                a.GetSection(It.Is<string>(s => s == "Assets:DefaultProfilePhoto")))
            .Returns(_configurationSectionMock.Object);

        _userConsumableMock = new Mock<IUserConsumable>();
        _hasherMock = new Mock<IHash>();

        _hasherMock.Setup(x => x.GenerateHash(It.IsAny<string>())).Returns("somehashedstring");

        _userService = new UserService(_userConsumableMock.Object, _hasherMock.Object, _configurationMock.Object);
    }

    [Test]
    public async Task UsernameExistsAsync_ReturnsFalse_If_NoModelExists_WithUsername()
    {
        _userConsumableMock.Setup(x => x.GetAllAsync()).Returns(Task.FromResult(UserModels1.AsEnumerable()));

        var result = await _userService.UsernameExistsAsync("Not real username");

        Assert.AreEqual(false, result);
    }

    [Test]
    public async Task UsernameExistsAsync_ReturnsTrue_If_ModelExists_WithUsername()
    {
        _userConsumableMock.Setup(x => x.GetAllAsync()).Returns(Task.FromResult(UserModels2.AsEnumerable()));

        var result = await _userService.UsernameExistsAsync("John");

        Assert.AreEqual(true, result);
    }

    [Test]
    public async Task UsernameExistsAsync_ReturnsFalseIfNoModelExistsWithUsername()
    {
        _userConsumableMock.Setup(x => x.GetAllAsync()).Returns(Task.FromResult(UserModels1.AsEnumerable()));

        var result = await _userService.UsernameExistsAsync(SampleId1);

        Assert.AreEqual(false, result);
    }

    [Test]
    public async Task UsernameExistsAsync_ReturnsTrueIfModelExistsWithUsername()
    {
        _userConsumableMock.Setup(x => x.GetAllAsync()).Returns(Task.FromResult(UserModels2.AsEnumerable()));

        var result = await _userService.UsernameExistsAsync("John");

        Assert.AreEqual(true, result);
    }

    [Test]
    public async Task AreCredentialsValidAsync_ReturnsFalseIfNoModelExistsForProvidedUsernameAndPassword()
    {
        _userConsumableMock.Setup(x => x.GetAllAsync()).Returns(Task.FromResult(UserModels1.AsEnumerable()));

        _hasherMock.Setup(x => x.GenerateHash("JohnsPassword")).Returns("Hashed JohnsPassword");

        var result = await _userService.CredentialsAreValidAsync(new UserModel
        { Username = "John", Password = "Hashed JohnsPassword" });

        Assert.AreEqual(false, result);
    }

    [Test]
    public async Task AreCredentialsValidAsync_ReturnsTrueIfModelExistsForProvidedUsernameAndPassword()
    {
        _userConsumableMock.Setup(x => x.GetAllAsync()).Returns(Task.FromResult(UserModels2.AsEnumerable()));

        _hasherMock.Setup(x => x.GenerateHash("JohnsPassword")).Returns("Hashed JohnsPassword");

        var result = await _userService.CredentialsAreValidAsync(new UserModel
        { Username = "John", Password = "JohnsPassword" });

        Assert.AreEqual(true, result);
    }

    [Test]
    public async Task GetQuantityOfUsersAsync_ReturnsQuantityOfUserModels()
    {
        _userConsumableMock.Setup(x => x.GetAllAsync()).Returns(Task.FromResult(UserModels1.AsEnumerable()));

        var result = await _userService.UserCount();

        Assert.AreEqual(2, result);
    }

    [Test]
    public async Task GetUserIdFromUsername_ThrowsKeyNotFoundExceptionIfItemDoesNotExist()
    {
        _userConsumableMock.Setup(x => x.GetAllAsync()).Returns(Task.FromResult(UserModels1.AsEnumerable()));

        Assert.ThrowsAsync<KeyNotFoundException>(async () => await _userService.GetUserId(SampleId1));
    }

    [Test]
    public async Task GetUserIdFromUsername_ReturnsUserIdWhenGivenValidUsername()
    {
        _userConsumableMock.Setup(x => x.GetAllAsync()).Returns(Task.FromResult(UserModels2.AsEnumerable()));

        var result = await _userService.GetUserId("John");

        Assert.AreEqual(SampleId1, result);
    }

    [Test]
    public async Task GetAllAsync_CallsGetAllAsyncOnceAsync()
    {
        await _userService.GetAllAsync();

        _userConsumableMock.Verify(x => x.GetAllAsync(), Moq.Times.Once());
    }

    [Test]
    public async Task GetByIdAsync_CallsGetByIdAsyncOnce()
    {
        await _userService.GetByIdAsync(SampleId1);

        _userConsumableMock.Verify(x => x.GetByIdAsync(SampleId1), Moq.Times.Once());
    }

    [Test]
    public async Task GetByIdAsync_ReturnsModelWithSpecifiedId()
    {
        _userConsumableMock.Setup(x => x.GetByIdAsync(SampleId1)).Returns(Task.FromResult(UserModel1));

        var result = await _userService.GetByIdAsync(SampleId1);

        Assert.AreEqual(SampleId1, result.Id);
    }

    #region UpdateAsync
    [Test]
    public async Task UpdateAsync_CallsGetByIdAsync_ToCheck_If_Username_Is_InUse()
    {
        const string username = "SomeUser";
        var userModel = new UserModel { Username = username, Password = "SomePassword" };
        var userModelList = new List<UserModel> { userModel };

        _userConsumableMock.Setup(x =>
            x.GetAllAsync()).Returns(Task.FromResult(userModelList.AsEnumerable()));

        _userConsumableMock.Setup(x =>
            x.GetByIdAsync(userModel.Id)).Returns(Task.FromResult(userModel));

        await _userService.UpdateAsync(null, userModel);

        _userConsumableMock.Verify(x => x.GetByIdAsync(userModel.Id), Moq.Times.Once);
    }

    [Test]
    public async Task UpdateAsync_ThrowsArgumentExceptionWithExpectedDetails_If_UsernameIs_InUse()
    {
        const string username = "SomeUser";
        var userModel = new UserModel { Username = username, Password = "SomePassword" };
        var someOtherUserModel = new UserModel { Username = "someOtherUsername", Password = "SomePassword" };
        var userModelList = new List<UserModel> { userModel };

        _userConsumableMock.Setup(x =>
            x.GetAllAsync()).Returns(Task.FromResult(userModelList.AsEnumerable()));

        _userConsumableMock.Setup(x =>
            x.GetByIdAsync(userModel.Id)).Returns(Task.FromResult(someOtherUserModel));

        var result = Assert.ThrowsAsync<ArgumentException>(async () =>
            await _userService.UpdateAsync(null, userModel));

        Assert.AreEqual("Username", result.ParamName);
        Assert.AreEqual("Username already in use (Parameter 'Username')", result.Message);
    }

    [Test]
    public async Task UpdateAsync_ThrowsArgumentExceptionWithExpectedDetails_If_Password_ContainsUserName()
    {
        const string username = "SomeUser";
        var userModel = new UserModel { Username = username, Password = "SomeUserPassword" };
        var userModelList = new List<UserModel> { userModel };

        _userConsumableMock.Setup(x =>
            x.GetAllAsync()).Returns(Task.FromResult(userModelList.AsEnumerable()));

        _userConsumableMock.Setup(x =>
            x.GetByIdAsync(userModel.Id)).Returns(Task.FromResult(userModel));

        var result = Assert.ThrowsAsync<ArgumentException>(async () =>
            await _userService.UpdateAsync(null, userModel));

        Assert.AreEqual("Password", result.ParamName);
        Assert.AreEqual("Password contains Username (Parameter 'Password')", result.Message);
    }

    [Test]
    public async Task UpdateAsync_UpdateAsync_Once()
    {
        const string username = "SomeUser";
        const string password = "SomePassword";
        const string id = "SomeId";
        var userModel = new UserModel { Id = id, Username = username, Password = "SomePassword" };
        var userModelList = new List<UserModel> { userModel };

        _userConsumableMock.Setup(x =>
            x.GetAllAsync()).Returns(Task.FromResult(userModelList.AsEnumerable()));

        _userConsumableMock.Setup(x =>
            x.GetByIdAsync(userModel.Id)).Returns(Task.FromResult(userModel));

        await _userService.UpdateAsync(null, userModel);

        _userConsumableMock.Verify(x => x.UpdateAsync(id, userModel), Moq.Times.Once());
    }
    #endregion

    #region ChangePassword
    [Test]
    public async Task ChangePassword_CallsGetByIdAsync_ToCheck_If_Username_Is_InUse()
    {
        const string username = "SomeUser";
        var userModel = new UserModel { Username = username, Password = "SomePassword" };
        var userModelList = new List<UserModel> { userModel };

        _userConsumableMock.Setup(x =>
            x.GetAllAsync()).Returns(Task.FromResult(userModelList.AsEnumerable()));

        _userConsumableMock.Setup(x =>
            x.GetByIdAsync(userModel.Id)).Returns(Task.FromResult(userModel));

        await _userService.ChangePassword(null, userModel);

        _userConsumableMock.Verify(x => x.GetByIdAsync(userModel.Id), Moq.Times.Once);
    }

    [Test]
    public async Task ChangePassword_ThrowsArgumentExceptionWithExpectedDetails_If_Password_ContainsUserName()
    {
        const string username = "SomeUser";
        var userModel = new UserModel { Username = username, Password = "SomeUserPassword" };
        var userModelList = new List<UserModel> { userModel };

        _userConsumableMock.Setup(x =>
            x.GetAllAsync()).Returns(Task.FromResult(userModelList.AsEnumerable()));

        _userConsumableMock.Setup(x =>
            x.GetByIdAsync(userModel.Id)).Returns(Task.FromResult(userModel));

        var result = Assert.ThrowsAsync<ArgumentException>(async () =>
            await _userService.ChangePassword(null, userModel));

        Assert.AreEqual("Password", result.ParamName);
        Assert.AreEqual("Password contains Username (Parameter 'Password')", result.Message);
    }

    [Test]
    public async Task ChangePassword_CallsGenerateHashOnce_With_ExpectedParameter()
    {
        const string username = "SomeUser";
        const string password = "SomePassword";
        var userModel = new UserModel { Username = username, Password = "SomePassword" };
        var userModelList = new List<UserModel> { userModel };

        _userConsumableMock.Setup(x =>
            x.GetAllAsync()).Returns(Task.FromResult(userModelList.AsEnumerable()));

        _userConsumableMock.Setup(x =>
            x.GetByIdAsync(userModel.Id)).Returns(Task.FromResult(userModel));

        await _userService.ChangePassword(null, userModel);

        _hasherMock.Verify(x => x.GenerateHash(password), Moq.Times.Once);
    }

    [Test]
    public async Task ChangePassword_UpdateAsync_Once()
    {
        const string username = "SomeUser";
        const string password = "SomePassword";
        const string id = "SomeId";
        var userModel = new UserModel { Id = id, Username = username, Password = "SomePassword" };
        var userModelList = new List<UserModel> { userModel };

        _userConsumableMock.Setup(x =>
            x.GetAllAsync()).Returns(Task.FromResult(userModelList.AsEnumerable()));

        _userConsumableMock.Setup(x =>
            x.GetByIdAsync(userModel.Id)).Returns(Task.FromResult(userModel));

        await _userService.ChangePassword(null, userModel);

        _userConsumableMock.Verify(x => x.UpdateAsync(id, userModel), Moq.Times.Once());
    }
    #endregion

    [Test]
    public async Task CreateAsync_ThrowsArgumentExceptionWithExpectedDetails_If_UsernameIs_InUse()
    {
        const string username = "SomeUser";
        var userModel = new UserModel { Username = username, Password = "SomePassword" };
        var someOtherUserModel = new UserModel { Username = "someOtherUsername", Password = "SomePassword" };
        var userModelList = new List<UserModel> { userModel };

        _userConsumableMock.Setup(x =>
            x.GetAllAsync()).Returns(Task.FromResult(userModelList.AsEnumerable()));

        _userConsumableMock.Setup(x =>
            x.GetByIdAsync(userModel.Id)).Returns(Task.FromResult(someOtherUserModel));

        var result = Assert.ThrowsAsync<ArgumentException>(async () =>
            await _userService.CreateAsync(userModel));

        Assert.AreEqual("Username", result.ParamName);
        Assert.AreEqual("Username already in use (Parameter 'Username')", result.Message);
    }

    [Test]
    public async Task CreateAsync_ThrowsArgumentExceptionWithExpectedDetails_If_Password_ContainsUserName()
    {
        const string username = "SomeUser";
        var userModel = new UserModel { Username = username, Password = "SomeUserPassword" };
        var userModelList = new List<UserModel> { userModel };

        _userConsumableMock.Setup(x =>
            x.GetAllAsync()).Returns(Task.FromResult(new List<UserModel>().AsEnumerable()));

        _userConsumableMock.Setup(x =>
            x.GetByIdAsync(userModel.Id)).Returns(Task.FromResult(userModel));

        var result = Assert.ThrowsAsync<ArgumentException>(async () =>
            await _userService.CreateAsync(userModel));

        Assert.AreEqual("Password", result.ParamName);
        Assert.AreEqual("Password contains Username (Parameter 'Password')", result.Message);
    }

    [Test]
    public async Task CreateAsync_CallsGenerateHashOnce_With_ExpectedParameter()
    {
        const string username = "SomeUser";
        const string password = "SomePassword";
        var userModel = new UserModel { Username = username, Password = "SomePassword" };
        var userModelList = new List<UserModel> { userModel };

        _userConsumableMock.Setup(x =>
            x.GetAllAsync()).Returns(Task.FromResult(new List<UserModel>().AsEnumerable()));

        _userConsumableMock.Setup(x =>
            x.GetByIdAsync(userModel.Id)).Returns(Task.FromResult(userModel));

        await _userService.CreateAsync(userModel);

        _hasherMock.Verify(x => x.GenerateHash(password), Moq.Times.Once);
    }

    [Test]
    public async Task CreateAsync_CallsCreateAsyncWithExpectedParameters()
    {
        const string username = "SomeUser";
        const string password = "SomePassword";
        var userModel = new UserModel { Username = username, Password = "SomePassword", ProfilePhoto = "photo" };

        _userConsumableMock.Setup(x =>
            x.GetAllAsync()).Returns(Task.FromResult(new List<UserModel>().AsEnumerable()));

        _userConsumableMock.Setup(x =>
            x.GetByIdAsync(userModel.Id)).Returns(Task.FromResult(userModel));

        await _userService.CreateAsync(userModel);

        _userConsumableMock.Verify(x =>
            x.CreateAsync(It.Is<UserModel>(x =>
                x.Password.Equals("somehashedstring")
                && LessThanOneSecondsHavePassed(x.Created)
                && x.Roles.Any(role => role.Equals(Role.User))
                && x.ProfilePhoto.Equals("photo"))));
    }

    private static bool LessThanOneSecondsHavePassed(DateTime? originalDate) =>
        (DateTime.Now - originalDate ??
         throw new ArgumentException("Original Date Was Null", nameof(originalDate), null))
        .TotalSeconds < 1;

    [Test]
    public async Task CreateAsync_CallsCreateAsync_WithDefaultPhoto_If_NoPhoto_WasProvided()
    {
        const string username = "SomeUser";
        const string password = "SomePassword";
        var userModel = new UserModel { Username = username, Password = "SomePassword" };

        _userConsumableMock.Setup(x =>
            x.GetAllAsync()).Returns(Task.FromResult(new List<UserModel>().AsEnumerable()));

        _userConsumableMock.Setup(x =>
            x.GetByIdAsync(userModel.Id)).Returns(Task.FromResult(userModel));

        await _userService.CreateAsync(userModel);

        _userConsumableMock.Verify(x =>
            x.CreateAsync(It.Is<UserModel>(x => x.ProfilePhoto.Equals("defaultProfilePhoto"))));
    }

    [Test]
    public async Task DeleteAsync()
    {
        await _userService.DeleteAsync(SampleId1);

        _userConsumableMock.Verify(x => x.DeleteAsync(SampleId1), Moq.Times.Once());
    }
}