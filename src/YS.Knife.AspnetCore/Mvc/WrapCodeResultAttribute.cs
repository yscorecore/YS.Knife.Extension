using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace YS.Knife.AspnetCore.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class WrapCodeResultAttribute : Attribute, IExceptionFilter, IResultFilter
    {
        private readonly ILogger<WrapCodeResultAttribute> logger;

        public WrapCodeResultAttribute(ILogger<WrapCodeResultAttribute> logger)
        {
            this.logger = logger;
        }
        public void OnException(ExceptionContext context)
        {
            var originalException = context.Exception;

            if (originalException is AggregateException aggregateException && aggregateException.InnerExceptions.Count == 1)
            {
                originalException = aggregateException.InnerExceptions[0];
            }
            else if (originalException is ValidationException validationException)
            {
                context.Result = new ObjectResult(
                    CodeResult.FromErrors(StatusCodes.Status400BadRequest.ToString(), validationException.Message, validationException.Data))
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
                context.ExceptionHandled = true;
            }
            else if (originalException.IsCodeException(out var code))
            {
                context.Result = new ObjectResult(CodeResult.FromErrors(code, originalException.Message, originalException.Data))
                ;
                context.ExceptionHandled = true;
            }
            else
            {
                context.Result = new ObjectResult(
                     CodeResult.FromErrors(StatusCodes.Status500InternalServerError.ToString(), context.Exception.Message, context.Exception.Data))
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                context.ExceptionHandled = true;
            }
            if (context.ExceptionHandled && context.Result is ObjectResult obj && obj.Value is CodeResult cr)
            {
                logger.LogError(originalException, "Exception handled to code result. Code:{code}, Message: {msg}.", cr.Code, cr.Message);
            }
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {

        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is ObjectResult obj && obj.Value is not CodeResult)
            {
                if (IsSuccessCode(obj))
                {
                    context.Result = new ObjectResult(CodeResult.FromData("0", "success", obj.Value)) { StatusCode = obj.StatusCode };
                }
                else
                {
                    context.Result = new ObjectResult(CodeResult.FromData($"{obj.StatusCode}", GetBadMessage(obj.Value), obj.Value)) { StatusCode = obj.StatusCode };

                }
            }
            else if (context.Result is EmptyResult)
            {
                context.Result = new ObjectResult(CodeResult.FromCode("0", "success"));
            }
            bool IsSuccessCode(ObjectResult obj)
            {
                return obj.StatusCode == null || obj.StatusCode >= 200 && obj.StatusCode < 300;
            }
            string GetBadMessage(object value)
            {
                if (value is ValidationProblemDetails b && b.Errors.Count > 0)
                {
                    return b.Errors.First().Value.FirstOrDefault() ?? string.Empty;
                }
                else
                {
                    return $"{value}";
                }
            }
        }



    }
}
