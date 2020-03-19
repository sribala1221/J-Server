using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface ITransferService
    {
        TransEligibleDetailsVm GetTransferEligibles(EligibleSearchVm eligibleSearch);
        Task<int> UpdateIncarceration(TransferEligibleVm eligible);
        List<TransferHistoryVm> GetTransferHistoryDetails(int incarcerationId);
        EligibleInmateCountVm GetInmateCount(int inmateId, int personId);
		InternalTransferVm GetInternalTransfer(DateTime transferDate, int facilityId, bool inProgress);
        ExternalTransferVm GetLocationCountDetails(int facilityId, DateTime startDate);
        ExternalTransferVm GetLocationInmateDetails(int locationId,int facilityId, bool isAppointment, DateTime startDate);
        List<KeyValuePair<string,int>> GetExternalLocations();
        List<KeyValuePair<string,int>> GetInventoryBinList();
        List<AoAppointmentVm> GetScheduleTransfer(DateTime fromDate, int facilityId);
        Task<AppointmentConflictCheck> InsertAddInmateAsync(InmateApptDetailsVm inmateApptDetails);
        Task<int> UpdateCheckInLibBook(List<int> inmateIds);
        Task<int> IssuedProperty(List<ExternalSearchVm> externalSearch, int isFlag);
        Task<int> UpdateFacilityTransfer(ExtTransferFacilityVm extlTransfer);
        List<AoAppointmentVm> HousingDetails(int facilityId, int inmateId);
        List<AppointmentHistoryList> GetAppointmentSavedHistory(int scheduledId);
    }
}