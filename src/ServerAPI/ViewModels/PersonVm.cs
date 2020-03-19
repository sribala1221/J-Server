﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace ServerAPI.ViewModels
{

    public class PersonVm
    {
        public int? InmateId { get; set; }
        public int PersonId { get; set; }
        public bool IsConfidential { get; set; }
        public int? InmatePreBookId { get; set; }
        public int FacilityId { get; set; }
        public int PersonnelId { get; set; }
        public string PersonFirstName { get; set; }
        public string PersonLastName { get; set; }
        public string PersonMiddleName { get; set; }
        public string PersonSuffix { get; set; }
        public DateTime? PersonDob { get; set; }
        public int? PersonAge { get; set; }
        public string PersonDlNumber { get; set; }
        public string FknLastName { set; get; }
        public string FknFirstName { set; get; }
        public string FknMiddleName { set; get; }
        public string FknSuffixName { set; get; }
        public List<AkaVm> Aka { get; set; }
        public string PersonDlState { set; get; }
        public string PersonDlClass { get; set; }
        public int? PersonDlNoExpiration { get; set; }
        public DateTime? PersonDlExpiration { set; get; }
        public string PersonSsn { set; get; }
        public string PersonFbiNo { set; get; }
        public string PersonCii { set; get; }
        public string PersonDoc { get; set; }
        public string PersonAlienNo { get; set; }
        public string AfisNumber { get; set; }
        public string PersonPhone { set; get; }
        public string PersonBusinessPhone { set; get; }
        public string PersonBusinessFax { set; get; }
        public string PhoneCode { get; set; }
        public string PersonPhone2 { set; get; }
        public string PersonCellPhone { set; get; }
        public string PersonCitizenship { get; set; }
        public string PersonEmail { get; set; }
        public string PersonPlaceOfBirth { set; get; }
        public string PersonPlaceOfBirthList { set; get; }
        public bool PersonIllegalAlien { set; get; }
        public bool PersonUsCitizen { set; get; }
        public string PersonMaidenName { set; get; }
        public string PersonOtherIdType { set; get; }
        public string PersonOtherIdNumber { set; get; }
        public string PersonOtherIdState { set; get; }
        public string PersonOtherIdDescription { set; get; }
        public string PersonSiteNumber { get; set; }
        public int? PersonSexLast { get; set; }
        public int? PersonRaceLast { get; set; }
        public int? PersonHeightLast { get; set; }
        public int? PersonEyeColor { get; set; }
        public int? PersonHeightPrimaryLast { get; set; }
        public int? PersonHeightSecondaryLast { get; set; }
        public DateTime? PersonOtherIdExpiration { set; get; }
        public DateTime? PersonDeceasedDate { set; get; }
        public string PersonDeceased { set; get; }
        public DateTime? PersonMissingDate { set; get; }
        public string PersonMissing { set; get; }
        public string RelationShip { set; get; }
        public string RelationShipFirstName { set; get; }
        public string RelationShipLastName { set; get; }
        public string RelationShipMiddleName { set; get; }
        public PersonAddressVm ResAddress { get; set; }
        public PersonAddressVm BusAddress { get; set; }
        public PersonAddressVm ContactAddress { get; set; }
        public PersonCharVm PersonChar { get; set; }
        public string InmateNumber { get; set; }
        public int? HousingUnitId { get; set; }
        public bool InmateActive { get; set; }
        public DateTime? LastReviewDate { get; set; }
        public int? InmateCurrentTrackId { get; set; }
        public string InmateCurrentTrack { get; set; }
        public string OfficerBadgeNumber { get; set; }
        public string PersonnelBadgeNumber { get; set; }
        public int? CreateBy { get; set; }
        public int? UpdateBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string MilitaryId { get; set; }
        public string PersonHistoryList { get; set; }
        public string PersonNumber { get; set; }
        public string InmateClassificationReason { get; set; }
        public int InmateClassificationId { get; set; }
        public DateTime? InmateTrakDateOut { get; set; }
        public bool NoKeeper { get; set; }
        public string MaritalStatusName { get; set; }
        public string EnrouteFinalLocation { get; set; }
        public bool EnrouteOutFlag { get; set; }
        public bool EnrouteInFlag { get; set; }
        public bool EnrouteFinalFlag { get; set; }
        public DateTime? EnrouteOutDate { get; set; }
        public DateTime? InmateCurrentTrakDateIn { get; set; }
        public DateTime? InmateCurrentTrakDateOut { get; set; }
        public string PhotoFilePath { get; set; }
        public DateTime? EnrouteStartOut { get; set; }
        public int? DuplicateId { get; set; }
        public string ExternalApi { get; set; }
        public string Gender { get; set; } 
        public string HousingUnitLocation { get; set; }
        public string HousingUnitNumber { get; set; }
        public string HousingUnitBedNumber { get; set; }
        public string HousingUnitBedLocation { get; set; }
        public string FacilityAbbr { get; set; }
    }

    public class PersonPreBookInfo
    {
        public string Photofilepath { get; set; }
        public string InmateNumber { get; set; }
        public bool ActiveInmate { get; set; }
        public string PersonFirstName { get; set; }
        public string PersonLastName { get; set; }
        public string PersonMiddleName { get; set; }
        public string PersonSuffix { get; set; }
        public DateTime? PersonDob { get; set; }
        public string PersonAlienNo { set; get; }
        public string PersonCii { set; get; }
        public string PersonDlNumber { get; set; }
        public string PersonFbiNo { set; get; }
        public int? InmateId { get; set; }
        public int PersonId { get; set; }
        public string PersonPhone { set; get; }
        public string PersonSsn { set; get; }
        public int InmatePrebookId { get; set; }
        public string AkaCii { get; set; }
        public string AkaSsn { get; set; }
        public string Moniker { get; set; }
        public string AkaDlNumber { get; set; }
        public string AkaInmateNumber { get; set; }
        public string PersonDlState { get; set; }
        public string AkaDlState { get; set; }
        public string OfficerId { get; set; }
        public int? PersonSexId{ get; set; }
        public int? PersonEyeColorId { get; set; }
        public int? PersonHeightPrimary { get; set; }
        public int? PersonDlNoExpiration { get; set; }
        public int? PersonHeightSecondary { get; set; }
        public int? PersonWeight { get; set; }
        public int? PersonHairColorId { get; set; }

    }

    public class PersonWeightedSearchResult
    {
        public PersonWeightedSearchResult(DataRow r)
        {
            PersonId = int.Parse(r["person_id"].ToString());
            PersonAlienNo = r["person_alien_no"].ToString();
            PersonCii = r["person_cii"].ToString();
            PersonDlNumber = r["person_dl_number"].ToString();
            PersonDob = ConvertStringToDateTime(r["person_dob"].ToString());
            PersonFbiNo = r["person_fbi_no"].ToString();
            PersonFirstName = r["person_first_name"].ToString();
            PersonLastName = r["person_last_name"].ToString();
            PersonMiddleName = r["person_middle_name"].ToString();
            PersonSsn = r["person_ssn"].ToString();
            PersonSuffix = r["person_Suffix"].ToString();
            PersonPhone = r["person_phone"].ToString();
            Photofilepath = r["identifier"].ToString();
            InmateNumber = r["inmate_number"].ToString();
            ActiveInmate = Convert.ToBoolean(r["inmate_active"]);
            OfficerId = r["personnel_id"].ToString();
            Aka = new List<AkaVmWeightedSearch>
            {
                new AkaVmWeightedSearch
                {
                    AkaFirstName = r["aka_first_name"].ToString(),
                    AkaLastName = r["aka_last_name"].ToString(),
                    AkaMiddleName = r["aka_middle_name"].ToString(),
                    AkaSuffix = r["aka_suffix"].ToString(),
                    AkaDob = ConvertStringToDateTime(r["aka_dob"].ToString())
                }
            };
        }
        public string Photofilepath { get; set; }
        public string InmateNumber { get; set; }
        public bool ActiveInmate { get; set; }
        public string PersonFirstName { get; set; }
        public string PersonLastName { get; set; }
        public string PersonMiddleName { get; set; }
        public string PersonSuffix { get; set; }
        public DateTime? PersonDob { get; set; }
        public string PersonAlienNo { set; get; }
        public string PersonCii { set; get; }
        public string PersonDlNumber { get; set; }
        public string PersonFbiNo { set; get; }
        public int? InmateId { get; set; }
        public int PersonId { get; set; }
        public string PersonPhone { set; get; }
        public string PersonSsn { set; get; }
        public List<AkaVmWeightedSearch> Aka { set; get; }
        public string OfficerId { get; set; }
        private static DateTime? ConvertStringToDateTime(string date) =>
            string.IsNullOrEmpty(date) ? null
            : (DateTime?) DateTime.ParseExact(date, "MM/dd/yyyy", CultureInfo.InvariantCulture);
    }
    public class AkaVmWeightedSearch
    {
        public string AkaLastName { get; set; }
        public string AkaFirstName { get; set; }
        public string AkaMiddleName { get; set; }
        public string AkaSuffix { get; set; }
        public DateTime? AkaDob { get; set; }
    }

    public class PersonSearchResult : PersonVm
    {

        public int Confidence { get; set; }
        public bool ActiveInmate { get; set; }
        public string Photofilepath { get; set; }
    }

    public class AkaVm
    {
        public int AkaId { get; set; }
        public int? PersonId { get; set; }
        public string AkaLastName { get; set; }
        public string AkaFirstName { get; set; }
        public string AkaMiddleName { get; set; }
        public string AkaSuffix { get; set; }
        public DateTime? AkaDob { get; set; }
        public string PersonGangName { get; set; }
        public string AkaDl { get; set; }
        public string AkaDlState { get; set; }
        public int? AkaDlNoExpiration { get; set; }
        public string AkaSsn { get; set; }
        public string AkaFbi { get; set; }
        public string AkaCii { get; set; }
        public string AkaAlienNo { get; set; }
        public string AkaDoc { get; set; }
        public string AkaOtherIdNumber { get; set; }
        public string AkaOtherIdType { get; set; }
        public string AkaOtherIdState { get; set; }
        public string AkaSocialMediaType { get; set; }
        public string AkaSocialMediaAccount { get; set; }
        public string AkaSocialMediaDescription { get; set; }
        public string PersonGangStatus { get; set; }
        public DateTime? AkaDlExpiration { get; set; }
        public string AkaAfisNumber { get; set; }
        public DateTime? CreateDate { get; set; }
        public int CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }

        public DateTime? UpdateDate { get; set; }
        public string CreateByPersonLastName { get; set; }
        public string CreateByPersonFirstName { get; set; }
        public string CreateByPersonMiddleName { get; set; }
        public string CreateByOfficerBadgeNumber { get; set; }
        public string UpdatedByPersonLastName { get; set; }
        public string UpdatedByPersonFirstName { get; set; }
        public string UpdatedByPersonMiddleName { get; set; }
        public string UpdatedByOfficerBadgeNumber { get; set; }
        public int? DeleteFlag { get; set; }
        public string AkaOtherIdDescription { get; set; }
        public string AkaOtherPhoneType { get; set; }
        public string AkaOtherPhoneNumber { get; set; }
        public string AkaOtherPhoneDescription { get; set; }
        public string AkaInmateNumber { get; set; }
        public string AkaSiteInmateNumber { get; set; }
        public string AkaHistoryList { get; set; }
        public int InmateId { get; set; }
    }

    public class HistoryVm
    {
        public int HistoryId { get; set; }
        public DateTime? CreateDate { get; set; }
        public int PersonId { get; set; }
        public string OfficerBadgeNumber { get; set; }
        public string HistoryList { get; set; }
        public string PersonLastName { get; set; }
        public string PersonFirstName { get; set; }
        public List<PersonHeader> Header { get; set; }
        public string PersonMiddleName { get; set; }
    }

    public class PersonAddressVm
    {
        public int AddressId { get; set; }
        public string Number { get; set; }
        public string Direction { get; set; }
        public string DirectionSuffix { get; set; }
        public string Street { get; set; }
        public string Suffix { get; set; }
        public string UnitType { get; set; }
        public string UnitNo { get; set; }
        public string Line2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string Employer { get; set; }
        public string AddressOtherNote { get; set; }
        public string Occupation { get; set; }
        public string BusinessPhone { get; set; }
        public string BusinessFax { get; set; }
        public bool IsHomeless { get; set; }
        public bool IsTransient { get; set; }
        public bool IsRefused { get; set; }
        public int PersonId { get; set; }
        public string AddressType { get; set; }
        public int CreateBy { get; set; }
        public int? UpdateBy { get; set; }
        public string AddressZone { get; set; }
        public string AddressGridLocation { get; set; }
        public string AddressBeat { get; set; }
        public int? AddressLookupId { get; set; }
        public string PersonEmployer { get; set; }
        public string PersonOccupation { get; set; }
        public string PersonBusinessPhone { get; set; }
        public string PersonBusinessFax { get; set; }
        public DateTime? PersonCreateDate { get; set; }
        public int PersonDescriptionId { get; set; }
        public string PersonPhone { get; set; }
        public string PersonCellPhone { get; set; }
    }

    public class PersonIncarceration : PersonInfoVm
    {
        public int IncarcerationId { get; set; }
        public DateTime? DateIn { get; set; }

        public DateTime? ReleaseDate { get; set; }

        //Description property planned to use in client side based on IncarcerationDescType
        public string Description { get; set; }

        public IncarcerationDescType IncarcerationDescType { get; set; }
        public string IncarcerationHistoryList { get; set; }
    }

    public class PersonHistoryVm
    {
        public int PersonHistoryId { get; set; }
        public int PersonCitizenshipHistoryId { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string PersonLastName { get; set; }
        public string OfficerBadgeNumber { get; set; }
        public int PersonId { get; set; }
        public List<PersonHeader> Header { get; set; }
        public int CreateBy { get; set; }
        public int? DeleteBy { get; set; }
        public int? UpdateBy { get; set; }
        public string DeleteByPersonLastName { get; set; }
        public string UpdateByPersonLastName { get; set; }
        public string CreateByPersonLastName { get; set; }
        public string DeleteByOfficerBadgeNumber { get; set; }
        public string UpdateByOfficerBadgeNumber { get; set; }
        public string CreateByOfficerBadgeNumber { get; set; }
        public PersonCitizenshipVm Citizenship { get; set; }
        public string PersonHistoryList { get; set; }
        public string FromPage { get; set; }
    }

    public class PersonPhoto
    {
        public int IdentifierId { get; set; }
        public int? PersonId { get; set; }
        public string PhotoType { get; set; }
        public string PhotoTypeName { get; set; }
        public string PhotographDate { get; set; }
        public int? PhotographTakenBy { get; set; }
        public string PhotographRelativePath { get; set; }
        public string DescriptorText { get; set; }
        public string PersonFirstName { get; set; }
        public string PersonLastName { get; set; }
        public string OfficerBadgeNumber { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? DescriptorId { get; set; }
        public string IdentifierDescription { get; set; }
        public string NarrativeText { get; set; }
        public string LocationText { get; set; }
        public int? IncidentId { get; set; }
        public string TempPhotoPath { get; set; }
        public string Base64String { get; set; }
        public int? PersonalInventoryId { get; set; }
    }

    public class PersonDescriptorVm
    {
        public int PersonDescriptorId { get; set; }
        public int? PersonId { get; set; }
        public string Code { get; set; }
        public string DescriptorText { get; set; }
        public string Category { get; set; }
        public string CategoryMap { get; set; }
        public string ItemLocation { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int DeleteFlag { get; set; }
        public int IdentifierId { get; set; }
        public string PhotographPath { get; set; }
        public string Region { get; set; }
    }

    public class DescriptorLookupVm
    {
        public int DescriptorLookupId { get; set; }
        public string Category { get; set; }
        public string CategoryMap { get; set; }
        public string ItemLocation { get; set; }
        public string Code { get; set; }
        public string BodyMapRegion { get; set; }
    }

    public class PersonDescriptorDetails
    {
        public List<PersonDescriptorVm> LstPersonDescripters { get; set; }
        public List<PersonDescriptorVm> LstDescriptorPhotoGraphs { get; set; }
        public List<string> LstBodyMapRegion { get; set; }
    }

    public class PersonBodyDescriptor
    {
        public List<DescriptorLookupVm> LstDescriptorLookup { get; set; }
        public List<IdentifierVm> LstIdentifier { get; set; }
        public List<PersonDescriptorVm> LstCurrentDescriptor { get; set; }
    }

    public class PersonHeader
    {
        public string Header { get; set; }
        public string Detail { get; set; }
    }

    public class PersonAddressDetails
    {
        public PersonAddressVm ResAddress { get; set; }
        public PersonAddressVm BusAddress { get; set; }
        public PersonAddressVm MailAddress { get; set; }
        public PersonAddressVm OtherAddress { get; set; }
        public List<PersonAddressVm> LstPersonAddress { get; set; }
        public int PersonId { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public PersonHistoryVm PersonHistoryVm { get; set; }
        public bool PersonAddressSave { get; set; }
        public string MailSiteOption { get; set; }
        public string OtherSiteOption { get; set; }
    }

    public class DnaVm
    {
        public int DnaId { get; set; }
        public int PersonId { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? GatheredDate { get; set; }
        public int? Disposition { get; set; }
        public string PerformedBy { get; set; }
        public string Notes { get; set; }
        public int? Requested { get; set; }
        public string DispositionText { get; set; }
        public string CreateByPersonLastName { get; set; }
        public string CreateByPersonFirstName { get; set; }
        public string OfficerBadgeNumber { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? DateProcessed { get; set; }
        public string ProcessedBy { get; set; }
        public string ProcessedDisposition { get; set; }
        public int DnaPersonnelId { get; set; }
        public string PersonDnaHistoryList { get; set; }

    }
    public class TestingVm : DnaVm
    {
        public int TestingId { get; set; }
        public int? Type { get; set; }
        public string TypeText { get; set; }
        public string TestingHistoryList { get; set; }
        public int PersonnelId { get; set; }
        public int? UpdatePersonnelId { get; set; }
        public string UpdateByPersonLastName { get; set; }
        public string UpdateByPersonFirstName { get; set; }
        public string UpdateOfficerBadgeNumber { get; set; }
    }

    public class ContactVm : PersonVm
    {
        public int ContactId { get; set; }
        public int? ContactPersonId { get; set; }
        public int? VictimNotify { get; set; }
        public string VictimNotifyNote { get; set; }
        public string ActiveFlag { get; set; }
        public int TypePersonId { get; set; }
        public int IsActiveInmate { get; set; }
        public string ContactType { get; set; }
        public string ContactDescription { get; set; }
        public string RelationshipId { get; set; }
        public PersonHistoryVm PersonHistoryVm { get; set; }
    }

    public class ContactAttemptVm : ContactVm
    {
        public DateTime? AttemptDate { get; set; }
        public string AttemptTypeLookup { get; set; }
        public string AttemptDispoLookup { get; set; }
        public string AttemptNotes { get; set; }
        public int AttemptId { get; set; }
        public int DeleteFlag { get; set; }
        public int AttemptCreateBy { get; set; }
        public string CreateByLastName { get; set; }
    }

    public class ContactDetails
    {
        public List<ContactVm> LstContact { get; set; }
        public List<ContactAttemptVm> LstContactAttempt { get; set; }
    }
    public enum IncarcerationDescType
    {
        // Commented text will be a output text of the corresponding property
        None,
        Incarceration, // "Incarceration"
        TempHold // "TEMP HOLD"
    }

    public enum PersonCitizenshipStatus
    {
        Insert,
        Delete,
        Update
    }

    public enum PersonInlineEditStatus
    {
        PersonCurrentName,
        PersonFknName,
        PresentIncarceration,
        PersonDetail,
        PersonFknDetail
    }

    public class PersonProfileVm
    {
        public int PersonId { get; set; }
        public int? MaritalStatus { get; set; }
        public int? Ethnicity { get; set; }
        public long? PrimLang { get; set; }
        public string Interpreter { get; set; }
        public bool IllegalAlien { get; set; }
        public bool USCitizen { get; set; }
        public string Citizen { get; set; }
        public string Religion { get; set; }
        public string GenderIdentity { get; set; }
        public string GenderIdentityDiff { get; set; }
        public string EduGrade { get; set; }
        public int EduGed { get; set; }
        public string EduDegree { get; set; }
        public string EduDiscipline { get; set; }
        public string EduSpecial { get; set; }
        public string MedInsuranceProvider { get; set; }
        public string MedInsuranceProviderOther { get; set; }
        public string MedInsurancePolicyNo { get; set; }
        public string MedInsuranceNote { get; set; }
        public int DesireClasses { get; set; }
        public int DesireFurlough { get; set; }
        public int DesireWorkCrew { get; set; }
        public List<PersonSkillTradeVm> SkillTrade { get; set; }
        public string PersonHistoryList { get; set; }
        public int IncarcerationId { get; set; }
        public int? PersonPreferenceNameAkaId { get; set; }
        public string PersonPreferencePronoun { get; set; }
        public string PersonPreferenceSearch { get; set; }
    }

    public class PersonSkillTradeVm
    {
        public int SkillAndTradeId { get; set; }
        public string SkillTrade { get; set; }
        public bool IsSkillTrade { get; set; }
    }

    public class WorkCrewRequestVm
    {
        public int WorkCrewLookupId { get; set; }
        public int? WorkFurloughFlag { get; set; }
        public int FacilityId { get; set; }
        public string CrewClassfilter { get; set; }
        public string CrewGenderfilter { get; set; }
        public string[] LstCrewClassfilter { get; set; }
        public string[] LstCrewGenderfilter { get; set; }
        public string CrewName { get; set; }
    }

    public class WorkCrowAndFurloughRequest
    {
        public List<WorkCrewRequestVm> LstWorkCrowAndFurlough { get; set; }
        public string RequestNote { get; set; }
        public int? InmateId { get; set; }
        public string AssignedCrewName { get; set; }
        public string Gender { get; set; }
        public string Class { get; set; }
    }

    public class ProgramAndClass
    {
        public string GenderFilter { get; set; }
        public string ClassFilter { get; set; }
        public int ProgramId { get; set; }
        public int? ClassOrServiceNumber { get; set; }
        public string ClassOrServiceName { get; set; }
        public string ProgramCategory { get; set; }
        public int? ClassFlag { get; set; }
        public bool IsDuplicated { get; set; }
        public bool IsRequested { get; set; }
        public string[] LstClassfilter { get; set; }
        public string[] LstGenderfilter { get; set; }
    }

    public class ProgramDetails
    {
        public List<ProgramAndClass> LstProgramAndClass { get; set; }
        public int InmateId { get; set; }
        public int? PriorityLevel { get; set; }
        public string RequestNote { get; set; }
        public string Class { get; set; }
        public string Gender { get; set; }
    }

    public class PersonSearchVm
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Suffix { get; set; }
        public string Dob { get; set; }
        public string InmateNum { get; set; }
        public string Dln { get; set; }
        public string Cii { get; set; }
        public string Fbi { get; set; }
        public string Ssn { get; set; }
        public string AlienNum { get; set; }
        public string WarrantNum { get; set; }
        public string BookingNum { get; set; }
        public string HomePhone { get; set; }
        public string BusPhone { get; set; }
        public string Gender { get; set; }
        public string Race { get; set; }
        public string HeightPrimary { get; set; }
        public string HeightSecondary { get; set; }
        public string Weight { get; set; }
        public string HairColor { get; set; }
        public string EyeColor { get; set; }
        public string TattooScars { get; set; }
        public string AddrNum { get; set; }
        public string AddrStreet { get; set; }
        public string AddrSuffix { get; set; }
        public string Line2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }

    public class PersonMilitaryVm
    {
        public int PersonMilitaryId { get; set; }
        public int PersonId { get; set; }
        public DateTime? StatusDate { get; set; }
        public int? ActiveMilitary { get; set; }
        public int? MilitaryVeteran { get; set; }
        public int? BranchId { get; set; }
        public string Branch { get; set; }
        public string Notes { get; set; }
        public string MilitaryStatus { get; set; }
        public string MilitaryRank { get; set; }
        public string MilitaryId { get; set; }
        public int CreateById { get; set; }
        public string CreateByLastName { get; set; }
        public string CreateByFirstName { get; set; }
        public string CreateByMiddleName { get; set; }
        public string CreateByOfficerBadgeNo { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? UpdateById { get; set; }
        public string UpdateByLastName { get; set; }
        public string UpdateByFirstName { get; set; }
        public string UpdateByMiddleName { get; set; }
        public string UpdateByOfficerBadgeNo { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string PersonMilitaryHistoryList { get; set; }
        public DateTime? DeleteDate {get; set;}
        public bool DeleteFlag{get; set;}
        public int? DeleteBy {get; set;}
    }


    public class CustomField
    {
        public int CustomFieldLookupId { get; set; }
        public int UserControlId { get; set; }
        public string FieldLabel { get; set; }
        public int? FieldOrder { get; set; }
        public int? FieldTypeTextFlag { get; set; }
        public int? FieldTypeDropDownFlag { get; set; }
        public int? FieldTypeCheckBoxFlag { get; set; }
        public int? FieldTextEntryMaxLength { get; set; }
        public int? FieldTextentryAlpha { get; set; }
        public int? FieldTextentryNumericonly { get; set; }
        public int? FieldTextentryNumericallowdecimal { get; set; }
        public int? FieldSizeSmall { get; set; }
        public int? FieldSizeMedium { get; set; }
        public int? FieldSizeLarge { get; set; }
        public int? FieldRequired { get; set; }
        public string CustomfieldEntry { get; set; }
        public List<CustomDropDownValues> DropDownValues { get; set; }
    }

    public class CustomDropDownValues
    {
        public int ListEntryID { get; set; }
        public string ListEntry { get; set; }
    }

}
