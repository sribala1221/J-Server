using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IWizardService
    {
        List<AoWizardVm> GetWizardSteps();
        //List<WizardStepVm> GetWizardStepDetails(int facilityId, WizardType wizardType, bool childWizard);
        //WizardDetails GetWizardDetails(int facilityId);
        //Task<int> UpdateIncarcerationWizardStep(IntakeVm intakeVm);
        //WizardCompleteVm GetIntakeWizardCompleteDetails(int incarcerationId);
        //Task<int> UpdateIntakeWizardComplete(bool completeFlag, int incarcerationId);
        //WizardCompleteVm GetBookingWizardCompleteDetails(int incarcerationId);
        //Task<int> UpdateBookingWizardComplete(bool completeFlag, int incarcerationId);
        //Task<int> UpdateBookingDataWizard(IntakeVm intakeVm);
        Task<int> CreateWizardProgress(AoWizardProgressVm wizardProgress);
        Task<AoWizardProgressVm> SetWizardStepComplete(AoWizardStepProgressVm wizardProgress);

        AoWizardProgressVm GetWizardProgress(int aoWizardProgressId);
        List<AoWizardVm> GetWizardSteps(int aoWizardId);
        AoWizardFacilityVm GetAoWizardFacility(int wizardId, int facilityId);
        int GetTemplateIdFromWizardSteps(int wizardStepId);
        bool WizardStepChanged(int facilityId, int wizardId, string wizardName);
    }
}
