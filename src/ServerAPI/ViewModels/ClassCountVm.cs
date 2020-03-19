using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{   
    public class HousingCount{
        public int Count { get; set; }
        public string Housing { get; set; }     
        public string Classification { get; set; }
        public string ClassType { get; set; }
        public DateTime? ClassDate { get; set; }
        public int InmateClassificationId { get; set; }            
        public PersonVm PersonDetails { get; set; }
        public string Narrative { get; set; }
        public string Days { get; set; }

        public TimeSpan? HousingDays{get; set;}
        public double DayCount { get; set; }
        public bool Review { get; set; }
        public int? Scheduledays{get; set;}

        public string ClassDays{get; set;}
        public double DaysNumber { get; set; }
    }

    public class ClassCountInputs
    {
        public int FacilityId { get; set; }
        public string Flag { get; set; }     
        public string Classification { get; set; }
        public string Association { get; set; }
        public int Gender { get; set; }
        public string FlagType { get; set; }
        public int AlertFlag { get; set; }
        public string DetailsFlag { get; set; }
        public bool CountFlag { get; set; }
        public string Housing { get; set; }

        public string Days { get; set; }
        public string ClassifyReason { get; set; }
        public bool PageLoadFlag { get; set; }
        public bool CountRefreshFlag { get; set; }
        public int? ClassifyDays { get; set; }
        public int? AssociationId { get; set; }
    }

    public class HousingDetails
    {
        public List<HousingCount> ParentHousingCount { get; set; }
        public List<HousingCount> ChildHousingCount { get; set; }
        public List<HousingCount> HousingDetailsList { get; set; }

        public List<HousingCount> HousingDayDetailsList { get; set; }
        public List<HousingCount> ParentClassDayCount { get; set; }
        public List<HousingCount> ChildClassDayCount { get; set; }
       
    }

    public class ClassCountHousing {
        public List<LookupVm> Flags { get; set; }
        public List<KeyValuePair<int, string>> Association { get; set; }
        public List<KeyValuePair<int, string>> Classify { get; set; }
        public List<KeyValuePair<int,string>> Gender { get; set; }
        public HousingDetails HousingDetailsList { get; set; }
        public HousingDetails ClassDayDetails { get; set; }
    }

    public class BookMarkDetails
    {
        public int FormBookMarkId { get; set; }
        public int PersonId { get; set; }
        public int ArrestId { get; set; }
        public string BookMarkName { get; set; }
        public string BookMarkValue { get; set; }
    }
}
