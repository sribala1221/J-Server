using System;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using ServerAPI.Utilities;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace ServerAPI.Services
{
    public class FacilityHousingService : IFacilityHousingService
    {
        private readonly AAtims _context;
        private IConfiguration Configuration { get; }

        public FacilityHousingService(AAtims context, IConfiguration configuration,
            IHttpContextAccessor ihHttpContextAccessor)
        {
            _context = context;
            Configuration = configuration;
        }

        public HousingDetail GetHousingDetails(int housingUnitId) =>

            _context.HousingUnit.Where(hu=>hu.HousingUnitId == housingUnitId)
                .Select(hu=> new HousingDetail
            {
                HousingUnitListId = hu.HousingUnitListId,
                HousingUnitId = hu.HousingUnitId,
                HousingUnitLocation = hu.HousingUnitLocation,
                HousingUnitNumber = hu.HousingUnitNumber,
                HousingUnitBedLocation = hu.HousingUnitBedLocation,
                HousingUnitBedNumber = hu.HousingUnitBedNumber,
                FacilityId = hu.FacilityId,
                FacilityAbbr = hu.Facility.FacilityAbbr,
                FacilityName = hu.Facility.FacilityName
            }).Single();

        //To get housing unit list base on housing unit id's
        public List<HousingDetail> GetHousingList(List<int> housingUnitIds) =>
            _context.HousingUnit.Where(hu => housingUnitIds.Contains(hu.HousingUnitId))
                .Select(hu => new HousingDetail
                {
                    HousingUnitListId = hu.HousingUnitListId,
                    HousingUnitId = hu.HousingUnitId,
                    HousingUnitLocation = hu.HousingUnitLocation,
                    HousingUnitNumber = hu.HousingUnitNumber,
                    HousingUnitBedLocation = hu.HousingUnitBedLocation,
                    HousingUnitBedNumber = hu.HousingUnitBedNumber,
                    FacilityId = hu.FacilityId,
                    FacilityAbbr = hu.Facility.FacilityAbbr,
                    FacilityName = hu.Facility.FacilityName
                }).ToList();

        //To get HousingUnit details based on facilityId
        public List<HousingDetail> GetHousing(int facilityId) =>
            _context.HousingUnit.Where(h => h.FacilityId == facilityId &&
                                            (h.HousingUnitInactive == 0 || h.HousingUnitInactive == null))
                .GroupBy(g => new
                {
                    g.HousingUnitLocation,
                    g.HousingUnitNumber,
                    g.HousingUnitListId,
                    g.HousingUnitBedNumber
                })
                .Select(h => new HousingDetail
                {
                    HousingUnitLocation = h.Key.HousingUnitLocation,
                    HousingUnitNumber = h.Key.HousingUnitNumber,
                    HousingUnitListId = h.Key.HousingUnitListId,
                    HousingUnitBedNumber = h.Key.HousingUnitBedNumber
                }).ToList();
    }
}
