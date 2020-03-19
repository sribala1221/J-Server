using Microsoft.Extensions.Configuration;
using ServerAPI.JMS.Tests.Helper;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class SearchServiceTest
    {
        private readonly SearchService _search;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        public SearchServiceTest(DbInitialize fixture)
        {
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            _search = new SearchService(fixture.Db,photosService);
        }

        // Due to under processing development
        //[Fact]
        //public void Search_GetBookingSearchList()
        //{
        //    //Arrange
        //    SearchRequestVm searchreq = new SearchRequestVm
        //    {
        //        FacilityId = 1,
        //        ArrestDateFrom = DateTime.Now.AddDays(-2),
        //        IsArrestSearch = true,
        //        BookingNumber = "160001194",
        //        CaseNumber = "CASE_01",
        //        CourtDocket = "DOCKET_100100",
        //        ArrestDateTo = DateTime.Now,
        //        BookingDateFrm = DateTime.Now,
        //        ReleaseReason = "BAIL",
        //        BookingDateTo = DateTime.Now,
        //        IsInmateSearch = true,
        //        IsPersonSearch = true,
        //        InmateSiteNumber = "IS1000",
        //        PrebookNumber = "B-4154511",
        //        IncarcerationSearch = true,
        //        ClearByOfficer = 12,
        //        IntakeOfficer = 12,
        //        ClearedDateFrom = DateTime.Now,
        //        ClearedDateTo = DateTime.Now,
        //        SentenceStartDate = DateTime.Now,
        //        SentenceEndDate = DateTime.Now,
        //        UnSentencedOnly = true,
        //        DateOfBirth = DateTime.Now.AddYears(-25),
        //        CiiNumber = "PC2015",
        //        PhoneCode = "04322",
        //        Classify = "MINIMUM",
        //        BillingAgency = 5,
        //        WarrantNumber = "WARNUM4501",
        //        IsCharSearch = true,
        //        Eyecolor = 7,
        //        Haircolor = 42,
        //        Race = 5,
        //        gender = 2,
        //        Weightfrom = 50,
        //        Weightto = 75,
        //        HSfrom = 10,
        //        HPfrom = 10,
        //        HSto = 10,
        //        HPto = 25,
        //        Occupation = "DRIVER",
        //        Descriptor = "MUSIC SYMBOLS",
        //        Location = "ARM,RIGHT",
        //        ItemCode = "TAT L ARM",
        //        Agefrom = 24,
        //        Ageto = 24,
        //        IsAddressSearch = true,
        //        ChargeType = "M",
        //        ChargeSection = "IRSR100",
        //        ChargeDescription = "IS CASE(1AS)",
        //        AddressNumber = "ADDS_4178",
        //        AddressDirection = "EAST",
        //        AddressSuffix = "CN",
        //        DirectionSuffix = "E",
        //        AddressStreet = "CHANDRA NAGAR",
        //        AddressCity = "RAMESHWARAM",
        //        AddressState = "TAMIL NADU",
        //        SiteBooking = "45000410",
        //        BookingType = 4,
        //        InmateNumber = "SV"
        //    };
        //    //Act
        //    List<SearchResult> lstsearchresult = _search.GetBookingSearchList(searchreq);
        //    //Assert
        //    SearchResult searchresult = lstsearchresult.Single(r => r.FacilityId == 1);
        //    Assert.Equal("SANGEETHA", searchresult.FirstName);
        //    Assert.Equal("MINIMUM", searchresult.Classify);
        //    Assert.Equal("http://localhost:5151/atims_dir/IDENTIFIERS\\PERSON\\FRONT_VIEW\\201509\\2_FACE.jpg", searchresult.Photofilepath);
        //}

        [Fact]
        public void Search_GetCrimeChargesDetails()
        {
            //Act
            List<BookingSearchSubData> lstbooksearch = _search.GetChargeDetails(7);
            //Assert
            BookingSearchSubData booksearchsubdate = lstbooksearch.Single(b => b.BailAmount == 5000M);
            Assert.Equal("I", booksearchsubdate.Type);
            Assert.Equal("NO BAIL", booksearchsubdate.BailType);
        }


        [Fact]
        public void Search_GetIncarcerationDetails()
        {
            //Act
            List<InmateIncarcerationDetails> lstinmateincarceration = _search.GetIncarcerationDetails(104);
            //Assert
            InmateIncarcerationDetails inmateincarceration = lstinmateincarceration.Single(i => i.InmateId == 104);
            Assert.Equal(14, inmateincarceration.IncarcerationId);
            IncarcerationArrestXrefDetails incarcerationarrest =
                inmateincarceration.IncarcerationArrestXrefDetailLSt.Single(i => i.IncarcerationId == 14);
            Assert.Equal("COURT", incarcerationarrest.BookingType);
            Assert.Equal("ACTIVE", incarcerationarrest.BookingStatus);
        }
    }
}
