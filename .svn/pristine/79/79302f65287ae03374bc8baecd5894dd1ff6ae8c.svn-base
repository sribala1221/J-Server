using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ServerAPI.ViewModels;
using ServerAPI.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using JwtDb.Models;
using System.Text;
using Microsoft.AspNetCore.Mvc.Filters;
using ServerAPI.Services;

namespace ServerAPI.Policies
{
    public class FuncPermissionRequirement : IAuthorizationRequirement
    {
        public int FunctionalityId { get; set; }
        public FuncPermissionType PermissionType { get; set; }
        public string Condition { get; set; }  
    }
    public class FuncPermissionHandler : AttributeAuthorizationHandler<FuncPermissionRequirement, FuncPermissionAttribute>
    {
        private readonly ILogger<FuncPermissionHandler> _logger;

        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPermissionsService _permissionsService;

        public FuncPermissionHandler(ILogger<FuncPermissionHandler> logger, UserManager<AppUser> userManager, 
            RoleManager<IdentityRole> roleManager, IHttpContextAccessor httpContextAccessor, IPermissionsService permissionsService)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
            _permissionsService = permissionsService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            FuncPermissionRequirement requirement, IEnumerable<FuncPermissionAttribute> attributes)
        {
            bool allowed = false;
            ClaimsPrincipal user = context.User;
            IEnumerable<FuncPermissionAttribute> funcPermissionAttributes = attributes as FuncPermissionAttribute[] ?? attributes.ToArray();
            requirement.FunctionalityId = funcPermissionAttributes.Select(s => s.FunctionalityId).First();
            requirement.PermissionType = funcPermissionAttributes.Select(s => s.PermissionType).SingleOrDefault();
            requirement.Condition = funcPermissionAttributes.Select(s => s.Condition).SingleOrDefault();
            
            string permissionTypeString = "FunctionPermission" + requirement.FunctionalityId + requirement.PermissionType;
            string userName = user.Claims.Single(s => s.Type == ClaimTypes.NameIdentifier).Value;
            AppUser userToVerify = await _userManager.FindByNameAsync(userName);

            IList<string> roles = await _userManager.GetRolesAsync(userToVerify);
            IList<IdentityRole> rolesList = roles.Select(async s => await _roleManager.FindByNameAsync(s)).Select(t => t.Result).ToList();

            bool? conditionResult = null;
            if (requirement.Condition != null) {
                conditionResult = _permissionsService.FunctionPermissionConditionCheck(requirement.Condition, _httpContextAccessor.HttpContext);
                permissionTypeString += "_" + conditionResult;
            }

            foreach (IdentityRole role in rolesList) {
                IList<Claim> forEachClaimList = await _roleManager.GetClaimsAsync(role);
                if (!forEachClaimList.Any(x => x.Type == "Group_Admin"
                || x.Type == permissionTypeString))
                    continue;
            
                    allowed = true;
                    break;
            }
            string header = _httpContextAccessor.HttpContext.Request.Headers["FunctionPermissionCheck"];
            if (allowed && header != "true")
            {
                _logger.LogInformation($"Permission {requirement.FunctionalityId} {requirement.PermissionType} Granted");
                context.Succeed(requirement);
            }
            else if (allowed && header == "true")
            {
               context.Fail();
            }
            else
            {
                AuthorizationFilterContext filterContext = context.Resource as AuthorizationFilterContext;
                HttpResponse response = filterContext?.HttpContext.Response;
                response?.OnStarting(async state => {
                    byte[] resp = Encoding.ASCII.GetBytes($"{requirement.FunctionalityId} {requirement.PermissionType}" + " " + conditionResult ?? "");
                    await response.Body.WriteAsync(resp, 0, resp.Length);
                }, null);
                context.Fail();
            }
        }
    }
}

