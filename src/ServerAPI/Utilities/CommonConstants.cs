﻿namespace ServerAPI.Utilities
{
    public static class AuthDetailConstants
    {
        public const string PERSONNELID = "personnelId";
        public const string PERSONID = "personId";
        public const string FACILITYID = "facilityId";
        public const string HOUSINGUNITLISTID = "housingUnitListId";
        public const string USEREXPIRES = "userExpires";
        public const string NOACCESS = "noAccess";
    }

    public static class AddressTypeConstants
    {
        public const string BUS = "BUS";
        public const string RES = "RES";
    }

    public static class EventNameConstants
    {
        public const string CLASSIFICATION = "CLASSIFICATION";
        public const string LOCATIONTRACKING = "LOCATION TRACKING";
        public const string INTAKECOMPLETE = "INTAKE COMPLETE";
        public const string BOOKINGCOMPLETE = "BOOKING COMPLETE";
        public const string CASECLEAR = "CASE CLEAR";
        public const string INTAKECOMPLETED = "INTAKE COMPLETED";
        public const string BOOKINGCOMPLETED = "BOOKING COMPLETED";
        public const string ASSESSMENTCOMPLETED = "ASSESSMENT COMPLETED";
        public const string SUPERVISORCOMPLETED = "SUPERVISOR COMPLETED";
        public const string CLEARCOMPLETED = "CLEAR COMPLETED";
        public const string RELEASECOMPLETED = "RELEASE COMPLETED";
        public const string HOUSINGMOVE = "HOUSING MOVE";
        public const string INCIDENTHEARINGSCHEDULED = "INCIDENT HEARING SCHEDULED";
        public const string PREBOOKFORMSAVE = "PREBOOK FORM SAVE";
        public const string NEWINTAKE = "NEW INTAKE";
        public const string NEWCASE = "NEW CASE";
        public const string CASEDATACOMPLETE = "CASE DATA COMPLETE";
        public const string INCIDENTCREATE = "INCIDENT CREATE";
        public const string INCARCERATIONCOMPLETE = "INCARCERATION COMPLETE";
        public const string ALERTFLAGSAVE = "ALERT FLAG SAVE";
        public const string INVENTORYDISPOSITION = "INVENTORY DISPOSITION";
        public const string HEADCOUNTCREATE = "HEADCOUNT CREATE";
        public const string MEDPRESCREENREJECT = "MED PRESCREEN REJECT";
        public const string MEDPRESCREENBYPASS = "MED PRESCREEN BYPASS";
        public const string MEDPRESCREENACCEPT = "MED PRESCREEN ACCEPT";
        public const string INTAKECURRENCY = "INTAKE CURRENCY";
        public const string DORELEASE = "DO RELEASE";
        public const string ASSESSMENTCOMPLETE = "ASSESSMENT COMPLETE";
        public const string HOUSINGASSIGNFROMTRANSFER = "HOUSING ASSIGN FROM TRANSFER";
        public const string KEEPERSET = "KEEPER SET";
        public const string NOKEEPERSET = "NO-KEEPER SET";
        public const string KEEPSEPINMATE = "KEEP SEP-INMATE";
        public const string KEEPSEPREMOVEINMATE = "KEEP SEP REMOVE INMATE";
        public const string KEEPSEPASSOC = "KEEP SEP-ASSOC";
        public const string KEEPSEPREMOVEASSOC = "KEEP SEP REMOVE ASSOC";
        public const string KEEPSEPSUBSET = "KEEP SEP-SUBSET";
        public const string KEEPSEPREMOVESUBSET = "KEEP SEP REMOVE SUBSET";
        public const string NEWALERTFLAG = "NEW ALERT FLAG";
        public const string REMOVEALERTFLAG = "REMOVE ALERT FLAG";
        public const string CASEBAIL = "CASE BAIL";
        public const string CASEINFORMATIONSAVE = "CASE INFORMATION SAVE";
        public const string CASESTATUSCHANGE = "CASE STATUS CHANGE";
        public const string CASECHARGECHANGE = "CASE CHARGE CHANGE";
        public const string CASEWARRANTCHANGE = "CASE WARRANT CHANGE";
        public const string APPOINTMENT = "APPOINTMENT";
        public const string APPOINTMENTDELETE = "APPOINTMENT DELETE";
        public const string ASSOCIATIONSAVE = "ASSOCIATION SAVE";
        public const string ASSOCIATIONREMOVE = "ASSOCIATION REMOVE";
        public const string CURRENTNAMESAVE = "CURRENT NAME SAVE";
        public const string FIRSTKNOWNNAMESAVE = "FIRST KNOWN NAME SAVE";
        public const string INCARCERATIONNAMESAVE = "INCARCERATION NAME SAVE";
        public const string FORCECHARGE = "FORCE CHARGE";
        public const string FORMSAVE = "FORM SAVE";
        public const string MERGE = "MERGE";
        public const string MILITARYSAVE = "MILITARY SAVE";
        public const string OVERALLSENTENCE = "OVERALL SENTENCE";
        public const string PERSONAKASAVE = "PERSON AKA SAVE";
        public const string PERSONCHARSAVE = "PERSON CHAR SAVE";
        public const string PERSONIDENTITYSAVE = "PERSON IDENTITY SAVE";
        public const string PERSONTESTING = "PERSON TESTING";
        public const string PROGRAMREQUEST = "PROGRAM REQUEST";
        public const string SENTENCE = "SENTENCE";
        public const string CHARGESTATUS = "CHARGE STATUS";
    }

    public static class FormTemplatesName
    {
        public const string RmsDemographicData = "RMS DEMOGRAPHIC DATA";
    }

    public static class SiteOptionsConstants
    {
        public const string NCIC_RUN = "NCIC RUN";
        public const string JAILSELINCAR = "JAIL_ALLOW_SELECT_INCARCERATION_NAME";
        public const string BAILSUMCHARNO = "BAIL_SUMMARY_CHARGES_NO_STATUS";
        public const string ON = "ON";
        public const string BOOKINGCOMREQCHAR = "BOOKING_DATA_COMPLETE_REQUIRE_CHARGE";
        public const string BOOKINGCOMPREQWAR = "BOOKING_DATA_COMPLETE_REQUIRE_WARRANT";
        public const string BOOKING1WAR = "BOOKING_ALLOW_ONLY_1_WARRANT";
        public const string SENTENCEBYCHARGE = "SENTENCE_BY_CHARGE";
        public const string HIGHESTBAILPERBOOKING = "HIGHEST_BAIL_PER_BOOKING";
        public const string NOHOUSECLASS = "NO_HOUSE_CLASS";
        public const string NOHOUSEBOOK = "NO_HOUSE_BOOK";
        public const string OFF = "OFF";
        public const string DONOTALLOWOVERCAPACITYHOUSING = "DO_NOT_ALLOW_OVER_CAPACITY_HOUSING";
        public const string ALLOWWEEKENDERSENTENCE = "ALLOW_WEEKENDER_SENTENCE";
        public const string ALLOWBOOKINGREACTIVATES = "ALLOW_BOOKING_REACTIVATES";
        public const string TURNONPREBOOKNUMBERING = "TURN_ON_PREBOOK_NUMBERING";
        public const string ALLOWCLASSIFYEDIT = "ALLOW_CLASSIFY_EDIT";
        public const string ALLOWNOTEEDIT = "ALLOW_NOTE_EDIT";
        public const string REQUIREDBILLINGAGENCY = "REQUIREDBILLINGAGENCY";
        public const string REQUIREDCOURT = "REQUIREDCOURT";
        public const string JMSREQUIRESEARCHOFFICER = "JMS_REQUIRE_SEARCH_OFFICER";
        public const string JMSREQUIRERECEIVEOFFICER = "JMS_REQUIRE_RECEIVE_OFFICER";
        public const string JMSREQUIRETRANSPORTOFFICER = "JMS_REQUIRE_TRANSPORT_OFFICER";
        public const string JMSDONOTREQUIREBOOKINGOFFICER = "JMS_DONOT_REQUIRE_BOOKING_OFFICER";
        public const string FORCETRANSFERDURINGFACILITYMOVE = "FORCE_TRANSFER_DURING_FACILITY_MOVE";
        public const string CLASSREVIEWQUEUEBYCLASSLEVEL = "CLASS_REVIEW_QUEUE_BYCLASSLEVEL";
        public const string CLASSIFYREVIEWBATCH = "CLASSIFY_REVIEW_BATCH";
        public const string BAILFROMPREBOOK = "BAIL_FROM_PREBOOK";
        public const string AFISEXTERNALID = "AFISEXTERNALID";
        public const string SANJOAQUIN = "SAN JOAQUIN";
        public const string SENTENCEDONOTROUNDCREDITS = "SENTENCE_DONOTROUND_CREDITS";
        public const string NODAYFORDAYVISIBLE = "NO_DAY_FOR_DAY_VISIBLE";
        public const string ADDRESSMAILINGVISIBLE = "ADDRESS_MAILING_VISIBLE";
        public const string ADDRESSOTHERVISIBLE = "ADDRESS_OTHER_VISIBLE";
        public const string MULTIPLEPREBOOKCASE = "MULTIPLEPREBOOKCASE";
        public const string INTAKEPREBOOKREVIEW = "INTAKE_PREBOOK_REVIEW";
        public const string INVENTORYDAYSAFTERRELEASEDEFAULT = "INVENTORY_DAYS_AFTER_RELEASE_DEFAULT";
        public const string BYPASSASSESSMENTFORNONKEEPER = "BYPASS_ASSESSMENT_FOR_NON_KEEPER";
        public const string APPLYCHARGE = "APPLY CHARGE";
        public const string APPLYBOOK = "APPLY BOOK";
        public const string CHARGERECALC = "CHARGE RECALC";
        public const string BOOKRECALC = "BOOK RECALC";
        public const string CONSECUTIVESENTENCESAMEDAYUSESTART = "CONSECUTIVE_SENTENCE_SAME_DAY_USE_START";
        public const string REMOVEINMATENUMPREFIXATVERIFYID = "REMOVE_INMATE_NUM_PREFIX_AT_VERIFY_ID";
        public const string REQUIREVERIFYIDATBOOKINGCOMPLETE = "REQUIRE_VERIFY_ID_AT_BOOKING_COMPLETE";
        public const string SETNOBAILAFTERSENTENCED = "SET_NO_BAIL_AFTER_SENTENCED";
        public const string VISITORINCUSTODY = "VISITOR_IN_CUSTODY";
        public const string VISITSTARTOFWEEK = "VISIT_START_OF_WEEK";
        public const string NOCLASSIFICATIONUNTILBOOKINGISCOMPLETE = "NO CLASSIFICATION UNTIL BOOKING IS COMPLETE";
        public const string BYPASSASSESSMENTFORKEEPER = "BYPASS_ASSESSMENT_FOR_KEEPER";



    }

    public enum CommonConstants
    {
        ALL,
        Appt,
        CREATEDBY,
        REVIEWEDBY,
        Merge
    }

    public static class FloorNote
    {
        public const string ALL = "ALL";
        public const string NOTYPE = "NO TYPE";
    }

    public static class CustomConstants
    {
        public const string Personnel = "Personnel";
        public const string Yes = "YES";
        public const string PERSONNELID = "personnel_id";
    }

    public static class ClassTypeConstants
    {
        public const string RECLASSIFICATION = "RE-CLASSIFICATION";
        public const string INITIAL = "INITIAL";
        public const string REVIEW = "REVIEW";
        public const string CLASSNOTE = "CLASS NOTE";
        public const string FORM = "FORM";
        public const string ATTACH = "ATTACH";
        public const string LINK = "LINK";
        public const string INTAKE = "INTAKE";
        public const string RELEASE = "RELEASE";
        public const string HOUSING = "HOUSING";
        public const string NOTE = "NOTE";
        public const string INCIDENT = "INCIDENT";
        public const string MESSAGE = "MESSAGE";
        public const string FLAG = "FLAG";
        public const string KEEPSEPINMATE = "KEEP SEP:INMATE";
        public const string KEEPSEPASSOC = "KEEP SEP:ASSOC";
        public const string KEEPSEPSUBSET = "KEEP SEP:SUBSET";
        public const string ASSOC = "ASSOC";
        public const string PRIVILEGES = "PRIVILEGES";
        public const string PRIVILEGE = "PRIVILEGE";
        public const string KEEPSEP = "KEEP SEP";
        public const string CHARGES = "CHARGES";
    }

    public static class LookupCategory
    {
        public const string PERSONID = "person_id";
        public const string CLASREAS = "CLASREAS";
        public const string ATTENDSTAT = "ATTENDSTAT";
    }

    public static class RequestTrackCategory
    {
        public const string NEWREQUEST = "NEW REQUEST";
        public const string UNDOREQUESTACCEPTED = "UNDO REQUEST ACCEPTED";
        public const string REQUESTACCEPTED = "REQUEST ACCEPTED";
        public const string REQUESTCLEARED = "REQUEST CLEARED";
        public const string UNDOREQUESTCLEARED = "UNDOREQUESTCLEARED";
        public const string TRANSFERPERSONNEL = "TRANSFER PERSONNEL";
        public const string TRANSFERACTION = "TRANSFER ACTION";
        public const string NOTE = "NOTE";
        public const string INMATERESPONSE = "INMATE RESPONSE";
        public const string DISPOSITION = "DISPOSITION";
    }

    public static class BailType
    {
        public const string NONE = "NONE";
        public const string NOBAIL = "NO BAIL";
        public const string CASHONLY = "CASH ONLY";
        public const string BONDABLE = "BONDABLE";
    }

    public static class RequestType
    {
        public const string PENDINGREQUEST = "PENDING";
        public const string ASSIGNEDREQUEST = "ASSIGNED";
        public const string RESPONSES = "RESPONSES";
    }

    public static class KeepSepLabel
    {
        public const string ASSOC = "ASSOC";
        public const string SUBSET = "SUBSET";
        public const string INMATE = "INMATE";
        public const string KEEPSEPINMATE = "KEEP SEP INMATE";
        public const string KEEPSEPASSOC = "KEEP SEP ASSOC";
        public const string KEEPSEPSUBSET = "KEEP SEP SUBSET";
        public const string ASSOCKEEPSEP = "ASSOC KEEP SEP";
        public const string SUBSETKEEPSEP = "SUBSET KEEP SEP";

        public const string ASSOCKEEPSEPASSOC = "ASSOC KEEP SEP ASSOC";
        public const string ASSOCKEEPSEPINMATE = "ASSOC KEEP SEP INMATE";
        public const string ASSOCKEEPSEPSUBSET = "ASSOC KEEP SEP SUBSET";
        public const string INMATEKEEPSEPASSOC = "INMATE KEEP SEP ASSOC";
        public const string INMATEKEEPSEPINMATE = "INMATE KEEP SEP INMATE";
        public const string INMATEKEEPSEPSUBSET = "INMATE KEEP SEP SUBSET";
        public const string SUBSETKEEPSEPASSOC = "SUBSET KEEP SEP ASSOC";
        public const string SUBSETKEEPSEPINMATE = "SUBSET KEEP SEP INMATE";
        public const string SUBSETKEEPSEPSUBSET = "SUBSET KEEP SEP SUBSET";
        public const string KEEPSEPASSOCASSOCSAMELOCATION = "KEEP SEP ASSOC-ASSOC SAME LOCATION";
        public const string KEEPSEPASSOCINMATESAMELOCATION = "KEEP SEP ASSOC-INMATE SAME LOCATION";
        public const string KEEPSEPASSOCSUBSETSAMELOCATION = "KEEP SEP ASSOC-SUBSET SAME LOCATION";
        public const string KEEPSEPINMATEASSOCSAMELOCATION = "KEEP SEP INMATE-ASSOC SAME LOCATION";
        public const string KEEPSEPINMATEINMATESAMELOCATION = "KEEP SEP INMATE-INMATE SAME LOCATION";
        public const string KEEPSEPINMATESUBSETSAMELOCATION = "KEEP SEP INMATE-SUBSET SAME LOCATION";
        public const string KEEPSEPSUBSETASSOCSAMELOCATION = "KEEP SEP SUBSET-ASSOC SAME LOCATION";
        public const string KEEPSEPSUBSETINMATESAMELOCATION = "KEEP SEP SUBSET-INMATE SAME LOCATION";
        public const string KEEPSEPSUBSETSUBSETSAMELOCATION = "KEEP SEP SUBSET-SUBSET SAME LOCATION";
        public const string ASSOCKEEPSEPASSOCBEDGROUP = "ASSOC KEEP SEP ASSOC BED GROUP";
        public const string ASSOCKEEPSEPINMATEBEDGROUP = "ASSOC KEEP SEP INMATE BED GROUP";
        public const string ASSOCKEEPSEPSUBSETBEDGROUP = "ASSOC KEEP SEP SUBSET BED GROUP";
        public const string INMATEKEEPSEPASSOCBEDGROUP = "INMATE KEEP SEP ASSOC BED GROUP";
        public const string INMATEKEEPSEPINMATEBEDGROUP = "INMATE KEEP SEP INMATE BED GROUP";
        public const string INMATEKEEPSEPSUBSETBEDGROUP = "INMATE KEEP SEP SUBSET BED GROUP";
        public const string SUBSETKEEPSEPASSOCBEDGROUP = "SUBSET KEEP SEP ASSOC BED GROUP";
        public const string SUBSETKEEPSEPINMATEBEDGROUP = "SUBSET KEEP SEP INMATE BED GROUP";
        public const string SUBSETKEEPSEPSUBSETBEDGROUP = "SUBSET KEEP SEP SUBSET BED GROUP";
        public const string ASSOCKEEPSEPASSOCBEDPOD = "ASSOC KEEP SEP ASSOC BED POD";
        public const string ASSOCKEEPSEPINMATEBEDPOD = "ASSOC KEEP SEP INMATE BED POD";
        public const string ASSOCKEEPSEPSUBSETBEDPOD = "ASSOC KEEP SEP SUBSET BED POD";
        public const string INMATEKEEPSEPASSOCBEDPOD = "INMATE KEEP SEP ASSOC BED POD";
        public const string INMATEKEEPSEPINMATEBEDPOD = "INMATE KEEP SEP INMATE BED POD";
        public const string INMATEKEEPSEPSUBSETBEDPOD = "INMATE KEEP SEP SUBSET BED POD";
        public const string SUBSETKEEPSEPASSOCBEDPOD = "SUBSET KEEP SEP ASSOC BED POD";
        public const string SUBSETKEEPSEPINMATEBEDPOD = "SUBSET KEEP SEP INMATE BED POD";
        public const string SUBSETKEEPSEPSUBSETBEDPOD = "SUBSET KEEP SEP SUBSET BED POD";

    }

    public static class KeepSepTypeName
    {
        public const string II = "I-I";
        public const string IA = "I-A";
        public const string IS = "I-S";
        public const string AI = "A-I";
        public const string AA = "A-A";
        public const string AS = "A-S";
        public const string SI = "S-I";
        public const string SS = "S-S";
    }

    public static class ConflictTypeConstants
    {
        public const string APPOINTMENTOVERLAPCONFLICT = "APPOINTMENT OVERLAP CONFLICT";
        public const string APPOINTMENTWARNINGBACKTOBACK = "APPOINTMENT WARNING, BACK TO BACK";
        public const string DARKDAY = "DARK DAY";
        public const string VISITPROFESSIONALCONFLICT = "VISIT PROFESSIONAL CONFLICT";
        public const string VISITSOCIALCONFLICT = "VISIT SOCIAL CONFLICT";
        public const string PROGRAMCONFLICT = "PROGRAM CONFLICT";
        public const string PROGRAMOVERLAPCONFLICT = "PROGRAM OVERLAP CONFLICT";
        public const string WORKCREWCONFLICT = "WORK CREW CONFLICT";
        public const string WORKFURLOUGHCONFLICT = "WORK FURLOUGH CONFLICT";

        public const string HOUSINGCLASSIFYCONFLICT = "HOUSING CLASSIFY CONFLICT";
        public const string HOUSINGOUTOFSERVICE = "HOUSING OUT OF SERVICE";
        public const string HOUSINGOVERCAPACITY = "HOUSING OVER CAPACITY";

        public const string UNCLASSIFIEDINMATE = "UNCLASSIFIED INMATE";
        public const string BOOKINGNOTCOMPLETE = "BOOKING NOT COMPLETE";
        public const string INMATEGENDERCONFLICT = "INMATE GENDER CONFLICT";
        public const string VISITORREGISTEREDCONFLICT = "VISITOR REGISTERED CONFLICT";
        public const string LOCATIONOVERCAPACITY = "LOCATION OVER CAPACITY";
        public const string LOCATIONOUTOFSERVICE = "LOCATION OUT OF SERVICE";
        public const string ROOMPRIVILEGECONFLICT = "ROOM PRIVILEGE CONFLICT";
        public const string LOCATIONGENDERCONFLICT = "LOCATION GENDER CONFLICT";
        public const string POSSIBLEAPPOINTMENTCONFLICT = "POSSIBLE APPOINTMENT CONFLICT";
        public const string POSSIBLEPROGRAMCONFLICT = "POSSIBLE PROGRAM CONFLICT";
        public const string DIFFERENTGENDER = "OPPOSITE GENDER WARNING - SELECTED INMATES HAVE DIFFERENT GENDER";

        public const string REVOKE = "REVOKE";
        public const string INMATEHASNOTBEENCLASSIFIED = "INMATE HAS NOT BEEN CLASSIFIED";
        public const string INMATEHASNOTCOMPLETEDTHEBOOKINGWIZARD = "INMATE HAS NOT COMPLETED THE BOOKING WIZARD";
        public const string OPPOSITEGENDERLOCATIONWARNING = "OPPOSITE GENDER LOCATION WARNING";
        public const string HOUSINGDOESNOTALLOW = "HOUSING DOES NOT ALLOW";
        public const string CLASSIFYLEVEL = "CLASSIFY LEVEL";
        public const string HASBEENPLACEDOUTOFSERVICE = "HAS BEEN PLACED OUT OF SERVICE";
        public const string ISOVERCAPACITY = "IS OVER CAPACITY";
        public const string ALREADYASSIGNEDTO = "(S) ALREADY ASSIGNED TO";

        public const string DIFFERENTHOUSINGGENDER =
            "OPPOSITE GENDER WARNING - SELECTED INMATE(S) GENDER IS DIFFERENT FROM HOUSING GENDER";
        public const string HOUSINGFLAGCONFLICT = "HOUSING FLAG CONFLICT";
        public const string INMATEISFLAGGED = "INMATE IS FLAGGED";
        public const string SELECTEDHOUSINGDOESNOTALLOWTHISFLAG = "SELECTED HOUSING DOES NOT ALLOW THIS FLAG";
        public const string INMATEGENDERHOUSINGCONFLICT = "INMATE GENDER HOUSING CONFLICT";
        public const string OPPOSITEGENDERHOUSINGWARNING = "OPPOSITE GENDER HOUSING WARNING";
        public const string KEEPSEPSAMECELLGROUPWARNING = "KEEP SEP A-A SAME CELL GROUP WARNING";

        public const string TASKNOTCOMPLETE = "TASK NOT COMPLETE";
        public const string TOTALSEPHOUSINGCONFLICT = "TOTAL SEPARATION HOUSING CONFLICT";
        public const string TOTALSEPINMATECONFLICT = "TOTAL SEPARATION INMATE CONFLICT";
        public const string TOTALSEPLOCCONFLICT = "TOTAL SEPARATION LOCATION CONFLICT";
        public const string TOTALSEPINMLOCCONFLICT = "TOTAL SEPARATION INMATE LOCATION CONFLICT";
        public const string INMATEALREADYINLOCATION = "INMATES ALREADY IN LOCATION ";
        public const string HAVETOTALSEPERATIONFLAG = " HAVE TOTAL SEPARATION FLAGGED";
        public const string INMATEISFLAGGEDFORTOTALSEP = "INMATE IS FLAGGED FOR TOTAL SEPERATION, INMATES ARE ALREADY IN LOCATION ";
        public const string INMATETOTALSEP = "INMATE IS FLAGGED FOR TOTAL SEPERATION";
        public const string INMATETOTALSEPLIST = "INMATE FLAGGED FOR TOTAL SEPERATION - ARE IN SELECTED LIST";
        public const string HOUSINGSUPPLYCHECKOUT = "HOUSING SUPPLY CHECKOUT";
        public const string TOTALSEPBATCHHOUSINGCONFLICT = "TOTAL SEPARATION BATCH HOUSING CONFLICT";
        public const string TOTALSEPBACTHHOUSINGCONFLICTLIST = "TOTAL SEPARATION BATCH HOUSING CONFLICT LIST";
        public const string APPOINTMENTKEEPSEPERATECONFLICT = "APPOINTMENT KEEP SEPERATE CONFLICT";
        public const string HOUSINGLOCKDOWNCONFLICT = "HOUSING LOCKDOWN CONFLICT";
        public const string INMATELOCKDOWNCONFLICT = "INMATE LOCKDOWN CONFLICT";

        public const string GENDERCONFLICT = "Gender Conflict";
        public const string KEEPSEPARATECONFLICT = "Keep Separate Conflict";
        public const string DIFFERENTGENDERANDLOCATION = "are in the same room at the same time. This visit should be denied, select confirm to override.";
        public const string CAPACITYCONFLICT = "Capacity Conflict";
        public const string CAPACITYOVERCONFLICT =
            "The Capacity for the visit room has been reached. This visit should be denied, select confirm to override.";
        public const string OVERALAPINGEVENTCONFLICT = "Overlaping Event Conflict";
        public const string SCHEDULEEVENTCONFLICT =
            "The selected inmate has other events scheduled. This visit should be denied, select confirm to override.";
        public const string RejectVisitorForInmate = "Reject Visitor for Inmate";
        public const string VisitsPerWeek = "Visits per week conflict";
        public const string RevokedVisitation = "Revoke visitation conflict";
        public const string VicitimContact = "Vicitim contact conflict";
        public const string VisitorInCustody = "Visitor in custody";
        public const string RevokeVisitNoteAcknowledgement = "Revoke visit note acknowledgement";
        public const string RevokeVisitNoteAcknowledgementDescription = "Please acknowledge note: ";
        public const string AssociationKeepSeparate = "Association keep separate conflict";
        public const string VisitorInCustodyDescription = "The selected visitor is prior a inmate and was recently in custody.The visit should be denied,select confirm to override.";
        public const string AssociationKeepSeparateDescription = "The selected inmate and visitor have a keep separate conflict.The visit should be denied,select confirm to override.";
        public const string VicitimContactDescription = "The selected visitor is the flagged as victim contact.The visit should be denied,select confirm to override.";
        public const string VisitsPerWeekDescription = "The selected inmate has reached the maximum visits per week.The visit should be denied,select confirm to override.";
        public const string RevokedVisitationDescription = "The selected inmate has visitation revoked.The visit should be denied,select confirm to override.";
        public const string RejectVisitorForInmateDescription = "The selected visitor should be rejected for this inmate.The visit should be denied,select confirm to override.";
        public const string HOUSINGLOCKDOWNDESCRIPTION = "is currently under housing lockdown. This visit should be denied, select confirm to override.";
        public const string INMATEFLAGRULECELLCONFLICT = "INMATE FLAG RULE CELL CONFLICT";
        public const string INMATEFLAGRULEPODCONFLICT = "INMATE FLAG RULE POD CONFLICT";
        public const string INMATEFLAGRULECELLGROUPCONFLICT = "INMATE FLAG RULE CELL GROUP CONFLICT";
        public const string INMATECLASSIFICATIONRULECELLCONFLICT = "INMATE CLASSIFICATION RULE CELL CONFLICT";
        public const string INMATECLASSIFICATIONRULEPODCONFLICT = "INMATE CLASSIFICATION RULE POD CONFLICT";
        public const string INMATECLASSIFICATIONRULECELLGROUPCONFLICT = "INMATE CLASSIFICATION RULE CELL GROUP CONFLICT";
    }

    public static class InventoryQueueConstants
    {
        public const string INTAKE = "INTAKE";
        public const string BOOKING = "BOOKING";
        public const string NOHOUSING = "NO HOUSING";
        public const string RELEASE = "RELEASE";
        public const string BOOKANDRELEASE = "BOOK AND RELEASE";
        public const string ASSESSMENT = "ASSESSMENT";
        public const string RECINTAKE = "REC INTAKE";
        public const string RECTRANSFER = "REC TRANSFER";
        public const string BIN003 = "BIN003";
        public const string PRO02 = "PRO02";
        public const string PRO03 = "PRO03";
        public const string PROPTRANSFER = "PROP TRANSFER";
        public const string PROPERTYRELEASESHEET = "PROPERTY RELEASE SHEET";
        public const string INVENTORYINSTORAGE = "INVENTORY IN STORAGE";
        public const string INVENTORYINSTORAGENAME = "Inventory Receipt";
        public const string PREBOOKPROPERTYNAME = "Prebook Property";
        public const string PREBOOKPROPERTY = "PREBOOK PROPERTY";
        public const string STANDARDRELEASE = "STANDARD RELEASE";
        public const string TRANSPORTRELEASE = "TRANSPORT RELEASE";
        public const string PROPERTYGROUP = "Property Group";
        public const string SENTENCE = "Sentence";
        public const string INTAKECURRENCY = "Intake Currency";
    }

    public static class HousingConstants
    {
        public const string NOHOUSING = "NO HOUSING";
        public const string NUMBER = "NUMBER";
        public const string BUILDING = "BUILDING";
        public const string DAYS = "DAYS";
        public const string NOCLASSIFY = "<NO CLASSIFY>";
        public const string PERSON = "PERSON";
        public const string INMATE = "INMATE";
        public const string PARENT = "PARENT";
        public const string NOTSET = "NOT SET";
    }

    public static class AltSentDetailConstants
    {
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Deleted = "Deleted";
        public const string None = "None";
    }

    public static class PathConstants
    {
        public const string SITEVARIABLES = "SiteVariables";
        public const string PHOTOPATH = "PhotoPath";
        public const string TEMPFILES = "TempFiles\\";
        public const string APPLETS = "\\APPLETS";
        public const string ATTACHMENTPATHS = "AttachmentPaths";
        public const string DATEPATH = "yyyyMM";
        public const string BACKWARDSLASH = "\\";
        public const string IDENTIFIERS = "IDENTIFIERS\\";
        public const string TMPIMAGE = "TMP_IMAGE\\";
        public const string BASE64 = "data:image/png;base64,";
        public const string JPGPATH = ".jpg";
        public const string PROPERTY = "PROPERTY";
        public const string TEMPPROPERTY = "TEMP_PROPERTY";
    }

    public static class AttachmentType
    {
        public const string MEDATTACHTYPE = "MEDATTACHTYPE";
        public const string BOOKATTACHTYPE = "BOOKATTACHTYPE";
        public const string CLASATTACHTYPE = "CLASATTACHTYPE";
        public const string INCATTACHTYPE = "INCATTACHTYPE";
        public const string INCARATTACHTYPE = "INCARATTACHTYPE";
        public const string REGATTACHTYPE = "REGATTACHTYPE";
        public const string HOUSATTACHTYPE = "HOUSATTACHTYPE";
        public const string PBPCATTACHTYPE = "PBPCATTACHTYPE";
        public const string ALTSENTREQATTYPE = "ALTSENTREQATTYPE";
        public const string ALTSENTATTACHTYPE = "ALTSENTATTACHTYPE";
        public const string GRIEVATTACHTYPE = "GRIEVATTACHTYPE";
        public const string PROGCASEATTACHTYPE = "PROGCASEATTACHTYPE";
        public const string MAILATTACHTYPE = "MAILATTACHTYPE";
    }

    public static class FloorNotesConflictConstants
    {
        public const string TYPE = "[Type] ";
        public const string DESCRIPTION = "[Desc] ";
        public const string CONFLICTCHECK = "CONFLICT CHECK";
        public const string VISITREGCONFLICTCHECK = "VISIT REG CONFLICT CHECK";
        public const string CONFLICTNOTE = "[CONFLICT NOTE]";
        public const string CONFLICTS = "[CONFLICTS]";
        public const string NOTE = "[Note] ";
    }

    public static class NumericConstants
    {
        public const int ZERO = 0;
        public const int ONE = 1;
        public const int TWO = 2;
        public const int THREE = 3;
        public const int FOUR = 4;
        public const int SEVEN = 7;
        public const int THREETHOUSANDFIFTEEN = 3015;
        public const int THIRTY = 30;
        public const int SIX = 6;
        public const int FOURTEEN = 14;
    }

    public static class DateConstants
    {
        public const string TIME = "HH:mm:ss";
        public const string HOURSMINUTES = "HH:mm";
        public const string STARTHRSMINUTES = "00:00";
    }

    public static class PersonConstants
    {
        public const string FROMPAGE = "FROMPAGE";
        public const string BUSINESS = "BUS";
        public const string RESIDENCE = "RES";
        public const string MAIL = "MAIL";
        public const string OTHER = "OTHER";
        public const string PERSONADDRESSSAVE = "PERSON ADDRESS SAVE";
        public const string AKA = "AKA";
        public const string INMATENUMBER = "INMATE NUMBER";
        public const string UNITEDSTATES = "UNITED STATES";
    }

    public static class VisitorAlerts
    {
        public const string REVOKEDVISIT = "REVOKED VISIT";
        public const string VISITWEEK = "VISIT/WEEK";
        public const string KEEPSEPREGISTER = "KEEP SEP REGISTER";
        public const string GENDERREGISTER = "GENDER REGISTER";
        public const string EXCLUDECLASSIFY = "EXCLUDE CLASSIFY";
        public const string BYPASSSCHEDULE = "BYPASS SCHEDULE";
        public const string CAPACITYALERT = "CAPACITY ALERT";
        public const string SCHEDULECONFLICT = "SCHEDULE CONFLICT";
        public const string REJECTALL = "REJECT ALL";
        public const string REJECTFORINMATE = "REJECT FOR INMATE";
        public const string PERSONOFINTEREST = "PERSON OF INTEREST";
        public const string GANGKEEPSEP = "GANG KEEP SEP";
        public const string VICTIMCONTACT = "VICTIM CONTACT";
        public const string ADDITIONALVISITOR = "ADDITIONAL VISITOR";
        public const string CUSTODY = "CUSTODY";
    }

    public static class OfficerFlag
    {
        public const string ARRESTTRANSPORT = "ARREST TRANSPORT";
        public const string RECEIVESEARCH = "RECEIVE SEARCH";
        public const string PROGRAMINSTRUCTOR = "PROGRAM INSTRUCTOR";
        public const string PROGRAMCASE = "PROGRAM CASE";
    }

    public static class RosterConstants
    {
        public const string NOHOUSING = "<NO HOUSING>";
        public const string NOHOUSING1 = "< NO HOUSING >";
        public const string POSITIVE = "POSITIVE > 0.00";
        public const string ZERO = "0.00 ZERO";
        public const string UnSent = "UnSent";
        public const string Sent = "Sent";
        public const string NoHousing = "<No Housing>";
    }

    public static class ViewerType
    {
        public const string CLOCKIN = "CLOCK IN";
        public const string CLOCKOUT = "CLOCK OUT";
        public const string SETHOUSING = "SET HOUSING";
        public const string SETLOCATION = "SET LOCATION";
        public const string SETSTATUS = "SET STATUS";
        public const string OFFICERLOG = "OFFICER LOG";
        public const string CELL = "CELL";
        public const string HEADCOUNT = "HEAD COUNT";
        public const string SAFETYCHECK = "SAFETY CHECK";
        public const string LOCATIONNOTE = "LOCATION NOTE";
        public const string GENERALNOTE = "GENERAL NOTE";
        public const string INMATENOTE = "INMATE NOTE";
        public const string HOUSINGIN = "HOUSING IN";
        public const string HOUSINGOUT = "HOUSING OUT";
        public const string TRACKINGIN = "TRACKING IN";
        public const string TRACKINGOUT = "TRACKING OUT";
    }

    public static class ClassifyQueueConstants
    {
        public const string SPECIALQUEUE = "SPECIALQUEUE";
        public const string READYFORINITIAL = "READY FOR INITIAL";
        public const string READYFORHOUSING = "READY FOR HOUSING";
        public const string CLASSIFY = "CLASSIFY";
        public const string WORKCREWREQUEST = "WORKCREW REQUEST";
        public const string WORKFURLOUGHREQUEST = "WORK FURLOUGH REQUEST";
        public const string REVIEWQUEUE = "REVIEWQUEUE";
        public const string WORKCREW = "WORKCREW";
        public const string WORKFURLOUGH = "WORKFURLOUGH";


    }

    public static class KeepSepErrorMessage
    {
        public const string ALREADYEXISTS = "Keep separate record already exists";

        public const string RELEASEDANDINACTIVEKEEPSEPEXISTS =
            "There is an inactive and released keep separate record already exists";

        public const string INACTIVEKEEPSEPEXISTS = "There is an inactive keep separate record already exists";
    }

    public static class SubModuleDetails
    {
        public const string WARRANT = "Warrant";
        public const string BOOKINGWARRANTDETAIL = "BOOKING WARRANT DETAIL";
    }

    public static class HeadCountConstants
    {
        public const string TimeFormat = "HH:mm";
    }

    public static class ConsoleReleaseConstants
    {
        public const string SCHEDULEDTODAY = "Scheduled Today";
        public const string CLEAREDFORRELEASE = "Cleared For Release";
        public const string AWAITINGTRANSPORT = "Awaiting Transport";
    }

    public static class ConsoleFormConstants
    {
        public const string NOFILTER = "NO FILTER";
    }

    public static class AlertsConstants
    {
        public const string OBSERVATIONLOG = "OBSERVATION LOG";
        public const string FACILITYTRANSFER = "FACILITY TRANSFER";
    }
    public static class ActionType
    {
        public const string CHECKOUT = "CHECKOUT";
        public const string CHECKIN = "CHECKIN";
        public const string MOVE = "MOVE";
        public const string CHECKLIST = "CHECKLIST";
    }

    public static class MoveRecordsConstants
    {
        public const string Appointment = "Appointment";
        public const string HousingMove = "Housing Move";
        public const string InmateTracking = "Inmate Tracking";
        public const string MedicalChartNumber = "Medical Chart Number";
        public const string MedChartLastUpdateHistory = "Med Chart Last Update History";
        public const string MedicalChartLocation = "Medical Chart Location";
        public const string InmateForms = "Inmate Forms";
        public const string Classification = "Classification";
    }

    public enum IntakePrebookConstants
    {
        allInProgress,
        prebookReady,
        medicallyRejected,
        inProgress,
        byPassed,
        intake,
        tempHold,
        identification,
        notReviewed,
        deniedReviewAgain,
        courtCommitTdy,
        courtCommitSchd,
        courtCommitOverdue
    }

    public enum Wizards
    {
        intake = 1,
        booking,
        release,
        bookAndRelease,
        temporaryHold,
        registrants,
        bookingSupervisor,
        releaseSupervisor,
        preBook,
        courtCommit,
        incident,
        grievance,
        visitorRegistration,
        bookingData,
        assessmentKeeper,
        assessmentNonKeeper,
        bookingDataSupervisorReview,
        bookingDataSupervisorClear,
        rmsIntake
    }

    public static class SealHistoryConstants
    {
        public const string SEAL = "SEAL";
        public const string SEALPERSON = "SEAL PERSON";
        public const string RECORD = "<RECORD>";
        public const string SEALED = "<SEALED>";
    }

    public static class PermissionTypes
    {
        public const string Access = "Access";
        public const string Print = "Print";
        public const string Add = "Add";
        public const string Edit = "Edit";
        public const string Delete = "Delete";
        public const string ReleaseEdit = "ReleaseEdit";
        public const string ReleaseDelete = "ReleaseDelete";
        public const string FunctionPermission = "FunctionPermission";
        public const string SystemReport = "SystemReport";
        public const string CustomQueue = "CustomQueue";

        public const string Application = "Application";
        public const string Module = "Module";
        public const string SubModule = "SubModule";
        public const string Detail = "Detail";
        public const string Permission = "Permission";
        public const string AllAccess = "AllAccess";
        public const string Admin = "Group_Admin";

        public const string Prebook = "PREBOOK";
        public const string PBPC = "PBPC";

        public const string PersonFlag = "PersonFlag";
        public const string InmateFlag = "InmateFlag";
        public const string MedFlag = "MedFlag";
    }

    public static class LockdownRegion
    {
        public const string Facility = "Facility";
        public const string Building = "Building";
        public const string HousingGroup = "HousingGroup";
        public const string Pod = "Pod";
        public const string Cell = "Cell";
        public const string CellGroup = "CellGroup";
    }
    public static class UserConstants
    {
        public const string PERSONNELID = "personnel_id";
        public const string GIVENNAME = "given_name";
        public const string FAMILYNAME = "family_name";
        public const string MIDDLENAME = "middle_name";
        public const string FACILITYID = "facility_id";
        public const string BADGENUMBER = "badge_number";
        public const string USEREXPIRES = "user_expires";
        public const string USERNAMEDOMAIN = "user_name_domain";
        public const string TESTUSER = "test_user";
        public const string DELETEFLAG = "delete_flag";
        public const string CREATEDATE = "create_date";
        public const string NOACCESS = "noAccess";
        public const string ROLEDESCRIPTION = "role_description";
        public const string ADMIN = "Group_Admin";
    }
    public static class StatusConstants
    {
        public const string ALL = "All";
        public const string ACTIVE = "Active";
        public const string INACTIVE = "Inactive";
    }

    public static class PersonnelConstants
    {
        public const string JUDGE = "Judge";
        public const string COMMISSIONED = "Commissioned";
        public const string AGENCYGROUP = "Agency Group";
        public const string ARRESTINGTRANSPORTINGOFFICER = "Arresting/Transporting Officer";
        public const string RECEIVINGSEARCHINGOFFICER = "Receiving/Searching Officer";
        public const string PROGRAMINSTRUCTOR = "Program Instructor";
        public const string PROGRAMCASEMANAGER = "Program Case Manager";
        public const string USERS = "USERS";
        public const string EXPIRES = "EXPIRES";
        public const string EXPIRED = "EXPIRED";
    }
    public static class MoneyConstants
    {
        public const string FEEWAIVED = " FEE WAIVED";
        public const string AUTOPAYFEE = "AUTO PAY - FEE";
        public const string PARTIALPAID = "PARTIALLY - PAID";
        public const string RETURNCASH = "RETURN CASH";
        public const string FINALRETURNCASH = "FINAL RETURN CASH";
        public const string JOURNAL = "JOURNAL";
        public const string CASH = "CASH";
        public const string FundCashDrawerForDepository = "FUND CASH DRAWER FOR DEPOSITORY";
        public const string TransferCashDrawerToBank = "TRANSFER CASH DRAWER TO BANK";
        public const string TransferBankToCashDrawer = "TRANSFER BANK TO CASH DRAWER";
        public const string VoidFundCashDrawerForDepository = "VOID - FUND CASH DRAWER FOR DEPOSITORY";
        public const string DEBIT = "DEBIT";
        public const string CREDIT = "CREDIT";
        public const string Waived = "WAIVED";
        public const string FUND = "FUND";
        public const string REFUND = "REFUND";
        public const string RUNFEECHECK = "RUN FEE CHECK";
        public const string UNPAIDFEE = "UNPAIDFEE";
        public const string FUNDLEDGER = "FUNDLEDGER";
        public const string RETURNCHECK = "RETURN CHECK";
    }
    //public enum FlagType
    //{
    //    PENDING;
    //     UNPAID;
    //     ACCOUNTTRANSACTION;
    //}

    public static class SentenceSettingConstants
    {
        public const string INCARCERATION = "Incarceration";
        public const string OVERALLERC = "overallsentERC";
        public const string OVERALLERCCLEAR = "overallsentERCClear";

    }

    public static class StatusBoardOverviewConstants
    {
        public const string READYFORMEDICALPRESCREEN = "Ready for medical prescreen";
        public const string COURTCOMMITSCHEDULED = "Court commit scheduled";
        public const string MEDICALLYREJECTEDLAST48HOURS = "Medically rejected last 48 hours";
        public const string MEDICALPRESCREENINPROGRESS = "Medical prescreen in progress";
        public const string BYPASSEDMEDICALPRESCREEN = "Bypassed medical prescreen";
        public const string READYFORINTAKE = "Ready for intake";
        public const string READYFORTEMPHOLD = "Ready for temp hold";
        public const string BOOKING = "Booking";
        public const string RELEASE = "Release";
        public const string SUPERVISORREVIEWOVERALL = "Supervisor review overall";
        public const string SUPERVISORREVIEWBOOKING = "Supervisor review booking";
        public const string REVIEWRELEASE = "Review release";
        public const string REVIEWCLEAR = "Review clear";
        public const string BOOKEDTODAY = "Booked today";
        public const string RELEASEDTODAY = "Released today";
        public const string SENTENCED = "Sentenced";
        public const string UNSENTENCED = "Unsentenced";
        public const string BOOKANDRELEASE = "Book and release";
        public const string COURTCOMMITOVERDUE = "Court commit overdue";
        public const string INTAKE = "Intake";
    }

    public static class MonitorTimeLineConstants
    {
        public const string PREBOOK = "Prebook";
        public const string INTAKE = "Intake";
        public const string BOOKING = "Booking";
        public const string INCARCERATION = "Incarceration";
        public const string RELEASE = "Release";
        public const string CLASSIFICATION = "Classification";
        public const string HOUSING = "Housing";
        public const string GRIEVANCE = "Grievance";
        public const string INCIDENT = "Incident";
        public const string NOTES = "Notes";
        public const string APPOINTMENT = "Appointment";
        public const string CELLLOG = "Cell Log";
        public const string VISITATION = "Visitation";
        public const string TRACKINGFACILITY = "Tracking (Facility)";
        public const string TRACKINGNOFACILITY = "Tracking (No Facility)";
        public const string MEDDISTRIBUTE = " Med Distribute";
    }

}