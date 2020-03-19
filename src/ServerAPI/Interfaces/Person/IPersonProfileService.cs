using System.Collections.Generic;
using ServerAPI.ViewModels;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
   public interface IPersonProfileService
    {
        PersonProfileVm GetProfileDetails(int personId, int inmateId);
        Task<int> InsertUpdatePersonProfile(PersonProfileVm personProfile);
        List<PersonSkillTradeVm> GetSkillAndTradedetails(int personId);
        Task<int> InsertSkillAndTradeDetails(PersonProfileVm personProfile);
        WorkCrowAndFurloughRequest GetWorkCrewRequestDetails(int facilityId,int inmateId);
      //  List<ProgramAndClass> GetProgramAndClass(int inmateId, int facilityId, string gender);
        List<ProgramAndClass> ValidateProgam(ProgramDetails program);
        //Task<List<ProgramAndClass>> ValidateProgam(ProgramDetails program);
        Task<int> InsertWorkCrowAndFurloughRequest(WorkCrowAndFurloughRequest workCrowAndFurlough);
        AkaDetails NamePreference(int personId);
        PersonAkaHeader DisplayString(int inmateId);
        ProgramDetails GetProgramAndClass(int inmateId, int facilityId);
    }
}
