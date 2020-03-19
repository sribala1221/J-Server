using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IPrebookWizardService
    {
        List<FacilityVm> GetFacilities();
        Task<InmatePrebookVm> InsertPrebook(InmatePrebookVm value);

        Task<int> UpdatePrebook(InmatePrebookVm inmatePrebookVm);

        Task<int> UpdatePrebookLastStep(InmatePrebookVm inmatePrebookVm);

        IEnumerable<CrimeLookupVm> GetCrimeSearch(CrimeLookupVm crimeDetails);

        Task<int> InsertInmatePrebookCharge(PrebookCharge prebookCharge);

        Task<int> InsertForceCharge(PrebookCharge prebookCharge);

        List<PrebookCharge> GetPrebookCharges(int inmatePrebookId, bool deleteFlag, int prebookWarrantId,
            int incarcerationId = 0, int arrestId = 0);

        Task<int> InsertCrimeHistory(CrimeHistoryVm crime);

        Task<int> DeleteInmatePrebookCharges(int inmatePrebookChargeId);

        Task<int> DeleteCrimeForces(int crimeForceId);

        Task<int> UndoDeleteInmatePrebookCharges(int inmatePrebookChargeId);

        Task<int> UndoDeleteCrimeForces(int crimeForceId);

        IEnumerable<InmatePrebookWarrantVm> GetPrebookWarrant(int inmatePrebookId, bool deleteFlag,
            int incarcerationId = 0, int arrestId = 0);

        InmatePrebookWarrantVm GetPrebookWarrantById(int inmatePrebookWarrantId);

        Task<int> InsertPrebookWarrant(InmatePrebookWarrantVm warrant);

        Task<int> UpdatePrebookWarrant(InmatePrebookWarrantVm warrant);

        Task<int> DeletePrebookWarrant(int inmatePrebookWarrantId);

        IEnumerable<PersonalInventoryVm> GetPersonalInventoryPrebook(int preBookId, bool deleteFlag);

        Task<int> InsertPersonalInventoryPrebook(PersonalInventoryVm[] properties);

        Task<int> DeletePersonalInventoryPrebook(int personalInventoryPreBookId);

        Task<int> UndoDeletePersonalInventoryPrebook(int personalInventoryPreBookId);

        Task<int> UndoDeletePrebookWarrant(int inmatePrebookWarrantId);

        IEnumerable<PrebookAttachment> GetPrebookAttachment(AttachmentSearch attachmentSearch);

        Task<int> InsertPrebookAttachment(PrebookAttachment applets);

        Task<int> UpdatePrebookAttachment(PrebookAttachment applets);

        AttachmentComboBoxes LoadPrebookAttachmentEntry(string attachType, int inmateId, int facilityId);

        List<HistoryVm> LoadAttachHistory(int appletSavedId);

        string OpenPrebookAttachment(int appletSavedId);

        Task<int> DeletePrebookAttachment(int appletSavedId);

        Task<int> UndoDeletePrebookAttachment(int appletSavedId, string history);
        IEnumerable<GetFormTemplates> GetPreBookForms(int inmatePrebookId, FormScreen formScreenFlag);

        IEnumerable<LoadSavedForms> LoadSavedForms(int inmatePrebookId, int formTemplateId, FormScreen formScreenFlag,
            bool deleteFlag);

        Task<int> DeletePreBookForm(int formRecordId);

        AddForm AddForm(int inmatePrebookId, int formTemplateId);

        ListForm ListForm(int formRecordId);

        Task<int> UpdateForm(LoadSavedForms formdata);

        List<AgencyVm> GetAgencies();
        IEnumerable<CrimeHistoryVm> GetCrimeHistory(int? inmatePrebookChargeId);
        Task<int> UpdateInmatePrebookCharge(PrebookCharge value);
        Task<int> UpdateForceCharge(PrebookCharge value);
        Task<int> UpdatePrebookComplete(int inmatePrebookId);
        Task<int> UpdatePrebookUndoComplete(int inmatePrebookId);
        IEnumerable<VehicleMakeVm> GetVehicleMake();
        IEnumerable<VehicleModelVm> GetVehicleModel(int vehicleMakeId);
        Task<int> UpdateMedPrescreenStatusStartComplete(MedPrescreenStatus medPrescreenStatus);
        Task<int> UpdateMedPrescreenStatus(MedPrescreenStatus medPrescreenStatus);
        IEnumerable<AppletsSavedVm> GetAttachmentHistory(int prebookId);
        List<KeyValuePair<string, int>> GetBookingNumbers(int inmateId);
        PrebookAttachment GetAttachmentDetail(int appletsSavedId);
        Task<int> CreateVehicleModel(VehicleModelVm vehicleModel);
        Task<int> CreateVehicleMake(VehicleMakeVm vehicleMake);
        Task<int> UpdateVehicleMake(VehicleMakeVm vehicleMakevm);
        Task<int> UpdateVehicleModel(VehicleModelVm vehicleModelVm);
        List<InmatePrebookCaseVm> GetPrebookCases(int inmatePrebookId, bool deleteFlag);
        List<InmatePrebookCaseVm> InsertUpdatePrebookCase(List<InmatePrebookCaseVm> prebookCaseVms, bool deleteFlag);
        Task<int> DeleteUndoPrebookCase(InmatePrebookCaseVm prebookCaseVm);
        PrebookProperty GetPropertyDetails(PrebookProperty propertyDetail);
        int GetDefaultAgencyId();
    }
}
