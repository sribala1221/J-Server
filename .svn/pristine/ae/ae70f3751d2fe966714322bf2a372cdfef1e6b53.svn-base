using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IInmateSummaryPdfService
    {
        InmateSummaryPdfVm GetInmateSummaryPdf(int inmateId, InmateSummaryType summaryType,int? incarcerationId);
		InmateSummaryPdfVm GetCaseSheetDetails(FormDetail formDetail);// int inmateId, int arrestId, string apiUrl);
        InmateSummaryPdfVm GetBookingSheetDetails(int inmateId, InmateSummaryType summaryType, int incarcerationId, bool autofill);
        bool GetBookComplete(int inmateId);
    }
}
