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
using System;
using System.Linq;
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class PersonContactServiceTest
    {
        private readonly PersonContactService _personContactService;
        private readonly DbInitialize _fixture;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();
      


        public PersonContactServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            PersonService personService = new PersonService(_fixture.Db);
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor {HttpContext = fixture.Context.HttpContext};

        InterfaceEngineService interfaceEngineService = new InterfaceEngineService(_fixture.Db,
            _configuration, _memory.Object);

        CommonService commonService = new CommonService(_fixture.Db, _configuration,httpContext,personService,appletsSavedService,interfaceEngineService);
            _personContactService = new PersonContactService(_fixture.Db, commonService, httpContext,personService);
        }

        [Fact]
        public void PersonContact_GetContactDetails()
        {
            //Act
            ContactDetails contactDetails = _personContactService.GetContactDetails(55);
            //Assert
            Assert.NotNull(contactDetails);
        }

        [Fact]
        public void PersonContact_InsertUpdateContactAttempt()
        {
            //Arrange
            ContactAttemptVm contactAttempt = new ContactAttemptVm
            {
                AttemptId = 10,
                AttemptDate = DateTime.Now,
                AttemptTypeLookup = "MISC",
                AttemptDispoLookup = "MISC",
                AttemptNotes = "FIRST TIME ATTEMPT"
            };
            ContactAttempt contact = _fixture.Db.ContactAttempt.Single(c => c.ContactAttemptId == 10);
            Assert.Null(contact.ContactAttemptNotes);

            //Act
            _personContactService.InsertUpdateContactAttempt(contactAttempt);

            //Assert
            contact = _fixture.Db.ContactAttempt.Single(c => c.ContactAttemptId == 10);
            Assert.NotNull(contact.ContactAttemptNotes);
        }

        [Fact]
        public void PersonContact_DeleteUndoContactAttempt()
        {
            ContactAttempt contactAttempt = _fixture.Db.ContactAttempt.Single(c => c.ContactAttemptId == 12);
            Assert.Equal(0, contactAttempt.DeleteFlag);

            //Act
            _personContactService.DeleteUndoContactAttempt(12);

            //Assert
            contactAttempt = _fixture.Db.ContactAttempt.Single(c => c.ContactAttemptId == 12);
            Assert.Equal(1, contactAttempt.DeleteFlag);
        }

        [Fact]
        public void PersonContact_InsertUpdateContact()
        {
            //Arrange
            ContactVm contactVm = new ContactVm
            {
                ContactPersonId = 65,
                InmateId = 103,
                RelationShip = "5",
                ContactType = "4",
                ActiveFlag = "0",
                VictimNotify = 6,
                VictimNotifyNote = "ORGANISE EVENTS TO HELP VICTIMS OF CRIME"
            };
            Contact contact = _fixture.Db.Contact.SingleOrDefault(c => c.PersonId == 65);
            Assert.Null(contact);
            //Act
            _personContactService.InsertUpdateContact(contactVm);
            //Assert
            contact = _fixture.Db.Contact.Single(c => c.PersonId == 65);
            Assert.NotNull(contact);
        }

        [Fact]
        public void PersonContact_DeleteUndoContact()
        {
            ContactVm contactt = new ContactVm
            {
                ContactId = 7,
                ActiveFlag = "0"
            };

            Contact contact = _fixture.Db.Contact.Single(c => c.ContactId == 7);
            Assert.Equal("1", contact.ContactActiveFlag);
            //Act
            _personContactService.DeleteUndoContact(contactt);
            //Assert
            contact = _fixture.Db.Contact.Single(c => c.ContactId == 7);
            Assert.Equal("0", contact.ContactActiveFlag);
        }

        [Fact]
        public void GetContactCreateUpdateDetails()
        {
            //Act
            ContactVm contact = _personContactService.GetContactCreateUpdateDetails(6);
            //Assert
            Assert.NotNull(contact);
            Assert.Equal(11, contact.UpdateBy);
        }


    }
}
