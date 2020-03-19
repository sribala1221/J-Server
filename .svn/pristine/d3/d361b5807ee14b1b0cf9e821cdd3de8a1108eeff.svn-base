using Microsoft.AspNetCore.Http;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using GenerateTables.Models;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{

    [Collection("Database collection")]
    public class MedicalFormsServiceTest
    {
        private readonly MedicalFormsService _medicalFormsService;
        private readonly DbInitialize _fixture;

        public MedicalFormsServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            _medicalFormsService = new MedicalFormsService(_fixture.Db, httpContext);
        }

        [Fact]
        public void GetMedicalCategory()
        {
            //Arrange
            MedicalFormsParam medicalFormsParam = new MedicalFormsParam
            {
                DateTo = DateTime.Now,
                InmateId = 140,
                FacilityId = 2,
                PersonnelId = 12,
                DateFrom = DateTime.Now.AddDays(-5)
            };

            //Act
            MedicalFormVm medicalForm = _medicalFormsService.GetMedicalCategory(medicalFormsParam);

            //Assert
            Assert.True(medicalForm.lstGetFormTemplates.Count > 0);

        }

        [Fact]
        public async Task InsertFormRecord()
        {
            //Arrange
            MedicalFormsVm medicalForms = new MedicalFormsVm
            {
                InmateId = 141,
                IncarcerationId = 15,
                FormTemplatesId = 16
            };
            //Before Insert
            FormRecord formRecord = _fixture.Db.FormRecord.SingleOrDefault(f => f.InmateId == 141);
            Assert.Null(formRecord);

            //Act
            await _medicalFormsService.InsertFormRecord(medicalForms);

            //Assert
            //After Insert
            formRecord = _fixture.Db.FormRecord.SingleOrDefault(f => f.InmateId == 141);
            Assert.NotNull(formRecord);
        }

        [Fact]
        public async Task UpdateFormRecord()
        {
            //Arrange
            MedicalFormsVm medicalForms = new MedicalFormsVm
            {
                FormNotes = "DISC INCIDENT",
                XmlData = @"{'form_Record_id':null,'arrest_agency':'DOJ GAMBLING CONTROL','arrest_booking_no':null,'person_dob':'03/02/1950','person_name_lfm':'test, one','inmate_number':null,'q1':'false','q2':'false'}"
            };

            //Before Update
            FormRecord formRecord = _fixture.Db.FormRecord.Single(f => f.FormRecordId == 11);
            Assert.Null(formRecord.FormNotes);


            //Act
            await _medicalFormsService.UpdateFormRecord(11, medicalForms);

            //After Update
            //Assert
            formRecord = _fixture.Db.FormRecord.Single(f => f.FormRecordId == 11);

            Assert.NotNull(formRecord.FormNotes);
        }

        [Fact]
        public async Task DeleteUndoFormRecord()
        {
            //Arrange
            MedicalFormsParam medicalFormsParam = new MedicalFormsParam
            {
                FormRecordId = 18
            };

            //Before Delete
            FormRecord formRecord = _fixture.Db.FormRecord.Single(f => f.FormRecordId == 18);
            Assert.Equal(0, formRecord.DeleteFlag);

            //Act
            await _medicalFormsService.DeleteUndoFormRecord(medicalFormsParam);

            //After Delete
            //Assert
            formRecord = _fixture.Db.FormRecord.Single(f => f.FormRecordId == 18);
            Assert.Equal(1, formRecord.DeleteFlag);
            }
    }
}
