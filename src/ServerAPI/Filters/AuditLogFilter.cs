using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace ServerAPI.Filters
{
    public class AuditLogFilter : IActionFilter
    {
        private readonly ILogger<AuditLogFilter> _logger;

        public AuditLogFilter(ILogger<AuditLogFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            //_logger.LogError("action executing");
            _logger.LogWarning("action executing information");
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            //_logger.LogError("action executed");
            _logger.LogWarning("action executed information");
        }
    }
}
