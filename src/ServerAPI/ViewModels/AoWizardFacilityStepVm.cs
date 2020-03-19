using System;
using ServerAPI.ViewModels;

namespace ServerAPI.ViewModels
{
    public class AoWizardFacilityStepVm
    {
        public int WizardFacilityStepId { get; set; }
        public AoComponentVm Component { get; set; }
        public bool IsActive { get; set; }
        public int Order { get; set; }

        public PersonnelVm CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public PersonnelVm UpdateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public string BookingTypeFilterString { get; set; }

        public int? FormTemplateId { get; set; }
        public string FormTemplateName { get; set; }
        public int? AoTaskLookupId { get; set; }
        public string TaskName { get; set; }
        public int? AoComponentParamId { get; set; }
    }
}