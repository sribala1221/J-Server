using Microsoft.AspNetCore.Authorization;
using ServerAPI.ViewModels;
using System;

namespace ServerAPI.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class FuncPermissionAttribute : AuthorizeAttribute
    {
        public int FunctionalityId { get; }
        public string Condition { get; }

        public FuncPermissionType PermissionType { get; }
        public FuncPermissionAttribute(int functionalityId, FuncPermissionType permissionType, string condition = null) : base("FuncPermission")
        {
            FunctionalityId = functionalityId;
            PermissionType = permissionType;
            Condition = condition;
        }
    }
}
