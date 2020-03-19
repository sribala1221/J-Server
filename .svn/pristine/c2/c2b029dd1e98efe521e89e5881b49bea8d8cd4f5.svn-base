using JwtDb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerAPI.JMS.Tests.Helper;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenerateTables.Models;
using Xunit;
using Microsoft.Extensions.Caching.Memory;

namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class PREAServiceTest
    {
        private readonly PREAService _preaService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly DbInitialize _fixture;
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public PREAServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            PersonService personService = new PersonService(fixture.Db);
            PhotosService photosService = new PhotosService(_fixture.Db, _configuration);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(_fixture.Db,
                _configuration, _memory.Object);
            CommonService commonService = new CommonService(_fixture.Db, _configuration,
                httpContext, personService,appletsSavedService,interfaceEngineService);
            _preaService = new PREAService(_fixture.Db, httpContext, personService, commonService);
        }

        [Fact]
        public void GetClassFileDetails()
        {
            //Act
            PreaDetailsVm preaDetails = _preaService.GetClassFileDetails(50, 100);

            //Assert
            Assert.True(preaDetails.AssociationsList.Count > 0);
            Assert.True(preaDetails.InvolvedGrievances.Count > 0);
            Assert.True(preaDetails.PreaForms.Count > 1);
            Assert.True(preaDetails.PreaNotes.Count > 0);
            Assert.True(preaDetails.InvolvedIncidents.Count > 0);
        }

        [Fact]
        public void GetPreaQueue()
        {
            //Act
            PreaQueueDetailsVm preaQueueDetails = _preaService.GetPreaQueue(1);

            //Assert
            Assert.True(preaQueueDetails.Association.Count > 0);
            Assert.True(preaQueueDetails.Flags.Count > 0);

        }

        [Fact]
        public void GetQueueDetails()
        {
            //Arrange
            PreaQueueSearch preaQueueSearch = new PreaQueueSearch
            {
                PersonIds = new List<int>
                {
                   50,100,70,55,65
                },
                AssocType = "REG.SEX"
            };

            //Act
            List<QueueDetailsVm> lstQueueDetails = _preaService.GetQueueDetails(preaQueueSearch);

            //Assert
            Assert.True(lstQueueDetails.Count > 2);

        }

        [Fact]
        public async Task InsertPreaReview()
        {
            //Arrange
            PreaReviewVm review = new PreaReviewVm
            {
                InmateId = 120,
                PreaReviewFlagId = 3
            };
            //Before Insert
            PREAReview preaReview = _fixture.Db.PREAReview.SingleOrDefault(p => p.InmateId == 120);
            Assert.Null(preaReview);

            //Act
            await _preaService.InsertPreaReview(review);

            //Assert
            //After Insert
            preaReview = _fixture.Db.PREAReview.Single(p => p.InmateId == 120);
            Assert.NotNull(preaReview);

        }

        [Fact]
        public async Task UpdatePreaReview()
        {
            //Arrange
            PreaReviewVm review = new PreaReviewVm
            {
                PreaReviewId = 12,
                PreaReviewDate = DateTime.Now,
                PreaReviewNote = "LAST REVIEW"
            };
            //Before Update
            PREAReview preaReview = _fixture.Db.PREAReview.Single(p => p.PREAReviewId == 12);
            Assert.Null(preaReview.PREAReviewNote);

            //Act
            await _preaService.UpdatePreaReview(review);

            //Assert
            //After Update
            preaReview = _fixture.Db.PREAReview.Single(p => p.PREAReviewId == 12);
            Assert.NotNull(preaReview.PREAReviewNote);
        }

        [Fact]
        public async Task InsertPreaNotes()
        {
            //Arrange
            PreaNotesVm notes = new PreaNotesVm
            {
                InmateId = 110,
                InvestigationFlagIndex = 3
            };
            //Before Insert
            PREANotes preaNotes = _fixture.Db.PREANotes.SingleOrDefault(p => p.InmateId == 110);
            Assert.Null(preaNotes);

            //Act
            await _preaService.InsertPreaNotes(notes);

            //Assert
            //After Insert
            preaNotes = _fixture.Db.PREANotes.Single(p => p.InmateId == 110);
            Assert.NotNull(preaNotes);

        }

        [Fact]
        public async Task UpdatePreaNotes()
        {
            //Arrange
            PreaNotesVm notes = new PreaNotesVm
            {
                PreaNotesId = 12,
                PreaNote = "SPECIAL"
            };
            //Before Update
            PREANotes preaNotes = _fixture.Db.PREANotes.Single(p => p.PREANotesId == 12);
            Assert.Null(preaNotes.PREANote);

            //Act
            await _preaService.UpdatePreaNotes(notes);

            //Assert
            //After Update
            preaNotes = _fixture.Db.PREANotes.Single(p => p.PREANotesId == 12);
            Assert.NotNull(preaNotes.PREANote);
        }
    }
}
