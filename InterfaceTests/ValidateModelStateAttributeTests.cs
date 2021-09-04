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

public class ValidateModelStateAttributeTests
{
    private ActionContext _actionContext;
    private ActionExecutingContext _actionExecutingContext;
    private ValidateModelStateAttribute _systemUnderTest;

    [SetUp]
    public void Setup()
    {
        _actionContext = new ActionContext
        {
            HttpContext = new DefaultHttpContext(),
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor()
        };

        _actionExecutingContext = new ActionExecutingContext(_actionContext, new List<IFilterMetadata>(),
            new Mock<IDictionary<string, object>>().Object, null);

        _systemUnderTest = new ValidateModelStateAttribute();
    }

    [Test]
    public void OnActionExecuting_DoesNotThrowException_If_ModelState_IsValid()
    {
        _systemUnderTest.OnActionExecuting(_actionExecutingContext);
    }

    [Test]
    public void OnActionExecuting_DoesTrowApiException_If_ModelState_IsInvalid()
    {
        _actionContext.ModelState.AddModelError("SomeField", "Some issue with SomeField");

        Assert.Throws<ApiException>(() => _systemUnderTest.OnActionExecuting(_actionExecutingContext));
    }

    [Test]
    public void OnActionExecuting_DoesTrowApiExceptionWithExpectedErrorCount_If_ModelState_IsInvalid()
    {
        _actionContext.ModelState.AddModelError("SomeField", "Some issue with SomeField");

        var exception =
            Assert.Throws<ApiException>(() => _systemUnderTest.OnActionExecuting(_actionExecutingContext));

        Assert.AreEqual(1, exception.Errors.Count());
    }

    [Test]
    public void OnActionExecuting_DoesTrowApiExceptionWithExpectedErrorValidationName_If_ModelState_IsInvalid()
    {
        _actionContext.ModelState.AddModelError("SomeField", "Some issue with SomeField");

        var exception =
            Assert.Throws<ApiException>(() => _systemUnderTest.OnActionExecuting(_actionExecutingContext));

        Assert.AreEqual("SomeField", exception.Errors.First().Name);
    }

    [Test]
    public void OnActionExecuting_DoesTrowApiExceptionWithExpectedErrorValidationReason_If_ModelState_IsInvalid()
    {
        _actionContext.ModelState.AddModelError("SomeField", "Some issue with SomeField");

        var exception =
            Assert.Throws<ApiException>(() => _systemUnderTest.OnActionExecuting(_actionExecutingContext));

        Assert.AreEqual("Some issue with SomeField", exception.Errors.First().Reason);
    }
}