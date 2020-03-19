using GenerateTables.Models;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ServerAPI.Static.Extension
{
    public static class CustomQueueParamApiCast
    {
        public static Expression<Func<CustomQueueParam, QueueParameterOptionalVm>> Convert = (param) =>
            new QueueParameterOptionalVm
            {
                CustomQueueParamId = param.CustomQueueParamId,
                Index = param.ParamIndex ?? 0,
                Label = param.ParamLabel,
                Description = param.ParamDescription,
                FreeForm = param.ParamFlagFreeForm ?? 0,
                CheckBoxFlag = param.ParamFlagCheckbox ?? 0,
                FieldDate = param.ParamFlagDate,
                FieldDateFlag = param.ParamFlagDate != null ? 1 : 0,
                DeleteFlag = param.DeleteFlag == 1,
                ParamType = param.ParamTypeId.HasValue 
                ? new List<CustomQueueParamType> { param.ParamType }
                    .AsQueryable()
                    .Select(CustomQueueParamTypeApiCast.Convert)
                    .FirstOrDefault() 
                : null
            };

        public static QueueParameterOptionalVm ConvertForApi(this CustomQueueParam param)
        {
            return new List<CustomQueueParam>() { param }.AsQueryable().Select(Convert).FirstOrDefault();
        }

        public static IEnumerable<QueueParameterOptionalVm> ConvertForApi(this IQueryable<CustomQueueParam> parameters)
        {
            return parameters.Select(Convert).ToList();
        }
    }
}
