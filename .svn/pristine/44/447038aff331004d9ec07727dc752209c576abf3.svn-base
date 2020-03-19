using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class PersonnelVm
    {
        public int PersonnelId { get; set; }
        public int PersonId { get; set; }
		public string PersonnelNumber { get; set; }
        public string PersonLastName { get; set; }
        public string PersonFirstName { get; set; }
        public string PersonMiddleName { get; set; }
        public string OfficerBadgeNumber { get; set; }
		public bool PersonnelTerminationFlag { get; set; }
        public bool ArrestTransportOfficerFlag { get; set; }
        public bool ReceiveSearchOfficerFlag { get; set; }
        public bool ProgramInstructorFlag { get; set; }
        public bool PersonnelAgencyFlag { get; set; }
        public int AgencyId { get; set; }
        public int GrvPersonnelId { get; set; }
        public string PersonnelFullDisplayName { get; set; }
        public  string AgencyName { get; set; }
    }

    public class PersonnelDetailsVm
    {
        public List<KeyValuePair<int, string>> AgencyList { get; set; }
        public List<KeyValuePair<string, bool>> RoleList { get; set; }
        public List<KeyValuePair<string, int>> StatusCount { get; set; }
        public List<KeyValuePair<string, int>> UserCount { get; set; }
        public List<KeyValuePair<string, int>> FlagsCount { get; set; }
        public List<KeyValuePair<string, int>> DepartmentCount { get; set; }
        public List<KeyValuePair<string, int>> PositionCount { get; set; }
        public List<MonitorPersonnelVm> PersonnelList { get; set; }
    }

    public class MonitorPersonnelVm
    {
        public int PersonnelId { get; set; }
        public int PersonId { get; set; }
        public string PersonLastName { get; set; }
        public string PersonFirstName { get; set; }
        public string PersonMiddleName { get; set; }
        public string OfficerBadgeNumber { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public int AgencyId { get; set; }
        public string Agency { get; set; }
        public DateTime? HireDate { get; set; }
        public bool JudgeFlag { get; set; }
        public bool Commissioned { get; set; }
        public bool AgencyGroupFlag { get; set; }
        public bool ArrestTransportOfficerFlag { get; set; }
        public bool ReceiveSearchOfficerFlag { get; set; }
        public bool ProgramInstructorFlag { get; set; }
        public DateTime? PersonnelTerminationDate { get; set; }
        public bool TerminationFlag { get; set; }
        public string DeleteFlag { get; set; }
        public UserVm User { get; set; }
        public string Email { get; set; }
        public int FacilityId { get; set; }
        public DateTime? ExpireDate { get; set; }
        public bool ActiveFlag { get; set; }
        public bool PersonFlag { get; set; }
        public bool ProgramCaseFlag { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool IsUser { get; set; }
        public string OfficerBadgeNum { get; set; }
    }


    public class PersonnelInputVm
    {
        public List<KeyValuePair<int, string>> AgencyList { get; set; }
        public List<KeyValuePair<int, string>> DepartmentList { get; set; }
        public List<KeyValuePair<int, string>> PositionList { get; set; }
        public MonitorPersonnelVm PersonnelDetails { get; set; }
        public List<string> RoleList { get; set; }
        public List<KeyValuePair<string, bool>> Roles { get; set; }
        public bool UseADAuthentication { get; set; }
        public string Mode { get; set; }
    }

    public class UserVm
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public bool DefaultDomain { get; set; }
        public string AdDomain { get; set; }
        public bool FormAuthentication { get; set; }
        public string OldPassword { get; set; }
        public string Password { get; set; }
        public string VerifyPassword { get; set; }
        public bool UserLocked { get; set; }
        public bool Expires { get; set; }
        public string UserExpires { get; set; }
        public bool TestUser { get; set; }
        public string CreateDate { get; set; }
        public string PersonnelId { get; set; }
        public string DeleteFlag { get; set; }
    }

    public class PersonnelFilter
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Number { get; set; }
        public string Role { get; set; }
        public int AgencyId { get; set; }
        public bool IsRefresh { get; set; }
    }

    public class DepartmentVm
    {
        public int AgencyCourtDeptId { get; set; }
        public int DepartmentId { get; set; }
        public int AgencyId { get; set; }
        public string DepartmentName { get; set; }
        public int DeleteFlag { get; set; }
    }
}
