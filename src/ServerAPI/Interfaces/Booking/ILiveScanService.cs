using ServerAPI.ViewModels;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface ILiveScanService
    {
         LiveScanDetail GetLiveScan(int inmateId, int userControlId);
         string PreviewLiveScanPayLoad(string arrestIds, int exportId);
    }
}