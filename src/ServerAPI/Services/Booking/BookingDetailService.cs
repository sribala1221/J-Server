using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public class BookingDetailService : IBookingDetailService
    {

        private readonly AAtims _context;
        private readonly int _personnelId;

        public BookingDetailService(AAtims context, IHttpContextAccessor ihHttpContextAccessor)
        {
            _context = context;
            _personnelId = Convert.ToInt32(ihHttpContextAccessor.HttpContext.User
                .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
        }

        public IncarcerationFormDetails GetIncarcerationFormsDetails(int incarcerationId, string filterName)
        {
            IncarcerationFormDetails incarcerationFormDetails = new IncarcerationFormDetails();

            IQueryable<PersonnelVm> lstPersonnel = _context.Personnel.Select(s => new PersonnelVm
            {
                PersonLastName = s.PersonNavigation.PersonLastName,
                PersonnelNumber = s.PersonnelNumber,
                PersonnelId = s.PersonnelId
            });
            if (!string.IsNullOrEmpty(filterName))
            {

                incarcerationFormDetails.templateCount = _context.FormCategoryFilter
                    .Where(w => w.FormCategoryId == 10 && w.FilterName == filterName)
                    .Select(s => new FormTemplateCount
                    {
                        CategoryId = s.FormCategoryFilterId,
                        CategoryName = s.FilterName,
                        Count = s.FormTemplates.Count(i => i.FormCategoryId == 10
                                                           && i.FormCategoryFilterId == s.FormCategoryFilterId &&
                                                           !i.Inactive.HasValue)
                    }).SingleOrDefault();
            }

            incarcerationFormDetails.lstIncarcerationForms = _context.FormRecord.Where(w =>
                    w.FormHousingClear == 0 && (!w.FormTemplates.Inactive.HasValue || w.FormTemplates.Inactive==0)
                                            && w.IncarcerationId == incarcerationId &&
                                            (incarcerationFormDetails.templateCount == null ||
                                             w.FormTemplates.FormCategoryFilterId ==
                                             incarcerationFormDetails.templateCount.CategoryId)
                                            )
                .Select(s => new IncarcerationForms
                {
                    FormRecordId = s.FormRecordId,
                    DisplayName = s.FormTemplates.DisplayName,
                    FormNotes = s.FormNotes,
                    ReleaseOut = s.Incarceration.ReleaseOut,
                    DateIn = s.Incarceration.DateIn,
                    DeleteFlag = s.DeleteFlag,
                    XmlData = HttpUtility.HtmlDecode(s.XmlData),
                    FormCategoryFolderName = s.FormTemplates.FormCategory.FormCategoryFolderName,
                    HtmlFileName = s.FormTemplates.HtmlFileName,
                    FormTemplatesId = s.FormTemplatesId,
                    FormInterfaceFlag = s.FormTemplates.FormInterfaceFlag,
                    FormInterfaceSent = s.FormInterfaceSent,
                    FormInterfaceByPassed = s.FormInterfaceBypassed,
                    CreatedBy = lstPersonnel.SingleOrDefault(w => w.PersonnelId == s.CreateBy),
                    CreateDate = s.CreateDate,
                    UpdatedBy = lstPersonnel.SingleOrDefault(w => w.PersonnelId == s.UpdateBy),
                    UpdateDate = s.UpdateDate,
                    FormCategoryFilterId = s.FormTemplates.FormCategoryFilterId,
                    FilterName = s.FormTemplates.FormCategoryFilter.FilterName,
                    NoSignature = s.NoSignatureReason
                }).ToList();

            return incarcerationFormDetails;
        }

        public Task<int> DeleteUndoIncarcerationForm(IncarcerationForms incFrom)
        {
            FormRecord dbFormRecord =
                _context.FormRecord.SingleOrDefault(fr => fr.FormRecordId == incFrom.FormRecordId);
            DateTime? deleteDate = DateTime.Now;
            if (dbFormRecord == null) return Task.FromResult(0);
            dbFormRecord.DeleteBy = incFrom.DeleteFlag == 1 ? new int?() : _personnelId;
            dbFormRecord.DeleteDate = incFrom.DeleteFlag == 1 ? new DateTime?() : deleteDate;
            dbFormRecord.DeleteFlag = incFrom.DeleteFlag;

            return _context.SaveChangesAsync();
        }

        public Task<int> UpdateFormInterfaceBypassed(IncarcerationForms incFrom)
        {
            FormRecord dbFormRecord =
                _context.FormRecord.SingleOrDefault(fr => fr.FormRecordId == incFrom.FormRecordId);
            if (dbFormRecord != null)
            {
                dbFormRecord.FormInterfaceBypassed = incFrom.FormInterfaceByPassed;
            }

            return _context.SaveChangesAsync();
        }
        
    }
}
