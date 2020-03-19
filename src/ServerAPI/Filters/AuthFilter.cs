using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace ServerAPI.Filters
{
    public class AuthFilter: IAsyncAuthorizationFilter
    {
        private readonly ILogger<AuthFilter> _logger;

        public AuthFilter(ILogger<AuthFilter> logger)
        {
            _logger = logger;
        }

        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            throw new NotImplementedException();
        }
    }
}
