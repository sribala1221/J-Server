using System;
using GenerateTables.Models;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void UserDetails()
        {
            Db.UserAccess.AddRange(
                new UserAccess
                {
                    UserId = 5,
                    PersonId = 50,
                    PersonnelId = 11
                },
                new UserAccess
                {
                    UserId = 6,
                    PersonId = 55,
                    PersonnelId = 2
                },
                new UserAccess
                {
                    UserId = 11,
                    PersonId = 60,
                    PersonnelId = 12,
                    UserName = "PALANI"
                }
            );
            Db.UserGroups.AddRange(
                new UserGroups
                {
                    GroupId = 5,
                    CreateDate = DateTime.Now.AddDays(-5),
                    GroupAdmin = "ENABLED",
                    GroupName = "ADMIN"
                },
                new UserGroups
                {
                    GroupId = 6,
                    CreateDate = DateTime.Now.AddDays(-5),
                    GroupAdmin = "ENABLED",
                    GroupName = "CORIZON ADMIN"
                }
                );

            Db.UserGroupXref.AddRange(
                new UserGroupXref
                {
                    UserGroupXrefId = 5,
                    UserId = 5,
                    GroupId = 5
                },
                new UserGroupXref
                {
                    UserGroupXrefId = 6,
                    UserId = 6,
                    GroupId = 6
                }
            );
            Db.UserConsoleSetting.AddRange(
                new UserConsoleSetting
                {
                    FacilityId = 1,
                    UserId = 11,
                    HousingUnitListId = 10,
                    UserConsoleSettingId = 10,
                    LocationId = 5
                }
                );
        }
    }
}
