﻿using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
	public interface IClassifyService
	{
		ClassificationVm GetInmateClassificationSummary(int inmateId);
		InmateDetail GetInitialClassification(int inmateId);
		InmateDetail GetInmateClassificationDetails(int inmateClassificationId, int inmateId);
		Task<int> InsertInmateClassificationEntry(InmateDetail details);
		Task<int> UpdateClassification(InmateClassificationVm classify);
		Task<int> InsertReviewEntry(InmateDetail details);
		Task<int> UpdateClassificationReview(InmateClassificationVm classify);
	    string GetLastNonPendingClassification(string inmateNumber);        
        int GetInmateCount(int inmateId);
        List<KeyValuePair<int, string>> GetClassifySubModules();
        ClassifyAlertsVm GetClassifyMessageAlerts(int faclityId);
        Task<int> SaveClassification(InmateClassificationVm classify);
    }
}
