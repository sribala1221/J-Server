using GenerateTables.Models;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void RecordDetails()
        {
            Db.RecordsCheckRequest.AddRange(
                new RecordsCheckRequest
                {
                    RecordsCheckRequestId = 10,
                    DeleteFlag = 0,
                    PersonId = 50,
                    RequestFacilityId = 1,
                    ResponseFlag = 1
                },
                new RecordsCheckRequest
                {
                    RecordsCheckRequestId = 11,
                    DeleteFlag = 0,
                    PersonId = 55,
                    RequestFacilityId = 2
                },
                new RecordsCheckRequest
                {
                    RecordsCheckRequestId = 12,
                    DeleteFlag = 0,
                    PersonId = 60,
                    RequestFacilityId = 1,
                    ResponseFlag = 1,
                    RequestBy = 12
                },
                new RecordsCheckRequest
                {
                    RecordsCheckRequestId = 13,
                    DeleteFlag = 0,
                    PersonId = 80,
                    RequestFacilityId = 2,
                    ResponseFlag = 1,
                    RequestBy = 11,
                    ResponseBy = 12
                },
                new RecordsCheckRequest
                {
                    RecordsCheckRequestId = 14,
                    DeleteFlag = 1,
                    DeleteBy = 11,
                    PersonId = 80,
                    RequestFacilityId = 1,
                    ResponseFlag = 1,
                    RequestBy = 11,
                    ClearBy = 12,
                    ResponseBy = 11,
                    ClearFlag = 1
                },
                new RecordsCheckRequest
                {
                    RecordsCheckRequestId = 15,
                    PersonId = 85,
                    RequestFacilityId = 2,
                    ResponseFlag = 1,
                    RequestBy = 12,
                    ClearBy = 11,
                    ResponseBy = 12
                }
            );

            Db.RecordsCheckRequestActions.AddRange(
                new RecordsCheckRequestActions
                {
                    RecordsCheckRequestActionId = 5,
                    RecordsCheckRequestId = 13,
                    RecordsCheckRequestAction = "OTHER"
                },
                new RecordsCheckRequestActions
                {
                    RecordsCheckRequestActionId = 6,
                    RecordsCheckRequestId = 14,
                    RecordsCheckRequestAction = "OTHER"
                }
            );
            Db.RecordsCheckResponseAlerts.AddRange(
                new RecordsCheckResponseAlerts
                {
                    RecordsCheckRequestId = 14,
                    RecordsCheckResponseAlertId = 6,
                    ResponseAlert = "OTHER"
                },
                new RecordsCheckResponseAlerts
                {
                    RecordsCheckRequestId = 17,
                    RecordsCheckResponseAlertId = 7,
                    ResponseAlert = "OTHER"
                }
            );

            Db.RecordsCheckClearActions.AddRange(
                new RecordsCheckClearActions
                {
                    RecordsCheckRequestId = 13,
                    RecordsCheckClearActionId = 5,
                    RecordsCheckAction = null
                },
                new RecordsCheckClearActions
                {
                    RecordsCheckRequestId = 13,
                    RecordsCheckClearActionId = 7,
                    RecordsCheckAction = "OTHERS"
                }
            );

        }
    }
}
