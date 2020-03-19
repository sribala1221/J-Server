using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
   public interface ISitesService
    {
        ProgramAndSite LoadPrimaryDetails();
        List<SiteScheduleDetails> GetScheduleDetails(int programId, int? day, int? siteId);


    }
}
