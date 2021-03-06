﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Interfaces
{

    public interface IClassifyAlertsKeepsepService
    {
        KeepSeparateAlertVm GetAlertKeepInmateList(KeepSepSearchVm keepSepSearch);
        KeepSeparateAlertVm GetKeepSeparateAssocSubsetList(KeepSepSearchVm keepSepSearch);
        HousingUnitListDetailVm GetHousingBuildingDetails(int facilityId);

        Task<bool> DeleteUndoKeepSeparate(KeepSeparateVm keepSepDetails);



    }

}
