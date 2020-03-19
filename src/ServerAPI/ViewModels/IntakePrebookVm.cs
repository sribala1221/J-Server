using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class IntakePrebookCountVm : InmatePrebookVm
    {
        public bool AppAoWizardStepVisible { get; set; }
        public int PrebookReady { get; set; }
        public int CourtCommitSchd { get; set; }
        public int CourtCommitOverdue { get; set; }
        public int MedicallyRejected { get; set; }
        public int InProgress { get; set; }
        public int ByPassed { get; set; }
       // public string ReadyForIntake { get; set; }
        public int ReadyForIntake { get; set; }
        public int ReadyForTempHold { get; set; }
        public int Identification { get; set; } 
    }

   public class IntakePrebookVm
    {

        public InmatePrebookVm LstPrebookReady { get; set; }
        public InmatePrebookVm LstCourtCommitSchd { get; set; }
        public InmatePrebookVm LstCourtCommitOverDue { get; set; }
        public InmatePrebookVm LstMedicallyRejected{ get; set; }
        public InmatePrebookVm LstPrebookInProgress { get; set; }
        public InmatePrebookVm LstPrebookByPassed { get; set; }
        public InmatePrebookVm LstPrebookIdentification { get; set; }
        public InmatePrebookVm LstPrebookIntake { get; set; }
        public InmatePrebookVm LstPrebookTempHold { get; set; }
    }



}
