using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace DDRK.LiveTV
{
    public class ExceptionFilter : IActionFilter, IOrderedFilter
    {
        private readonly ILogger<ExceptionFilter> _logger;

        public ExceptionFilter(ILogger<ExceptionFilter> logger)
        {
            _logger = logger;
        }

        public int Order { get; } = int.MaxValue - 10;

        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception != null)
            {
                if (context.Result == null)
                {
                    context.Result = new ObjectResult("处理请求的过程中发生异常。")
                    {
                        StatusCode = 500
                    };
                }

                if (!context.ExceptionHandled)
                {
                    _logger.LogException(context.Exception, "Error occurred during processing action \"{action}\".", context.ActionDescriptor.DisplayName);
                    context.ExceptionHandled = true;
                }
            }
        }
    }
}
