using GenerateTables.Models;
using System;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void WebserviceDetails()
        {
            Db.WebServiceEventSetting.AddRange(
                new WebServiceEventSetting
                {
                    WebServiceEventSettingId = 5,
                    EventQueueFlag = 1,
                    MaximumNumberOfAttempt = 5,
                    NotificationSmtpAddress = "smtpout.secureserver.net",
                    NotificationEmailRecipient = "sramasamy@dssiindia.com",
                    NotificationEmailSender = "jmstest@dssiindia.com",
                    HoursBetweenEmails = 5
                }
            );
            Db.WebServiceEventQueue.AddRange(
                new WebServiceEventQueue
                {
                    WebServiceEventQueueId = 5,
                    CreateBy = 11,
                    CreateDate = DateTime.Now,
                    WebServiceEventParameter1 = "20",
                    WebServiceEventAssignId = 5
                },
                new WebServiceEventQueue
                {
                    WebServiceEventQueueId = 6,
                    CreateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-3),
                    WebServiceEventParameter1 = "50",
                    WebServiceEventAssignId = 5
                }
            );
            Db.WebServiceEventType.AddRange(
                new WebServiceEventType
                {
                    WebServiceEventTypeId = 10,
                    WebServiceEventName = "CLASSIFICATION",
                    WebServiceEventInactive = 1,
                    WebServiceEventRunHistory = 1
                },
                new WebServiceEventType
                {
                    WebServiceEventTypeId = 11,
                    WebServiceEventName = "APPOINTMENT",
                    WebServiceEventInactive = 1,
                    WebServiceEventRunHistory = 1
                },
                new WebServiceEventType
                {
                    WebServiceEventTypeId = 12,
                    WebServiceEventName = "PERSON ADDRESS SAVE",
                    WebServiceEventInactive = null,
                    WebServiceEventRunHistory = 5
                },
                new WebServiceEventType
                {
                    WebServiceEventTypeId = 13,
                    WebServiceEventName = "INCIDENT HEARING SCHEDULED",
                    WebServiceEventInactive = null,
                    WebServiceEventRunHistory = null,
                    WebServiceEventParamName2 = null
                }
            );

            Db.WebServiceAuth.AddRange(
                new WebServiceAuth
                {
                    WebServiceAuthId = 1,
                    CreateDate = DateTime.Now.AddDays(-15),
                    UpdateDate = DateTime.Now.AddDays(-15),
                    CreateBy = 12,
                    UpdateBy = 11,
                    Vendor = "INTERNAL",
                    User = "atimswebservice",
                    Password = "At1ms!"
                }
                );
            Db.WebServiceEventAssign.AddRange(
                new WebServiceEventAssign
                {
                    WebServiceEventAssignId = 5,
                    WebServiceEventTypeId = 10,
                    WebServiceEventAuthId = 0
                }
            );

        }
    }
}
