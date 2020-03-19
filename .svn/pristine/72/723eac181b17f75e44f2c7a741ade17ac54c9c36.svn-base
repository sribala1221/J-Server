using GenerateTables.Models;
using System;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void AppointmentDetails()
        {

            Db.AppointmentProgramAssign.AddRange(
                new AppointmentProgramAssign
                {
                    AppointmentProgramAssignId = 5,
                    AppointmentId = 5,
                    DeleteDate = DateTime.Now,
                    DeleteFlag = 0,
                    InmateId = 100
                },
                new AppointmentProgramAssign
                {
                    AppointmentProgramAssignId = 6,
                    AppointmentId = 7,
                    DeleteFlag = 0,
                    InmateId = 101
                },
                new AppointmentProgramAssign
                {
                    AppointmentProgramAssignId = 7,
                    AppointmentId = 10,
                    DeleteFlag = 0,
                    InmateId = 102
                },
                new AppointmentProgramAssign
                {
                    AppointmentProgramAssignId = 8,
                    AppointmentId = 10,
                    DeleteFlag = 0,
                    InmateId = 120
                },
                new AppointmentProgramAssign
                {
                    AppointmentProgramAssignId = 9,
                    AppointmentId = 14,
                    CreateBy = 12
                }

            );
            Db.Agency.AddRange(
                new Agency
                {
                    AgencyId = 5,
                    AgencyCounty = "SRIRANGAM",
                    AgencyCity = "TRICHY",
                    AgencyAddress = "NO 10 PILLAIYAR KOVIL STRRET",
                    AgencyFax = "(234) 454-5454",
                    AgencyName = "JUNIOR BAIL LEAF MANAGER",
                    AgencyAbbreviation = "JBLM"
                },
                new Agency
                {
                    AgencyId = 6,
                    AgencyCounty = "MYLAPORE",
                    AgencyCity = "CHENNAI",
                    AgencyAddress = "65,GANDHI ROAD MGR NAGAR",
                    AgencyFax = "(234) 454-4574",
                    AgencyName = "SENIOR ADVOCATE",
                    AgencyAbbreviation = "SA",
                    AgencyBookingFlag = true
                },
                new Agency
                {
                    AgencyId = 7,
                    AgencyCounty = "THIRUMAYAM",
                    AgencyCity = "PUDUKKOTTAI",
                    AgencyAddress = "13,CAMP ROAD",
                    AgencyCourtFlag = true,
                    AgencyInactiveFlag = 0,
                    AgencyName = "PUBLIC DEFENDER",
                    AgencyAbbreviation = "PD",
                    AgencyJailFlag = true,
                    AgencyArrestingFlag = true
                },
                new Agency
                {
                    AgencyId = 8,
                    AgencyCounty = "MADIPAKKAM",
                    AgencyCity = "CHENNAI",
                    AgencyAddress = "15,NERHU NAGAR",
                    AgencyCourtFlag = true,
                    AgencyInactiveFlag = 0,
                    AgencyName = "INCHARGE OF IMMIGRATION DEPARTMENT",
                    AgencyAbbreviation = "ID",
                    AgencyFax = "(234) 454-5454"
                }

                );

            Db.AgencyCourtDept.AddRange(
                new AgencyCourtDept
                {
                    AgencyCourtDeptId = 10,
                    AgencyId = 5,
                    CreateDate = DateTime.Now,
                    CreateBy = 11,
                    DeleteDate = DateTime.Now.AddDays(-1),
                    DepartmentName = "CIVIL DEPT"
                },
                new AgencyCourtDept
                {
                    AgencyCourtDeptId = 15,
                    AgencyId = 6,
                    CreateDate = DateTime.Now,
                    CreateBy = 12,
                    DeleteDate = DateTime.Now,
                    DepartmentName = "CRIMINAL DEPT"
                });

        }
    }
}
