using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class MailVendorVm
    {
        public int MailVendorid { get; set; }
        public int FacilityId { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateBy { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime? DeleteDate { get; set; }
        public int? DeleteBy { get; set; }
        public string VendorName { get; set; }
        public int MailType { get; set; }
        public string VendorAddress { get; set; }
        public string VendorCity { get; set; }
        public string VendorState { get; set; }
        public string VendorZip { get; set; }
        public bool AllowFlag { get; set; }
        public bool AllowFlagSubscribe { get; set; }
        public bool AllowFlagBatch { get; set; }
        public int? NoSubscribeRefusalReason { get; set; }
        public bool NotAllowedFlag { get; set; }
        public int? NotAllowedRefusalReason { get; set; }
        public string MailTypeName { get; set; }

    }

    public class MailVendorSubscribeFacilityVm
    {
        public List<MailVendorSubscribeHousingLocationVm> LstVendorSubscribeFacility { get; set; }
        public List<MailVendorSubscribeHousingCountVm> LstMailVendorSubscribeFacilityCount { get; set; }

    }

    public class MailVendorSubscribeHousingLocationVm
    {
        public int InmateId { get; set; }
        public int LocationId { get; set; }
        public string Location { get; set; }
        public string TrakLocation { get; set; }
        public HousingCapacityVm HousingDetail { get; set; }
        public PersonInfoVm Person { get; set; }

        public string InmateNumber { get; set; }

        public string ImageName { get; set; }
        public string FacilityAbbr { get; set; }
        public int FacilityId { get; set; }

        public string MailTypeName { get; set; }

        public string InmateName { get; set; }

        public string VendorName { get; set; }

        public int MailVendorSubscribeid { get; set; }
        public int MailVendorid { get; set; }
        public string SubscribeVendorZip { get; set; }

        public string SubscribeVendorState { get; set; }

        public string SubscribeVendorCity { get; set; }

        public string SubscribeVendorAddress { get; set; }
        public DateTime SubscribeStart { get; set; }
        public DateTime SubscribeEnd { get; set; }
        public string SubscribeNote { get; set; }

        public bool DeleteFlag { get; set; }


    }

    public class MailVendorSubscribeHousingCountVm
    {
        public int LocationId { get; set; }
        public string TrakLocation { get; set; }
        public int Count { get; set; }
        public string HousingUnitLocation { get; set; }
        public string HousingUnitNumber { get; set; }
        public int Assigned { get; set; }
        public int CurrentCapacity { get; set; }
        public int OutofService { get; set; }
        public bool DisplayFlag { get; set; }
        public DateTime? AppointmentEndDate { get; set; }
        public string ApptStartDate { get; set; }
        public bool EndDateFlag { get; set; }
        public DateTime? AllDayApptEndDate { get; set; }
        public bool ApptAlertEndDate { get; set; }
        public bool ReoccurFlag { get; set; }
        public int HousingUnitListId { get; set; }
        public bool EnrouteFlag { get; set; }
    }



}