namespace AdministrationServer.Filters;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

public class ExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is ServerException)
        {
            context.Result = new ObjectResult($"Server error: {context.Exception.Message}")
            {
                StatusCode = 500 
            };
        }
        else if (context.Exception is Exception)
        {
            context.Result = new ObjectResult($"Server error: {context.Exception.Message}")
            {
                StatusCode = 500
            };
        }
        context.ExceptionHandled = true;
    }
}
