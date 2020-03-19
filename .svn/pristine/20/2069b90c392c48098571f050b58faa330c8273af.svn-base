using GenerateTables.Models;
using JwtDb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerAPI.JMS.Tests.Helper;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class PersonAddressServiceTest
    {
        private readonly PersonAddressService _personAddressService;
        private readonly DbInitialize _fixture;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public PersonAddressServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db, _configuration,
                _memory.Object);
            PersonService personService = new PersonService(_fixture.Db);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            CommonService commonService = new CommonService(_fixture.Db, _configuration,
                httpContext, personService,appletsSavedService,interfaceEngineService);
            _personAddressService = new PersonAddressService(fixture.Db, commonService, httpContext, personService,
                interfaceEngineService);
        }

        //[Fact]
        //public void PersonAddressService_GetPersonAddressDetails()
        //{
        //    //Act
        //    PersonAddressDetails personAddressDetails = _personAddressService.GetPersonAddressDetails(55);
        //    //Assert
        //    PersonAddressVm personAddress = personAddressDetails.LstPersonAddress.Single(p => p.AddressId == 11);
        //    Assert.Equal("BUS", personAddress.AddressType);
        //    Assert.Equal("THIRUVALLUVAR NAGAR", personAddress.Street);
        //    Assert.Equal("TAMIL NADU", personAddress.State);
        //    Assert.Equal("VENDOR", personAddress.PersonOccupation);
        //}

        [Fact]
        public async Task PersonAddressService_InsertUpdateAddressDetails()
        {
            //Arrange
            //By Default date taken ,so no need to give the date for Createdate & Updatedate
            PersonAddressDetails personAddressDetails = new PersonAddressDetails
            {
                PersonAddressSave = true,
                PersonId = 70,
                BusAddress = new PersonAddressVm
                {
                    AddressId = 10,
                    AddressType = "BUS",
                    City = "ERODE",
                    BusinessPhone = "41578-451245",
                    Number = "ADSS_100400",
                    State = "TAMIL NADU",
                    Occupation = "DRIVER",
                    Street = "DIAMOND NAGAR 1 ND STREET",
                    Employer = "MECHANICAL WORKER",
                    PersonId = 70
                },
                ResAddress = new PersonAddressVm
                {
                    AddressType = "RES",
                    Street = "AMBETHKER NAGAR",
                    Occupation = "STUDENT",
                    City = "VELLORE",
                    State = "TAMIL NADU",
                    PersonId = 70,
                    PersonBusinessFax = "(111) 412-4154",
                    Suffix = "RESTAN",
                    Direction = "E",
                    AddressOtherNote = "DON'T HAVE MAIL ADDRESS",
                    AddressLookupId = 11,
                    AddressBeat = "NOR"
                }
            };
            Address address =
               _fixture.Db.Address.SingleOrDefault(a => a.AddressCity == "VELLORE");
            Assert.Null(address);
            PersonDescription description =
                 _fixture.Db.PersonDescription.Single(p => p.PersonDescriptionId == 8);
            Assert.Null(description.PersonEmployer);
            //Act
            await _personAddressService.InsertUpdateAddressDetails(personAddressDetails);
            //Assert
            //Inserted values into Address
            address = _fixture.Db.Address.Single(a => a.AddressCity == "VELLORE");
            Assert.Equal("RES", address.AddressType);

            //Updated Values into PersonDescription
            description = _fixture.Db.PersonDescription.Single(p => p.PersonDescriptionId == 8);
            Assert.Equal("MECHANICAL WORKER", description.PersonEmployer);
        }


    }
}
