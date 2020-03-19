using GenerateTables.Models;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{

    [Collection("Database collection")]
    public class WizardServiceTest
    {
        private readonly WizardService _wizardService;
        private readonly DbInitialize _fixture;

        public WizardServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            AtimsHubService atimsHubService = new AtimsHubService(fixture.HubContext.Object);
            _wizardService = new WizardService(_fixture.Db, atimsHubService);
        }

        [Fact]
        public void AoWizardProgressVm()
        {
            //Check with 0
            //Act
            List<AoWizardVm> lstAoWizard = _wizardService.GetWizardSteps(0);

            //Assert
            Assert.True(lstAoWizard.Count > 3);

            //Check with value
            //Act
            lstAoWizard = _wizardService.GetWizardSteps(10);

            //Assert
            Assert.True(lstAoWizard.Count > 0);
            AoWizardVm aoWizard = lstAoWizard.Single(l => l.WizardId == 10);
            Assert.Equal("COURT COMMIT", aoWizard.WizardName);
        }

        [Fact]
        public async Task CreateWizardProgress()
        {
            //Arrange
            AoWizardProgressVm aoWizardProgress = new AoWizardProgressVm
            {
                WizardId = 5,
                IncarcerationId = 10
            };

            //Act
            int result = await _wizardService.CreateWizardProgress(aoWizardProgress);

            //Assert
            Assert.Equal(102, result);
        }

        [Fact]
        public void GetWizardProgress()
        {
            //Arrange
            AoWizardProgressVm aoWizardProgress = _wizardService.GetWizardProgress(100);

            //Act
            Assert.Equal(4, aoWizardProgress.WizardId);
        }

        [Fact]
        public async Task SetWizardStepComplete()
        {
            //Arrange
            AoWizardStepProgressVm aoWizardStepProgress = new AoWizardStepProgressVm
            {
                WizardStepProgressId = 16,
                DispInmateId = 101,
                AoWizardFacilityStepId = 10,
                StepCompleteById = 12,
                StepCompleteNote = "WIZARD STEP COMPLETED",
                NextStepComponentId = 21,
                WizardProgressId = 102
            };
            //Before Update
            DisciplinaryInmate disciplinary = _fixture.Db.DisciplinaryInmate.Single(d => d.DisciplinaryInmateId == 101);
            Assert.Null(disciplinary.IncidentWizardStep);
            //Act
            await _wizardService.SetWizardStepComplete(aoWizardStepProgress);

            //Assert
            //After Update
            disciplinary = _fixture.Db.DisciplinaryInmate.Single(d => d.DisciplinaryInmateId == 101);
            Assert.Equal(21, disciplinary.IncidentWizardStep);
        }

        [Fact]
        public void GetTemplateIdFromWizardSteps()
        {
            //Act
            int result = _wizardService.GetTemplateIdFromWizardSteps(5);
            Assert.Equal(4,result);

        }

        //SignalR concept
        //[Fact]
        //public void WizardStepChanged()
        //{
        //    bool result = _wizardService.WizardStepChanged(2, "INAKE");
        //}

    }
}
