using System;

namespace ServerAPI.ViewModels
{
    public class WizardVm
    {
        public int WizardId { get; set; }
        public string WizardName { get; set; }
        public bool IsSubwizard { get; set; }
        public bool HasFixedOrer { get; set; }
        public AoWizardFacilityVm WizardFacility { get; set; }
        public AoWizardProgressVm WizardProgress { get; set; }
    }
}