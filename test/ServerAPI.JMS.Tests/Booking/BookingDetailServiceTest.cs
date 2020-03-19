using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System.Linq;
using Xunit;

namespace ServerAPI.JMS.Tests
{

    [Collection("Database collection")]
    public class BookingDetailServiceTest
    {
        private readonly BookingDetailService _bookingDetailService;
        private readonly DbInitialize _fixture;

        public BookingDetailServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = _fixture.Context.HttpContext };
            _bookingDetailService = new BookingDetailService(_fixture.Db, httpContext);
        }

        [Fact]
        public void GetIncarcerationFormsDetails()
        {
            //Act
            IncarcerationFormDetails incarcerationFormDetails = _bookingDetailService.
                GetIncarcerationFormsDetails(10, "PERSONAL INVENTORY");
            //Assert
            Assert.Equal(10, incarcerationFormDetails.templateCount.CategoryId);
            Assert.Equal("PERSONAL INVENTORY", incarcerationFormDetails.templateCount.CategoryName);
            Assert.True(incarcerationFormDetails.templateCount.Count > 0);

        }

        [Fact]
        public void DeleteUndoIncarcerationForm()
        {
            //Arrange
            IncarcerationForms incarcerationForms = new IncarcerationForms
            {
                FormRecordId = 11,
                DeleteFlag = 1
            };
            //Before  Delete
            FormRecord formRecord = _fixture.Db.FormRecord.Single(f => f.FormRecordId == 11);
            Assert.Equal(0, formRecord.DeleteFlag);

            //Act
            _bookingDetailService.DeleteUndoIncarcerationForm(incarcerationForms);

            //Assert
            //After Delete
            formRecord = _fixture.Db.FormRecord.Single(f => f.FormRecordId == 11);
            Assert.Equal(1, formRecord.DeleteFlag);


        }

        [Fact]
        public void UpdateFormInterfaceBypassed()
        {
            //Arrange
            IncarcerationForms incarcerationForms = new IncarcerationForms
            {
                FormRecordId = 12,
                FormInterfaceByPassed = 1
            };
            //Before Update
            FormRecord formRecord = _fixture.Db.FormRecord.Single(f => f.FormRecordId == 12);
            Assert.Null(formRecord.FormInterfaceBypassed);

            //Act
            _bookingDetailService.UpdateFormInterfaceBypassed(incarcerationForms);

            //Assert
            //After Update
            formRecord = _fixture.Db.FormRecord.Single(f => f.FormRecordId == 12);
            Assert.Equal(1, formRecord.FormInterfaceBypassed);

           
        }



    }
}

