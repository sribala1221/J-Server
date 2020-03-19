using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.ViewModels;
using GenerateTables.Models;
using ServerAPI.Policies;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class ModuleController : ControllerBase
    {
        private readonly AAtims _context;
        private readonly IUserPermissionPolicy _userPermissionPolicy;

        public ModuleController(AAtims context,
        IUserPermissionPolicy userPermissionPolicy)
        {
            _context = context;
            _userPermissionPolicy = userPermissionPolicy;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            List<Module> lstModule = (from a in _context.AppAo
            from m in _context.AppAoModule
            where a.AppAoId == m.AppAoId && a.AppAoVisible == 1 && a.AppAoId == id
                  && m.AppAoModuleVisible == 1
            select new Module {
                ModuleName = m.AppAoModuleName,
                ModuleId = m.AppAoModuleId,
                AppAoModuleIcon = m.AppAoModuleIcon,
                SubModuleList = (from sm in _context.AppAoSubModule
                    where m.AppAoModuleId == sm.AppAoModuleId 
                          && sm.AppAoSubModuleVisible == 1
                                 select new SubModule {
                        SubModuleName = sm.AppAoSubModuleName,
                        Route = sm.AppAoSubModuleRoute,
                        SubModuleId = sm.AppAoSubModuleId,
                        SubModuleOrder =sm.AppAoSubModuleOrder,
                        SubModuleDetailList = _context.AppAoDetail
                                  .Where(w => w.AppAoSubModuleId == sm.AppAoSubModuleId && w.AppAoDetailVisible == 1)
                                  .Select(s => new SubModuleDetail
                                  {
                                      DetailName = s.AppAoDetailName,
                                      DetailRoute = s.AppAoDetailRoute,
                                      DetailTooltip = s.AppAoDetailToolTip,
                                      DetailOrder = s.AppAoDetailOrder,
                                      DetailId = s.AppAoDetailId
                                  }).OrderBy(d=> d.DetailOrder).ToList()
               }).OrderBy(o=>o.SubModuleOrder).ToList()
            }).ToList();
            lstModule = _userPermissionPolicy.GetMenuPermission(lstModule,id);
            return Ok(lstModule);
        }
    }
}