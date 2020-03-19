using System;
using GenerateTables.Models;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace ServerAPI.Services
{
    public class PersonPhotoService : IPersonPhotoService
    {
        private readonly AAtims _context;
        private readonly IConfiguration _configuration;
        private readonly int _personnelId;
        private readonly string _filePath;
        private readonly IPersonService _personService;
        private readonly IPhotosService _photos;

        #region Constructor

        public PersonPhotoService(AAtims context, IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor, IPersonService personService, IPhotosService photosService)
        {
            _context = context;
            _configuration = configuration;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
            _filePath = configuration.GetSection("SiteVariables")["PhotoPath"] + "\\";
            _personService = personService;
            _photos = photosService;
        }

        #endregion

        #region Methods        

        //To Get PersonPhoto Details
        public List<PersonPhoto> GetPersonPhoto(int personId, bool showDeleted = false)
        {
            //Below query take 2ms
            List<PersonPhoto> lstPersonPhotos = (from i in _context.Identifiers
                                                 where i.PersonId == personId
                                                 select new PersonPhoto
                                                 {
                                                     IdentifierId = i.IdentifierId,
                                                     PersonId = i.PersonId,
                                                     PhotoType = i.IdentifierType,
                                                     PhotographDate = i.PhotographDate,
                                                     PhotographTakenBy = i.PhotographTakenBy,
                                                     PhotographRelativePath = _photos.GetPhotoByIdentifier(i),
                                                     DescriptorText = i.PersonDescriptor.DescriptorText,
                                                     DeleteFlag = i.DeleteFlag == 1,
                                                     CreateDate = i.CreateDate,
                                                     IdentifierDescription = i.IdentifierDescription,
                                                     NarrativeText = i.IdentifierNarrative,
                                                     LocationText = i.IdentifierLocation,
                                                     DescriptorId = i.PersonDescriptorId
                                                 }).ToList();

            if (showDeleted == false)
            {
                lstPersonPhotos = lstPersonPhotos.Where(w => w.DeleteFlag == false).ToList();
            }

            //To get Lookupdesc in Lookup
            //Below query take 1ms
            List<Lookup> lstLookup = (from lkup in _context.Lookup
                                      where lkup.LookupType == LookupConstants.IDTYPE
                                      select new Lookup
                                      {
                                          LookupIndex = lkup.LookupIndex,
                                          LookupDescription = lkup.LookupDescription
                                      }).ToList();

            int[] personnelIds =
                lstPersonPhotos.Select(p => p.PhotographTakenBy).Where(p => p.HasValue).Select(p => p.Value).ToArray();

            //Below query take 1ms
            var lstPersonnelIds = (from i in _context.Personnel
                                   where personnelIds.Contains(i.PersonnelId)
                                   select new
                                   {
                                       i.PersonnelId,
                                       i.PersonNavigation.PersonLastName,
                                       i.PersonNavigation.PersonFirstName,
                                       i.OfficerBadgeNum
                                   }).ToList();

            lstPersonPhotos.ForEach(item =>
            {
                //To Get IdentifierTypeName based on LookUpBinary
                item.PhotoTypeName =
                    lstLookup.SingleOrDefault(l => l.LookupIndex.ToString() == item.PhotoType.Trim())
                        ?.LookupDescription;

                if (item.PhotographTakenBy.HasValue)
                {
                    //To Get Each Personnel Details from lstPersonnelIds
                    var myPerson = (from p in lstPersonnelIds
                                    where p.PersonnelId == item.PhotographTakenBy
                                    select p).Single();

                    //Assigning the values to PersonPhoto Model
                    item.PersonLastName = myPerson.PersonLastName;
                    item.PersonFirstName = myPerson.PersonFirstName;
                    item.OfficerBadgeNumber = myPerson.OfficerBadgeNum;
                }
            });

            return lstPersonPhotos;
        }

        //Get Person Descriptor details
        public List<PersonDescriptorVm> GetPersonPhotoDescriptor(int personId) =>
            (from pd in _context.PersonDescriptor
             where pd.PersonId == personId && pd.DeleteFlag == 0
             select new PersonDescriptorVm
             {
                 PersonDescriptorId = pd.PersonDescriptorId,
                 Code = pd.Code,
                 DescriptorText = pd.DescriptorText,
                 CreateDate = pd.CreateDate.Value,
                 UpdateDate = pd.UpdateDate
             }).ToList();

        //To Insert And Update Person Details
        public async Task<int> InsertUpdatePersonPhoto(PersonPhoto personPhoto)
        {
            //Below query take 1ms
            Identifiers identifiers =
                _context.Identifiers.SingleOrDefault(x => x.IdentifierId == personPhoto.IdentifierId) ??
                new Identifiers();

            if (personPhoto.IdentifierId <= 0)
            {
                identifiers.IdentifierType = personPhoto.PhotoType;
                identifiers.PhotographDate = personPhoto.PhotographDate;
                identifiers.PhotographTakenBy = _personnelId;
            }

            identifiers.PersonDescriptorId = personPhoto.DescriptorId;
            identifiers.IdentifierDescription = personPhoto.IdentifierDescription;
            identifiers.IdentifierNarrative = personPhoto.NarrativeText;
            identifiers.IdentifierLocation = personPhoto.LocationText;
            identifiers.DisciplinaryIncidentId = personPhoto.IncidentId;
            identifiers.PersonId = personPhoto.PersonId;

            if (personPhoto.IdentifierId <= 0)
            {
                //Save Identifiers table
                _context.Identifiers.Add(identifiers);
                await _context.SaveChangesAsync();
                //Getting Folder Path Depend On IdentifierType
                string folderPath = GetPersonPhotoPath(identifiers.IdentifierType);
                string savepath =
                    $@"{_configuration.GetSection(PathConstants.SITEVARIABLES)[PathConstants.PHOTOPATH]}\{folderPath}";

                if (!Directory.Exists(savepath))
                {
                    Directory.CreateDirectory(savepath);
                }

                string imageName = identifiers.IdentifierId.ToString().PadLeft(5, '0') + PathConstants.JPGPATH;
                savepath = $@"{savepath}\{imageName}";
                
                if (!string.IsNullOrEmpty(personPhoto.Base64String))
                {
                    byte[] imageBytes = Convert.FromBase64String(personPhoto.Base64String.Replace(PathConstants.BASE64, String.Empty));
                    using (var stream = new FileStream(savepath, FileMode.Create))
                    {
                        stream.Write(imageBytes, 0, imageBytes.Length);
                        stream.Flush();
                    }

                    //Delete Image From Temparary Folder
                    DeleteTempPhoto(personPhoto.TempPhotoPath);
                    //if (File.Exists(tempPath))
                    //{
                    //    File.Delete(tempPath);
                    //}

                    //Update PhotographRelativePath
                    identifiers.PhotographRelativePath = $@"{folderPath}\{imageName}";
                }

            }

            return await _context.SaveChangesAsync();
        }

        //Delete Or Undo Person Photo 
        public async Task<int> DeleteOrUndoPhoto(DeleteParams deleteParams)
        {
            Identifiers identifiers = _context.Identifiers.Single(a => a.IdentifierId == deleteParams.Id);
            identifiers.DeleteFlag = deleteParams.DeleteFlag > 0 ? 1 : 0;
            return await _context.SaveChangesAsync();
        }

        #endregion

        #region Get Person Photo Path

        private string GetPersonPhotoPath(string identifierType)
        {
            //Get PhotoType from Lookup using IdentifierType
            //Below query take 2ms
            string photoType =
                    _context.Lookup.SingleOrDefault(
                            lk => lk.LookupIndex.ToString() == identifierType &&
                                  lk.LookupType == LookupConstants.IDTYPE)
                        ?.LookupDescription,
                path = "";

            switch (photoType)
            {
                case LookupDescriptionConstants.FRONT_VIEW:
                case LookupDescriptionConstants.SIDE_VIEW:
                case LookupDescriptionConstants.TATTOO:
                case LookupDescriptionConstants.PIERCING:
                case LookupDescriptionConstants.COMPOSITE:
                case LookupDescriptionConstants.IDMARK:
                    path = "IDENTIFIERS\\PERSON\\" + photoType + "\\" + DateTime.Now.ToString("yyyyMM");
                    break;
                case LookupDescriptionConstants.EVIDENCE:
                    path = "IDENTIFIERS\\EVIDENCE\\" + DateTime.Now.ToString("yyyyMM");
                    break;
                case LookupDescriptionConstants.PROPERTY:
                    path = "IDENTIFIERS\\PROPERTY\\" + DateTime.Now.ToString("yyyyMM");
                    break;
                case LookupDescriptionConstants.OTHER:
                    path = "IDENTIFIERS\\OTHER\\" + DateTime.Now.ToString("yyyyMM");
                    break;
            }

            return path;
        }

        #endregion

        //To get images path from Temp Folder
        public List<string> GetTempImages(string facilityName)
        {
            string path =
                $@"{_configuration.GetSection(PathConstants.SITEVARIABLES)[PathConstants.PHOTOPATH]}\{PathConstants.IDENTIFIERS}{PathConstants.TMPIMAGE}{facilityName}\";
            List<string> arrayList = new List<string>();
            if (!Directory.Exists(path)) return arrayList;

            string[] filter = { ".bmp", ".jpg", ".jpeg", ".png" };
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            FileInfo[] fileInfo = directoryInfo.GetFiles();
            arrayList.AddRange(from fi in fileInfo from s in filter where s == fi.Extension select fi.FullName);

            List<string> arrayListServerPath = new List<string>();
            arrayList.ForEach(f=> {
                string[] img = f.Split("\\IDENTIFIERS");
                f = _configuration.GetSection(PathConstants.SITEVARIABLES)["ATIMSPhotoPath"] + "\\IDENTIFIERS" + img[1];
                arrayListServerPath.Add(f);
            });
            return arrayListServerPath;
        }

        //Upload Image To Temp Folder 
        public string UploadTempPhoto(IFormFile uploadedFile, string facilityName)
        {
            string savepath = $@"{_configuration.GetSection(PathConstants.SITEVARIABLES)[PathConstants.PHOTOPATH]}";
            string dbsavepath;
            if (facilityName != null)
            {
                dbsavepath = $@"\{PathConstants.IDENTIFIERS}{PathConstants.TMPIMAGE}{facilityName}";
            }
            else
            {
                dbsavepath = $@"\{PathConstants.IDENTIFIERS}{PathConstants.TEMPPROPERTY}{PathConstants.BACKWARDSLASH}";
                string datePath = $@"{DateTime.Now.ToString(PathConstants.DATEPATH)}{PathConstants.BACKWARDSLASH}";
                dbsavepath += datePath;

            }

            savepath += $@"{dbsavepath}";
            if (!Directory.Exists(savepath))
            {
                Directory.CreateDirectory(savepath);
            }
            savepath = $@"{savepath}\{uploadedFile.FileName}";
            if (File.Exists(savepath))
            {
                File.Delete(savepath);
            }
            FileStream fs = new FileStream(savepath, FileMode.Create);
            uploadedFile.CopyTo(fs);
            fs.Dispose();
            if (facilityName == null)
            {
                string[] img = savepath.Split("\\IDENTIFIERS");
                savepath = _configuration.GetSection(PathConstants.SITEVARIABLES)["ATIMSPhotoPath"] + "\\IDENTIFIERS" + img[1];
            }
            return savepath;
        }

        //Delete Image From Temp Folder
        public void DeleteTempPhoto(string fileName)
        {
            string splitFileName = "IDENTIFIERS" +  fileName.Split("IDENTIFIERS")[1];
            string savedpath =
                $@"{_configuration.GetSection(PathConstants.SITEVARIABLES)[PathConstants.PHOTOPATH]}\{splitFileName}";
            if (File.Exists(savedpath))
            {
                File.Delete(savedpath);
            }
        }

        //Get Inmate Photos
        public List<IdentifierVm> GetIdentifier(int personId)
        {
            List<Lookup> lstLookup = (from lookup in _context.Lookup
                where lookup.LookupType == LookupConstants.IDTYPE
                select lookup).ToList();

            List<IdentifierVm> lstPhotoHistory =
                (from i in _context.Identifiers
                    where i.PersonId == personId && i.DeleteFlag == 0
                    select new IdentifierVm
                    {
                        IdentifierId = i.IdentifierId,
                        PhotographDate = i.PhotographDate,
                        IdentifierNarrative = i.IdentifierNarrative,
                        PhotographRelativePath = _photos.GetPhotoByIdentifier(i),
                        CreatedDate = i.CreateDate,
                        IdentifierType = i.IdentifierType,
                        PhotoTakenBy = i.PhotographTakenBy ?? 0,
                        IdentifierDescription = i.IdentifierDescription,
                        IdentifierLocation = i.IdentifierLocation
                    }).OrderByDescending(i => i.IdentifierId).ToList();

            List<int> lstPersonnelIds = lstPhotoHistory.Select(p => p.PhotoTakenBy).ToList();
            List<PersonnelVm> lstPersonnelVm = _personService.GetPersonNameList(lstPersonnelIds);

            lstPhotoHistory.ForEach(item =>
            {
                if (!string.IsNullOrEmpty(item.IdentifierType))
                {
                    item.IdentifierTypeName = lstLookup.SingleOrDefault(
                        l => Equals(l.LookupIndex, Convert.ToDouble(item.IdentifierType)))?.LookupDescription;
                }

                item.Officer = lstPersonnelVm.SingleOrDefault(w => w.PersonnelId == item.PhotoTakenBy);
            });

            return lstPhotoHistory;
        }
    }
}
