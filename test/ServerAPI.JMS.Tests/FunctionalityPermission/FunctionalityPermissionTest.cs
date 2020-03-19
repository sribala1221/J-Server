using System;
using JwtDb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using ServerAPI.Controllers;
using ServerAPI.JMS.Tests.Helper;
using ServerAPI.Policies;
using ServerAPI.Tests;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using ServerAPI.Services;
using Xunit;
using Microsoft.Extensions.Configuration;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class FunctionalityPermissionTest
    {
        private readonly Mock<RoleManager<IdentityRole>> _roleMgr;
        private readonly IdentityRole _role;
        private readonly FuncPermissionHandler _fun;
        private readonly ClaimsPrincipal _user;
        private readonly ControllerContext _actioncontext;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();

        public FunctionalityPermissionTest(DbInitialize fixture)
        {
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            PermissionsService permissionsService = new PermissionsService(_configuration, fixture.Db);
            _user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "john") }));

            AppUser appUser = new AppUser { UserName = "john", Id = Guid.NewGuid().ToString() };

            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();

            userMgr.Setup(x => x.FindByNameAsync(It.Is<string>(u => u.Equals("john")))).ReturnsAsync(appUser);

            userMgr.Setup(x => x.GetRolesAsync(It.Is<AppUser>(u => u.Equals(appUser))))
                .ReturnsAsync(new List<string> { "Correctional_officer" });

            _roleMgr = MockHelpers.MockRoleManager<IdentityRole>();

            _role = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Correctional_officer",
                NormalizedName = "Correctional officer"
            };

            _roleMgr.Setup(x => x.FindByNameAsync(It.Is<string>(u => u.Equals("Correctional_officer")))).ReturnsAsync(_role);

            _fun = new FuncPermissionHandler(Mock.Of<ILogger<FuncPermissionHandler>>(),
                userMgr.Object, _roleMgr.Object, httpContext, permissionsService);

            _actioncontext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData(),
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()
            };

        }
        [Fact]
        public async Task FunctionalityPermissionTestAdmin()
        {
            List<Claim> idclaim = new List<Claim> { new Claim("Group_Admin", "1") };

            _roleMgr.Setup(x => x.GetClaimsAsync(It.Is<IdentityRole>(u => u.Equals(_role)))).ReturnsAsync(idclaim);

            _actioncontext.ActionDescriptor.ControllerTypeInfo = (TypeInfo)typeof(PrebookWizardController);

            _actioncontext.ActionDescriptor.MethodInfo = _actioncontext.ActionDescriptor
                .ControllerTypeInfo.GetMethod("UpdatePrebook");

            AuthorizationFilterContext authcontext = new AuthorizationFilterContext(_actioncontext, new List<IFilterMetadata>());

            AuthorizationHandlerContext context = new AuthorizationHandlerContext(
                new List<IAuthorizationRequirement> { new FuncPermissionRequirement() }, _user, authcontext);

            await _fun.HandleAsync(context);

            Assert.True(context.HasSucceeded);
        }
        [Fact]
        public async Task FunctionalityPermissionTestPass()
        {
            List<Claim> idclaim = new List<Claim> { new Claim("FunctionPermission602Edit", "1") };

            _roleMgr.Setup(x => x.GetClaimsAsync(It.Is<IdentityRole>(u => u.Equals(_role)))).ReturnsAsync(idclaim);

            _actioncontext.ActionDescriptor.ControllerTypeInfo = (TypeInfo)typeof(PrebookWizardController);

            _actioncontext.ActionDescriptor.MethodInfo = _actioncontext.ActionDescriptor
                .ControllerTypeInfo.GetMethod("UpdatePrebook");

            AuthorizationFilterContext authcontext = new AuthorizationFilterContext(_actioncontext, new List<IFilterMetadata>());

            AuthorizationHandlerContext context = new AuthorizationHandlerContext(
                new List<IAuthorizationRequirement> { new FuncPermissionRequirement() }, _user, authcontext);

            await _fun.HandleAsync(context);

            Assert.True(context.HasSucceeded);
        }
        [Fact]
        public async Task FunctionalityPermissionTestFail()
        {
            List<Claim> idclaim = new List<Claim> { new Claim("FunctionPermission604Add", "1") };

            _roleMgr.Setup(x => x.GetClaimsAsync(It.Is<IdentityRole>(u => u.Equals(_role)))).ReturnsAsync(idclaim);

            _actioncontext.ActionDescriptor.ControllerTypeInfo = (TypeInfo)typeof(PrebookWizardController);

            _actioncontext.ActionDescriptor.MethodInfo = _actioncontext.ActionDescriptor.ControllerTypeInfo.GetMethod("UpdatePrebook");

            AuthorizationFilterContext authcontext = new AuthorizationFilterContext(_actioncontext, new List<IFilterMetadata>());

            AuthorizationHandlerContext context = new AuthorizationHandlerContext(
                new List<IAuthorizationRequirement> { new FuncPermissionRequirement() }, _user, authcontext);

            await _fun.HandleAsync(context);

            Assert.False(context.HasSucceeded);
        }
    }
}
