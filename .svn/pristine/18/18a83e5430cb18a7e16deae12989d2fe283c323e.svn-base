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
using System.Collections.Generic;
using System.Linq;
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class IntakeServiceTest
    {
        private readonly IntakeService _intake;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public IntakeServiceTest(DbInitialize fixture)
        {
            DbInitialize fixture1 = fixture;
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            PersonService personService = new PersonService(fixture1.Db);
            AtimsHubService atimsHubService = new AtimsHubService(fixture.HubContext.Object);
            PhotosService photosService = new PhotosService(fixture1.Db, _configuration);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture1.Db,
                _configuration, _memory.Object);
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture1.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            CommonService commonService = new CommonService(fixture.Db, _configuration,httpContext, personService,appletsSavedService,interfaceEngineService);
            PersonPhotoService personPhotoService = new PersonPhotoService(fixture1.Db, _configuration, httpContext,
                personService, photosService);
            _intake = new IntakeService(fixture1.Db, commonService, personPhotoService,httpContext,atimsHubService);
        }

        [Fact]
        public void Intake_GetIntake()
        {
            //Act
            IntakeInmate intakeInmate = _intake.GetIntake(2, true);
            //Assert    
            IntakeVm intakeVm = intakeInmate.IntakeDetails.Single(i => i.IncarcerationId == 11);
            Assert.Equal("PJS001", intakeVm.InmateNumber);
            Assert.Equal("B-4154510", intakeVm.PrebookNumber);
            Assert.Equal("SUGIR", intakeVm.PersonFirstName);
        }


        [Fact]
        public void GetInmateIdFromPrebook()
        {
            //Act
            IntakeVm intake = _intake.GetInmateIdFromPrebook(26);

            //Assert
            Assert.Equal(107, intake.InmateId);
            Assert.Equal(18, intake.IncarcerationId);

        }

        [Fact]
        public void GetTasksDetails()
        {
            //Arrange
            int[] inmateIds = new int[]
            {
               101,105,108,120

            };

            //Act
            List<TasksCountVm> lstList = _intake.GetTasksDetails(2, inmateIds);

            //Assert
            Assert.True(lstList.Count > 0);
            TasksCountVm tasksCount = lstList.Single(l => l.TaskLookupId == 12);
            Assert.Equal("PRE CLASS", tasksCount.TaskName);

        }

    }
}
