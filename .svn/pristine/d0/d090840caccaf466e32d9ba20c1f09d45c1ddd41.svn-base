using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace ServerAPI.Filters
{
    public class ErrorLogFilter : IExceptionFilter
    {
        private readonly ILogger<ErrorLogFilter> _logger;

        public ErrorLogFilter(ILogger<ErrorLogFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError("exception happened");
        }
    }
}
