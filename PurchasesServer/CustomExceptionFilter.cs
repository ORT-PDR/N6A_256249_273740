using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace PurchasesServer
{
    public class CustomExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            
            context.Result = new ObjectResult($"Error: {exception.Message}")
            {
                StatusCode = 500 
            };
            
            context.ExceptionHandled = true;
        }
    }
}