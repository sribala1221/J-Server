using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace ServerAPI.Services
{
    public class PersonDescriptorService : IPersonDescriptorService
    {
        private readonly AAtims _context;
        private readonly string _filePath;
        private readonly int _personnelId;
        private readonly IPhotosService _photos;

        public PersonDescriptorService(AAtims context, IConfiguration configuration,
            IHttpContextAccessor ihHttpContextAccessor, IPhotosService photosService)
        {
            _context = context;
            _personnelId = Convert.ToInt32(ihHttpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
            _filePath = configuration.GetSection("SiteVariables")["PhotoPath"] + "\\";
            _photos = photosService;
        }

        public List<PersonDescriptorVm> GetDescriptorDetails(int personId)
        {
            List<PersonDescriptorVm> personDescriptor = (from pd in _context.PersonDescriptor
                where pd.PersonId == personId && !string.IsNullOrEmpty(pd.Code)
                select new PersonDescriptorVm
                {
                    PersonDescriptorId = pd.PersonDescriptorId,
                    PersonId = pd.PersonId,
                    DeleteFlag = pd.DeleteFlag,
                    Category = pd.Category,
                    CategoryMap = pd.CategoryMap,
                    ItemLocation = pd.ItemLocation,
                    Code = pd.Code,
                    DescriptorText = pd.DescriptorText
                }).ToList();
            return personDescriptor;
        }

        public PersonDescriptorDetails GetIdentifierDetails(int personId,bool isDelete)
        {
            PersonDescriptorDetails personDescDetails = new PersonDescriptorDetails
            {
                LstPersonDescripters =
                    GetDescriptorDetails(personId).Where(p => isDelete || p.DeleteFlag == 0).ToList(),
                LstDescriptorPhotoGraphs = (from i in _context.Identifiers
                    where i.PersonDescriptorId.HasValue && i.PersonId == personId
                                                        && !string.IsNullOrEmpty(i.PersonDescriptor.Code) &&
                                                        (isDelete || i.PersonDescriptor.DeleteFlag == 0)
                    select new PersonDescriptorVm
                    {
                        IdentifierId = i.IdentifierId,
                        PersonDescriptorId = i.PersonDescriptorId ?? 0,
                        DeleteFlag = i.PersonDescriptor.DeleteFlag,
                        PersonId = i.PersonDescriptor.PersonId,
                        Category = i.PersonDescriptor.Category,
                        CategoryMap = i.PersonDescriptor.CategoryMap,
                        ItemLocation = i.PersonDescriptor.ItemLocation,
                        Code = i.PersonDescriptor.Code,
                        DescriptorText = i.PersonDescriptor.DescriptorText,
                        PhotographPath = _photos.GetPhotoByIdentifier(i)
                    }).ToList()
            };
            
            if (personDescDetails.LstPersonDescripters.Any())
            {
                List<int> personDescrIds =
                    personDescDetails.LstDescriptorPhotoGraphs.Select(de => de.PersonDescriptorId).ToList();
                personDescDetails.LstPersonDescripters.ForEach(pds =>
                {
                    if (!personDescrIds.Contains(pds.PersonDescriptorId))
                    {
                        pds.PhotographPath = string.Empty; // Person descriptor doesn't have Photograph
                        personDescDetails.LstDescriptorPhotoGraphs.Add(pds);
                    }
                    pds.Region = _context.PersonDescriptorLookup
                .Where(l => l.Code == pds.Code && l.ItemLocation==pds.ItemLocation).Select(s => s.BodyMapRegion).SingleOrDefault();
                });
            }
            string[] itemLocation = personDescDetails.LstPersonDescripters.Select(i => i.Code).ToArray();

            personDescDetails.LstBodyMapRegion = _context.PersonDescriptorLookup
                .Where(bm => itemLocation.Contains(bm.Code)).Select(bm => bm.BodyMapRegion).ToList();
            return personDescDetails;
        }

        public PersonBodyDescriptor GetDescriptorLookupDetails(string[] bodyMap, int personId)
        {
            PersonBodyDescriptor personBodyDescriptor = new PersonBodyDescriptor
            {
                LstDescriptorLookup = (from dl in _context.PersonDescriptorLookup
                                           //  where dl.BodyMapRegion == bodyMap
                                       where bodyMap.Contains(dl.BodyMapRegion)
                    select new DescriptorLookupVm
                    {
                        Code = dl.Code,
                        Category = dl.Category,
                        ItemLocation = dl.ItemLocation
                    }).ToList()
            };

            string[] code = personBodyDescriptor.LstDescriptorLookup.Select(c => c.Code).ToArray();

            personBodyDescriptor.LstCurrentDescriptor = (from pd in _context.PersonDescriptor
                where pd.DeleteFlag == 0 && pd.PersonId == personId && code.Contains(pd.Code)
                select new PersonDescriptorVm
                {
                    PersonDescriptorId = pd.PersonDescriptorId,
                    Code = pd.Code,
                    DescriptorText = pd.DescriptorText
                }).ToList();

            int[] pdId = personBodyDescriptor.LstCurrentDescriptor.Select(c => c.PersonDescriptorId).ToArray();

            personBodyDescriptor.LstIdentifier = (from i in _context.Identifiers
                where i.IdentifierType != "1" && i.PersonDescriptorId.HasValue &&
                      pdId.Contains(i.PersonDescriptorId.Value) && i.PersonId == personId
                select new IdentifierVm
                {
                    PhotographRelativePath = _photos.GetPhotoByIdentifier(i),
                    IdentifierType = i.IdentifierType
                }).ToList();

            return personBodyDescriptor;
        }

        public Task<int> DeleteUndoDescriptor(PersonDescriptorVm descriptor)
        {
            PersonDescriptor dbPerDescriptor = (from d in _context.PersonDescriptor
                where d.PersonDescriptorId == descriptor.PersonDescriptorId
                select d).SingleOrDefault();
            DateTime? deleteDate = DateTime.Now;
            if (dbPerDescriptor != null)
            {
                dbPerDescriptor.DeleteDate = descriptor.DeleteFlag == 1 ? null : deleteDate;
                dbPerDescriptor.DeleteFlag = descriptor.DeleteFlag == 1 ? 0 : 1;

            }
            return _context.SaveChangesAsync();
        }

        public Task<int> InsertUpdateDescriptor(PersonDescriptorVm descriptor)
        {

            PersonDescriptor dpPerDescriptor = (from pd in _context.PersonDescriptor
                where pd.PersonDescriptorId == descriptor.PersonDescriptorId
                select pd).SingleOrDefault();
            if (dpPerDescriptor is null)
            {
                dpPerDescriptor = new PersonDescriptor
                {
                    PersonId = descriptor.PersonId,
                    CreateDate = DateTime.Now,
                    CreatedBy = _personnelId,
                    Code = descriptor.Code,
                    Category = descriptor.Category,
                    ItemLocation = descriptor.ItemLocation,
                };
            }
            else
            {
                dpPerDescriptor.UpdateDate = DateTime.Now;
                dpPerDescriptor.UpdatedBy = _personnelId;
            }
            dpPerDescriptor.DescriptorText = descriptor.DescriptorText;
            if (dpPerDescriptor.PersonDescriptorId <= 0)
            {
                _context.PersonDescriptor.Add(dpPerDescriptor);
                _context.SaveChanges();
            }
            return _context.SaveChangesAsync();
        }

        public List<PersonDescriptorVm> PersonDescriptorLookup() => _context.PersonDescriptorLookup
            .Select(desc => new PersonDescriptorVm
            {
                Category = desc.Category,
                ItemLocation = desc.ItemLocation,
                Code = desc.Code
            }).ToList();
    }
}