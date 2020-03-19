using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Audit.Core;
using Microsoft.Extensions.Configuration;
using Audit.Mvc;
using System;

namespace ServerAPI
{
    public static class AuditStartup
    {
        private const string CORRELATION_ID_FIELD = "CorrelationId";

        /// <summary>
        /// Add the global audit filter to the MVC pipeline
        /// </summary>

        /// <summary>
        /// Global Audit configuration
        /// </summary>
        /// 
        public static MvcOptions AddAudit(this MvcOptions mvcOptions)

        {



            mvcOptions.Filters.Add(new Audit.Mvc.AuditAttribute()

            {

                EventTypeName = "MVC:{verb}:{controller}:{action}",

                IncludeHeaders = false,

                IncludeModel = true,

                IncludeRequestBody = false,

                IncludeResponseBody = false,

                SerializeActionParameters=true

            });

            return mvcOptions;

        }
        public static IServiceCollection ConfigureAudit(this IServiceCollection serviceCollection, IConfiguration configuration)
        {

            Configuration.Setup()
                  .UseCustomProvider(new CustomAuditDataProvider(configuration));

            return serviceCollection;
        }


        /// <summary>
        /// Add a RequestId so the audit events can be grouped per request
        /// </summary>
        public static void UseAuditCorrelationId(this IApplicationBuilder app,
            IHttpContextAccessor ctxAccessor)
        {
            Configuration.AddCustomAction(ActionType.OnScopeCreated, scope =>
            {
                var httpContext = ctxAccessor.HttpContext;
                scope.Event.CustomFields[CORRELATION_ID_FIELD] =
                    httpContext.TraceIdentifier;


            });


            Audit.Core.Configuration.AddCustomAction(ActionType.OnEventSaving, scope =>
            {
                var mvcAction = scope.Event.GetMvcAuditAction();
                mvcAction.ActionParameters["credentials"] = "***";
            

                //...
            });


        }
        
    }
}


