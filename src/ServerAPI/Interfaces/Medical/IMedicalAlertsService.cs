using System.Collections.Generic;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IMedicalAlertsService
    { 
        MedicalAlertInmateVm GetMedicalAlertInmate(MedicalAlertInmateVm inputs);
    }
}