using System;
using System.IO;
using Audit.Core;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace ServerAPI
{
    public class CustomAuditDataProvider : AuditDataProvider
    {
        public CustomAuditDataProvider(IConfiguration configuration) => _configuration = configuration;

        private readonly IConfiguration _configuration;

        public override object InsertEvent(AuditEvent auditEvent)
        {
            string auditLogDirectoryValue = _configuration.GetValue<string>("SiteVariables:AuditLogPath");

           // AuditEvent provides a ToJson() method
           string fileName = Directory.CreateDirectory(path: auditLogDirectoryValue)
                              .CreateSubdirectory(DateTime.Now.ToString("yyyy"))
                              .CreateSubdirectory(DateTime.Now.ToString("MM"))
                              .CreateSubdirectory(DateTime.Now.Day.ToString())
                             + $"/{DateTime.Now.Hour}.json";

            using (Serilog.Core.Logger log = new LoggerConfiguration()
                .WriteTo.File(fileName).CreateLogger())
            {
                log.Information(auditEvent.ToJson());
            }
            return fileName;
        }

        // Replaces an existing event given the ID and the event
        public override void ReplaceEvent(object eventId, AuditEvent auditEvent)
        {
            // Override an existing event
            string fileName = eventId.ToString();
            File.WriteAllText(fileName, auditEvent.ToJson());
        }
    }
}
