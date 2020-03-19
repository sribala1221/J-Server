using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public class CustomClientMappingService : ICustomClientMappingService
    {
        private readonly AAtims _context;

        public CustomClientMappingService(AAtims context)
        {
            _context = context;
        }

        public List<CustomMappingVm> GetClientCustomLabels()
        {
            List<CustomMappingVm> customMappings = _context.CustomMapping.Select(s => new CustomMappingVm
            {
                CustomMappingId = s.CustomMappingId,
                DisplayValue = string.IsNullOrEmpty(s.CustomValue) ? s.DefaultValue : s.CustomValue,
                FieldMapping = s.FieldMapping
            }).ToList();
            return customMappings;
        }

        public List<CustomFieldVm> GetCustomFieldLookups(int appAoUserControlId)
        {
            List<CustomFieldVm> fields = _context.CustomFieldLookup
                .Where(w => w.AppAoUserControlId == appAoUserControlId && w.DeleteFlag != 1).Select(s => new CustomFieldVm(s)).ToList();
            fields.ForEach(f =>
                {
                    f.DropDownOptions = _context.CustomFieldDropDown
                        .Where(w => w.CustomFieldLookupId == f.CustomFieldLookupId && w.DeleteFlag != 1)
                        .Select(s => new CustomFieldDropDownOption
                        {
                            CustomFieldDropDownId = s.CustomFieldDropDownId,
                            CustomFieldDropDownText = s.DropDownText
                        }).ToList();
                });
            return fields;
        }
    }
}