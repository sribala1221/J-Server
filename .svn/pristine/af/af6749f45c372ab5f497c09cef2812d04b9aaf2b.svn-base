using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using Microsoft.Extensions.Configuration;

namespace ServerAPI.Services
{
    public class PhotosService : IPhotosService
    {
        private readonly AAtims _context;
        private IConfiguration Configuration { get; }
        private readonly string _externalPath;

        private readonly string _path;
        private readonly string _jmsPath;

        private readonly string _pbpcPath;
        //Get Identifier image from PhotoPath if relative path is null
        public PhotosService(AAtims context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
            _externalPath = Configuration.GetValue<string>("SiteVariables:ClientExternalPhotoPath");
            _path = Configuration.GetValue<string>("SiteVariables:AtimsPhotoPath");
            _jmsPath = Configuration.GetValue<string>("SiteVariables:JMSUrl");
            _pbpcPath = Configuration.GetValue<string>("SiteVariables:PBPCUrl");
        }

        public string GetExternalPath()
        {
            return _externalPath;
        }

        public string GetPath()
        {
            return _path;
        }
        public string GetJmsPath()
        {
            return _jmsPath;
        }

        public string GetPbpcPath()
        {
            return _pbpcPath;
        }

        public string GetPhotoByPersonId(int personId) => _context.Identifiers.Where(w =>
                w.PersonId == personId && w.DeleteFlag == 0 && !string.IsNullOrEmpty(w.IdentifierType) &&
                w.IdentifierType.Trim() == "1")
            .OrderByDescending(o => o.IdentifierId)
            .Select(s => s.PhotographRelativePath == null ? _externalPath + s.PhotographPath : 
                _path + s.PhotographRelativePath).FirstOrDefault();

        public string GetPhotoByPerson(Person person) => person.Identifiers.Where(w =>
            w.DeleteFlag == 0 && !string.IsNullOrEmpty(w.IdentifierType) &&
            w.IdentifierType.Trim() == "1")
            .OrderByDescending(o => o.IdentifierId)
            .Select(s => s.PhotographRelativePath == null ? _externalPath + s.PhotographPath : 
                _path + s.PhotographRelativePath).FirstOrDefault();

        public string GetPhotoByIdentifierId(int identifierId) => _context.Identifiers.Where(w => 
            w.IdentifierId == identifierId)
            .Select(s => s.PhotographRelativePath == null ? _externalPath + s.PhotographPath : _path + s.PhotographRelativePath)
            .FirstOrDefault();

        public string GetPhotoByIdentifier(Identifiers identifier) =>
            identifier != null
                ? identifier.PhotographRelativePath == null
                    ? _externalPath + identifier.PhotographPath
                    : _path + identifier.PhotographRelativePath
                : "";

        public string GetPhotoByCollectionIdentifier(ICollection<Identifiers> identifiers) => identifiers.Where(w =>
            w.DeleteFlag == 0 && !string.IsNullOrEmpty(w.IdentifierType) &&
            w.IdentifierType.Trim() == "1")
            .OrderByDescending(o => o.IdentifierId)
            .Select(s => s.PhotographRelativePath == null ? _externalPath + s.PhotographPath : 
                _path + s.PhotographRelativePath).FirstOrDefault();
    }
}
