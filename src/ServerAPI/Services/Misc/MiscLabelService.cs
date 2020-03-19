using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;
using jsreport.Client;
using jsreport.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public class MiscLabelService:IMiscLabelService
    {
        private readonly AAtims _context;
        private readonly IPhotosService _photos;
        private readonly Uri _jsReportUrl;
        private readonly ICommonService _commonService;
		private readonly int _personnelId;

		public MiscLabelService(AAtims aatims, IPhotosService photosService, IConfiguration configuration,
			IHttpContextAccessor httpContextAccessor, ICommonService commonService)
        {
            _context = aatims;
            _photos = photosService;
            _jsReportUrl = new Uri(configuration.GetSection("SiteVariables")["ReportUrl"]);
            _commonService = commonService;
			_personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
			   .FindFirst("personnelId")?.Value);
		}

        //Misc=>Label 
        public List<FormTemplateVm> GetMisclabel(int inmateId, string fromScreen, int wizardStepId, int arrestId,
            int personnelId)
        {
            int personId = _context.Inmate.Single(s => s.InmateId == inmateId).PersonId;
            List<FormTemplateVm> lstPersonFormTemplate = _context.PersonFormTemplate
                .Where(s => s.ShowInLabel == true && !s.DeleteFlag.HasValue)
                .Select(s => new FormTemplateVm
                {
                    TemplateName = s.TemplateName,
                    RequireBookingSelect = s.RequireBookingSelect ?? false,
                    PersonFormTemplateId = s.PersonFormTemplateId,
                    TemplateSql = s.TemplateSql,
                    DeleteFlag = s.DeleteFlag ?? false,
                    ShowInLabel = s.ShowInLabel ?? false
                }).ToList();

            lstPersonFormTemplate = fromScreen == "Personnel"
                ? lstPersonFormTemplate.Where(s => s.ShowInPersonnel).ToList()
                : lstPersonFormTemplate.ToList();

            if (wizardStepId > 0)
            {
                int formTemplateId = _context.AppAoWizardSteps
                                         .SingleOrDefault(aa => aa.AppAoWizardStepsId == wizardStepId)
                                         ?.AppAoUserControlParam ?? 0;
                lstPersonFormTemplate =
                    lstPersonFormTemplate.Where(s => s.PersonFormTemplateId == formTemplateId).ToList();
            }

            if (inmateId <= 0) return lstPersonFormTemplate;
            {
                List<PersonFormTemplateCtl> lstPersonFormTemplateCtls = _context.PersonFormTemplateCtl.ToList();
                lstPersonFormTemplate.ForEach(item =>
                {
                    if (item.RequireBookingSelect)
                    {
                        item.ArrestBookingNo = _context.IncarcerationArrestXref
                            .Where(i => i.Incarceration.InmateId == inmateId)
                            .OrderByDescending(a => a.Incarceration.IncarcerationId)
                            .ThenBy(a => a.Arrest.ArrestId)
                            .Select(a => new KeyValuePair<int, string>(a.Arrest.ArrestId, a.Arrest.ArrestBookingNo))
                            .ToList();
                        arrestId = arrestId > 0 ? arrestId : item.ArrestBookingNo[0].Key;
                    }

                    FormTemplateDetailVm personFormTemplateDetail = new FormTemplateDetailVm
                    {
                        PersonFormTemplateId = item.PersonFormTemplateId,
                        PhotoFilePath = _photos.GetPhotoByPersonId(personId),
                        LstPersonFormTemplateCtrl = lstPersonFormTemplateCtls
                            .Where(s => s.PersonFormTemplateId == item.PersonFormTemplateId).Select(s =>
                                new FormTemplateCt1Vm
                                {
                                    Type = s.CtlType,
                                    FieldName = s.CtlFieldName,
                                    Value = s.CtlValue,
                                    XPos = s.CtlCordx ?? 0,
                                    YPos = s.CtlCordy ?? 0,
                                    Width = s.CtlCordw ?? 0,
                                    Height = s.CtlCordh ?? 0,
                                    Font = s.CtlFont,
                                    FontSize = s.CtlFontSize ?? 0,
                                    ForeColor = s.CtlColor1,
                                    Backcolor = s.CtlColor2
                                }).ToList(),
                        TemplateData = item.RequireBookingSelect ? TemplateValueSql(item.TemplateSql, arrestId) :
                            fromScreen == "Personnel" ? TemplateValueSql(item.TemplateSql, personnelId) :
                            personId > 0 ? TemplateValueSql(item.TemplateSql, personId) : null,
                        PersonColorFlag = PersonName(personId)
                    };
                    item.TemplateSql = "";
                    item.PersonFormTemplateDetail = personFormTemplateDetail;
                });
            }
            return lstPersonFormTemplate;
        }

        private List<KeyValuePair<int, string>> PersonName(int personId)
        {
            IQueryable<Lookup> lookuplst = _context.Lookup.Where(w => w.LookupType == LookupConstants.PERSONCAUTION
                                                                      || w.LookupType == LookupConstants.TRANSCAUTION &&
                                                                      w.LookupInactive == 0);

            IQueryable<PersonFlag> personFlag = _context.PersonFlag.Where(w =>
                w.PersonId == personId && w.DeleteFlag == 0
                                       && (w.InmateFlagIndex > 0 || w.PersonFlagIndex > 0));

            List<KeyValuePair<int, string>> personColorFlag = personFlag.SelectMany(
                p => lookuplst.Where(w => w.LookupType == LookupConstants.PERSONCAUTION
                                          && Equals(w.LookupIndex, (double?)(p.PersonFlagIndex))
                                          && p.PersonFlagIndex > 0).Select(s =>
                    new KeyValuePair<int, string>(p.PersonFlagIndex ?? 0, s.LookupColor))
            ).ToList();

            personColorFlag.AddRange(personFlag.SelectMany(
                p => lookuplst.Where(w => w.LookupType == LookupConstants.TRANSCAUTION
                                          && Equals(w.LookupIndex, (double?)(p.PersonFlagIndex))
                                          && p.InmateFlagIndex > 0).Select(s =>
                    new KeyValuePair<int, string>(p.InmateFlagIndex ?? 0, s.LookupColor)).ToList()));

            return personColorFlag;
        }

        private Dictionary<string, object> TemplateValueSql(string sqlSp, int id)
        {
            if (string.IsNullOrEmpty(sqlSp)) return null;
            Dictionary<string, object> templateData = null;

            SqlConnection connection = new SqlConnection(Startup.ConnectionString);
            connection.Open();
            SqlCommand command = new SqlCommand(sqlSp, connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@ID", id);

            using (SqlDataReader dr = command.ExecuteReader())
            {
                List<string> cols = new List<string>();
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    cols.Add(dr.GetName(i));
                }

                if (dr.Read())
                {
                    templateData = new Dictionary<string, object>();
                    foreach (string col in cols)
                    {
                        templateData.Add(col, dr[col]);
                    }
                }
            }


            connection.Close();
            return templateData;
        }

        public async Task<List<FormTemplateVm>> GetMiscPdfLabel(int inmateId, string fromScreen, int wizardStepId, int arrestId,
            int personnelId)
        {
            int personId = _context.Inmate.Single(s => s.InmateId == inmateId).PersonId;
            List<FormTemplateVm> lstPersonFormTemplate = _context.PersonFormTemplate
                .Where(s => s.ShowInLabel == true && s.DeleteFlag != true
				&& s.Inactive != true && !string.IsNullOrEmpty(s.ShortId) 
				&& (fromScreen == "Personnel" ? s.ShowInPersonnel == true : true) )
                .Select(s => new FormTemplateVm
                {
                    TemplateName = s.TemplateName,
                    RequireBookingSelect = s.RequireBookingSelect ?? false,
                    PersonFormTemplateId = s.PersonFormTemplateId,
                    TemplateSql = s.TemplateSql,
                    DeleteFlag = s.DeleteFlag ?? false,
                    ShowInLabel = s.ShowInLabel ?? false,
                    ShortId = s.ShortId,
					ParamName = s.ParamName
                }).ToList();

            if (wizardStepId > 0)
            {
                int formTemplateId = _context.AoWizardFacilityStep
										 .SingleOrDefault(aa => aa.AoWizardFacilityStepId == wizardStepId)
                                         ?.AoComponentParamId ?? 0;
                lstPersonFormTemplate =
                    lstPersonFormTemplate.Where(s => s.PersonFormTemplateId == formTemplateId).ToList();
            }


			for (int l = 0; l < lstPersonFormTemplate.Count; l++)
			{
				var item = lstPersonFormTemplate[l];
				if (item.RequireBookingSelect && inmateId > 0)
				{
					item.ArrestBookingNo = _context.IncarcerationArrestXref
						.Where(i => i.Incarceration.InmateId == inmateId)
						.OrderByDescending(a => a.Incarceration.IncarcerationId)
						.ThenBy(a => a.Arrest.ArrestId)
						.Select(a => new KeyValuePair<int, string>(a.Arrest.ArrestId, a.Arrest.ArrestBookingNo))
						.ToList();
					arrestId = arrestId > 0 ? arrestId : item.ArrestBookingNo[0].Key;
				}

				string templateData = string.Empty;
				if (item.RequireBookingSelect)
				{
					templateData = GetTemplateSqlDataJsonString(item.TemplateSql, inmateId);
				}
				else if (fromScreen == "Personnel")
				{
					templateData = GetTemplateSqlDataJsonString(item.TemplateSql, personnelId);
				}
				else if (inmateId > 0 && item.ParamName.ToLower() == "inmate_id")
				{
					templateData = GetTemplateSqlDataJsonString(item.TemplateSql, inmateId, 0, true);
				}
				else if (personId > 0)
				{
					templateData = GetTemplateSqlDataJsonString(item.TemplateSql, personId, personId);
				}

				item.TemplateSql = "";
				ReportingService rs = new ReportingService(_jsReportUrl.ToString());
                _commonService.atimsReportsContentLog(item.ShortId, templateData);
                Report report = await rs.RenderAsync(item.ShortId, templateData);
				item.LabelPdf = new FileContentResult(_commonService.ConvertStreamToByte(report.Content), "application/pdf");
			}
           
            return lstPersonFormTemplate;
        }

        public string GetTemplateSqlDataJsonString(string StoredProcedureName, object ParameterId, int? personId = 0, bool isPersonnel=false)
        {
            SqlConnection connection = new SqlConnection(Startup.ConnectionString);
            connection.Open();
            SqlCommand command = new SqlCommand(StoredProcedureName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@ID", ParameterId);
            DataTable resultTable = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(resultTable);
            connection.Close();
            string jsonDataString = "";

			if (resultTable.Rows.Count > 0) {
                jsonDataString = resultTable.Rows[0]["JsonData"].ToString();
            }
            
            JObject data = (jsonDataString == "") ? JObject.FromObject(new { Data = new JObject[0] }) : JObject.Parse(jsonDataString);
			Object externalDetails = new Object();

			if (isPersonnel)
			{
				externalDetails = _context.Personnel.Where(p => p.PersonnelId == _personnelId)
					.Select(pr => new
					{
						pr.OfficerBadgeNum,
						OfficerFirstName = pr.PersonNavigation.PersonFirstName,
						OfficerLastName = pr.PersonNavigation.PersonLastName,
						OfficerMiddleName = pr.PersonNavigation.PersonMiddleName,
						PhotoFilePath = personId > 0 ? _photos.GetPhotoByPersonId(personId ?? 0) : string.Empty,
					}).Single();
			}
			else if(personId > 0)
			{
				externalDetails = new
				{
					PhotoFilePath = _photos.GetPhotoByPersonId(personId ?? 0)
				};
			}
			

			JObject data2 = JObject.Parse(JsonConvert.SerializeObject(externalDetails));
            data.Merge(data2, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Concat });

			return JsonConvert.SerializeObject(data);
        }
    }
}
