using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using JwtDb.Models;
using System;
using ServerAPI.Utilities;
using System.Collections.Generic;
using ServerAPI.ViewModels;
using System.Security.Claims;
using System.Linq;
using GenerateTables.Models;

namespace ServerAPI.Policies
{
    public class UserPermissionPolicy : IUserPermissionPolicy
    {
        private readonly AAtims _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly int _personnelId;

        public UserPermissionPolicy(AAtims context,
        UserManager<AppUser> userManager, 
        RoleManager<IdentityRole> roleManager,
        IHttpContextAccessor iHttpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _personnelId = Convert.ToInt32(iHttpContextAccessor.HttpContext.User
            .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
        }
        //Get claims list based on user role
        public List<string> GetClaims()
        {
            Claim claim = new Claim(CustomConstants.PERSONNELID, _personnelId.ToString());

            AppUser appUser = _userManager.GetUsersForClaimAsync(claim).Result.FirstOrDefault();

            List<string> roleName = _userManager.GetRolesAsync(appUser).Result.ToList();

            List<IdentityRole> roles = _roleManager.Roles.Where(w=>roleName.Contains(w.Name)).ToList(); 

            List<string> roleClaims = new List<string>();

            roles.ForEach(f=>{
                roleClaims.AddRange(_roleManager.GetClaimsAsync(f).Result.Select(s=>s.Type).ToList());
            }); 

            roleClaims = roleClaims.Distinct().ToList();
            
            return roleClaims;
        }
        //Application access permission
        public List<KeyValuePair<string,int>> GetAppPermission()
        {
            List<string> roleClaims = GetClaims();
            List<KeyValuePair<string,int>> appData = _context.AppAo.Select(s=> new KeyValuePair<string,int>(s.AppAoName,s.AppAoId)).ToList();
            appData.ToList().ForEach(f=>
            {
                if(!(roleClaims.Contains(PermissionTypes.Admin) || roleClaims.Contains(PermissionTypes.Application+PermissionTypes.Permission+f.Value)))
                {
                    appData.Remove(f);
                }
            });
            return appData;
        }
        //Module & Submodule access permission
        public List<Module> GetMenuPermission(List<Module> lstModule, int appAoId)
        {
            List<string> roleClaims = GetClaims();
            lstModule.ToList().ForEach(f=>{
                if(!(roleClaims.Contains(PermissionTypes.Admin) || roleClaims.Contains(PermissionTypes.Application+PermissionTypes.AllAccess+appAoId)))
                {
                    if(!roleClaims.Contains(PermissionTypes.Module+PermissionTypes.Permission+f.ModuleId))
                    {
                        lstModule.Remove(f);
                    } else
                    {
                        if(!roleClaims.Contains(PermissionTypes.Module+PermissionTypes.AllAccess+f.ModuleId))
                        {
                            f.SubModuleList.ToList().ForEach(sub=>{
                                if(!roleClaims.Contains(PermissionTypes.SubModule+PermissionTypes.Permission+sub.SubModuleId))
                                {
                                    lstModule.Find(x=>x == f).SubModuleList.Remove(sub);
                                } else
                                {
                                    if(!roleClaims.Contains(PermissionTypes.SubModule+PermissionTypes.AllAccess+sub.SubModuleId))
                                    {
                                        sub.SubModuleDetailList.ToList().ForEach(det=>{
                                            if(!roleClaims.Contains(PermissionTypes.Detail+PermissionTypes.Permission+det.DetailId))
                                            {
                                                lstModule.Find(x=>x == f).SubModuleList.Find(y=> y == sub).SubModuleDetailList.Remove(det);
                                            }
                                        });
                                    }
                                }
                            });
                        }
                    }
                }
            });
            return lstModule;
        }
        //Flag alerts access permission
        public List<LookupVm> GetFlagPermission(List<LookupVm> staticFlagLst)
        {
            List<string> roleClaims = GetClaims();
            staticFlagLst.ToList().ForEach(f=>{
                    if(!(roleClaims.Any(c=> c == PermissionTypes.Admin
                    || c == PermissionTypes.PersonFlag+PermissionTypes.Access+f.LookupIdentity 
                    || c == PermissionTypes.InmateFlag+PermissionTypes.Access+f.LookupIdentity
                    || c == PermissionTypes.MedFlag+PermissionTypes.Access+f.LookupIdentity
                    || (c == PermissionTypes.PersonFlag+PermissionTypes.AllAccess && f.LookupType == LookupConstants.PERSONCAUTION)
                    || (c == PermissionTypes.InmateFlag+PermissionTypes.AllAccess && f.LookupType == LookupConstants.TRANSCAUTION)
                    || (c == PermissionTypes.MedFlag+PermissionTypes.AllAccess && (f.LookupType == LookupConstants.DIET || f.LookupType == LookupConstants.MEDFLAG))
                    )))
                    {
                        staticFlagLst.Remove(f);
                    }
                });
            return staticFlagLst;
        }
    }
}
