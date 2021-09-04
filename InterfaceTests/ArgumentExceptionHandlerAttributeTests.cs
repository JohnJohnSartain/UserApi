using AutoWrapper.Wrappers;
using Interface.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;

namespace InterfaceTests;

public class ArgumentExceptionHandlerAttributeTests
{
    private ActionContext _actionContext;
    private Mock<ArgumentException> _argumentExceptionMock;
    private ExceptionContext _exceptionContext;
    private ArgumentExceptionHandlerAttribute _systemUnderTest;

    [SetUp]
    public void Setup()
    {
        _actionContext = new ActionContext
        {
            HttpContext = new DefaultHttpContext(),
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor()
        };

        _argumentExceptionMock = new Mock<ArgumentException>();

        _argumentExceptionMock.Setup(e => e.Message).Returns("Reason for exception");
        _argumentExceptionMock.Setup(e => e.ParamName).Returns("NameOfProperty");

        _exceptionContext = new ExceptionContext(_actionContext, new List<IFilterMetadata>())
        { Exception = _argumentExceptionMock.Object };

        _systemUnderTest = new ArgumentExceptionHandlerAttribute();
    }

    [Test]
    public void OnException_DoNothing_If_ExceptionIsAlreadyHandled()
    {
        _exceptionContext.ExceptionHandled = true;

        _systemUnderTest.OnException(_exceptionContext);
    }

    [Test]
    public void OnException_DoNothing_If_Exception_IsNot_ArgumentException()
    {
        _exceptionContext.ExceptionHandled = false;
        _exceptionContext.Exception = new InvalidCastException();

        _systemUnderTest.OnException(_exceptionContext);
    }

    [Test]
    public void OnException_ThrowApiException_If_ExceptionParameterName_Is_Null()
    {
        _exceptionContext.ExceptionHandled = false;
        _exceptionContext.Exception = new ArgumentException();

        Assert.Throws<ApiException>(() => _systemUnderTest.OnException(_exceptionContext));
    }

    [Test]
    public void OnException_ThrowApiException_If_ExceptionParameterName_Is_Null_EnsureCorrectMessage()
    {
        var originalException = new ArgumentException("Test Exception");

        _exceptionContext.ExceptionHandled = false;
        _exceptionContext.Exception = originalException;

        var exception = Assert.Throws<ApiException>(() => _systemUnderTest.OnException(_exceptionContext));

        Assert.AreEqual(originalException.Message, exception.Message);
    }

    [Test]
    public void OnException_ThrowApiException_If_ExceptionParameterName_IsNot_Null_EnsureCorrectErrorCount()
    {
        var exception = Assert.Throws<ApiException>(() => _systemUnderTest.OnException(_exceptionContext));

        Assert.AreEqual(1, exception.Errors.Count());
    }

    [Test]
    public void OnException_ThrowApiException_If_ExceptionParameterName_IsNot_Null_EnsureCorrectNameError()
    {
        var exception = Assert.Throws<ApiException>(() => _systemUnderTest.OnException(_exceptionContext));

        Assert.AreEqual("NameOfProperty", exception.Errors.First().Name);
    }

    [Test]
    public void OnException_ThrowApiException_If_ExceptionParameterName_IsNot_Null_EnsureCorrectReasonError()
    {
        var exception = Assert.Throws<ApiException>(() => _systemUnderTest.OnException(_exceptionContext));

        Assert.AreEqual("Reason for exception", exception.Errors.First().Reason);
    }

    [Test]
    public void OnException_ThrowApiException_If_ExceptionParameterName_IsNot_Null_EnsureCorrectStatusCode()
    {
        var exception = Assert.Throws<ApiException>(() => _systemUnderTest.OnException(_exceptionContext));

        Assert.AreEqual(400, exception.StatusCode);
    }

    [Test]
    public void OnException_ThrowApiException_If_ExceptionParameterName_IsNot_Null_EnsureIsValidationError()
    {
        var exception = Assert.Throws<ApiException>(() => _systemUnderTest.OnException(_exceptionContext));

        Assert.AreEqual(true, exception.IsModelValidatonError);
    }
}