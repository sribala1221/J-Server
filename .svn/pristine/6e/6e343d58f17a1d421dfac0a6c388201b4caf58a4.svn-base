using System.Collections.Generic;
using ServerAPI.ViewModels;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IBookingDetailService
    {
        IncarcerationFormDetails GetIncarcerationFormsDetails(int incarcerationId, string filterName);
        Task<int> DeleteUndoIncarcerationForm(IncarcerationForms incFrom);
        Task<int> UpdateFormInterfaceBypassed(IncarcerationForms incFrom);
    }
}
