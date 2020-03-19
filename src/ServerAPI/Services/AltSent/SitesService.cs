using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public class SitesService:ISitesService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;

        public SitesService(AAtims context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
        }

        public ProgramAndSite LoadPrimaryDetails()
        {
            ProgramAndSite programAndSite = new ProgramAndSite
            {
                ProgramLst = _context.AltSentProgram.Where(p => p.InactiveFlag != 1 && p.Facility.AltSentFlag == 1)
                .Select(s => new PrimaryProgram
                {
                    AltsentProgramId = s.AltSentProgramId,
                    FacilityAbbr = s.Facility.FacilityAbbr,
                    AltsentProgramabbr = s.AltSentProgramAbbr
                }).OrderBy(o => o.AltsentProgramId).ToList(),

                SitesLst=_context.AltSentSite.Where(a=>a.InactiveFlag!=1)
                .Select(s=> new AltSentSites {
                   ProgramId=s.AltSentProgramId,
                   SiteId=s.AltSentSiteId,
                   SiteName=s.AltSentSiteName
                }).ToList()
            };
            return programAndSite;
        }


        public List<SiteScheduleDetails> GetScheduleDetails(int programId,int? day,int? siteId)
        {
            List<SiteScheduleDetails> siteScheduleDetails = new List<SiteScheduleDetails>();
            siteScheduleDetails = _context.AltSentSiteSchd.Where(a => a.InactiveFlag != 1 && a.AltSentSite.InactiveFlag != 1 
            && a.AltSentSite.AltSentProgramId== programId &&(!day.HasValue ||
                                a.AltSentSiteSchdDayOfWeek == day) && (!siteId.HasValue || a.AltSentSite.AltSentSiteId== siteId))
                .Select(s => new SiteScheduleDetails
                {
                    SiteSchdId=s.AltSentSiteSchdId,
                    SiteName=s.AltSentSite.AltSentSiteName,
                    SiteSchdDayOfWeek=s.AltSentSiteSchdDayOfWeek,
                    SiteSchdTimeFrom=s.AltSentSiteSchdTimeFrom,
                    SiteSchdTimeThru=s.AltSentSiteSchdTimeThru,
                    SiteId=s.AltSentSite.AltSentSiteId,
                    SiteSchdCapacity=s.AltSentSiteSchdCapacity,
                    SiteSchdDescription=s.AltSentSiteSchdDescription
                }).ToList();

            IQueryable<AltSent> dbAltSents = _context.AltSent;

            siteScheduleDetails.ForEach(ss =>
            {
                ss.ASNCountOld = dbAltSents.Count(c => c.PrimaryAltSentSiteId == ss.SiteId);

                switch (ss.SiteSchdDayOfWeek)
                {
                    case 1:
                        ss.ASNCount = dbAltSents.Count(c => c.DefaultSunAltSentSiteAssignId == ss.SiteSchdId && c.PrimaryAltSentSiteId == ss.SiteId 
                        && c.AltSentClearFlag!=1);
                        break;
                    case 2:
                        ss.ASNCount = dbAltSents.Count(c => c.DefaultMonAltSentSiteAssignId == ss.SiteSchdId && c.PrimaryAltSentSiteId == ss.SiteId
                        && c.AltSentClearFlag != 1);
                        break;
                    case 3:
                        ss.ASNCount = dbAltSents.Count(c => c.DefaultTueAltSentSiteAssignId == ss.SiteSchdId && c.PrimaryAltSentSiteId == ss.SiteId
                        && c.AltSentClearFlag != 1);
                        break;
                    case 4:
                        ss.ASNCount = dbAltSents.Count(c => c.DefaultWedAltSentSiteAssignId == ss.SiteSchdId && c.PrimaryAltSentSiteId == ss.SiteId
                        && c.AltSentClearFlag != 1);
                        break;
                    case 5:
                        ss.ASNCount = dbAltSents.Count(c => c.DefaultThuAltSentSiteAssignId == ss.SiteSchdId && c.PrimaryAltSentSiteId == ss.SiteId
                        && c.AltSentClearFlag != 1);
                        break;
                    case 6:
                        ss.ASNCount = dbAltSents.Count(c => c.DefaultFriAltSentSiteAssignId == ss.SiteSchdId && c.PrimaryAltSentSiteId == ss.SiteId
                        && c.AltSentClearFlag != 1);
                        break;
                    case 7:
                        ss.ASNCount = dbAltSents.Count(c => c.DefaultSatAltSentSiteAssignId == ss.SiteSchdId && c.PrimaryAltSentSiteId == ss.SiteId
                        && c.AltSentClearFlag != 1);
                        break;
                }

            });


            return siteScheduleDetails;
        }

        public List<SiteAssignedInmates> GetAssignedInmate(int siteId,int siteSchdId)
        {
            List<SiteAssignedInmates> assignedInmates = new List<SiteAssignedInmates>();

            assignedInmates = _context.AltSent.Where(a => a.PrimaryAltSentSiteId == siteId)
                .Select(s => new SiteAssignedInmates
                {
                    InmateNumber=s.Incarceration.Inmate.InmateNumber,
                    InmateId=s.Incarceration.Inmate.InmateId,
                    IncarcerationId=s.IncarcerationId??0,
                    AltsentId=s.AltSentId,
                    Availablesun=s.AvailableSun,
                    AvailableMon=s.AvailableMon,
                    AvailableTue=s.AvailableTue,
                    AvailableWed=s.AvailableWed,
                    AvailableThu=s.AvailableThu,
                    AvailableFri=s.AvailableFri,
                    AvailableSat=s.AvailableSat,
                    AltsentSiteId=s.PrimaryAltSentSiteId,
                    AltsentStart=s.AltSentStart,
                    AltsentProgramId=s.AltSentProgramId,
                    ProgramAbbr=s.AltSentProgram.AltSentProgramAbbr,
                    FacilityAbbr=s.AltSentProgram.Facility.FacilityAbbr,
                    AltsentAdts=s.AltSentAdts,
                    TotalAttend=s.AltSentTotalAttend,
                    Totalowed=s.AltSentTotalOwed,
                    TotalCollected=s.TotalCollected,
                    TotalBalance=s.TotalBalance
                }).ToList();
            return assignedInmates;
        }

        public async Task<int> UpdatePrimarySites(PrimarySite site)
        {
            AltSent dbAltSent = _context.AltSent.Where(a => a.AltSentId == site.AltsentId).SingleOrDefault();
            if(dbAltSent != null)
            {
                dbAltSent.AvailableSun = site.Availablesun;
                dbAltSent.AvailableMon = site.AvailableMon;
                dbAltSent.AvailableTue = site.AvailableTue;
                dbAltSent.AvailableWed = site.AvailableWed;
                dbAltSent.AvailableThu = site.AvailableThu;
                dbAltSent.AvailableFri = site.AvailableFri;
                dbAltSent.AvailableSat = site.AvailableSat;
                dbAltSent.PrimaryAltSentSiteId = site.PrimaryAltsentSiteId;
                dbAltSent.DefaultSunAltSentSiteAssignId = site.DefaultSunSiteAssignId;
                dbAltSent.DefaultMonAltSentSiteAssignId = site.DefaultMonSiteAssignId;
                dbAltSent.DefaultTueAltSentSiteAssignId = site.DefaultTueSiteAssignId;
                dbAltSent.DefaultWedAltSentSiteAssignId = site.DefaultWedSiteAssignId;
                dbAltSent.DefaultThuAltSentSiteAssignId = site.DefaultThuSiteAssignId;
                dbAltSent.DefaultFriAltSentSiteAssignId = site.DefaultFriSiteAssignId;
                dbAltSent.DefaultSatAltSentSiteAssignId = site.DefaultSatSiteAssignId;
            }

            AltSentPrimarySiteSaveHistory dbHistory = new AltSentPrimarySiteSaveHistory
            {
                AltSentId = site.AltsentId,
                SaveDate = DateTime.Now,
                SaveBy = 1,
                AvailableSun = site.Availablesun,
                AvailableMon = site.AvailableMon,
                AvailableTue = site.AvailableTue,
                AvailableWed = site.AvailableWed,
                AvailableThu = site.AvailableThu,
                AvailableFri = site.AvailableFri,
                AvailableSat = site.AvailableSat,
                PrimaryAltSentSiteId = site.PrimaryAltsentSiteId,
                DefaultSunAltSentSiteAssignId = site.DefaultSunSiteAssignId,
                DefaultMonAltSentSiteAssignId = site.DefaultMonSiteAssignId,
                DefaultTueAltSentSiteAssignId = site.DefaultTueSiteAssignId,
                DefaultWedAltSentSiteAssignId = site.DefaultWedSiteAssignId,
                DefaultThuAltSentSiteAssignId = site.DefaultThuSiteAssignId,
                DefaultFriAltSentSiteAssignId = site.DefaultFriSiteAssignId,
                DefaultSatAltSentSiteAssignId = site.DefaultSatSiteAssignId,
            };

           return await _context.SaveChangesAsync();
        }

    }
}
