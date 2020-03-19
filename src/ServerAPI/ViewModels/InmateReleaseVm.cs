using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.ViewModels
{
    public class InmateReleaseVm
    {
        public bool ReleaseClearFlag { get; set; }
        public bool ArrestActive { get; set; }
        public string OverallConditionOfRelease { get; set; }
        public DateTime? OverallFinalReleaseDate { get; set; }
        public bool TransportFlag { get; set; }
        public string AgencyName { get; set; }
        public string CoOpRoute { get; set; }
        public DateTime? TransportScheduleDate { get; set; }
        public decimal? DebtAmt { get; set; }
        public string PersonalInventory { get; set; }
        public int InmateSupply { get; set; }
        public string InmateLocation { get; set; }
        public bool HousingFlag { get; set; }
        public HousingDetail HousingDetails { get; set; }
        public string InmateClassification { get; set; }
        public bool WorkCrewFlag { get; set; }
        public string WorkCrewName { get; set; }
        public List<KeyValuePair<string, string>> Privilege { get; set; }
        public string[] WorkCrewRequest { get; set; }
        public string[] WorkFurloughRequest { get; set; }
        public int InmateId { get; set; }
        public int IncarcerationId { get; set; }
        public bool InmateActive { get; set; }

        public int Contact { get; set; }
        public string[] AltSentRequest { get; set; }
        public string[] AltSentProgram { get; set; }
        public int IssuedProperty { get; set; }
        public List<KeyValuePair<string, string>> CondOfRelFlags { get; set; }
        public bool AltSentFlag { get; set; }
        public int AltSentCount { get; set; }
        public KeyValuePair<int?, int?> AltSentAttend { get; set; }
        public int DNA { get; set; }
        public VechileVm Vehicle { get; set; }
        public int PersonAlert { get; set; }
        public string ProgramAssign { get; set; }
        public string ProgramRequest { get; set; }
        public List<KeyValuePair<int, string>> TaskDetails { get; set; }
        public int VerifyId { get; set; }
    }

    public class VechileVm {
        public string License { get; set; }
        public string Disposition { get; set; }
        public string Location { get; set; }
        public string MakeName { get; set; }
        public string ModelName { get; set; }
    }
}
