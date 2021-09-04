using System;
using AutoWrapper.Extensions;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Interface.Filters
{
    public class ArgumentExceptionHandlerAttribute : IExceptionFilter
    {
        public void OnException(ExceptionContext exceptionContext)
        {
            if (exceptionContext.ExceptionHandled) return;
            if (exceptionContext.Exception is not ArgumentException exception) return;
            if (exception.ParamName == null) throw new ApiException(exception);

            var modelState = exceptionContext.ModelState;

            modelState.AddModelError(exception.ParamName, GetExceptionMessageWithoutParameterName(exception));
            throw new ApiException(modelState.AllErrors());
        }

        private static string GetExceptionMessageWithoutParameterName(ArgumentException exception) =>
            exception.Message.Replace($" (Parameter '{exception.ParamName}')", "");
    }
}