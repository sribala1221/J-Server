using System;

namespace ServerAPI.ViewModels
{
    public class AoAvailableComponentVm
    {
        public int AoAvailableComponentId { get; set; }
        public AoWizardVm AoWizard { get; set; }
        public AoComponentVm AoComponent { get; set; }
        public bool IsActive { get; set; }
        public int Order { get; set; }
        public PersonnelVm CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public PersonnelVm UpdateBy { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}