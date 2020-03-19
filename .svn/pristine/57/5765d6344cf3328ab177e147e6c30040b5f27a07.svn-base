using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GenerateTables.Models;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public class FieldSettingsService : IFieldSettingsService
    {
        #region Properties

        private readonly AAtims _context;
     
        #endregion

        #region Constructor

        public FieldSettingsService(AAtims context)
        {
            _context = context;
        
        }

        #endregion

        #region Methods

        public List<AppAoFieldLabelVm> GetFieldLabels(int[] fieldLabelId)
        {
        
            List<AppAoFieldLabelVm> appAoFieldLabelVm = _context.AppAoFieldLabel
                .Where(f => fieldLabelId.Contains(f.AppAoFieldLabelId)).OrderBy(o => o.AppAoFieldLabelId)
                .Select(s => new AppAoFieldLabelVm
                {
                    AppAoFieldLabelId = s.AppAoFieldLabelId,
                    FieldLabel = s.FieldLabel,
                    FieldName = s.FieldName
                }).ToList();
           
            return appAoFieldLabelVm;
        }

        public List<AppAoUserControlFieldsVm> GetFieldSettings()
        {
            try
            {

                List<AppAoUserControlFieldsVm> lstAppAoUserControlFields = _context.AppAoUserControlFields
                    .OrderBy(o => o.AppAoUserControlFieldsId)
                    .Select(f => new AppAoUserControlFieldsVm
                    {
                        AppAoUserControlFieldsId = f.AppAoUserControlFieldsId,
                        AppAoUserControlId = f.AppAoUserControlId,
                        FieldVisible = f.FieldVisible == 1,
                        FieldRequired = f.FieldRequired == 1,
                        FieldLabel = string.IsNullOrWhiteSpace(f.FieldLabel)
                            ? f.AppAoFieldLabel.FieldLabel
                            : f.FieldLabel,
                        FieldTag = f.FieldTagId
                    }).ToList();
                return lstAppAoUserControlFields;
            }

            catch (Exception e)
            {
                string m = e.Message;
            }

            return null;
        }

        public AppAoUserControlFieldsVm GetFieldSettings(int appAoUserControlId) {
            List<AppAoUserControlFieldsVm> lstAppAoUserControlFields = GetFieldSettings();
            return lstAppAoUserControlFields[appAoUserControlId];
        }

        #endregion
    }
}
