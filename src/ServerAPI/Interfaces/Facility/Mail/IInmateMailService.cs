using System.Collections.Generic;
using ServerAPI.ViewModels;


// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public interface IInmateMailService
    {
        InmateMailHousingDetailVm GetHousingDetails(int inmateId);
        InmateMailVm GetMailRecordDefaultList(int facilityId);
        int InsertMailRecord(MailRecordVm value);
        int InsertMailVendorSubscribe(MailVendorSubscribeVm subscribe);
        List<MailVendorVm> MailVendorInsert(MailVendorVm mailVendor);
        int UpdateMailRecord(MailRecordVm value);

        int DeleteMailRecord(MailRecordVm value);
        List<MailVendorVm> VendorForSelect();

        List<MailVendorSubscribeVm> LoadMailVendorSubscribe();

        MailVendorSubscribeFacilityVm GetHousingList(int facilityId);

        int DeleteVendorSubscribe(int subscribeId);

        int UndoVendorSubscribe(int subscribeId);
        InmateMailPrivilegeVm GetPrivilegeByOfficer(int facilityId);
        List<PrebookAttachment> LoadMailAttachment(string type, int id);

        int UpdateDeliveryData(List<MailRecordVm> mailRecord);
        List<MailRecordVm> MailSearchRecord(MailSearchRecordVm record);
        List<HousingUnitVm> GetHousingNumber();
        List<MailRecordVm> GetMailRecordList(int facilityId);

       InmateMailVm LoadMailRecordByHousingId(int housingUnitListId, int facilityId, int housingGroupId);

        MailRecordVm GetMailRecordById(int id, int facilityId);
        List<MailBinVm> GetBinDetailsList(int facilityId);
    }
}