using GenerateTables.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void CustomDetails()
        {
            Db.CustomFieldLookup.AddRange(
                new CustomFieldLookup
                {
                    CustomFieldLookupId = 20,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = 0,
                    DeleteDate = null,
                    UpdateDate = DateTime.Now.AddDays(-10),
                    CreateBy = 10,
                    FieldTypeDropDownFlag = 0,
                    FieldTypeCheckboxFlag = 0,
                    UpdateBy = 11,
                    DeleteBy = 0,
                    FieldSizeLarge = 0,
                    FieldSizeMedium = 1,
                    FieldSizeSmall = 0,
                    FieldLabel = "NUMBER"
                }
            );
            Db.CustomFieldSaveData.AddRange(
                new CustomFieldSaveData
                {
                    CustomFieldSaveDataId = 10,
                    CreateDate = DateTime.Now.AddDays(-16),
                    UpdateDate = DateTime.Now.AddDays(-10),
                    CreateBy = 11,
                    UpdateBy = 12,
                    CustomFieldLookupId = 20
                }
                );
        }
    }
}
