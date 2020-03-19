using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class AoWizardProgressInmatePrebookVm
    {
         public int InmatePrebookId { get; set; }
         public int WizardProgressId { get; set; }
         public List<AoWizardStepProgressVm> WizardStepProgress { get; set; }
    }
}