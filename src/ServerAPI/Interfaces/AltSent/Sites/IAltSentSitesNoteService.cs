using System.Collections.Generic;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IAltSentSitesNoteService
    {
        List<AltSentProgramDetails> GetAltSentProgramDetails();
        List<KeyValuePair<int, string>> GetAltSentSiteDetails(int altSentProgramId);
        List<SiteApptDetails> GetSiteApptDetails(SiteApptParam objParam);
        Task<int> InsertSiteNote(SiteApptDetails siteNoteParam);
        Task<int> UpdateSiteNote(int siteNoteId, SiteApptDetails siteNoteParam);
        Task<int> DeleteSiteNote(int siteNoteId);
    }
}
