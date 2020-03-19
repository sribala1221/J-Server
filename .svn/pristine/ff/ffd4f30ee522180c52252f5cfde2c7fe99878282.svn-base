using System;
using GenerateTables.Models;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void SafetyDetails()
        {
            
           Db.SafetyCheck.AddRange(

               new SafetyCheck
               {
                   SafetyCheckId = 10,
                   FacilityId = 1,
                   HousingUnitListId = 5,
                   CreateDate = DateTime.Now.AddDays(-15),
                   CreateBy = 11,
                   UpdateDate = DateTime.Now.AddDays(-10),
                   UpdateBy = 12,
                   DeleteFlag = null,
                   DeleteDate = null,
                   DeleteBy = null,
                   LocationId = 6,
                   HousingUnitLocation = "FLOOR1",
                   HousingUnitNumber = "UP-A",
                   SafetyCheckLateEntryFlag = null,
                   SafetyCheckNote = "NO FIRE ALARAM"
               },
               new SafetyCheck
               {
                   SafetyCheckId = 11,
                   FacilityId = 1,
                   HousingUnitListId = 5,
                   CreateDate = DateTime.Now.AddDays(-15),
                   CreateBy = 11,
                   UpdateDate = DateTime.Now.AddDays(-10),
                   UpdateBy = 12,
                   DeleteFlag = null,
                   DeleteDate = null,
                   DeleteBy = null,
                   LocationId = 6,
                   HousingUnitLocation = "FLOOR1",
                   HousingUnitNumber = "UP-A",
                   SafetyCheckLateEntryFlag = null,
                   SafetyCheckNote = "CHECKED CIVIL BUILDING"
               }

               );

        }
    }
}