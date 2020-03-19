using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using Microsoft.Extensions.Configuration;

namespace ServerAPI.Services
{
    public class AppletsSavedService : IAppletsSavedService
    {
        private readonly AAtims _context;
        private IConfiguration _configuration { get; }
        private readonly string _externalPath;
        private readonly string _path;

        public AppletsSavedService(AAtims context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _externalPath = _configuration.GetValue<string>("SiteVariables:ClientExternalAppletsPath");
            _path = _configuration.GetValue<string>("SiteVariables:AtimsExternalAppletsPath");
        }

        public string GetExternalPath()
        {
            return _externalPath;
        }

        public string GetPath()
        {
            return _path;
        }
    }
}
