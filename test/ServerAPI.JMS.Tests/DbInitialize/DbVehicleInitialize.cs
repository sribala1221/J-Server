using System;
using GenerateTables.Models;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void VehicleDetails()
        {

            Db.VehicleMake.AddRange(
                new VehicleMake
                {
                    VehicleMakeId = 10,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = 0,
                    DeleteDate = null,
                    DeleteBy = null,
                    CreateBy = 11,
                    VehicleMakeName = "YAMAHA",
                    MakeCode = "YAMA"
                },
                new VehicleMake
                {
                    VehicleMakeId = 11,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = 0,
                    DeleteDate = null,
                    DeleteBy = null,
                    CreateBy = 12,
                    MakeCode = "MERS",
                    VehicleMakeName = "MERCEDES-BENZ"
                },
                new VehicleMake
                {
                    VehicleMakeId = 12,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = 0,
                    DeleteDate = null,
                    DeleteBy = null,
                    CreateBy = 12,
                    MakeCode = "FERARI",
                    VehicleMakeName = "FERARI CLUB"
                }
            );


            Db.VehicleModel.AddRange(
               new VehicleModel
               {
                   VehicleModelId = 10,
                   VehicleMakeId = 11,
                   DeleteFlag = 0,
                   DeleteDate = null,
                   DeleteBy = null,
                   CreateDate = DateTime.Now.AddDays(-15),
                   CreateBy = 11,
                   ModelCode = "BENZ",
                   VehicleModelName = "E-CLASS"
               },
               new VehicleModel
               {
                   VehicleModelId = 11,
                   VehicleMakeId = 11,
                   DeleteFlag = 0,
                   DeleteDate = null,
                   DeleteBy = null,
                   CreateDate = DateTime.Now.AddDays(-15),
                   CreateBy = 12,
                   ModelCode = "BENZ",
                   VehicleModelName = "C-CLASS"
               },
               new VehicleModel
               {
                   VehicleModelId = 12,
                   VehicleMakeId = 11,
                   DeleteFlag = 0,
                   DeleteDate = null,
                   DeleteBy = null,
                   CreateDate = DateTime.Now.AddDays(-15),
                   CreateBy = 12,
                   ModelCode = "BENZ",
                   VehicleModelName = "E-CLASS"
               },
               new VehicleModel
               {
                   VehicleModelId = 13,
                   VehicleMakeId = 11,
                   DeleteFlag = 0,
                   DeleteDate = null,
                   DeleteBy = null,
                   CreateDate = DateTime.Now.AddDays(-15),
                   CreateBy = 12,
                   ModelCode = "BENZ",
                   VehicleModelName = "GLC-CLASS"
               }
              );

        }
    }
}
