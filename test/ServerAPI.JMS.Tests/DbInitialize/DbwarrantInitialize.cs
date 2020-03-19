using GenerateTables.Models;
using System;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void Warrantdetails()
        {
            Db.WarrantHold.AddRange(
                new WarrantHold
                {
                    WarrantHoldId = 10,
                    CreateDate = DateTime.Now.AddDays(-2),
                    PersonId = 50,
                    UpdateDate = DateTime.Now,
                    WarrantHoldLocation = "CHEANNAI",
                    WarrantHoldIssued = DateTime.Now,
                    WarrantHoldNote = "INVOLVED THREE COLLEGE BOYS"
                },
                new WarrantHold
                {
                    WarrantHoldId = 11,
                    CreateDate = DateTime.Now,
                    PersonId = 55,
                    UpdateDate = DateTime.Now,
                    WarrantHoldLocation = "MADURAI",
                    WarrantHoldIssued = DateTime.Now,
                    WarrantHoldNote = "HE IS A POLITICIAN"
                },
                new WarrantHold
                {
                    WarrantHoldId = 12,
                    CreateDate = DateTime.Now,
                    PersonId = 60,
                    UpdateDate = DateTime.Now,
                    WarrantHoldLocation = "ERODE",
                    WarrantHoldIssued = DateTime.Now,
                    WarrantHoldNote = "CASE DISMISSED"
                });
            Db.Warrant.AddRange(
                new Warrant
                {
                    WarrantId = 5,
                    CreateDate = DateTime.Now.AddDays(-10),
                    PersonId = 50,
                    UpdateDate = DateTime.Now,
                    LocalWarrantFlag = 1,
                    WarrantClearedDate = null,
                    ArrestId = 6,
                    WarrantNumber = "WARNUM4500"
                },
                new Warrant
                {
                    WarrantId = 6,
                    CreateDate = DateTime.Now.AddDays(-10),
                    PersonId = 55,
                    UpdateDate = DateTime.Now,
                    LocalWarrantFlag = 1,
                    WarrantClearedDate = null,
                    ArrestId = 5,
                    WarrantNumber = "WARNUM4501",
                    WarrantBailType = "BAIL",
                    WarrantBailAmount = 5000,
                    WarrantDescription = "NO FIR FILE BOOKED",
                    WarrantCounty = "THIRUVANMAIYUR",
                    WarrantType = "BENCH"
                },
                new Warrant
                {
                    WarrantId = 7,
                    CreateDate = DateTime.Now.AddDays(-10),
                    PersonId = 55,
                    UpdateDate = DateTime.Now,
                    LocalWarrantFlag = 0,
                    WarrantClearedDate = DateTime.Now,
                    ArrestId = 7,
                    WarrantNumber = "WARNUM4100",
                    WarrantBailType = "NO BAIL",
                    WarrantBailAmount = 1500,
                    WarrantDescription = null,
                    WarrantCounty = "ADAYAR",
                    WarrantType = "ARREST",
                    WarrantAgencyId = 5
                },
                new Warrant
                {
                    WarrantId = 8,
                    CreateDate = DateTime.Now.AddDays(-10),
                    PersonId = 60,
                    UpdateDate = DateTime.Now,
                    LocalWarrantFlag = 1,
                    WarrantClearedDate = DateTime.Now,
                    ArrestId = 6,
                    WarrantNumber = "WARNU7845",
                    WarrantBailAmount = 4000,
                    WarrantDescription = "NO FIR FILE BOOKED",
                    WarrantCounty = "MEDAVAKKAM",
                    WarrantType = "RELEASE",
                    WarrantChargeType = "I"
                },
                new Warrant
                {
                    WarrantId = 9,
                    CreateDate = DateTime.Now.AddDays(-9),
                    PersonId = 70,
                    UpdateDate = DateTime.Now,
                    LocalWarrantFlag = 1,
                    WarrantClearedDate = DateTime.Now,
                    ArrestId = 8,
                    WarrantNumber = "WARNU7846",
                    WarrantBailAmount = 4000,
                    WarrantDescription = "DRINK AND DRIVE",
                    WarrantCounty = "NUNGABAKAM",
                    WarrantType = "HOLD",
                    WarrantChargeType = "I"
                });
        }
    }
}
