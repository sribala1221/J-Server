using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class BookingServiceTest
    {
        private readonly BookingService _bookingService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly DbInitialize _fixture;
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public BookingServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            AtimsHubService atimsHubService = new AtimsHubService(_fixture.HubContext.Object);
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor
            { HttpContext = _fixture.Context.HttpContext };
            PersonService personService = new PersonService(_fixture.Db);
            PhotosService photosService = new PhotosService(_fixture.Db, _configuration);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db,
                _configuration, _memory.Object);
            CommonService commonService = new CommonService(_fixture.Db, _configuration,httpContext, personService,appletsSavedService,interfaceEngineService);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(fixture.Db, httpContext,
                personService);
            InmateService inmateService = new InmateService(_fixture.Db, commonService, personService, httpContext,
                facilityPrivilegeService, photosService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(_fixture.Db, _configuration, httpContext);
            RecordsCheckService recordsCheckService = new RecordsCheckService(_fixture.Db, commonService, httpContext);
            BookingReleaseService bookingReleaseService = new BookingReleaseService(
                _fixture.Db, commonService, httpContext, personService, inmateService, recordsCheckService,
                facilityHousingService,interfaceEngineService);
            AltSentService altSentService = new AltSentService(_fixture.Db, commonService, httpContext, photosService,
                personService);
            CellService cellService = new CellService(fixture.Db, commonService, httpContext,
                facilityPrivilegeService, facilityHousingService, _fixture.JwtDb, userMgr.Object);

            PrebookWizardService prebookWizardService = new PrebookWizardService(_fixture.Db,
                _configuration, commonService, cellService, httpContext, atimsHubService,
                personService, inmateService, bookingReleaseService, altSentService,interfaceEngineService,
                photosService,appletsSavedService);
            WizardService wizardService = new WizardService(_fixture.Db,atimsHubService);
            SentenceService sentenceService = new SentenceService(_fixture.Db, commonService, httpContext, personService,
                interfaceEngineService);
            PrebookActiveService prebookActiveService = new PrebookActiveService(_fixture.Db, httpContext, atimsHubService);
            _bookingService = new BookingService(_fixture.Db, commonService, httpContext,
                bookingReleaseService, prebookWizardService, prebookActiveService, sentenceService, wizardService,
                personService, inmateService, photosService, interfaceEngineService);
        }


        [Fact]
        public async Task UpdateBookingComplete()
        {
            //Arrange
            BookingComplete bookingComplete = new BookingComplete
            {
                IncarcerationId = 30,
                InmateId = 103,
                IsComplete = true
            };

            //Before Update
            Incarceration incarceration = _fixture.Db.Incarceration.Single(i => i.IncarcerationId == 30);
            Assert.Null(incarceration.BookCompleteFlag);

            //Act
            await _bookingService.UpdateBookingComplete(bookingComplete);

            //Assert
            //After Update
            incarceration = _fixture.Db.Incarceration.Single(i => i.IncarcerationId == 30);
            Assert.Equal(1, incarceration.BookCompleteFlag);
        }

        [Fact]
        public async Task UpdateRequest()
        {
            //Arrange
            RequestClear requestClear = new RequestClear
            {
                RequestId = 17
            };

            //Before Update
            Request request = _fixture.Db.Request.Single(r => r.RequestId == 17);
            Assert.Equal(11, request.PendingBy);

            //Before Insert
            RequestTrack requestTrack = _fixture.Db.RequestTrack.SingleOrDefault(r => r.RequestId == 17);
            Assert.Null(requestTrack);

            //Act
            await _bookingService.UpdateRequest(requestClear);

            //Assert
            //After Insert
            requestTrack = _fixture.Db.RequestTrack.Single(r => r.RequestId == 17);
            Assert.NotNull(requestTrack);

            //After Update
            request = _fixture.Db.Request.Single(r => r.RequestId == 17);
            Assert.Null(request.PendingBy);
        }

        [Fact]
        public async Task UpdateClearRequest()
        {
            //Arrange
            RequestClear requestClear = new RequestClear
            {
                RequestId = 18
            };
            //Before Clear in request table
            Request request = _fixture.Db.Request.Single(r => r.RequestId == 18);
            Assert.Null(request.ClearedBy);

            RequestTrack requestTrack = _fixture.Db.RequestTrack.SingleOrDefault(r => r.RequestId == 18);
            Assert.Null(requestTrack);

            //Act
            await _bookingService.UpdateClearRequest(requestClear);


            //Assert
            //After clear in request table
            requestTrack = _fixture.Db.RequestTrack.Single(r => r.RequestId == 18);
            Assert.Equal("REQUEST CLEARED", requestTrack.RequestTrackCategory);

            request = _fixture.Db.Request.Single(r => r.RequestId == 18);
            Assert.Equal(11, request.ClearedBy);
        }

        // for testing
        //[Fact]
        //public void GetBookingNoteDetails()
        //{
        //    BookingNoteVm bookingNote = _bookingService.GetBookingNoteDetails(15, false);

        //    Assert.True(bookingNote.ListBookingNoteCount.Count > 0);
        //    Assert.True(bookingNote.ListBookingNoteDetails.Count > 0);

        //}

        [Fact]
        public async Task DeleteBookingNote()
        {
            //Arrange
            BookingNoteDetailsVm bookingNoteDetails = new BookingNoteDetailsVm
            {
                DeleteFlag = 1,
                ArrestNoteId = 9
            };
            ArrestNote arrestNote = _fixture.Db.ArrestNote.Single(a => a.ArrestNoteId == 9);
            Assert.Equal(0, arrestNote.DeleteFlag);

            //Act
            await _bookingService.DeleteBookingNote(bookingNoteDetails);

            //Assert
            arrestNote = _fixture.Db.ArrestNote.Single(a => a.ArrestNoteId == 9);
            Assert.Equal(1, arrestNote.DeleteFlag);
        }

        [Fact]
        public void GetExternalAttachmentDetails()
        {
            //Act
            ExtAttachApproveVm extAttachApprove = _bookingService.GetExternalAttachmentDetails(141);

            //Assert
            Assert.True(extAttachApprove.BookingDetails.Count > 0);
            Assert.True(extAttachApprove.IncarcerationDetails.Count > 0);
            Assert.True(extAttachApprove.IncarcerationType.Count > 2);

        }

        [Fact]
        public async Task UpdateExternalAttachment()
        {
            //Arrange
            ExternalAttachmentsVm externalAttachments = new ExternalAttachmentsVm
            {
                AppletsSavedId = 18,
                InmateId = 120,
                AppletsSavedKeyword1 = "BELT",
                AppletsSavedKeyword4 = "CHAIN",
                AppletsSavedKeyword3 = "RING",
                ArrestId = 15,
                IncarcerationId = 20
            };
            AppletsSaved appletsSaved = _fixture.Db.AppletsSaved.Single(a => a.AppletsSavedId == 18);

            Assert.Null(appletsSaved.ExternalInmateId);
            Assert.Null(appletsSaved.AppletsSavedKeyword3);

            //Act
            await _bookingService.UpdateExternalAttachment(externalAttachments);


            //Assert
            appletsSaved = _fixture.Db.AppletsSaved.Single(a => a.AppletsSavedId == 18);

            Assert.Equal(120, appletsSaved.ExternalInmateId);
            Assert.Equal("RING", appletsSaved.AppletsSavedKeyword3);

        }

        [Fact]
        public void LoadInmateBookings()
        {
            //Act
            BookNoteEntryVm bookNoteEntry = _bookingService.LoadInmateBookings(107);

            //Assert
            Assert.True(bookNoteEntry.ListNoteEntryDetails.Count > 0);
        }

        [Fact]
        public async Task SaveBookingNoteDetails()
        {
            //Arrange
            BookingNoteDetailsVm bookingNoteDetails = new BookingNoteDetailsVm
            {
                ArrestId = 10,
                NoteType = "MISC"
            };
            ArrestNote arrestNote = _fixture.Db.ArrestNote.SingleOrDefault(a => a.ArrestId == 10);
            Assert.Null(arrestNote);

            //Act
            await _bookingService.SaveBookingNoteDetails(bookingNoteDetails);

            //Assert
            arrestNote = _fixture.Db.ArrestNote.Single(a => a.ArrestId == 10);
            Assert.NotNull(arrestNote);
        }


        [Fact]
        public void GetBookingClearDetails()
        {
            //Act
            List<BookingClearVm> lstBookingClear = _bookingService.GetBookingClearDetails(15);

            //Assert
            Assert.True(lstBookingClear.Count > 0);

        }

        [Fact]
        public void GetClearHistoryDetails()
        {
            //Act
            List<HistoryVm> lstHistory = _bookingService.GetClearHistoryDetails(6);

            //Assert
            HistoryVm history = lstHistory.Single(a => a.HistoryId == 11);
            Assert.Equal(50, history.PersonId);
        }

        [Fact]
        public void GetBookingClearlist()
        {
            //Act
            BookClearVm bookClear = _bookingService.GetBookingClearlist(8);

            //Assert
            Assert.True(bookClear.Chargeslist.Count > 0);
            Assert.True(bookClear.ClearBaillist.Count > 0);
            Assert.True(bookClear.WarningClearList.Count > 2);
        }

        [Fact]
        public async Task UpdateClear()
        {
            //Arrange
            BookingClearVm bookingClear = new BookingClearVm
            {
                IncarcerationArrestXrefId = 23,
                ArrestId = 17,
                IncarcerationId = 22,
                PersonId = 70,
                ClearNotes = null,
                ClearReason = "NO COMPLAINT",
                ArrestHistoryList = @"{'BOOKING NUMBER':'TMPB00001089','COURT DOCKET':'<NONE>','SCHEDULED CLEAR':'UNSENT','BOOKING BAIL':'$0.00','CLEAR REASON':'BAIL COMBO-TREASURY BOND & CASH','CLEAR OFFICER':'MORRIS, MATTHEW N. 1511','CLEAR DATE':'11/27/2017 02:39:07 PM','CLEAR NOTE':'GFHGF','WARNING':'None'}"
            };

            IncarcerationArrestXref arrestXref =
                _fixture.Db.IncarcerationArrestXref.Single(i => i.IncarcerationArrestXrefId == 23);
            Assert.Null(arrestXref.ReleaseReason);

            ArrestClearHistory arrestClearHistory =
                _fixture.Db.ArrestClearHistory.SingleOrDefault(a => a.ArrestId == 17);
            Assert.Null(arrestClearHistory);


            //Act
            await _bookingService.UpdateClear(bookingClear);

            //Assert
            arrestXref =
                _fixture.Db.IncarcerationArrestXref.Single(i => i.IncarcerationArrestXrefId == 23);
            Assert.Equal("NO COMPLAINT", arrestXref.ReleaseReason);

            arrestClearHistory =
                _fixture.Db.ArrestClearHistory.Single(a => a.ArrestId == 17);
            Assert.NotNull(arrestClearHistory);
        }

        [Fact]
        public void CheckOverallSentDetails()
        {
            //Act
            OverallSentvm overallSent = _bookingService.CheckOverallSentDetails(11, 14);

            //Assert
            BookingCountDetails bookingCountDetails = overallSent.BookingCountDetails.
                Single(b => b.IncarcerationId == 14);

            Assert.Equal("HOLD", bookingCountDetails.ActiveBookingHold);

        }


        [Fact]
        public void GetSentenceCharges()
        {
            //Arrange
            int[] arrestId = {
                6,8,10
            };
            //Act
            List<ClearChargesVm> lsClearCharges = _bookingService.GetSentenceCharges(arrestId);

            //Assert
            Assert.True(lsClearCharges.Count > 4);

        }

        [Fact]
        public void GetCurrentStatus()
        {
            //Act
            CurrentStatus currentStatus = _bookingService.GetCurrentStatus(15);

            //Assert
            Assert.NotNull(currentStatus.OverAllSentReleaseDate);
        }

        [Fact]
        public void GetOercDetails()
        {
            //Arrange
            OercMethod oercMethod = _bookingService.GetOercDetails();

            //Assert
            Assert.Equal(1, oercMethod.OERCCredit);
        }

        [Fact]
        public async Task UpdateOverAllSentence()
        {
            //Arrange
            OverallSentence overallSentence = new OverallSentence
            {
                IncarcerationId = 31,
                SentERCClear = 0,
                SentERC = 2,
                SentStartDate = DateTime.Now,
                FacilityId = 1,
                AltSentId = 7,
                EventName = "INCIDENT HEARING SCHEDULED"
            };

            IncarcerationSentSaveHistory sentSaveHistory = _fixture.Db.IncarcerationSentSaveHistory.SingleOrDefault(

                i => i.IncarcerationId == 31);
            Assert.Null(sentSaveHistory);

            //Act
            await _bookingService.UpdateOverAllSentence(overallSentence);

            //Assert
            sentSaveHistory = _fixture.Db.IncarcerationSentSaveHistory.Single(
                i => i.IncarcerationId == 31);
            Assert.NotNull(sentSaveHistory);


        }

        [Fact]
        public void GetCautionflag()
        {
            //Act
            List<string> result = _bookingService.GetCautionflag(60);

            //Assert
            Assert.True(result.Count > 3);
        }

        [Fact]
        public async Task UndoClearBook()
        {
            //Arrange
            UndoClearBook undobClearBook = new UndoClearBook
            {
                PersonId = 50,
                ArrestId = 7,
                IncarcerationArrestXrefId = 22,
                IncarcerationId = 30
            };
            //Before Update
            Incarceration incarceration = _fixture.Db.Incarceration.Single(i => i.IncarcerationId == 30);
            Assert.Null(incarceration.BailNoBailFlagTotal);


            BailSaveHistory saveHistory = _fixture.Db.BailSaveHistory.SingleOrDefault(b =>
                b.ArrestId == 7);
            Assert.Null(saveHistory);

            //Act
            await _bookingService.UndoClearBook(undobClearBook);

            //Assert
            //After Update
            incarceration = _fixture.Db.Incarceration.Single(i => i.IncarcerationId == 30);
            Assert.Equal(1, incarceration.BailNoBailFlagTotal);


            saveHistory = _fixture.Db.BailSaveHistory.Single(b =>
                b.ArrestId == 7);
            Assert.NotNull(saveHistory);

        }

        [Fact]
        public void GetActiveBooking()
        {
            //Act
            IEnumerable<BookingActive> bookingActives = _bookingService.GetActiveBooking(2, BookingActiveStatus.All);

            //Assert
            BookingActive active = bookingActives.Single(b => b.InmateId == 107);
            Assert.Equal("MEDIUM", active.Classify);
            Assert.Equal("CHS0410", active.InmateNumber);

        }

        [Fact]
        public void GetBookingPrebookForms()
        {
            //Act
            BookingPrebook bookingPrebook = _bookingService.GetBookingPrebookForms(17);

            //Assert
            Assert.Equal("B-4478500", bookingPrebook.PrebookNumber);
        }

        [Fact]
        public void LoadFormDetails()
        {
            //Act
            IncarcerationFormListVm incarcerationFormList = _bookingService.LoadFormDetails(15, 10, 8, 12);

            //Assert
            Assert.True(incarcerationFormList.FormTemplateCountList.Count > 1);
            Assert.True(incarcerationFormList.BookingForms.Count > 0);
            Assert.True(incarcerationFormList.IncarcerationForms.Count > 0);

        }

        [Fact]
        public void GetAllCompleteTasks()
        {
            //Act
            List<TaskOverview> lstTaskOverview = _bookingService.GetAllCompleteTasks(104);

            //Assert
            Assert.True(lstTaskOverview.Count>0);
        }

    }
}
