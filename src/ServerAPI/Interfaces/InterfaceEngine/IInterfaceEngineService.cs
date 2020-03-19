using System;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IInterfaceEngineService
    {
        object Inbound(InboundRequestVM values);
        void Export(ExportRequestVm values);
        object TestExportRequest(ExportRequestVm values);
        InmatePrebookVm GetInmatePrebookSacramento(string inmateNumber);
        InmatePrebookVm SaveInmatePrebookSacramento(InmatePrebookSacramento prebook);
    }
}
