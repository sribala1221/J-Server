using ServerAPI.ViewModels;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.Utilities;

namespace ServerAPI.Services
{
    public interface IInvestigationService
    {
        InvestigationVm InsertUpdateInvestigation(InvestigationVm iInvestigation);
        InvestigationDataVm GetInvestigations(InvestigationInputs inputs);
        int InsertUpdateInvestigationFlags(InvestigationFlag iInvestigation);
        int InsertUpdateInvestigationPersonnel(InvestigationPersonnelVm iInvestigation);
        int InsertUpdateInvestigationNotes(InvestigationNotesVm iInvestigation);
        InvestigationAllDetails GetInvestigationAllDetails(int investigationId);
        List<KeyValuePair<int, string>> GetInvestigationIncidents();
        int InsertUpdateInvestigationIncident(InvestigationIncident iInvestigation);
        List<KeyValuePair<int, string>> GetInvestigationGrievance();
        int InsertUpdateInvestigationGrievance(InvestigationIncident iInvestigation);
        int DeleteInvestigationAttachment(int attachmentId);
        int InsertUpdateInvestigationLink(InvestigationLinkVm iInvestigation);
        int DeleteInvestigationForms(int formId);
        int UpdateInvestigationComplete(InvestigationVm investigation);
        int DeleteInvestigation(InvestigationVm investigation);
        List<HistoryVm> GetInvestigationHistoryDetails(int investigationId);
    }
}
