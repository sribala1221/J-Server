using GenerateTables.Models;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace ServerAPI.Services
{
    public class FacilityAppointmentService : IFacilityAppointmentService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;

        public FacilityAppointmentService(AAtims context, ICommonService commonService)
        {
            _context = context;
            _commonService = commonService;
        }

        public List<AppointmentOverallList> GetAppointmentList(FacilityApptVm value)
        {
            // TO Do
            return null;
        }

        public AppointmentFilterVm LoadApptFilterlist(int facilityId)
        {
            // Housing Details
            IQueryable<HousingUnit> housingUnit = _context.HousingUnit
                .Where(hu => (!hu.HousingUnitInactive.HasValue || hu.HousingUnitInactive == 0)
            && hu.FacilityId == facilityId);

            AppointmentFilterVm appointmentFilterVm = new AppointmentFilterVm
            {
                //To get Appointment Location
                Location = _context.Privileges
                    .Where(p => p.InactiveFlag == 0 && p.ShowInAppointments
                         && (p.FacilityId == facilityId || p.FacilityId == 0 || !p.FacilityId.HasValue))
                    .OrderBy(x => x.PrivilegeDescription)
                    .Select(p => new KeyValuePair<int, string>(p.PrivilegeId, p.PrivilegeDescription)).ToList(),

                //To get Appointment Reason
                AppointmentReason = _commonService.GetLookupKeyValuePairs(LookupConstants.APPTREAS),

                //To get Appointment Type
                AppointmentType = _commonService.GetLookupKeyValuePairs(LookupConstants.APPTYPE),

                //location
                HousingUnitLoc = housingUnit.Select(s => s.HousingUnitLocation.Trim()).Distinct().OrderBy(x => x)
                    .ToList(),
                HousingUnitNum = housingUnit
                    .OrderBy(g => g.HousingUnitNumber)
                    .Select(s => new HousingDropDown
                    {
                        HousingUnitListId = s.HousingUnitListId,
                        HousingUnitLocation = s.HousingUnitLocation,
                        HousingUnitNumber = s.HousingUnitNumber
                    }).Distinct().ToList(),
                CourtList = _context.Agency.Where(w =>
                        w.AgencyCourtFlag && (w.AgencyInactiveFlag == 0 || !w.AgencyInactiveFlag.HasValue))
                    .Select(s => new KeyValuePair<int, string>(s.AgencyId, s.AgencyName))
                    .OrderBy(o => o.Value).ToList(),
                CourtDepartmentList = _context.AgencyCourtDept.Where(x => x.DeleteFlag == 0 || !x.DeleteFlag.HasValue)
                    .Select(x => new CourtDepartmentList
                    {
                        AgencyId = x.AgencyId,
                        AgencyCourtDeptId = x.AgencyCourtDeptId,
                        DepartmentName = x.DepartmentName
                    }).ToList(),
                Program = _context.Program.Where(p =>
                    p.DeleteFlag == 0
                    && p.ProgramCategory.DeleteFlag == 0
                    && p.ProgramCategory.FacilityId == facilityId).Select(p =>
                    new ProgramVm
                    {
                        ProgramId = p.ProgramId,
                        ProgramCategory = p.ProgramCategory.ProgramCategory1,
                        ClassOrServiceName = p.ClassOrServiceName
                    }).ToList(),
                FacilityEvent = _context.FacilityEvent
                    .Select(s => new KeyValuePair<int, string>(s.FacilityEventId, s.FacilityEventName)).ToList(),
            };

            return appointmentFilterVm;
        }
    }
}
 