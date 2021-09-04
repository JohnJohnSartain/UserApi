using Consumables;
using Contexts;
using Moq;
using NUnit.Framework;
using SharedModels;

namespace ConsumablesTests;
public class Tests
{
    private UserConsumable _systemUnderTest;
    private Mock<IUserContext> _userContextMock;

    [SetUp]
    public void Setup()
    {
        _userContextMock = new Mock<IUserContext>();

        _systemUnderTest = new UserConsumable(_userContextMock.Object);
    }

    [Test]
    public async Task GetAllAsync_Calls_GetAllAsyncOnceAsync()
    {
        await _systemUnderTest.GetAllAsync();

        _userContextMock.Verify(x => x.GetAllAsync(), Times.Once());
    }

    [Test]
    public async Task GetByIdAsync_Calls_GetByIdAsyncOnceAsync()
    {
        await _systemUnderTest.GetByIdAsync("random string");

        _userContextMock.Verify(x => x.GetByIdAsync(It.IsAny<string>()), Times.Once());
    }

    [Test]
    public async Task GetByIdAsync_Calls_GetByIdAsyncWithExpectedParameterAsync()
    {
        const string someId = "someId";

        await _systemUnderTest.GetByIdAsync(someId);

        _userContextMock.Verify(x => x.GetByIdAsync(It.Is<string>(x => x.Equals(someId))), Times.Once());
    }

    [Test]
    public async Task UpdateAsync_Calls_UpdateAsyncOnceAsync()
    {
        await _systemUnderTest.UpdateAsync("random string", new UserModel());

        _userContextMock.Verify(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<UserModel>()), Times.Once());
    }

    [Test]
    public async Task UpdateAsync_Calls_UpdateAsyncWithExpectedParametersAsync()
    {
        const string someId = "someId";
        const string sampleUserName = "sampleUsername";
        const string sampleUserPassword = "samplePassword";
        var userModel = new UserModel
        {
            Username = sampleUserName,
            Password = sampleUserPassword
        };

        await _systemUnderTest.UpdateAsync(someId, userModel);

        _userContextMock.Verify(
            x => x.UpdateAsync(
                It.Is<string>(id => id.Equals(someId)),
                It.Is<UserModel>(model =>
                    model.Username.Equals(sampleUserName) && model.Password.Equals(sampleUserPassword))),
            Times.Once());
    }

    [Test]
    public async Task CreateAsync_Calls_CreateAsyncOnceAsync()
    {
        await _systemUnderTest.CreateAsync(new UserModel());

        _userContextMock.Verify(x => x.CreateAsync(It.IsAny<UserModel>()), Times.Once());
    }

    [Test]
    public async Task CreateAsync_Calls_CreateAsyncWithExpectedParametersAsync()
    {
        const string sampleUserName = "sampleUsername";
        const string sampleUserPassword = "samplePassword";
        var userModel = new UserModel
        {
            Username = sampleUserName,
            Password = sampleUserPassword
        };

        await _systemUnderTest.CreateAsync(userModel);

        _userContextMock.Verify(
            x => x.CreateAsync(It.Is<UserModel>(userModel =>
                userModel.Username.Equals(sampleUserName) && userModel.Password.Equals(sampleUserPassword))),
            Times.Once());
    }

    [Test]
    public async Task DeleteAsync_Calls_DeleteAsyncOnceAsync()
    {
        await _systemUnderTest.DeleteAsync("random string");

        _userContextMock.Verify(x => x.DeleteAsync(It.IsAny<string>()), Times.Once());
    }

    [Test]
    public async Task DeleteAsync_Calls_DeleteAsyncWithExpectedParameterAsync()
    {
        const string someId = "someId";

        await _systemUnderTest.DeleteAsync(someId);

        _userContextMock.Verify(x => x.DeleteAsync(It.Is<string>(x => x.Equals(someId))), Times.Once());
    }
}