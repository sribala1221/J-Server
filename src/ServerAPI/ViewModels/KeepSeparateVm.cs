﻿using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{

    public class KeepSeparateVm
    {
        public int PersonId { get; set; }
        public int? LocationId { get; set; }
        public int? HousingUnitId { get; set; }
        public int? HousingUnitListId { get; set; }

        public int KeepSeparateId { get; set; }
        public int KeepSepInmateId { get; set; }
        public int KeepSepInmate2Id { get; set; }
        public int KeepSepAssocInmateId { get; set; }
        public int KeepSepSubsetInmateId { get; set; }
        public int KeepSeparateOfficerId { get; set; }

        public string PersonLastName { get; set; }
        public string PersonFirstName { get; set; }
        public string PersonMiddleName { get; set; }
        public string InmateNumber { get; set; }
        public string KeepSepInmateNumber { get; set; }
        public string KeepSepLabel { get; set; }
        public string Assoc { get; set; }
        public string Subset { get; set; }
        public string KeepSepAssoc { get; set; }
        public string KeepSepSubset { get; set; }
        public string KeepSepAssocSubset { get; set; }
        public string KeepSepType { get; set; }
        public string KeepSepReason { get; set; }
        public string KeepSepTypeName { get; set; }
        public KeepSepType Type { get; set; }
        public PersonVm KeepSepInmateDetail { get; set; }
        public HousingDetail HousingDetail { get; set; }
        public HousingDetail KeepsepHousingDetail { get; set; }
        public string Reason { get; set; }
        public string KeepSeparateNote { get; set; }
        public string KeepSepHistoryList { get; set; }
        public string FacilityAbbr { get; set; }
        public bool InmateActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? KeepSeparateDate { get; set; }
        public int? KeepSepAssoc1SubsetId { get; set; }
        public int? KeepSepAssoc1Id { get; set; }

        public int? KeepSepAssoc2Id { get; set; }
        public string Number { get; set; }
        public string Classify { get; set; }
        public int Deleteflag { get; set; }
        public string keepSepAssoc1 { get; set; }
        public string keepSepAssoc1Subset { get; set; }
        public int KeepSepAssocAssocId { get; set; }
        public string keepSepAssoc2 { get; set; }
        public int KeepSepAssocSubsetId { get; set; }
        public int KeepSepSubsetSubsetId { get; set; }
        public string KeepSepAssoc2Subset { get; set; }
        public int? AssocId { get; set; }
        public int? SubsetId { get; set; }
        public int? Count { get; set; }
        public int? GenderId { get; set; }
        public int? RaceId { get; set; }
        public int? ClassifyId { get; set; }
        public bool IllegalAlienOnly { get; set; }

        public string keepsep { get; set; }

        public string KeepSepPersonLastName { get; set; }
        public string KeepSepPersonFirstName { get; set; }
        public string KeepSepPersonMiddleName { get; set; }
        public int KeepSepPersonId { get; set; }
        public int? keepSepHousingUnitListId { get; set; }
        public int? keepSepHousingUnitId { get; set; }


    }



    public class KeepSeparateAlertVm
    {
        public List<KeepSeparateVm> KeepSeparateAssocList { get; set; }
        public List<KeepSeparateVm> KeepSeparateInmateList { get; set; }
        public List<KeepSeparateVm> KeepSeparateSubsetList { get; set; }
        public List<AssociationCount> KeepSeparateAssocCount { get; set; }
        public List<SubsetCount> KeepSeparateSubsetCount { get; set; }
        //public List<KeepSeparateVm> PersonClassificationDetails { get; set; }
        public HousingUnitListDetailVm HousingDetails { get; set; }

    }

    public class SubsetCount
    {
        public string Association { get; set; }
        public string Subset { get; set; }
        public int? Count { get; set; }
        public List<KeepSeparateVm> SubsetList { get; set; }
    }

    public class AssociationCount
    {
        public string Association { get; set; }
        public int? Count { get; set; }
        public List<KeepSeparateVm> AssocList { get; set; }
        public List<KeyValuePair<int,string>> Subsetlist {get; set;}
    }


    public class KeepSepInmateDetailsVm
    {
        public int PeronId { get; set; }
        public int InmateId { get; set; }
        public int FacilityId { get; set; }
        public int HousingUnitId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Location { get; set; }
        public string InmateNumber { get; set; }
        public string ConflictType { get; set; }
        public HousingDetail Housing { get; set; }
    }

    public class KeepSepSearchVm
    {
        public int GenderId { get; set; }
        public int RaceId { get; set; }
        public int AssociationId { get; set; }
        public int InmateId { get; set; }
        public int FacilityId { get; set; }
        public int ClassifyId { get; set; }
        public string Classify { get; set; }
        public int SubsetId { get; set; }
        public int HousingUnitListId { get; set; }
        public int HousingUnitId { get; set; }
        public int HousingGroupId { get; set; }
        public string HousingLocation { get; set; }
        public bool IllegalAlienOnly { get; set; }
        public string ClassificationReason { get; set; }
        public string ClassificationStatus { get; set; }
        public int FlagIndex { get; set; }
        public int Flag { get; set; }
        public AlertFLag AlertFLag { get; set; }
        public string InmateCurrentTrack { get; set; }
        public HousingUnitVm HousingUnit { get; set; }
        public bool Deleted { get; set; }

    }

    public class HousingUnitListDetailVm
    {
        public List<HousingUnitListVm> HousingNumber { get; set; }
        public List<string> HousingBuilding { get; set; }
        public List<HousingGroupAssignVm> HousingGroups { get; set; }

    }


}
