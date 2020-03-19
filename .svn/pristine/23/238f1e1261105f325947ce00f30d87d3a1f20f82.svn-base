using GenerateTables.Models;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerAPI.Services
{
    public class ProgramService : IProgramService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly IPhotosService _photo;

        public ProgramService(AAtims context, ICommonService commonService, IPhotosService photosService)
        {
            _context = context;
            _commonService = commonService;
            _photo = photosService;
        }

        public IEnumerable<KeyValuePair<int, string>> GetLocationList(int? facilityId) => _context.Privileges.Where(p =>
             p.InactiveFlag == 0 && p.ShowInProgram.HasValue && p.ShowInProgram.Value == 1 && 
             (p.FacilityId == facilityId || p.FacilityId == 0 || !p.FacilityId.HasValue)).OrderBy(s => s.PrivilegeDescription)
            .Select(p => new KeyValuePair<int, string>(p.PrivilegeId, p.PrivilegeDescription)).ToList();

        public IEnumerable<ProgramVm> GetProgramList(int? facilityId) => _context.Program.Where(p =>
            p.DeleteFlag == 0
            && p.ProgramCategory.DeleteFlag == 0
            && (!facilityId.HasValue || p.ProgramCategory.FacilityId == facilityId)).OrderBy(s => s.ProgramCategory.ProgramCategory1).Select(p =>
             new ProgramVm
             {
                 ProgramId = p.ProgramId,
                 ProgramCategory = p.ProgramCategory.ProgramCategory1,
                 ClassOrServiceName = p.ClassOrServiceName,
                 ClassOrServiceNumber = p.ClassOrServiceNumber,
                 ClassOrServiceDescription = p.ClassOrServiceDescription
             }).ToList();

        public IEnumerable<object> GetProgramAppts()
        {
            return null;
        }
    }
}
