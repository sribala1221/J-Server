﻿using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{ 
    public class CustomQueueDetailsVm
    {
        public int CustomQueueId { get; set; }
        public string QueueGroupName { get; set; }
        public string QueueDescription { get; set; }
        public string QueueName { get; set; }
        public string QueueSql { get; set; }
        public bool DeleteFlag { get; set; }
        public string QueueConnectionString { get; set; }
        public int ColumnInmateId { get; set; }
        public int ColumnArrestId { get; set; }
        public bool ExternalData { get; set; }
        public bool ShowQueueKiosk { get; set; }
    }

    public class QueueParameterOptionalVm
    {
        public int CustomQueueParamId { get; set; }
        public int Index { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public int FreeForm { get; set; }
        public int CheckBoxFlag { get; set; }
        public DateTime? FieldDate { get; set; }
        public int FieldDateFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime? FieldDateValue { get; set; }
        public string FreeFormValue { get; set; }
        public bool CheckBoxValue { get; set; }

        public QueueParamTypeVm ParamType { get; set; }
    }

    public class QueueParamTypeVm
    {
        public int Id { get; set; }

        public string Label { get; set; }
        
        public string Description { get; set; }
        
        public string ComponentName { get; set; }
    }

    public class CustomQueueSearchInput
    {
        public CustomQueueDetailsVm CustomQueueItem { get; set; }
        public List<QueueParameterOptionalVm> CustomQueueParam { get; set; }
    }
}
