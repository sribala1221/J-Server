using GenerateTables.Models;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ServerAPI.Static.Extension
{
    public static class CustomQueueParamTypeApiCast
    {
        public static Expression<Func<CustomQueueParamType, QueueParamTypeVm>> Convert = (paramType) =>
            new QueueParamTypeVm
            {
                ComponentName = paramType.ComponentName,
                Description = paramType.Description,
                Label = paramType.Label,
                Id = paramType.Id
            };

        public static QueueParamTypeVm ConvertForApi(this CustomQueueParamType param)
        {
            return new List<CustomQueueParamType>() { param }.AsQueryable().Select(Convert).FirstOrDefault();
        }

        public static IEnumerable<QueueParamTypeVm> ConvertForApi(this IQueryable<CustomQueueParamType> parameters)
        {
            return parameters.Select(Convert).ToList();
        }
    }
}
