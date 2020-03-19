using GenerateTables.Models;
using System;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void BailDetails()
        {
            Db.BailTransaction.AddRange(
                new BailTransaction
                {
                    ArrestId = 5,
                    BailTransactionId = 5,
                    AmountPosted = 500,
                    CreateDate = DateTime.Now.AddDays(-15),
                    CreateBy = 11,
                    BailAgentId = 10,
                    BailCompanyId = 100
                },
                new BailTransaction
                {
                    ArrestId = 5,
                    BailTransactionId = 6,
                    AmountPosted = 1000,
                    CreateDate = DateTime.Now.AddDays(-15),
                    CreateBy = 12,
                    BailAgentId = 10,
                    BailCompanyId = 100
                },
                new BailTransaction
                {
                    BailTransactionId = 7,
                    ArrestId = 14,
                    CreateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-4),
                    AmountPosted = 1500,
                    BailAgentId = 11,
                    BailCompanyId = 101
                },
                new BailTransaction
                {
                    ArrestId = 5,
                    BailTransactionId = 8,
                    AmountPosted = 500,
                    CreateDate = DateTime.Now.AddDays(-15),
                    CreateBy = 12,
                    BailAgentId = 10,
                    BailCompanyId = 100
                },
                new BailTransaction
                {
                    BailTransactionId = 9,
                    ArrestId = 7,
                    AmountPosted = 1000,
                    CreateDate = DateTime.Now.AddDays(-15),
                    CreateBy = 12,
                    BailAgentId = 11,
                    BailCompanyId = 101
                },
                new BailTransaction
                {
                    BailTransactionId = 10,
                    ArrestId = 8,
                    AmountPosted = 2500,
                    CreateDate = DateTime.Now.AddDays(-15),
                    CreateBy = 11,
                    BailAgentId = 12,
                    BailCompanyId = 101
                }
            );

            Db.BailAgent.AddRange(
                new BailAgent
                {
                    BailAgentId = 10,
                    BailAgentCreateBy = 12,
                    BailAgentCreateDate = DateTime.Now.AddDays(-10),
                    BailAgentUpdateBy = 11,
                    BailAgentUpdateDate = DateTime.Now,
                    BailAgentDeleteBy = null,
                    BailAgentDeleteDate = null,
                    BailAgentFirstName = "RAJIV",
                    BailAgentLastName = "BHAI",
                    BailAgentMiddleName = "GANDHI"
                },
                new BailAgent
                {
                    BailAgentId = 11,
                    BailAgentCreateBy = 11,
                    BailAgentCreateDate = DateTime.Now.AddDays(-9),
                    BailAgentUpdateBy = 12,
                    BailAgentUpdateDate = DateTime.Now,
                    BailAgentDeleteBy = 11,
                    BailAgentDeleteDate = DateTime.Now,
                    BailAgentFirstName = "MARVEL",
                    BailAgentLastName = "RAW",
                    BailAgentMiddleName = null
                },
                new BailAgent
                {
                    BailAgentId = 12,
                    BailAgentCreateBy = 11,
                    BailAgentCreateDate = DateTime.Now.AddDays(-10),
                    BailAgentUpdateBy = 11,
                    BailAgentUpdateDate = DateTime.Now,
                    BailAgentDeleteBy = null,
                    BailAgentDeleteDate = null,
                    BailAgentDeleteFlag = null,
                    BailAgentFirstName = null,
                    BailAgentLastName = null,
                    BailAgentMiddleName = null
                }

            );
            Db.BailAgentHistory.AddRange(
                
                new BailAgentHistory
                {
                    BailAgentHistoryId = 10,
                    BailAgentId = 10,
                    CreateDate = DateTime.Now.AddDays(-10),
                    PersonnelId = 11,
                    BailAgentHistoryList = @"{'LAST NAME':'BHAI','FIRST NAME':'RAJIV','MIDDLE NAME':'GANDHI','LICENSE NUMBER':'56565','BAIL COMPANIES':'HIFI'}"
                },

                new BailAgentHistory
                {
                    BailAgentHistoryId = 11,
                    BailAgentId = 10,
                    CreateDate = DateTime.Now.AddDays(-10),
                    PersonnelId = 11,
                    BailAgentHistoryList = @"{'LAST NAME':'BHAI','FIRST NAME':'RAJIV','MIDDLE NAME':'GANDHI','LICENSE NUMBER':'56565','BAIL COMPANIES':'HIFI'}"
                }
                );
            Db.BailSaveHistory2.AddRange(
                new BailSaveHistory2
                {
                    BailSaveHistory2Id = 10,
                    ArrestId = 5,
                    BailType = "BAIL AMOUNT FOR WARRANT",
                    CreateDate = DateTime.Now.AddDays(-10),
                    CreateBy = 11,
                    BailAmount = 10000
                },
                new BailSaveHistory2
                {
                    BailSaveHistory2Id = 11,
                    ArrestId = 5,
                    BailType = "BAIL AMOUNT FOR WARRANT",
                    CreateDate = DateTime.Now.AddDays(-10),
                    CreateBy = 12,
                    BailAmount = 5000
                }

                );
            Db.BailCompany.AddRange(
                
                new BailCompany
                {
                    BailCompanyId =100,
                    BailCompanyInactiveFlag =  null,
                    BailCompanyAddressNum = "415",
                    BailCompanyCity = "INDIA",
                    DeleteBy = null,
                    DeleteDate = null,
                    DeleteFlag = null,
                    BailCompanyName = "HIFI",
                    BailCompanyStreetName = "SRI RAM NAGAR",
                    BailCompanyState = "TN",
                    CreateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-10),
                    UpdateBy = 12,
                    UpdateDate = DateTime.Now
                   
                },
                new BailCompany
                {
                    BailCompanyId = 101,
                    BailCompanyInactiveFlag = null,
                    BailCompanyAddressNum = "416",
                    BailCompanyCity = "INDIA",
                    DeleteBy = null,
                    DeleteDate = null,
                    DeleteFlag = null,
                    BailCompanyName = "MSS",
                    BailCompanyStreetName = "SATHYA SAI NAGAR",
                    BailCompanyState = "TN",
                    CreateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-10),
                    UpdateBy = null,
                    UpdateDate = null
                },
                new BailCompany
                {
                    BailCompanyId = 102,
                    BailCompanyInactiveFlag = null,
                    BailCompanyAddressNum = "416",
                    BailCompanyCity = "INDIA",
                    DeleteBy = null,
                    DeleteDate = null,
                    DeleteFlag = null,
                    BailCompanyName = "HIATH",
                    BailCompanyStreetName = "RAM RAJ NAGAR",
                    BailCompanyState = "TN",
                    CreateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-10),
                    UpdateBy = 12,
                    UpdateDate = DateTime.Now
                }
                );

            Db.BailCompanyAgentXref.AddRange(
                new BailCompanyAgentXref
                {
                    BailCompanyAgentXrefId = 10,
                    BailAgentId =11,
                    CreateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = 0,
                    DeleteDate = null,
                    DeleteBy = null,
                    BailCompanyId = 100
                   
                },
                new BailCompanyAgentXref
                {
                    BailCompanyAgentXrefId = 11,
                    BailAgentId = 12,
                    CreateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = 0,
                    DeleteDate = null,
                    DeleteBy = null,
                    BailCompanyId = 100
                }
                );
        }
    }
}
