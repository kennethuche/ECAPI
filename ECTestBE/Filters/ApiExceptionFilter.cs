using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using ECTest.Service.UserException;

namespace ECTestBE.Filters
{
    public class ApiExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            if (exception != null)
            {
                if (exception is CustomValidationException)
                {
                    var loggerFactory = context.HttpContext.RequestServices.GetService<ILoggerFactory>();
                    if (loggerFactory != null)
                    {
                        var logMessage = $"Error when processing request. QueryString: {context.HttpContext.Request.QueryString}";
                        loggerFactory.CreateLogger("ExceptionHandler").LogError(exception, logMessage);
                    }

                    context.Result = new ConflictObjectResult(new { ErrorMessage = context.Exception.Message });
                }
                else
                {
                    var loggerFactory = context.HttpContext.RequestServices.GetService<ILoggerFactory>();
                    if (loggerFactory != null)
                    {
                        var logMessage = $"Error when processing request. QueryString: {context.HttpContext.Request.QueryString}";
                        loggerFactory.CreateLogger("ExceptionHandler").LogError(exception, logMessage);
                    }

                    context.Result = new ObjectResult(exception.Message)
                    {
                        StatusCode = 500
                    };
                }

                context.ExceptionHandled = true;
            }
        }
    }
}
