using System.Collections.Generic;
using GenerateTables.Models;

namespace ServerAPI.ViewModels
{
    public class CustomMappingVm
    {
        public int CustomMappingId { get; set; }
        public string CustomValue { get; set; }
        public string DefaultValue { get; set; }
        public string FieldType { get; set; }
        public string DisplayValue { get; set; }
        public string FieldMapping { get; set; }
    }

    public class CustomFieldVm
    {
        public CustomFieldVm(CustomFieldLookup cfl)
        {
            CustomFieldLookupId = cfl.CustomFieldLookupId;
            FieldLabel = cfl.FieldLabel;
            FieldRequired = cfl.FieldRequired == 1 ? true : false;
            FieldSize = (cfl.FieldSizeSmall == 1
                ? CustomFieldSize.Small
                : (cfl.FieldSizeMedium == 1 ? CustomFieldSize.Medium : CustomFieldSize.Large));
            FieldType = (cfl.FieldTypeTextFlag == 1 
                ? CustomFieldType.Text 
                : (cfl.FieldTypeDropDownFlag == 1 
                    ? CustomFieldType.DropDown 
                    : (cfl.FieldTypeCheckboxFlag == 1 
                       ? CustomFieldType.Checkbox 
                       : (cfl.FieldTextEntryNumericOnly == 1 
                          ? CustomFieldType.NumericOnly : CustomFieldType.NumericAllowDecimal))) );
            FieldTag = "CustomField_" + cfl.CustomFieldLookupId;
            MaxLength = cfl.FieldTextEntryMaxLength;
        }
        public int CustomFieldLookupId { get; set; }
        public string FieldLabel { get; set; }
        public bool FieldRequired { get; set; }
        public CustomFieldSize FieldSize { get; set; }
        public CustomFieldType FieldType { get; set; }
        public List<CustomFieldDropDownOption> DropDownOptions { get; set; }
        public string FieldTag { get; set; }
        public int? MaxLength { get; set; }
    }

    public class CustomFieldDropDownOption
    {
        public int CustomFieldDropDownId { get; set; }
        public string CustomFieldDropDownText { get; set; }
    }

    public enum CustomFieldSize
    {
        Small,
        Medium,
        Large
    }

    public enum CustomFieldType
    {
        Text,
        DropDown,
        Checkbox,
        NumericOnly,
        NumericAllowDecimal
    }
}
