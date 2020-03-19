using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using System;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Xml;
using System.Text;
using System.IO;
using System.Web;
using Newtonsoft.Json;

namespace ServerAPI.Services
{
    public class LiveScanService : ILiveScanService
    {
        private readonly AAtims _context;       
        public LiveScanService(AAtims context)
        {
            _context = context;          
        }

        public LiveScanDetail GetLiveScan(int inmateId, int userControlId)
        {
            LiveScanDetail getdetail = new LiveScanDetail
            {
                Location = webServicedetails(userControlId),
                ArrestBookingDetail = LoadLiveScan(inmateId),
                IsAccessProvided = IsAccessProvided()
            };
            return getdetail;
        }          
        private List<LoadLiveScanBooking> LoadLiveScan(int inmateId)
        {
            IQueryable<Lookup> lstLook =
                _context.Lookup.Where(look => look.LookupType == LookupConstants.ARRTYPE
                 && look.LookupInactive == 0);

            List<LoadLiveScanBooking> value = _context.Arrest.Where(w =>w.InmateId == inmateId
                && w.IncarcerationArrestXref.Any(a => !a.Incarceration.ReleaseOut.HasValue))
             .Select(s => new LoadLiveScanBooking()
             {
                 ArrestId = s.ArrestId,
                 ArrestBookingNumber = s.ArrestBookingNo,
                 ArrestDate = s.ArrestDate.HasValue ? s.ArrestDate.Value : default,
                 BookingCompleteFlag = s.BookingCompleteFlag == 1,
                 AgencyAbbreviation =s.ArrestingAgency.AgencyAbbreviation,
                 ArrestCourtDocket =s.ArrestCourtDocket,
                 ArrestCaseNumber =s.ArrestCaseNumber, 
                 ArrestType = s.ArrestType != null
                       ? lstLook.Where(look =>                           
                          look.LookupIndex == Convert.ToInt32(Convert.ToDouble(s.ArrestType)) &&
                           look.LookupType == LookupConstants.ARRTYPE)
                           .Select(f=>f.LookupDescription).SingleOrDefault()
                       : null,
                 ReleaseDate = s.IncarcerationArrestXref.Select(iax => iax.ReleaseDate).FirstOrDefault()
             }).OrderBy(o => o.BookingCompleteFlag)
             .ThenBy(o => o.ReleaseDate)
             .ThenBy(o => o.ArrestId).ToList();

            return value;
        }      

        private List<KeyValuePair<int, string>> webServicedetails(int userControlId) =>
            _context.WebServiceExport.Where(s => s.UserInitiatedControlId == userControlId 
                && s.Inactive != 1)
            .Select(d => new KeyValuePair<int, string>
                (d.WebServiceExportId, d.Description))
            .OrderBy(o => o.Value).ToList();

        private bool IsAccessProvided() => 
            _context.WebServiceExport.Any(w => w.Inactive != 1 &&
                w.UserInitiatedControlId == _context.Lookup.Where(l => l.LookupType == LookupConstants.WEBSVCCTL
                    && l.LookupDescription == LookupConstants.LIVESCAN).Select(lk => lk.LookupIndex).SingleOrDefault());

        public string PreviewLiveScanPayLoad(string arrestIds, int exportId)
        {
            WebServiceExport webServiceExport = _context.WebServiceExport
                .Where(w => w.WebServiceExportId == exportId)
                .SingleOrDefault();
            DataTable dt = new DataTable();
            DataSet ds = new DataSet("live-scan");
            if(webServiceExport != null)
            {
                if(webServiceExport.OutputXml ?? false)
                {
                    string sql = webServiceExport.Sql;
                    SqlConnection connection = (SqlConnection)_context.Database.GetDbConnection();
                    connection.Open();
                    for(int i = 0; i< arrestIds.Split(',').Length; i++)
                    {
                        using (SqlCommand cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = sql.Replace("<@PARAMETER3@>", arrestIds.Split(',')[i].ToString());
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandTimeout = 0;
                            SqlDataAdapter da = new SqlDataAdapter(cmd);
                            DataSet dsNew = new DataSet();
                            da.Fill(dsNew);
                            dsNew.Tables[0].TableName = "person-info";
                            foreach (DataTable table in dsNew.Tables)
                            {
                                dt.Merge(table);
                            }
                        }
                    }
                }
            }
            if(dt.Rows.Count > 0)
            {
                ds.Tables.Add(dt);
                ds.Tables[0].TableName = "person-info";
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(ds.GetXml());
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);
                XmlTextWriter xmlTextWriter = new XmlTextWriter(sw);
                xmlTextWriter.Formatting = System.Xml.Formatting.Indented;
                xmlDoc.WriteTo(xmlTextWriter);
                return JsonConvert.SerializeXmlNode(xmlDoc);
            }
            return "{}";
        }
    }
}