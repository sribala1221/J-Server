using GenerateTables.Models;
using JwtDb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;

using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JetBrains.Annotations;
using ServerAPI.Utilities;


namespace ServerAPI.Services
{
    [UsedImplicitly]
    public class PersonnelService : IPersonnelService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;
        private readonly JwtDbContext _jwtDbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public PersonnelService(JwtDbContext jwtDbContext, AAtims context,
            IHttpContextAccessor httpContextAccessor,
            UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _jwtDbContext = jwtDbContext;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst(UserConstants.PERSONNELID)?.Value);
        }

        public PersonnelDetailsVm GetPersonnelDetails(PersonnelFilter value)
        {
            List<IdentityUserClaim<string>> userClaimList = _jwtDbContext.UserClaims.Where(w =>
                w.ClaimType == UserConstants.PERSONNELID
                || w.ClaimType == UserConstants.USERNAMEDOMAIN
                || w.ClaimType == UserConstants.USEREXPIRES
                || w.ClaimType == UserConstants.DELETEFLAG
                || w.ClaimType == UserConstants.CREATEDATE).ToList();

            List<UserVm> user = _userManager.Users
                .Select(s => new UserVm
                {
                    UserId = s.Id,
                    UserName = s.UserName,
                    PersonnelId = userClaimList.FirstOrDefault(w => w.ClaimType == UserConstants.PERSONNELID
                                    && w.UserId == s.Id).ClaimValue,
                    AdDomain = userClaimList.FirstOrDefault(w => w.ClaimType == UserConstants.USERNAMEDOMAIN
                                    && w.UserId == s.Id).ClaimValue,
                    UserExpires = userClaimList.FirstOrDefault(w => w.ClaimType == UserConstants.USEREXPIRES
                                    && w.UserId == s.Id).ClaimValue,
                    DeleteFlag = userClaimList.FirstOrDefault(w => w.ClaimType == UserConstants.DELETEFLAG
                                    && w.UserId == s.Id).ClaimValue,
                    CreateDate = userClaimList.FirstOrDefault(w => w.ClaimType == UserConstants.CREATEDATE
                                    && w.UserId == s.Id).ClaimValue
                }).ToList();

            PersonnelDetailsVm personnelDetails = new PersonnelDetailsVm
            {
                PersonnelList = _context.Personnel.Where(w => !value.IsRefresh || value.IsRefresh
                    && (value.AgencyId == 0 || w.AgencyId == value.AgencyId)
                    && (value.LastName == null || w.PersonNavigation.PersonLastName.StartsWith(value.LastName))
                    && (value.FirstName == null || w.PersonNavigation.PersonFirstName.StartsWith(value.FirstName))
                    && (value.Number == null || w.OfficerBadgeNumber.StartsWith(value.Number)))
                .Select(s => new MonitorPersonnelVm
                {
                    PersonnelId = s.PersonnelId,
                    PersonId = s.PersonId,
                    PersonLastName = s.PersonNavigation.PersonLastName,
                    PersonFirstName = s.PersonNavigation.PersonFirstName,
                    PersonMiddleName = s.PersonNavigation.PersonMiddleName,
                    OfficerBadgeNumber = s.OfficerBadgeNumber,
                    Department = s.PersonnelDepartment,
                    Position = s.PersonnelPosition,
                    Agency = s.Agency.AgencyAbbreviation,
                    HireDate = s.PersonnelHireDate,
                    JudgeFlag = s.PersonnelJudgeFlag,
                    Commissioned = s.PersonnelCommissioned,
                    AgencyGroupFlag = s.PersonnelAgencyGroupFlag,
                    ArrestTransportOfficerFlag = s.ArrestTransportOfficerFlag,
                    ReceiveSearchOfficerFlag = s.ReceiveSearchOfficerFlag,
                    ProgramInstructorFlag = s.ProgramInstructorFlag,
                    PersonnelTerminationDate = s.PersonnelTerminationDate,
                    TerminationFlag = s.PersonnelTerminationFlag,
                    ProgramCaseFlag = s.PersonnelProgramCaseFlag,
                    OfficerBadgeNum = s.OfficerBadgeNum,
                    User = user.FirstOrDefault(w => w.PersonnelId == s.PersonnelId.ToString()),
                    IsUser = user.Count(w => w.PersonnelId == s.PersonnelId.ToString() && w.DeleteFlag == null) > 0
                }).OrderBy(o => o.PersonLastName).ThenBy(t => t.PersonFirstName).ToList()
            };

            if (value.IsRefresh && value.Role != null)
            {
                string roleId = _jwtDbContext.Roles.Where(w => w.Name == value.Role)
                    .Select(s => s.Id).FirstOrDefault();

                personnelDetails.PersonnelList = personnelDetails.PersonnelList
                    .Where(w => w.User?.UserId != null && _jwtDbContext.UserRoles.Where(u => u.RoleId == roleId)
                    .Select(s => s.UserId).Contains(w.User.UserId)).ToList();
            }

            if (value.IsRefresh) return personnelDetails;
            List<Lookup> lookups = _context.Lookup.ToList();

            //Status count
            personnelDetails.StatusCount = new List<KeyValuePair<string, int>> {
                new KeyValuePair<string,int>(StatusConstants.ALL,personnelDetails.PersonnelList.Count),
                new KeyValuePair<string,int>(StatusConstants.ACTIVE,personnelDetails.PersonnelList.Count(c=>!c.TerminationFlag)),
                new KeyValuePair<string,int>(StatusConstants.INACTIVE,personnelDetails.PersonnelList.Count(c=>c.TerminationFlag)),
            };

            //Department count
            personnelDetails.DepartmentCount = new List<KeyValuePair<string, int>> {
                new KeyValuePair<string, int>(StatusConstants.ALL,personnelDetails.PersonnelList
                    .Count(c=>!string.IsNullOrEmpty(c.Department)
                    && !c.TerminationFlag && !string.IsNullOrEmpty(c.PersonLastName)))
            };

            personnelDetails.DepartmentCount.AddRange(lookups.Where(w => w.LookupType == LookupConstants.DEPARTMENT)
                .Select(s => new KeyValuePair<string, int>(s.LookupDescription,
                    personnelDetails.PersonnelList.Count(c => !c.TerminationFlag && c.PersonLastName != null
                        && c.Department == s.LookupDescription))).OrderBy(o => o.Key));

            //Position count
            personnelDetails.PositionCount = new List<KeyValuePair<string, int>> {
                new KeyValuePair<string, int>(StatusConstants.ALL,personnelDetails.PersonnelList
                    .Count(c=>c.Position!=null && !c.TerminationFlag && c.PersonLastName != null))
            };

            personnelDetails.PositionCount.AddRange(lookups.Where(w => w.LookupType == LookupConstants.POSITION)
                .Select(s => new KeyValuePair<string, int>(s.LookupDescription,
                    personnelDetails.PersonnelList.Count(c => !c.TerminationFlag && c.PersonLastName != null
                        && c.Position == s.LookupDescription))).OrderBy(o => o.Key));

            //Agency List
            personnelDetails.AgencyList = _context.Agency.OrderBy(o => o.AgencyName).Select(s =>
                  new KeyValuePair<int, string>(s.AgencyId, s.AgencyName)).Distinct().ToList();

            //Flag count
            personnelDetails.FlagsCount = new List<KeyValuePair<string, int>> {
                new KeyValuePair<string, int>(StatusConstants.ALL,personnelDetails.PersonnelList.Count(c=>
                    (c.JudgeFlag || c.Commissioned && c.AgencyGroupFlag || c.ArrestTransportOfficerFlag ||
                    c.ReceiveSearchOfficerFlag || c.ProgramInstructorFlag || c.ProgramCaseFlag) && !c.TerminationFlag)),
                new KeyValuePair<string, int>(PersonnelConstants.JUDGE,
                    personnelDetails.PersonnelList.Count(c=>c.JudgeFlag && !c.TerminationFlag)),
                new KeyValuePair<string, int>(PersonnelConstants.COMMISSIONED,
                    personnelDetails.PersonnelList.Count(c=>c.Commissioned && !c.TerminationFlag)),
                new KeyValuePair<string, int>(PersonnelConstants.AGENCYGROUP,
                    personnelDetails.PersonnelList.Count(c=>c.AgencyGroupFlag && !c.TerminationFlag)),
                new KeyValuePair<string, int>(PersonnelConstants.ARRESTINGTRANSPORTINGOFFICER,
                    personnelDetails.PersonnelList.Count(c=> c.ArrestTransportOfficerFlag && !c.TerminationFlag)),
                new KeyValuePair<string, int>(PersonnelConstants.RECEIVINGSEARCHINGOFFICER,
                    personnelDetails.PersonnelList.Count(c=>c.ReceiveSearchOfficerFlag && !c.TerminationFlag)),
                new KeyValuePair<string, int>(PersonnelConstants.PROGRAMINSTRUCTOR,
                    personnelDetails.PersonnelList.Count(c=>c.ProgramInstructorFlag && !c.TerminationFlag)),
                new KeyValuePair<string, int>(PersonnelConstants.PROGRAMCASEMANAGER,
                    personnelDetails.PersonnelList.Count(c=>c.ProgramCaseFlag && !c.TerminationFlag))
            };

            personnelDetails.UserCount = new List<KeyValuePair<string, int>> {
                new KeyValuePair<string, int>(StatusConstants.ALL,
                    personnelDetails.PersonnelList.Count(c=>c.User?.UserId != null && c.User?.DeleteFlag==null)),
                new KeyValuePair<string, int>(PersonnelConstants.USERS,personnelDetails.PersonnelList
                    .Count(c=>c.User?.UserId != null && c.User.UserExpires==null && c.User?.DeleteFlag==null)),
                new KeyValuePair<string, int>(PersonnelConstants.EXPIRES,personnelDetails.PersonnelList
                    .Count(c=>c.User?.UserId != null && c.User.UserExpires!=null && c.User?.DeleteFlag==null)),
                new KeyValuePair<string, int>(PersonnelConstants.EXPIRED,personnelDetails.PersonnelList
                    .Count(c=>c.User?.UserId != null && c.User?.DeleteFlag==null && c.User.UserExpires!=null && Convert.ToDateTime(c.User.UserExpires)<DateTime.Now))
            };

            personnelDetails.RoleList = _roleManager.Roles
                .Select(s => new KeyValuePair<string, bool>(s.Name, false)).OrderBy(o => o.Key).ToList();
            return personnelDetails;
        }

        public PersonnelInputVm GetPersonnelInputDetails(int personnelId)
        {
            PersonnelInputVm personnelInput = new PersonnelInputVm
            {
                AgencyList = _context.Agency.Where(w => w.AgencyInactiveFlag != 1)
                    .Select(s => new KeyValuePair<int, string>(s.AgencyId, s.AgencyName))
                    .OrderBy(o => o.Value).ToList(),

                UseADAuthentication = _context.SiteSpecificVariables
                    .Select(s => s.UseAdAuthentication == 1).FirstOrDefault(),

                Roles = _roleManager.Roles
                .Select(s => new KeyValuePair<string, bool>(s.Name, false)).OrderBy(o => o.Key).ToList()
            };

            List<Lookup> lookup = _context.Lookup.Where(w => w.LookupType == LookupConstants.DEPARTMENT
            || w.LookupType == LookupConstants.POSITION).ToList();

            personnelInput.DepartmentList = lookup.Where(w => w.LookupType == LookupConstants.DEPARTMENT)
                 .OrderBy(o => o.LookupOrder).ThenBy(t => t.LookupDescription)
                .Select(s => new KeyValuePair<int, string>(s.LookupIndex, s.LookupDescription))
               .ToList();

            personnelInput.PositionList = lookup.Where(w => w.LookupType == LookupConstants.POSITION)
                 .OrderBy(o => o.LookupOrder).ThenBy(t => t.LookupDescription)
                .Select(s => new KeyValuePair<int, string>(s.LookupIndex, s.LookupDescription))
                .ToList();

            if (personnelId <= 0) return personnelInput;
            personnelInput.PersonnelDetails = _context.Personnel.Where(w => w.PersonnelId == personnelId)
                .Select(s => new MonitorPersonnelVm
                {
                    PersonnelId = s.PersonnelId,
                    PersonId = s.PersonId,
                    PersonLastName = s.PersonNavigation.PersonLastName,
                    PersonFirstName = s.PersonNavigation.PersonFirstName,
                    PersonMiddleName = s.PersonNavigation.PersonMiddleName,
                    DateOfBirth = s.PersonNavigation.PersonDob,
                    OfficerBadgeNumber = s.OfficerBadgeNumber,
                    Department = s.PersonnelDepartment,
                    Position = s.PersonnelPosition,
                    AgencyId = s.AgencyId,
                    Agency = s.Agency.AgencyName,
                    HireDate = s.PersonnelHireDate,
                    JudgeFlag = s.PersonnelJudgeFlag,
                    Commissioned = s.PersonnelCommissioned,
                    AgencyGroupFlag = s.PersonnelAgencyGroupFlag,
                    ArrestTransportOfficerFlag = s.ArrestTransportOfficerFlag,
                    ReceiveSearchOfficerFlag = s.ReceiveSearchOfficerFlag,
                    ProgramInstructorFlag = s.ProgramInstructorFlag,
                    PersonnelTerminationDate = s.PersonnelTerminationDate,
                    TerminationFlag = s.PersonnelTerminationFlag,
                    ActiveFlag = !s.PersonnelTerminationFlag,
                    Email = s.PersonNavigation.PersonEmail,
                    ProgramCaseFlag = s.PersonnelProgramCaseFlag,
                    OfficerBadgeNum = s.OfficerBadgeNum,
                }).OrderBy(o => o.PersonLastName).ThenBy(t => t.PersonFirstName).FirstOrDefault();

            UserVm user = new UserVm
            {
                UserId = _jwtDbContext.UserClaims.Where(w => w.ClaimType == UserConstants.PERSONNELID
                    && w.ClaimValue == personnelInput.PersonnelDetails.PersonnelId.ToString())
                    .Select(s => s.UserId).FirstOrDefault()
            };

            AppUser appUser = _userManager.FindByIdAsync(user.UserId).Result;

            if (user.UserId == null) return personnelInput;
            if (personnelInput.PersonnelDetails != null)
                personnelInput.PersonnelDetails.IsUser = true;

            user.UserName = appUser.UserName; //_userManager.FindByIdAsync(user.UserId).Result.UserName;

            user.Password = user.VerifyPassword = appUser.PasswordHash;// _userManager.FindByIdAsync(user.UserId).Result.PasswordHash;

            List<IdentityUserClaim<string>> userClaimList = _jwtDbContext.UserClaims
                .Where(w => w.UserId == user.UserId).ToList();

            user.DeleteFlag = userClaimList.Where(w => w.ClaimType == UserConstants.DELETEFLAG
                                                       && w.UserId == user.UserId).Select(se => se.ClaimValue).FirstOrDefault();
            user.AdDomain = userClaimList.Where(w => w.ClaimType == UserConstants.USERNAMEDOMAIN
                                                     && w.UserId == user.UserId).Select(s => s.ClaimValue)
                .FirstOrDefault();

            user.UserExpires = userClaimList.Where(w => w.ClaimType == UserConstants.USEREXPIRES
                && w.UserId == user.UserId).Select(s => s.ClaimValue).FirstOrDefault();

            if (user.DeleteFlag == "1")
            {
                user.Password = user.VerifyPassword = null;
                if (personnelInput.PersonnelDetails != null) personnelInput.PersonnelDetails.IsUser = false;
            }

            if (user.UserExpires != null)
            {
                user.Expires = true;
            }

            user.CreateDate = userClaimList.Where(w => w.ClaimType == UserConstants.CREATEDATE
                && w.UserId == user.UserId).Select(s => s.ClaimValue).FirstOrDefault();
            if (personnelInput.PersonnelDetails != null)
            {
                personnelInput.PersonnelDetails.DeleteFlag = userClaimList
                    .Where(w => w.ClaimType == UserConstants.DELETEFLAG
                    && w.UserId == user.UserId).Select(s => s.ClaimValue).FirstOrDefault();

                personnelInput.PersonnelDetails.User = user;
            }

            user.UserLocked = appUser.AccessFailedCount >= 5;

            personnelInput.RoleList = _userManager.GetRolesAsync(appUser).Result.ToList();

            return personnelInput;
        }

        public async Task<int> InsertUserPersonnel(PersonnelInputVm value)
        {
            if (value.Mode == "Add")
            {
                Person person = new Person
                {
                    PersonLastName = value.PersonnelDetails.PersonLastName,
                    PersonMiddleName = value.PersonnelDetails.PersonMiddleName,
                    PersonFirstName = value.PersonnelDetails.PersonFirstName,
                    CreateBy = _personnelId,
                    CreateDate = DateTime.Now,
                    FknLastName = value.PersonnelDetails.PersonLastName,
                    FknMiddleName = value.PersonnelDetails.PersonMiddleName,
                    FknFirstName = value.PersonnelDetails.PersonFirstName,
                    PersonEduGed = 0,
                    PersonEmail = value.PersonnelDetails.Email,
                    PersonDob = value.PersonnelDetails.DateOfBirth,
                    PersonAge = 0,
                    PersonContactId = 0,
                    IllegalAlienFlag = false,
                    UsCitizenFlag = false,
                };
                _context.Person.Add(person);

                Personnel personnel = new Personnel
                {
                    AgencyId = value.PersonnelDetails.AgencyId,
                    PersonnelNumber = value.PersonnelDetails.OfficerBadgeNumber,
                    OfficerBadgeNumber = value.PersonnelDetails.OfficerBadgeNumber,
                    OfficerBadgeNum = value.PersonnelDetails.OfficerBadgeNum,
                    PersonnelDepartment = value.PersonnelDetails.Department,
                    PersonnelPosition = value.PersonnelDetails.Position,
                    PersonnelHireDate = value.PersonnelDetails.HireDate,
                    PersonnelTerminationDate = !value.PersonnelDetails.ActiveFlag ? value.PersonnelDetails.PersonnelTerminationDate : null,
                    PersonnelBadgeExpirationDate = value.PersonnelDetails.ExpireDate,
                    PersonnelTerminationFlag = !value.PersonnelDetails.ActiveFlag,
                    CreateDate = DateTime.Now,
                    PersonId = person.PersonId,
                    PersonnelJudgeFlag = value.PersonnelDetails.JudgeFlag,
                    PersonnelAgencyGroupFlag = value.PersonnelDetails.AgencyGroupFlag,
                    PersonnelCommissioned = value.PersonnelDetails.Commissioned,
                    ArrestTransportOfficerFlag = value.PersonnelDetails.ArrestTransportOfficerFlag,
                    ReceiveSearchOfficerFlag = value.PersonnelDetails.ReceiveSearchOfficerFlag,
                    ProgramInstructorFlag = value.PersonnelDetails.ProgramInstructorFlag,
                    PersonnelProgramCaseFlag = value.PersonnelDetails.ProgramCaseFlag
                };

                _context.Personnel.Add(personnel);

                await _context.SaveChangesAsync();

                if (value.PersonnelDetails.IsUser)
                {
                    AppUser userIdentity = new AppUser
                    {
                        UserName = value.PersonnelDetails.User.UserName,
                        Email = value.PersonnelDetails.Email,
                        AccessFailedCount = 0
                    };

                    if (value.PersonnelDetails.User.Password != null)
                    {
                        await _userManager.CreateAsync(userIdentity,
                       value.PersonnelDetails.User.Password);
                    }
                    else
                    {
                        await _userManager.CreateAsync(userIdentity);
                    }

                    List<Claim> claims = new List<Claim>
                    {
                    new Claim(UserConstants.GIVENNAME, value.PersonnelDetails.PersonFirstName),
                    new Claim(UserConstants.FAMILYNAME, value.PersonnelDetails.PersonLastName),
                    new Claim(UserConstants.PERSONNELID, personnel.PersonnelId.ToString()),
                    new Claim(UserConstants.FACILITYID, value.PersonnelDetails.FacilityId.ToString()),
                    new Claim(UserConstants.BADGENUMBER, value.PersonnelDetails.OfficerBadgeNumber),
                    new Claim(UserConstants.CREATEDATE,DateTime.Now.ToString(CultureInfo.InvariantCulture))
                    };

                    if (value.PersonnelDetails.PersonMiddleName != null)
                    {
                        claims.Add(new Claim(UserConstants.MIDDLENAME, value.PersonnelDetails.PersonMiddleName));
                    }

                    if (value.PersonnelDetails.User.UserExpires != null)
                    {
                        claims.Add(new Claim(UserConstants.USEREXPIRES, value.PersonnelDetails.User.UserExpires));
                    }
                    if (value.PersonnelDetails.User.TestUser)
                    {
                        claims.Add(new Claim(UserConstants.TESTUSER, value.PersonnelDetails.User.TestUser.ToString()));
                    }

                    if (value.PersonnelDetails.User.DefaultDomain && value.PersonnelDetails.User.AdDomain != null)
                    {
                        claims.Add(new Claim(UserConstants.USERNAMEDOMAIN, value.PersonnelDetails.User.AdDomain));
                    }

                    await _userManager.AddClaimsAsync(userIdentity, claims);

                    if (value.RoleList.Count > 0)
                    {
                        await _userManager.AddToRolesAsync(userIdentity, value.RoleList);
                    }

                    await _jwtDbContext.SaveChangesAsync();
                }
            }
            else
            {
                Personnel personnel = _context.Personnel.SingleOrDefault(s => s.PersonnelId == value.PersonnelDetails.PersonnelId);

                if (!(personnel is null))
                {
                    personnel.AgencyId = value.PersonnelDetails.AgencyId;
                    personnel.PersonnelNumber = value.PersonnelDetails.OfficerBadgeNumber;
                    personnel.OfficerBadgeNumber = value.PersonnelDetails.OfficerBadgeNumber;
                    personnel.OfficerBadgeNum = value.PersonnelDetails.OfficerBadgeNum;
                    personnel.PersonnelJudgeFlag = value.PersonnelDetails.JudgeFlag;
                    personnel.UpdateDate = DateTime.Now;
                    personnel.PersonnelAgencyGroupFlag = value.PersonnelDetails.AgencyGroupFlag;
                    personnel.PersonnelDepartment = value.PersonnelDetails.Department;
                    personnel.PersonnelPosition = value.PersonnelDetails.Position;
                    personnel.PersonnelCommissioned = value.PersonnelDetails.Commissioned;
                    personnel.PersonnelHireDate = value.PersonnelDetails.HireDate;
                    personnel.PersonnelTerminationDate = !value.PersonnelDetails.ActiveFlag ?
                        value.PersonnelDetails.PersonnelTerminationDate : null;
                    personnel.PersonnelBadgeExpirationDate = value.PersonnelDetails.ExpireDate;
                    personnel.PersonnelTerminationFlag = !value.PersonnelDetails.ActiveFlag;
                    personnel.ArrestTransportOfficerFlag = value.PersonnelDetails.ArrestTransportOfficerFlag;
                    personnel.ReceiveSearchOfficerFlag = value.PersonnelDetails.ReceiveSearchOfficerFlag;
                    personnel.ProgramInstructorFlag = value.PersonnelDetails.ProgramInstructorFlag;
                    personnel.PersonnelProgramCaseFlag = value.PersonnelDetails.ProgramCaseFlag;
                }

                Person person = _context.Person.SingleOrDefault(s => s.PersonId == value.PersonnelDetails.PersonId);
                if (person != null)
                {
                    person.PersonEmail = value.PersonnelDetails.Email;

                    person.PersonLastName = value.PersonnelDetails.PersonLastName;
                    person.PersonMiddleName = value.PersonnelDetails.PersonMiddleName;
                    person.PersonFirstName = value.PersonnelDetails.PersonFirstName;
                    person.FknLastName = value.PersonnelDetails.PersonLastName;
                    person.FknMiddleName = value.PersonnelDetails.PersonMiddleName;
                    person.FknFirstName = value.PersonnelDetails.PersonFirstName;
                    person.PersonDob = value.PersonnelDetails.DateOfBirth;

                }

                UserVm user = new UserVm
                {
                    UserId = _jwtDbContext.UserClaims.Where(w => w.ClaimType == UserConstants.PERSONNELID
                     && w.ClaimValue == value.PersonnelDetails.PersonnelId.ToString())
                .Select(s => s.UserId).FirstOrDefault()
                };

                AppUser appUser = _userManager.FindByIdAsync(user.UserId).Result;

                if (user.UserId != null)
                {
                    IList<Claim> claims = await _userManager.GetClaimsAsync(appUser);
                    if (value.PersonnelDetails.IsUser)
                    {
                        if (appUser.PasswordHash != value.PersonnelDetails.User.Password)
                        {
                            await _userManager.RemovePasswordAsync(appUser);
                            await _userManager.AddPasswordAsync(appUser, value.PersonnelDetails.User.Password);
                        }

                        appUser.Email = value.PersonnelDetails.Email;
                        appUser.NormalizedEmail = value.PersonnelDetails.Email;
                        appUser.AccessFailedCount = value.PersonnelDetails.User.UserLocked ? 5 : 0;
                        await _userManager.UpdateAsync(appUser);

                        Claim badgeClaim = claims.FirstOrDefault(f => f.Type == UserConstants.BADGENUMBER);
                        if (badgeClaim != null)
                        {
                            await _userManager.RemoveClaimAsync(appUser, badgeClaim);
                            await _userManager.AddClaimAsync(appUser, new Claim(UserConstants.BADGENUMBER,
                                value.PersonnelDetails.OfficerBadgeNumber));
                        }

                        Claim givenName = claims.FirstOrDefault(f => f.Type == UserConstants.GIVENNAME);
                        if (givenName != null)
                        {
                            await _userManager.RemoveClaimAsync(appUser, givenName);

                        }
                        if (value.PersonnelDetails.PersonFirstName != null)
                        {
                            await _userManager.AddClaimAsync(appUser, new Claim(UserConstants.GIVENNAME,
                                value.PersonnelDetails.PersonFirstName));
                        }

                        Claim familyName = claims.FirstOrDefault(f => f.Type == UserConstants.FAMILYNAME);
                        if (familyName != null)
                        {
                            await _userManager.RemoveClaimAsync(appUser, familyName);

                        }
                        if (value.PersonnelDetails.PersonLastName != null)
                        {
                            await _userManager.AddClaimAsync(appUser, new Claim(UserConstants.FAMILYNAME,
                                value.PersonnelDetails.PersonLastName));
                        }

                        Claim middleName = claims.FirstOrDefault(f => f.Type == UserConstants.MIDDLENAME);
                        if (middleName != null)
                        {
                            await _userManager.RemoveClaimAsync(appUser, middleName);

                        }
                        if (value.PersonnelDetails.PersonMiddleName != null)
                        {
                            await _userManager.AddClaimAsync(appUser, new Claim(UserConstants.MIDDLENAME,
                                value.PersonnelDetails.PersonMiddleName));
                        }


                        Claim userExpires = claims.FirstOrDefault(f => f.Type == UserConstants.USEREXPIRES);
                        if (userExpires != null)
                        {
                            await _userManager.RemoveClaimAsync(appUser, userExpires);

                        }
                        if (value.PersonnelDetails.User.UserExpires != null)
                        {
                            await _userManager.AddClaimAsync(appUser, new Claim(UserConstants.USEREXPIRES,
                                value.PersonnelDetails.User.UserExpires));
                        }
                        Claim testUser = claims.FirstOrDefault(f => f.Type == UserConstants.TESTUSER);
                        if (testUser != null)
                        {
                            await _userManager.RemoveClaimAsync(appUser, testUser);
                        }


                        if (value.PersonnelDetails.User.TestUser)
                        {
                            await _userManager.AddClaimAsync(appUser, new Claim(UserConstants.TESTUSER,
                                value.PersonnelDetails.User.TestUser.ToString()));
                        }
                        Claim userNameDomain = claims.FirstOrDefault(f => f.Type == UserConstants.USERNAMEDOMAIN);
                        if (userNameDomain != null)
                        {
                            await _userManager.RemoveClaimAsync(appUser, userNameDomain);
                        }

                        if (value.PersonnelDetails.User.DefaultDomain && value.PersonnelDetails.User.AdDomain != null)
                        {
                            await _userManager.AddClaimAsync(appUser, new Claim(UserConstants.USERNAMEDOMAIN,
                                value.PersonnelDetails.User.AdDomain));
                        }

                        List<string> roleList = _userManager.GetRolesAsync(appUser).Result.ToList();

                        if (roleList.Count > 0)
                            await _userManager.RemoveFromRolesAsync(appUser, roleList);

                        if (value.RoleList.Count > 0)
                        {
                            await _userManager.AddToRolesAsync(appUser, value.RoleList);
                        }

                        Claim deleteClaim = claims.FirstOrDefault(f => f.Type == UserConstants.DELETEFLAG);
                        if (deleteClaim != null)
                        {
                            await _userManager.RemoveClaimAsync(appUser, deleteClaim);
                        }
                    }
                    else
                    {
                        Claim deleteClaim = claims.FirstOrDefault(f => f.Type == UserConstants.DELETEFLAG);
                        if (deleteClaim != null)
                        {
                            await _userManager.RemoveClaimAsync(appUser, deleteClaim);
                            await _userManager.AddClaimAsync(appUser, new Claim(UserConstants.DELETEFLAG, "1"));
                        }
                        else
                        {
                            await _userManager.AddClaimAsync(appUser, new Claim(UserConstants.DELETEFLAG, "1"));
                        }
                    }
                    await _jwtDbContext.SaveChangesAsync();
                }
                else
                {
                    if (value.PersonnelDetails.IsUser)
                    {
                        AppUser userIdentity = new AppUser
                        {
                            UserName = value.PersonnelDetails.User.UserName,
                            Email = value.PersonnelDetails.Email,
                            AccessFailedCount = 0
                        };
                        if (value.PersonnelDetails.User.Password != null)
                        {
                            await _userManager.CreateAsync(userIdentity,
                           value.PersonnelDetails.User.Password);
                        }
                        else
                        {
                            await _userManager.CreateAsync(userIdentity);
                        }

                        // await _userManager.CreateAsync(userIdentity,
                        //    value.PersonnelDetails.User.Password);

                        List<Claim> claims = new List<Claim>
                        {
                            new Claim(UserConstants.GIVENNAME, value.PersonnelDetails.PersonFirstName),
                            new Claim(UserConstants.FAMILYNAME, value.PersonnelDetails.PersonLastName),
                            new Claim(UserConstants.FACILITYID, value.PersonnelDetails.FacilityId.ToString()),
                            new Claim(UserConstants.BADGENUMBER, value.PersonnelDetails.OfficerBadgeNumber),
                            new Claim(UserConstants.CREATEDATE,DateTime.Now.ToString(CultureInfo.InvariantCulture))
                        };
                        if (personnel != null)
                        {
                            claims.Add(new Claim(UserConstants.PERSONNELID, personnel.PersonnelId.ToString()));
                        }

                        if (value.PersonnelDetails.PersonMiddleName != null)
                        {
                            claims.Add(new Claim(UserConstants.MIDDLENAME, value.PersonnelDetails.PersonMiddleName));
                        }

                        if (value.PersonnelDetails.User.UserExpires != null)
                        {
                            claims.Add(new Claim(UserConstants.USEREXPIRES, value.PersonnelDetails.User.UserExpires));
                        }
                        if (value.PersonnelDetails.User.TestUser)
                        {
                            claims.Add(new Claim(UserConstants.TESTUSER, value.PersonnelDetails.User.TestUser.ToString()));
                        }

                        if (value.PersonnelDetails.User.DefaultDomain && value.PersonnelDetails.User.AdDomain != null)
                        {
                            claims.Add(new Claim(UserConstants.USERNAMEDOMAIN, value.PersonnelDetails.User.AdDomain));
                        }

                        await _userManager.AddClaimsAsync(userIdentity, claims);

                        if (value.RoleList.Count > 0)
                        {
                            await _userManager.AddToRolesAsync(userIdentity, value.RoleList);
                        }

                        await _jwtDbContext.SaveChangesAsync();

                    }
                }
            }
            return await _context.SaveChangesAsync();
        }       

        public List<KeyValuePair<int, string>> GetDepartmentPosition(string forFlag)
        {
            IQueryable<Lookup> lookup = forFlag == "Department" ?
                _context.Lookup.Where(w => w.LookupType == LookupConstants.DEPARTMENT) :
                _context.Lookup.Where(w => w.LookupType == LookupConstants.POSITION);
            return lookup.ToList().OrderBy(o => o.LookupOrder).ThenBy(t => t.LookupDescription)
                .Select(s => new KeyValuePair<int, string>(s.LookupIndex, s.LookupDescription))
                .ToList();
        }
        public async Task<int> InsertDepartmentPosition(KeyValuePair<string, string> param)
        {
            (string key, string value) = param;
            if (key == "Department")
            {
                int lookupIndexCount = _context.Lookup.Where(w => w.LookupType == LookupConstants.DEPARTMENT)
                    .Select(s => s.LookupIndex).Max();

                Lookup lookup = new Lookup
                {
                    LookupType = LookupConstants.DEPARTMENT,
                    LookupDescription = value,
                    LookupIndex = lookupIndexCount + 1,
                    LookupName = value
                };
                _context.Lookup.Add(lookup);
            }
            else
            {
                int lookupIndexCount = _context.Lookup.Where(w => w.LookupType == LookupConstants.POSITION)
                    .Select(s => s.LookupIndex).Max();

                Lookup lookup = new Lookup
                {
                    LookupType = LookupConstants.POSITION,
                    LookupDescription = value,
                    LookupIndex = lookupIndexCount + 1,
                    LookupName = value
                };
                _context.Lookup.Add(lookup);
            }

            return await _context.SaveChangesAsync();
        }
    }
       
}
