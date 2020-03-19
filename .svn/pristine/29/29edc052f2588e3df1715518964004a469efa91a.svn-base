using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using GenerateTables.Models;

namespace ServerAPI.Services
{
    public interface IInmateBookingCaseService
    {
        BookingBailDetails GetInmateBookingBailDetails(int arrestId);
        List<BailTransactionDetails> GetBookingBailTransactionDetails(int arrestId);       
        Task<int> DeleteUndoBailTransaction(int bailTransactionId);
        List<BailSaveHistory2Vm> GetBailSaveHistory(int arrestId);
        BailCompanyDetails GetBailCompanyDetails();
        Task<int> UpdateBail(BailDetails bailDetails);
        List<HistoryVm> GetBailAgentHistoryDetails(int bailAgentId);
        Task<int> InsertUpdateBailAgent(BailAgentVm agent);
        Task<int> DeleteBailAgent(int bailAgentId);
        Task<KeyValuePair<int, BailTransaction>> InsertBailTransaction(BailTransactionDetails bailTransaction);
        BailDetails GetBookingBailAmount(int arrestId, int personId);
    }
}
