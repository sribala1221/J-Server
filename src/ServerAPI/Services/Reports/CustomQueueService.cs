using System;
using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;
using Microsoft.AspNetCore.Http;
using JwtDb.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using ServerAPI.Static.Extension;
using Microsoft.EntityFrameworkCore;

namespace ServerAPI.Services
{
    public class CustomQueueService : ICustomQueueService
    {
        // Fields       
        private readonly AAtims _context;
        private readonly int _personnelId;      
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CustomQueueService(AAtims context, IHttpContextAccessor httpContextAccessor,
            UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
              .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);          
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public List<CustomQueueDetailsVm> GetCustomQueue()
        {            

            Claim claim = new Claim("personnel_id", _personnelId.ToString());

            AppUser appUser = _userManager.GetUsersForClaimAsync(claim).Result.FirstOrDefault();         

            List<string> roleNames = _userManager.GetRolesAsync(appUser).Result.ToList();

            List<IdentityRole> roles = _roleManager.Roles.Where(w => roleNames.Contains(w.Name)).ToList();

            List<string> claimsLst = new List<string>();

            roles.ForEach(f => {
                claimsLst.AddRange(_roleManager.GetClaimsAsync(f).Result.Select(s => s.Type).ToList());
            });           

            List<CustomQueueDetailsVm> customQueueDetails = _context.CustomQueue
                .Where(w => !w.DeleteFlag.HasValue && (claimsLst.Any(c => c == PermissionTypes.Admin) ||
                claimsLst.Contains(PermissionTypes.CustomQueue + PermissionTypes.Permission + w.CustomQueueId)))
                .OrderBy(o => o.QueueGroupName)
                .Select(s => new CustomQueueDetailsVm
                {
                    CustomQueueId = s.CustomQueueId,
                    QueueGroupName = s.QueueGroupName,
                    QueueName = s.QueueName,
                    QueueDescription = s.QueueDescription,
                    DeleteFlag = s.DeleteFlag == 0,
                    QueueSql = s.QueueSql,
                    QueueConnectionString = s.QueueConnectionString,
                    ColumnInmateId = s.ColumnInmateId ?? 0,
                    ColumnArrestId = s.ColumnArrestId ?? 0,
                    ExternalData = s.QueueExternalData == 1,
                    ShowQueueKiosk = s.ShowInKioskFlag == 1
                }).ToList();

            return customQueueDetails;
        }

        /// <summary>
        /// Returns list of custom queue parameters.
        /// </summary>
        /// <param name="customQueueId"></param>
        /// <returns></returns>
        public IEnumerable<QueueParameterOptionalVm> GetCustomQueueEntryDetails(int customQueueId)
        {
            return _context.CustomQueueParam
                .Where(w => w.CustomQueueId == customQueueId
                            && !w.DeleteFlag.HasValue)
                .AsNoTracking()
                .ConvertForApi();
        }

        public async Task<string> GetCustomQueueSearch(CustomQueueSearchInput customQueueSearch)
        {
            string jsonDataString = "";

            jsonDataString = GetTemplateSqlDataJsonString(customQueueSearch.CustomQueueItem.QueueSql, customQueueSearch.CustomQueueItem.QueueConnectionString, customQueueSearch.CustomQueueParam);

            return await Task.FromResult(jsonDataString);
        }

        public string GetTemplateSqlDataJsonString(string storedProcedureName, string connectionString, List<QueueParameterOptionalVm> paramQueue)
        {
            if (String.IsNullOrEmpty(connectionString))
            {
                connectionString = Startup.ConnectionString;
            }
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = new SqlCommand(storedProcedureName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            paramQueue.ForEach(item =>
            {
                if (item.FreeForm == 1)
                {
                    command.Parameters.AddWithValue("@Parameter" + item.Index, item.FreeFormValue);
                }
                else if (item.FieldDateFlag == 1)
                {
                    command.Parameters.AddWithValue("@Parameter" + item.Index, item.FieldDateValue);
                }
                else
                {
                    command.Parameters.AddWithValue("@Parameter" + item.Index, item.CheckBoxValue);
                }
            });

            DataTable resultTable = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(resultTable);
            connection.Close();
            string jsonDataString = "";

            if (resultTable.Rows.Count > 0)
            {
                jsonDataString = resultTable.Rows[0]["JsonData"].ToString();
            }            

            return jsonDataString;
        }
    }
}
