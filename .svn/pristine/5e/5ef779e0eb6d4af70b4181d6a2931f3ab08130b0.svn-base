using ServerAPI.ViewModels;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IIntakeCurrencyService
    {
        IntakeCurrencyViewerVm GetIntakeCurrencyViewer(int incarcerationId);
        IntakeCurrencyPdfViewerVm GetIntakeCurrencyPdfViewer(int incarcerationId);
        Task<int> InsertIntakeCurrency(IntakeCurrencyVm value);
    }
}
