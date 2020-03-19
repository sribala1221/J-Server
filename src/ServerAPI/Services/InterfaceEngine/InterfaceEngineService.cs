using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ServerAPI.ViewModels;
using GenerateTables.Models;
using System.Data.SqlClient;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using ServerAPI.Extensions;
using System.Threading;

namespace ServerAPI.Services
{
    public class InterfaceEngineService : IInterfaceEngineService
    {
        private readonly AAtims _context;
        private readonly HttpClient _interfaceEngine = new HttpClient();
        private readonly IMemoryCache _cache;
        private const string AtimsApiKey = "zp8d7aaykvewlo36";

        public InterfaceEngineService(AAtims context, IConfiguration configuration, IMemoryCache memoryCache)
        {
            _context = context;
            if (configuration.GetSection("SiteVariables")["InterfaceEngine"] != null)
            {
                _interfaceEngine.BaseAddress = new Uri(configuration.GetSection("SiteVariables")["InterfaceEngine"]);
            }
            _cache = memoryCache;
        }

        public object Inbound(InboundRequestVM values)
        {
            SqlConnection connection = new SqlConnection(Startup.ConnectionString);

            string storeProcedureName = _context.WebServiceReturn.Where(w => w.MethodName == values.MethodName).Select(s => s.Sql).FirstOrDefault();
            if (storeProcedureName == null) return new { Content = "ERROR, METHOD DOES NOT EXISTS" };

            connection.Open();
            SqlCommand command = new SqlCommand(storeProcedureName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (values.Data != null)
            {
                command.Parameters.AddWithValue("@JSON", values.Data.ToString());
            }


            try
            {
                DataTable resultTable = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(resultTable);
                connection.Close();

                string jsonDataString = resultTable.Rows[0]["Content"].ToString();

                if (jsonDataString == "")
                {
                    return new { Content = "" };
                }

                JObject data = JObject.Parse(jsonDataString);

                return new { Content = data };
            }
            catch (Exception)
            {
                return new { Content = "An Error prevented the request to be completed." };
            }
        }

        public void Export(ExportRequestVm exportVm)
        {
            bool exists = _context.WebServiceEventSetting.Any(x => x.EventQueueFlag == 1);
            
            if (exportVm.EventType == null)
            {
                exportVm.EventType = _context.WebServiceEventType.Where(w => w.WebServiceEventName == exportVm.EventName)
                    .Select(s => s.WebServiceEventTypeId).FirstOrDefault();
            }

            if (!exists || exportVm.EventType == null)
            {
                return;
            }

            WebServiceAuth serviceAuth = GetValidEventAuth();
            exportVm.UserName = serviceAuth.User;
            exportVm.Pwd = serviceAuth.Password;
            AddToQueue(exportVm);

            if (checkIfRestIsRequired(exportVm)) {
                SendInterfaceRequestExport(exportVm);
            }
        }

        private void SendInterfaceRequestExport(ExportRequestVm exportVm)
        {
            ThreadPool.QueueUserWorkItem(callback =>
            {
                _interfaceEngine.PostAsync($@"{"/Export/ExportRequest"}", new JsonContent(exportVm));
            });
        }

        private bool checkIfRestIsRequired(ExportRequestVm exportVm)
        {
            List<WebServiceEventQueue> queue = _context.WebServiceEventQueue
                .Where(w => w.WebServiceEventTypeId == exportVm.EventType && w.ProcessingFlag == 0
                && w.Attempt == 0).Select(s => s)
                .ToList();
            List<WebServiceExport> webServiceExport = _context.WebServiceExport
                .Where(w => queue.Any(q => q.WebServiceEventExportId == w.WebServiceExportId) && w.WebServiceExportType == (int?)ExportTypeEnum.Rest).Select(s => s).ToList();
            if (webServiceExport.Count == 0) return false;
            return true;
        }

        private void AddToQueue(ExportRequestVm exportVm)
        {
            List<WebServiceEventAssign> eventAssigns = _context.WebServiceEventAssign
                .Where(w => w.WebServiceEventTypeId == exportVm.EventType && w.WebServiceEventInactive != 1
                && w.WebServiceEventType.WebServiceEventName == exportVm.EventName
                && (!exportVm.WebServiceEventAssignId.HasValue || w.WebServiceEventAssignId == (exportVm.WebServiceEventAssignId)))
                .Select(s => s).ToList();
            foreach (WebServiceEventAssign x in eventAssigns)
            {
                //WebServiceExport webServiceExport = _context.WebServiceExport
                //    .Where(w => w.WebServiceExportId == x.WebServiceEventExportId).Select(s => s).FirstOrDefault();
                //if (webServiceExport == null) continue;
                _context.WebServiceEventQueue.Add(new WebServiceEventQueue
                {
                    Attempt = 0,
                    CreateBy = exportVm.PersonnelId ?? 1,
                    CreateDate = DateTime.Now,
                    ProcessingFlag = 0,
                    WebServiceEventAssignId = x.WebServiceEventAssignId,
                    WebServiceEventAuthId = x.WebServiceEventAuthId,
                    WebServiceEventExportId = x.WebServiceEventExportId,
                    WebServiceEventParameter1 = exportVm.Param1 ?? "",
                    WebServiceEventParameter2 = exportVm.Param2 ?? "",
                    WebServiceEventTypeId = x.WebServiceEventTypeId
                });

                WebServiceEventType webServiceEventType = _context.WebServiceEventType
                    .Where(w => w.WebServiceEventTypeId == exportVm.EventType).Select(s => s).FirstOrDefault();

                if (webServiceEventType != null && webServiceEventType.WebServiceEventRunHistory == 1)
                {
                    WebServiceEventTypeHistory wseth = new WebServiceEventTypeHistory
                    {
                        WebServiceEventTypeId = x.WebServiceEventAssignId,
                        CreateDate = DateTime.Now,
                        CreateBy = exportVm.PersonnelId ?? 1,
                        WebServiceEventParameter1 = exportVm.Param1 ?? "",
                        WebServiceEventParameter2 = exportVm.Param2 ?? ""
                    };
                    _context.Add(wseth);
                }
                _context.SaveChanges();
            }
        }

        private WebServiceAuth GetValidEventAuth() =>
            _context.WebServiceAuth.Where(w => w.WebServiceAuthId == 1).Select(s => s).FirstOrDefault();

        public object TestExportRequest(ExportRequestVm vm)
        {
            //do normal jms operations
            //trigger event
            //get parameters to be passed

            Export(vm);
            return new { message = "wroked" };
        }

        public InmatePrebookVm GetInmatePrebookSacramento(string inmateNumber) =>
            _cache.Get<InmatePrebookVm>("inmateNumber_" + inmateNumber);

        public InmatePrebookVm SaveInmatePrebookSacramento(InmatePrebookSacramento values)
        {
            if(CheckAtimsApiKey(values.AtimsApiKey) == false)
            {
                return null;
            }

            MemoryCacheEntryOptions cacheExpirationOptions = new MemoryCacheEntryOptions {
                AbsoluteExpiration = DateTime.Now.AddDays(1), Priority = CacheItemPriority.Normal
            };
            _cache.Set("inmateNumber_" + values.InmateNumber, values.Prebook, cacheExpirationOptions);
            return values.Prebook;
        }

        private static bool CheckAtimsApiKey(string key) => key == AtimsApiKey;

        public async Task<object> AtimsOnlineServiceRunExport(AtimsOnlineServiceRunExport payload)
        {
            return await _interfaceEngine.PostAsync($@"{"/AtimsOnlineService/RunExport"}", new JsonContent(payload));
        }
    }
}
