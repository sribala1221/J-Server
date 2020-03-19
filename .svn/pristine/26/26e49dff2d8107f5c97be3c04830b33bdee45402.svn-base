using GenerateTables.Models;
using System;
using ServerAPI.Utilities;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void LookUpDetails()
        {
            Db.Lookup.AddRange(
                new Lookup
                {
                    LookupDescription = "MISC",
                    LookupBinary = 2,
                    LookupType = LookupConstants.TRANSTYPE,
                    LookupInactive = 0,
                    LookupIndex = 6,
                    LookupNoAlert = 9

                },
                new Lookup
                {
                    LookupDescription = "MEDICAL",
                    LookupBinary = 1,
                    LookupType = LookupConstants.DISCINTYPE,
                    LookupInactive = 0,
                    LookupIndex = 5,
                    LookupNoAlert = 10
                },
                new Lookup
                {
                    LookupDescription = "VISIT REG CONFLICT CHECK",
                    LookupBinary = null,
                    LookupType = LookupConstants.NOTETYPEINMATE,
                    LookupInactive = 0,
                    LookupIndex = 5,
                    LookupNoAlert = 10
                },
                new Lookup
                {
                    LookupDescription = "INTELLIGENCE",
                    LookupBinary = null,
                    LookupType = LookupConstants.INVESTIGATIONNOTETYPE,
                    LookupInactive = 0,
                    LookupIdentity = 1290,
                    LookupIndex = 10,
                    LookupNoAlert = 10,
                    LookupName = "INTELLIGENCE"
                },
                new Lookup
                {
                    LookupDescription = "INTERVIEW",
                    LookupBinary = null,
                    LookupType = LookupConstants.INVESTIGATIONNOTETYPE,
                    LookupInactive = 0,
                    LookupIdentity = 5191,
                    LookupIndex = 6,
                    LookupNoAlert = 10,
                    LookupName = "INTERVIEW"
                },
                new Lookup
                {
                    LookupDescription = "ENTRY ERROR",
                    LookupBinary = null,
                    LookupType = LookupConstants.INTAKECURMODREAS,
                    LookupInactive = 0,
                    LookupIdentity = 1821,
                    LookupIndex = 1,
                    LookupNoAlert = 10,
                    LookupName = "ERR",
                    LookupInactiveBy = 11
                },
                new Lookup
                {
                    LookupDescription = "ADDITIONAL AMOUNT",
                    LookupBinary = null,
                    LookupType = LookupConstants.INTAKECURMODREAS,
                    LookupInactive = 0,
                    LookupIdentity = 1822,
                    LookupIndex = 2,
                    LookupNoAlert = null,
                    LookupName = "ADD"
                },
                new Lookup
                {
                    LookupDescription = "EVIDENCE",
                    LookupBinary = null,
                    LookupType = LookupConstants.INTAKECURMODREAS,
                    LookupInactive = 0,
                    LookupIdentity = 1823,
                    LookupIndex = 3,
                    LookupNoAlert = null,
                    LookupName = "EVI"
                },
                new Lookup
                {
                    LookupDescription = "OTHER",
                    LookupBinary = null,
                    LookupType = LookupConstants.INTAKECURMODREAS,
                    LookupInactive = 0,
                    LookupIdentity = 1824,
                    LookupIndex = 4,
                    LookupNoAlert = null,
                    LookupName = "OTH"
                },
                new Lookup
                {
                    LookupDescription = "MISC",
                    LookupBinary = null,
                    LookupType = LookupConstants.BOOKNOTETYPE,
                    LookupInactive = 0,
                    LookupIndex = 1,
                    LookupNoAlert = 10
                },
                new Lookup
                {
                    LookupDescription = "EIGHTA-ARMENIAN POWER/PRIDE",
                    LookupBinary = null,
                    LookupType = LookupConstants.CLASSGROUPSUB,
                    LookupInactive = 0,
                    LookupIndex = 1,
                    LookupNoAlert = 10,
                    LookupName = "EIGHTA"
                },
                new Lookup
                {
                    LookupDescription = "EIGHTB-RUSSIAN GANGS/ORGANIZED CRIME",
                    LookupBinary = null,
                    LookupType = LookupConstants.CLASSGROUPSUB,
                    LookupInactive = 0,
                    LookupIndex = 2,
                    LookupNoAlert = 10,
                    LookupName = "EIGHTB"
                },
                new Lookup
                {
                    LookupDescription = "EIGHTC-GREENBACK MAFIA",
                    LookupBinary = null,
                    LookupType = LookupConstants.CLASSGROUPSUB,
                    LookupInactive = 0,
                    LookupIndex = 3,
                    LookupNoAlert = 10,
                    LookupName = "EIGHTC"
                },
                new Lookup
                {
                    LookupBinary = 5,
                    LookupIndex = 4,
                    CreateDate = DateTime.Now.AddDays(-6),
                    LookupInactive = 0,
                    LookupDescription = "MAINTENANCE",
                    LookupType = LookupConstants.NOTETYPECELL
                },
                new Lookup
                {
                    LookupBinary = 5,
                    LookupIndex = 5,
                    CreateDate = DateTime.Now.AddDays(-6),
                    LookupInactive = 0,
                    LookupDescription = "HOLD",
                    LookupType = LookupConstants.ARRESTTYPE,
                    LookupField = "arrest_type"
                },
                new Lookup
                {
                    LookupBinary = 5,
                    LookupIndex = 1,
                    CreateDate = DateTime.Now.AddDays(-6),
                    LookupInactive = 0,
                    LookupDescription = "OTHER",
                    LookupType = LookupConstants.SENTFLAG
                },
                new Lookup
                {
                    LookupBinary = 10,
                    LookupIndex = 2,
                    CreateDate = DateTime.Now.AddDays(-5),
                    LookupInactive = 0,
                    LookupDescription = "AMENDED SENT",
                    LookupType = LookupConstants.SENTFLAG
                },
                new Lookup
                {
                    LookupDescription = "STEAMFITTER",
                    LookupBinary = null,
                    LookupType = LookupConstants.SKILLTRADES,
                    LookupInactive = 0,
                    LookupIndex = 15,
                    LookupNoAlert = 10
                },
                new Lookup
                {
                    LookupDescription = "INVENTORY SHEET",
                    LookupBinary = null,
                    LookupType = LookupConstants.INCARATTACHTYPE,
                    LookupInactive = 0,
                    LookupIndex = 1,
                    LookupNoAlert = 10
                },
                new Lookup
                {
                    LookupDescription = "TEST IN MONEY",
                    LookupBinary = null,
                    LookupType = LookupConstants.INCARATTACHTYPE,
                    LookupInactive = 0,
                    LookupIndex = 2,
                    LookupNoAlert = 10
                },
                new Lookup
                {
                    LookupDescription = "TEST2SAD",
                    LookupBinary = null,
                    LookupType = LookupConstants.INCARATTACHTYPE,
                    LookupInactive = 0,
                    LookupIndex = 3,
                    LookupNoAlert = 10
                },
                new Lookup
                {
                    LookupDescription = "DETENTION ONLY",
                    LookupBinary = null,
                    LookupType = LookupConstants.TEMPHOLDTYPE,
                    LookupInactive = 0,
                    LookupIndex = 1,
                    LookupNoAlert = null,
                    LookupName = "DO"
                },
                new Lookup
                {
                    LookupDescription = "TRANSFER",
                    LookupBinary = null,
                    LookupType = LookupConstants.TEMPHOLDTYPE,
                    LookupInactive = 0,
                    LookupIndex = 3,
                    LookupNoAlert = null,
                    LookupName = "TR"
                },
                new Lookup
                {
                    LookupDescription = "3 HOUR HOLD",
                    LookupBinary = null,
                    LookupType = LookupConstants.TEMPHOLDTYPE,
                    LookupInactive = 0,
                    LookupIndex = 2,
                    LookupNoAlert = null,
                    LookupName = "3H"
                },
                new Lookup
                {
                    LookupDescription = "BAIL POSTED",
                    LookupBinary = null,
                    LookupType = LookupConstants.TEMPHOLDDISPO,
                    LookupInactive = 0,
                    LookupIndex = 1,
                    LookupNoAlert = null,
                    LookupName = "BA"
                },
                new Lookup
                {
                    LookupDescription = "FOUNDED",
                    LookupBinary = null,
                    LookupType = "INVESTIGATIONCOMPLETEDISPO",
                    LookupInactive = 0,
                    LookupIndex = 1,
                    LookupNoAlert = null,
                    LookupName = "FOUNDED"
                },
                new Lookup
                {
                    LookupDescription = "UNFOUNDED",
                    LookupBinary = null,
                    LookupType = "INVESTIGATIONCOMPLETEDISPO",
                    LookupInactive = 0,
                    LookupIndex = 2,
                    LookupNoAlert = null,
                    LookupName = "UNFOUNDED"
                },
                new Lookup
                {
                    LookupDescription = "INCONCLUSIVE",
                    LookupBinary = null,
                    LookupType = "INVESTIGATIONCOMPLETEDISPO",
                    LookupInactive = 0,
                    LookupIndex = 3,
                    LookupInactiveBy = null,
                    LookupName = "INCONCLUSIVE"
                },
                new Lookup
                {
                    LookupDescription = "ENTERED IN ERROR",
                    LookupBinary = null,
                    LookupType = "INVESTIGATIONCOMPLETEDISPO",
                    LookupInactive = 0,
                    LookupIndex = 4,
                    LookupNoAlert = null,
                    LookupName = "ENTERED IN ERROR"
                },

                new Lookup
                {
                    LookupDescription = "COURT MISC",
                    LookupBinary = null,
                    LookupType = LookupConstants.BOOKATTACHTYPE,
                    LookupInactive = 0,
                    LookupIndex = 10,
                    LookupNoAlert = 10,
                    LookupName = "C"
                },
                new Lookup
                {
                    LookupDescription = "NAVY",
                    LookupBinary = null,
                    LookupType = LookupConstants.MILBRANCH,
                    LookupInactive = 0,
                    LookupIndex = 3,
                    LookupNoAlert = 10,
                    LookupName = "NAVY",
                    CreateDate = DateTime.Now
                },
                new Lookup
                {
                    LookupDescription = "AIR FORCE",
                    LookupBinary = null,
                    LookupType = LookupConstants.MILBRANCH,
                    LookupInactive = 0,
                    LookupIndex = 2,
                    LookupNoAlert = 15,
                    LookupName = "AIR FORCE",
                    LookupIdentity = 1287,
                    CreateDate = DateTime.Now
                },
                new Lookup
                {
                    LookupDescription = "REG.ARSON",
                    LookupBinary = null,
                    LookupType = LookupConstants.CLASSGROUP,
                    LookupInactive = 0,
                    LookupIndex = 5,
                    LookupNoAlert = 15,
                    LookupName = "ARS",
                    CreateDate = DateTime.Now,
                    LookupFlag7 = 1
                },
                new Lookup
                {
                    LookupDescription = "REG.SEX",
                    LookupBinary = null,
                    LookupType = LookupConstants.CLASSGROUP,
                    LookupInactive = 0,
                    LookupIndex = 6,
                    LookupNoAlert = 15,
                    LookupName = "SEX",
                    CreateDate = DateTime.Now,
                    LookupFlag8 = 1
                },
                new Lookup
                {
                    LookupDescription = "BLOOD",
                    LookupBinary = null,
                    LookupType = LookupConstants.CLASSGROUP,
                    LookupInactive = 0,
                    LookupIndex = 7,
                    LookupNoAlert = 15,
                    LookupName = "ARS",
                    CreateDate = DateTime.Now,
                    LookupFlag6 = 1
                },
                new Lookup
                {
                    LookupDescription = "NORTENO",
                    LookupBinary = null,
                    LookupType = LookupConstants.CLASSGROUP,
                    LookupInactive = 0,
                    LookupIndex = 8,
                    LookupNoAlert = 15,
                    LookupName = "GWN",
                    CreateDate = DateTime.Now,
                    LookupFlag9 = 1
                },
                new Lookup
                {
                    LookupDescription = "NEW ASSOCIATION",
                    LookupBinary = null,
                    LookupType = LookupConstants.CLASSGROUP,
                    LookupInactive = 0,
                    LookupIndex = 10,
                    LookupNoAlert = 15,
                    LookupName = "NEW ASSOCIATION",
                    CreateDate = DateTime.Now
                },
                new Lookup
                {
                    LookupDescription = "ATTORNEY",
                    LookupBinary = null,
                    LookupType = LookupConstants.VISTYPE,
                    LookupInactive = 0,
                    LookupIndex = 2,
                    LookupNoAlert = 15,
                    LookupName = "AT",
                    LookupIdentity = 1004,
                    CreateDate = DateTime.Now
                },
                new Lookup
                {
                    LookupDescription = "INTERVIEW",
                    LookupBinary = null,
                    LookupType = LookupConstants.PREANOTETYPE,
                    LookupInactive = 0,
                    LookupIndex = 2,
                    LookupNoAlert = 15,
                    LookupName = "INTERVIEW",
                    CreateDate = DateTime.Now

                },
                new Lookup
                {
                    LookupDescription = "PROGRESS",
                    LookupBinary = null,
                    LookupType = LookupConstants.PREANOTETYPE,
                    LookupInactive = 0,
                    LookupIndex = 3,
                    LookupNoAlert = 15,
                    LookupName = "PROGRESS",
                    CreateDate = DateTime.Now
                },
                new Lookup
                {
                    LookupDescription = "CONTACT",
                    LookupBinary = null,
                    LookupType = LookupConstants.PREANOTETYPE,
                    LookupInactive = 0,
                    LookupIndex = 4,
                    LookupNoAlert = 15,
                    LookupName = "CONTACT",
                    CreateDate = DateTime.Now
                },
                new Lookup
                {
                    LookupDescription = "SPECIAL NEEDS",
                    LookupBinary = null,
                    LookupType = LookupConstants.PREANOTETYPE,
                    LookupInactive = 0,
                    LookupIndex = 5,
                    LookupNoAlert = 15,
                    LookupName = "SPECIAL NEEDS",
                    CreateDate = DateTime.Now
                },
                new Lookup
                {
                    LookupDescription = "COMPOSITE",
                    LookupBinary = 1,
                    LookupType = LookupConstants.IDTYPE,
                    LookupInactive = 0,
                    LookupIndex = 10,
                    LookupNoAlert = null
                },
                new Lookup
                {
                    LookupDescription = "MEDICAL",
                    LookupBinary = 3,
                    LookupType = LookupConstants.OBSTYPE,
                    LookupInactive = 0,
                    LookupIndex = 9,
                    LookupNoAlert = 5,
                    LookupColor = "#ADFF2F"
                },

                new Lookup
                {
                    LookupDescription = "DISCIPLINARY PROBLEM",
                    LookupBinary = 2,
                    LookupType = LookupConstants.PERSONCAUTION,
                    LookupInactive = 0,
                    LookupIndex = 1,
                    LookupNoAlert = null,
                    LookupColor = "6",
                    LookupFlag6 = 1

                },
                new Lookup
                {
                    LookupDescription = "GANG DROPOUT",
                    LookupBinary = 2,
                    LookupColor = "2",
                    LookupType = LookupConstants.PERSONCAUTION,
                    LookupInactive = 0,
                    LookupIndex = 2,
                    LookupFlag7 = 1,
                    LookupNoAlert = null,
                    LookupAlertOrder = 22
                },
                new Lookup
                {
                    LookupDescription = "ARSON",
                    LookupBinary = 2,
                    LookupColor = "5",
                    LookupType = LookupConstants.PERSONCAUTION,
                    LookupInactive = 0,
                    LookupIndex = 20,
                    LookupFlag7 = 1,
                    LookupNoAlert = null,
                    LookupAlertOrder = 22
                },
                new Lookup
                {
                    LookupDescription = "WRONGFUL FINDING",
                    LookupBinary = 3,
                    LookupType = LookupConstants.INCAPPEALREAS,
                    LookupInactive = 0,
                    LookupIndex = 9,
                    LookupNoAlert = 5
                },
                new Lookup
                {
                    LookupDescription = "MEDICAL ISOLATION",
                    LookupBinary = 2,
                    LookupColor = "2",
                    LookupType = LookupConstants.PERSONCAUTION,
                    LookupInactive = 0,
                    LookupIndex = 3,
                    LookupNoAlert = null,
                    LookupAlertOrder = 22,
                    LookupFlag7 = 1,
                    LookupName = "ISO"
                },
                new Lookup
                {
                    LookupDescription = "MISC",
                    LookupBinary = 2,
                    LookupColor = "7",
                    LookupType = LookupConstants.WAREHOUSECAT,
                    LookupInactive = 0,
                    LookupIndex = 1,
                    LookupNoAlert = null,
                    LookupAlertOrder = 22,
                    LookupName = "MISC"
                },
                new Lookup
                {
                    LookupDescription = "HYGIENE MATERIALS",
                    LookupBinary = 2,
                    LookupColor = "7",
                    LookupType = LookupConstants.WAREHOUSECAT,
                    LookupInactive = 0,
                    LookupIndex = 1,
                    LookupNoAlert = null,
                    LookupAlertOrder = 22,
                    LookupName = "HYMA",
                    CreateDate = DateTime.Now
                },
                new Lookup
                {
                    LookupDescription = "MISC",
                    LookupBinary = 2,
                    LookupColor = "7",
                    LookupType = LookupConstants.DELIVERDISPO,
                    LookupInactive = 0,
                    LookupIndex = 4,
                    LookupNoAlert = null,
                    LookupAlertOrder = 15,
                    LookupName = "MISC",
                    CreateDate = DateTime.Now
                },
                new Lookup
                {
                    LookupDescription = "WATCH",
                    LookupBinary = 15,
                    LookupType = LookupConstants.INVARTCL,
                    LookupInactive = 0,
                    LookupColor = null,
                    LookupIndex = 35,
                    LookupName = "WAT"
                },
                new Lookup
                {
                    LookupDescription = "JACKET",
                    LookupBinary = 15,
                    LookupType = LookupConstants.INVARTCL,
                    LookupInactive = 0,
                    LookupColor = null,
                    LookupIndex = 1,
                    LookupName = "JCT"

                },
                new Lookup
                {
                    LookupDescription = "ESCAPE RISK",
                    LookupBinary = 2,
                    LookupColor = "7",
                    LookupType = LookupConstants.TRANSCAUTION,
                    LookupInactive = 0,
                    LookupIndex = 1,
                    LookupNoAlert = null,
                    LookupAlertOrder = 22
                },
                new Lookup
                {
                    LookupDescription = "ESCAPE RISK",
                    LookupBinary = 2,
                    LookupColor = "7",
                    LookupType = LookupConstants.TRANSCAUTION,
                    LookupInactive = 0,
                    LookupIndex = 15,
                    LookupNoAlert = null,
                    LookupAlertOrder = 22
                },
                new Lookup
                {
                    LookupDescription = "ESCAPE RISK",
                    LookupBinary = 2,
                    LookupType = LookupConstants.DIET,
                    LookupInactive = 0,
                    LookupIndex = 8,
                    LookupNoAlert = null,
                    LookupAlertOrder = 20
                },
                new Lookup
                {
                    LookupDescription = "NO RISK",
                    LookupBinary = 2,
                    LookupType = LookupConstants.DIET,
                    LookupInactive = 0,
                    LookupIndex = 5,
                    LookupNoAlert = null,
                    LookupAlertOrder = 20
                },
                new Lookup
                {
                    LookupDescription = "ASSAULT",
                    LookupBinary = 2,
                    LookupType = "INVESTIGATIONTYPE",
                    LookupInactive = 0,
                    LookupIndex = 1,
                    LookupName = "ASSAULT",
                    LookupNoAlert = null,
                    LookupAlertOrder = 10
                },
                new Lookup
                {
                    LookupDescription = "DRUGS",
                    LookupType = "INVESTIGATIONTYPE",
                    LookupInactive = 0,
                    LookupIndex = 2,
                    LookupName = "DRUGS",
                    LookupNoAlert = null,
                    LookupAlertOrder = 20
                },
                new Lookup
                {
                    LookupDescription = "PREA",
                    LookupType = "INVESTIGATIONTYPE",
                    LookupInactive = 0,
                    LookupIndex = 3,
                    LookupName = "PREA",
                    LookupNoAlert = null,
                    LookupAlertOrder = 20
                },
                new Lookup
                {
                    LookupDescription = "HOMICIDE",
                    LookupType = "INVESTIGATIONTYPE",
                    LookupInactive = 0,
                    LookupIndex = 4,
                    LookupName = "HOMICIDE",
                    LookupNoAlert = null,
                    LookupAlertOrder = 20
                },
                new Lookup
                {
                    LookupDescription = "GANG",
                    LookupType = "INVESTIGATIONTYPE",
                    LookupInactive = 0,
                    LookupName = "GANG",
                    LookupIndex = 5,
                    LookupNoAlert = null,
                    LookupAlertOrder = 10
                },
                new Lookup
                {
                    LookupDescription = "ADMINITRATIVE",
                    LookupBinary = 2,
                    LookupType = "INVESTIGATIONTYPE",
                    LookupInactive = 0,
                    LookupIndex = 6,
                    LookupName = "ADMINITRATIVE",
                    LookupNoAlert = null,
                    LookupAlertOrder = 20
                },
                new Lookup
                {
                    LookupDescription = "GRIEVTYPE",
                    LookupBinary = 4,
                    LookupType = LookupConstants.GRIEVTYPE,
                    LookupInactive = 0,
                    LookupNoAlert = 5,
                    LookupIndex = 22,
                    LookupAlertOrder = 22
                },
                new Lookup
                {
                    LookupDescription = "MISC",
                    LookupBinary = 2,
                    LookupType = LookupConstants.GRVAPPEALTYPE,
                    LookupInactive = 0,
                    LookupIndex = 15,
                    LookupNoAlert = null,
                    LookupAlertOrder = 20
                },
                new Lookup
                {
                    LookupDescription = "RED",
                    LookupBinary = 22,
                    LookupType = LookupConstants.HAIRCOL,
                    LookupInactive = 0,
                    LookupIndex = 22,
                    LookupNoAlert = 5,
                    LookupAlertOrder = 15,
                    LookupName = "REDHAIR"
                },
                new Lookup
                {
                    LookupDescription = "BLACK",
                    LookupBinary = 23,
                    LookupType = LookupConstants.EYECOLOR,
                    LookupInactive = 0,
                    LookupIndex = 6,
                    LookupNoAlert = null,
                    LookupName = "BLK"
                },
                new Lookup
                {
                    LookupDescription = "BACKGROUND",
                    LookupBinary = 17,
                    LookupType = LookupConstants.VISITDENYREAS,
                    LookupInactive = 0,
                    LookupIndex = 5,
                    LookupNoAlert = null,
                    LookupName = "BG"
                },
                new Lookup
                {
                    LookupDescription = "ACTION TRANS TO JUV",
                    LookupBinary = 12,
                    LookupType = LookupConstants.CRIMETYPE,
                    LookupInactive = 0,
                    LookupIndex = 6,
                    LookupNoAlert = null,
                    LookupName = "A6"
                },
                new Lookup
                {
                    LookupDescription = "FEMALE",
                    LookupBinary = 2,
                    LookupType = LookupConstants.SEX,
                    LookupInactive = 0,
                    LookupIndex = 2,
                    LookupNoAlert = 10,
                    LookupName = "F"
                },
                new Lookup
                {
                    LookupDescription = "MALE",
                    LookupBinary = 1,
                    LookupType = LookupConstants.SEX,
                    LookupInactive = 0,
                    LookupIndex = 4,
                    LookupNoAlert = 10,
                    LookupName = "M"
                },
                new Lookup
                {
                    LookupDescription = "OTHERS",
                    LookupBinary = 4,
                    LookupType = LookupConstants.SEX,
                    LookupInactive = 0,
                    LookupIndex = 5,
                    LookupNoAlert = 10,
                    LookupName = "x"
                },
                new Lookup
                {
                    LookupDescription = "SAMOAN",
                    LookupBinary = 10,
                    LookupType = LookupConstants.RACE,
                    LookupInactive = 0,
                    LookupIndex = 10,
                    LookupNoAlert = 15,
                    LookupAlertOrder = 5,
                    LookupName = "S"
                },
                new Lookup
                {
                    LookupDescription = "OUTSIDE WORK CREW",
                    LookupBinary = 3,
                    LookupColor = "1",
                    LookupType = LookupConstants.TRANSCAUTION,
                    LookupInactive = 0,
                    LookupIndex = 3,
                    LookupNoAlert = 1,
                    LookupAlertOrder = 15
                },
                new Lookup
                {
                    LookupDescription = "NOT AVAILABLE",
                    LookupBinary = 12,
                    LookupColor = "3",
                    LookupType = LookupConstants.TRACKREFUSALREAS,
                    LookupInactive = 0,
                    LookupIndex = 5,
                    LookupNoAlert = null,
                    LookupAlertOrder = 15
                },
                new Lookup
                {
                    LookupDescription = "PRELIMINARY EXAMINATION",
                    LookupBinary = 12,
                    LookupColor = "8",
                    LookupType = LookupConstants.APPTYPE,
                    LookupInactive = 0,
                    LookupIndex = 10,
                    LookupNoAlert = null,
                    LookupAlertOrder = 15
                },
                new Lookup
                {
                    LookupDescription = "VICTIM",
                    LookupBinary = 10,
                    LookupName = "VICTIM",
                    LookupType = LookupConstants.RELATIONS,
                    LookupInactive = 0,
                    LookupIndex = 15,
                    LookupAlertOrder = 12
                },
                new Lookup
                {
                    LookupDescription = "COLLECTED",
                    LookupBinary = 10,
                    LookupName = "C",
                    LookupType = LookupConstants.DNADISPO,
                    LookupInactive = 0,
                    LookupIndex = 2,
                    LookupAlertOrder = 15
                },
                new Lookup
                {
                    LookupIndex = 5,
                    LookupBinary = 10,
                    LookupDescription = "REJECTED",
                    LookupName = "R",
                    LookupType = LookupConstants.DNADISPO,
                    LookupAlertOrder = 4,
                    LookupInactive = 0

                },
                new Lookup
                {
                    LookupDescription = "COUSIN",
                    LookupBinary = 5,
                    LookupName = "COUSIN",
                    LookupType = LookupConstants.RELATIONS,
                    LookupInactive = 0,
                    LookupIndex = 14,
                    LookupAlertOrder = 10
                },
                new Lookup
                {
                    LookupDescription = "MISC",
                    LookupBinary = 5,
                    LookupName = "MISC",
                    LookupType = LookupConstants.DNAPROCESSEDDISPO,
                    LookupInactive = 0,
                    LookupIndex = 14,
                    LookupAlertOrder = null
                },
                new Lookup
                {
                    LookupDescription = "ATTEMPT SUICIDE",
                    LookupBinary = 15,
                    LookupColor = "9",
                    LookupType = LookupConstants.DISCTYPE,
                    LookupInactive = 0,
                    LookupIndex = 10,
                    LookupNoAlert = 1,
                    LookupAlertOrder = 10,
                    LookupName = "AS"
                },
                new Lookup
                {
                    LookupDescription = "CONFIDENTIAL INFORMATION",
                    LookupBinary = 10,
                    LookupColor = "5",
                    LookupType = LookupConstants.DISCTYPE,
                    LookupInactive = 0,
                    LookupIndex = 15,
                    LookupNoAlert = 1,
                    LookupAlertOrder = 10,
                    LookupName = "CI"
                },
                new Lookup
                {
                    LookupDescription = "LOCAL BOYS",
                    LookupBinary = 10,
                    LookupType = LookupConstants.CLASSGROUP,
                    LookupInactive = 0,
                    LookupIndex = 9,
                    LookupNoAlert = 1,
                    LookupAlertOrder = 10
                },
                new Lookup
                {
                    LookupDescription = "CLASS A",
                    LookupBinary = 5,
                    LookupType = LookupConstants.DLCLASS,
                    LookupInactive = 0,
                    LookupIndex = 10,
                    LookupNoAlert = 1,
                    LookupAlertOrder = 10,
                    LookupName = "A"
                },
                new Lookup
                {
                    LookupDescription = "JR",
                    LookupBinary = 12,
                    LookupType = LookupConstants.NAMESUFFIX,
                    LookupInactive = 0,
                    LookupIndex = 14,
                    LookupNoAlert = 1,
                    LookupAlertOrder = 11
                },
                new Lookup
                {
                    LookupDescription = "PASSPORT",
                    LookupBinary = 4,
                    LookupType = LookupConstants.OTHERIDTYPE,
                    LookupInactive = 0,
                    LookupIndex = 15,
                    LookupNoAlert = 1,
                    LookupAlertOrder = 5
                },
                new Lookup
                {
                    LookupDescription = "LETTER",
                    LookupBinary = 12,
                    LookupType = LookupConstants.APPTYPE,
                    LookupInactive = 0,
                    LookupIndex = 5,
                    LookupNoAlert = null,
                    LookupAlertOrder = 11
                },
                new Lookup
                {
                    LookupDescription = "MED H&P MENS",
                    LookupBinary = 5,
                    LookupName = "BG",
                    LookupType = LookupConstants.POB,
                    LookupInactive = 0,
                    LookupIndex = 14,
                    LookupNoAlert = 1,
                    LookupAlertOrder = 12
                },
                new Lookup
                {
                    LookupDescription = "CITIZEN STATUS",
                    LookupBinary = 12,
                    LookupName = "ALIEN TEMPORARY",
                    LookupType = LookupConstants.CITIZENSTATUS,
                    LookupInactive = 0,
                    LookupIndex = 10,
                    LookupNoAlert = null,
                    LookupAlertOrder = 11
                },
                new Lookup
                {
                    LookupDescription = "INDIANA",
                    LookupBinary = 4,
                    LookupType = LookupConstants.STATE,
                    LookupInactive = 0,
                    LookupIndex = 4,
                    LookupNoAlert = 1,
                    LookupAlertOrder = 5
                },
                new Lookup
                {
                    LookupDescription = "COURT",
                    LookupBinary = 5,
                    LookupType = LookupConstants.ARRTYPE,
                    LookupInactive = 0,
                    LookupIndex = 5,
                    LookupNoAlert = 1,
                    LookupAlertOrder = 6
                },
                new Lookup
                {
                    LookupDescription = "SKY BLUE",
                    LookupBinary = 4,
                    LookupType = LookupConstants.INVCOLOR,
                    LookupIndex = 36,
                    LookupName = "SB"

                },
                new Lookup
                {
                    LookupDescription = "MENTAL HEALTH",
                    LookupBinary = 5,
                    LookupName = "MEN",
                    LookupType = LookupConstants.APPTREAS,
                    LookupInactive = 0,
                    LookupIndex = 12,
                    LookupNoAlert = 0,
                    LookupAlertOrder = 6
                },
                new Lookup
                {
                    LookupDescription = "DLN",
                    LookupBinary = 12,
                    LookupName = "DLN",
                    LookupType = LookupConstants.VISPERIDTYPE,
                    LookupInactive = 0,
                    LookupIndex = 12,
                    LookupNoAlert = 0,
                    LookupAlertOrder = 5
                },
                new Lookup
                {
                    LookupDescription = "MAXIMUM",
                    LookupBinary = 4,
                    LookupName = "MAX",
                    LookupType = LookupConstants.CLASREAS,
                    LookupInactive = 0,
                    LookupIndex = 2,
                    LookupCategory = "25"
                },
                new Lookup
                {
                    LookupDescription = "PENDING",
                    LookupBinary = 10,
                    LookupName = "F",
                    LookupType = LookupConstants.CLASREAS,
                    LookupInactive = 0,
                    LookupIndex = 3,
                    LookupCategory = "30"
                },
                new Lookup
                {
                    LookupDescription = "INFORMATIONAL ONLY",
                    LookupBinary = 5,
                    LookupName = "INFO",
                    LookupType = LookupConstants.CLASSLINKTYPE,
                    LookupInactive = 0,
                    LookupIndex = 5
                },
                new Lookup
                {
                    LookupDescription = "KEEP",
                    LookupBinary = 4,
                    LookupName = "K",
                    LookupType = LookupConstants.INVDISP,
                    LookupIndex = 4
                },
                new Lookup
                {
                    LookupDescription = "DONATED",
                    LookupBinary = 5,
                    LookupName = "D",
                    LookupType = LookupConstants.INVDISP,
                    LookupInactive = 0,
                    LookupIndex = 5
                },
                new Lookup
                {
                    LookupDescription = "MAIL",
                    LookupBinary = 10,
                    LookupName = "M",
                    LookupType = LookupConstants.INVDISP,
                    LookupInactive = 0,
                    LookupIndex = 10
                },
                new Lookup
                {
                    LookupDescription = "RELEASED TO PERSON",
                    LookupBinary = 6,
                    LookupName = "RTP",
                    LookupType = LookupConstants.INVDISP,
                    LookupInactive = 0,
                    LookupIndex = 6
                },
                new Lookup
                {
                    LookupDescription = "EVIDENCE",
                    LookupBinary = 11,
                    LookupName = "E",
                    LookupType = LookupConstants.INVDISP,
                    LookupIndex = 11
                },
                new Lookup
                {
                    LookupDescription = "STORAGE",
                    LookupBinary = 3,
                    LookupName = "S",
                    LookupType = LookupConstants.INVDISP,
                    LookupInactive = 0,
                    LookupIndex = 3
                },
                new Lookup
                {
                    LookupDescription = "SPELLING",
                    LookupBinary = 5,
                    LookupName = "SP",
                    LookupType = LookupConstants.INVDELREAS,
                    LookupInactive = 0,
                    LookupIndex = 10
                },
                new Lookup
                {
                    LookupDescription = "MISC",
                    LookupBinary = 10,
                    LookupName = "MI",
                    LookupType = LookupConstants.CHARGEQUALIFIER,
                    LookupInactive = 0,
                    LookupIndex = 7
                },
                new Lookup
                {
                    LookupDescription = "MISC",
                    LookupBinary = 10,
                    LookupName = "MI",
                    LookupType = LookupConstants.CALLLOGTYPE,
                    LookupInactive = 0,
                    LookupIndex = 15
                },
                new Lookup
                {
                    LookupDescription = "YOGA CLASS",
                    LookupBinary = 15,
                    LookupName = "YC",
                    LookupType = LookupConstants.DLCLASS,
                    LookupInactive = 0,
                    LookupIndex = 12
                },
                new Lookup
                {
                    LookupDescription = "MISC",
                    LookupBinary = 12,
                    LookupName = "MIC",
                    LookupType = LookupConstants.MEDFLAG,
                    LookupInactive = 0,
                    LookupIndex = 10,
                    LookupAlertOrder = 2
                },
                new Lookup
                {
                    LookupDescription = "ADA ALERT-BATHING ASSIST",
                    LookupBinary = 12,
                    LookupName = "ADA ALERT-BATHING ASSIST",
                    LookupType = LookupConstants.MEDFLAG,
                    LookupInactive = 0,
                    LookupIndex = 11,
                    LookupFlag6 = 1,
                    LookupAlertOrder = 2,
                    LookupColor = "RED"

                },
                new Lookup
                {
                    LookupDescription = "REQUIRE REVIEW",
                    LookupBinary = 12,
                    LookupName = "REQUIRE REVIEW",
                    LookupType = LookupConstants.PREAFLAGS,
                    LookupInactive = 0,
                    LookupIndex = 5


                },
                new Lookup
                {
                    LookupDescription = "SEPARATED AGINAST WILL",
                    LookupBinary = 12,
                    LookupName = "SEPARATED AGINAST WILL",
                    LookupType = LookupConstants.PREAFLAGS,
                    LookupInactive = 0,
                    LookupIndex = 6,
                    LookupAlertOrder = 0,
                    LookupCategory = "50"

                },
                new Lookup
                {
                    LookupDescription = "NONCONSENSUAL SEXUAL ACTS",
                    LookupBinary = 12,
                    LookupName = "NONCONSENSUAL SEXUAL ACTS",
                    LookupType = LookupConstants.PREAFLAGS,
                    LookupInactive = 0,
                    LookupIndex = 7,
                    LookupAlertOrder = 0


                },
                new Lookup
                {
                    LookupDescription = "TAMIL",
                    LookupBinary = 1452,
                    LookupName = "Tamil",
                    LookupType = LookupConstants.LANGUAGE,
                    LookupInactive = 0,
                    LookupIndex = 10,
                    LookupAlertOrder = 2
                },
                new Lookup
                {
                    LookupDescription = "FORCE USED",
                    LookupBinary = 15,
                    LookupName = "FU",
                    LookupType = LookupConstants.INCFLAG,
                    LookupInactive = 0,
                    LookupIndex = 10,
                    LookupAlertOrder = 15
                },
                new Lookup
                {
                    LookupDescription = "OTHER",
                    LookupBinary = 12,
                    LookupName = "OTHER",
                    LookupType = LookupConstants.RECCHKREQACTION,
                    LookupInactive = 0,
                    LookupIndex = 15,
                    LookupAlertOrder = 11
                },
                new Lookup
                {
                    LookupDescription = "BLOOD PRESURE",
                    LookupBinary = 10,
                    LookupName = "BP",
                    LookupType = LookupConstants.TESTTYPE,
                    LookupInactive = 0,
                    LookupIndex = 3
                },
                new Lookup
                {
                    LookupDescription = "TB TEST",
                    LookupBinary = 10,
                    LookupName = "TB",
                    LookupType = LookupConstants.TESTTYPE,
                    LookupInactive = 0,
                    LookupIndex = 2
                },
                new Lookup
                {
                    LookupDescription = "OTHER",
                    LookupBinary = 20,
                    LookupName = "OTHER",
                    LookupType = LookupConstants.RECCHKREQTYPE,
                    LookupInactive = 0,
                    LookupIndex = 11,
                    LookupAlertOrder = 15
                },
                new Lookup
                {
                    LookupDescription = "MISC",
                    LookupBinary = 15,
                    LookupName = "MISC",
                    LookupType = LookupConstants.TESTPROCESSDISPO,
                    LookupInactive = 0,
                    LookupIndex = 11,
                    LookupAlertOrder = 10
                },
                new Lookup
                {
                    LookupDescription = "MISC",
                    LookupBinary = 20,
                    LookupName = "MISC",
                    LookupType = LookupConstants.TESTDISPO,
                    LookupInactive = 0,
                    LookupIndex = 11,
                    LookupAlertOrder = 20
                },
                new Lookup
                {
                    LookupDescription = "NCICREQTRANS",
                    LookupType = LookupConstants.NCICREQTRANS,
                    LookupBinary = 12,
                    LookupInactive = 0,
                    LookupIndex = 15,
                    LookupAlertOrder = 15,
                    LookupName = "NCIC"
                },
                new Lookup
                {
                    LookupInactive = 0,
                    LookupBinary = 15,
                    LookupDescription = "INTERPRETER REQUIRED",
                    LookupType = LookupConstants.GRIEVFLAG,
                    LookupName = "I"
                },
                new Lookup
                {
                    LookupDescription = "BOOK'S",
                    LookupType = LookupConstants.LIBBOOKTYPE,
                    LookupBinary = 12,
                    LookupInactive = 0,
                    LookupIndex = 15,
                    LookupAlertOrder = 15,
                    LookupName = "BOOK",
                },
                new Lookup
                {
                    LookupDescription = "NEWSPAPER",
                    LookupType = LookupConstants.LIBBOOKTYPE,
                    LookupBinary = 10,
                    LookupInactive = 0,
                    LookupIndex = 5,
                    LookupAlertOrder = 15,
                    LookupName = "NEWSPAPER",
                },
                new Lookup
                {
                    LookupDescription = "JOURNAL",
                    LookupType = LookupConstants.LIBBOOKTYPE,
                    LookupBinary = 10,
                    LookupInactive = 0,
                    LookupIndex = 5,
                    LookupAlertOrder = 15,
                    LookupName = "JOURNAL",
                },

                new Lookup
                {
                    LookupDescription = "ART AND ARCHITECTUREEE",
                    LookupType = LookupConstants.LIBBOOKCAT,
                    LookupBinary = 12,
                    LookupInactive = 0,
                    LookupIndex = 16,
                    LookupAlertOrder = 15,
                    LookupName = "ART",
                },
                new Lookup
                {
                    LookupDescription = "CONTROVERSIAL TOPICS",
                    LookupType = LookupConstants.LIBBOOKCAT,
                    LookupBinary = 12,
                    LookupInactive = 0,
                    LookupIndex = 17,
                    LookupAlertOrder = 15,
                    LookupName = "CONTROVERSIAL",
                },
                new Lookup
                {
                    LookupDescription = "ECONOMICS AND BUSINESS",
                    LookupType = LookupConstants.LIBBOOKCAT,
                    LookupBinary = 12,
                    LookupInactive = 0,
                    LookupIndex = 18,
                    LookupAlertOrder = 15,
                    LookupName = "ECONOMICS"
                },
                new Lookup
                {
                    LookupDescription = "MUSIC AND PERFORMING ARTS",
                    LookupType = LookupConstants.LIBBOOKCAT,
                    LookupBinary = 15,
                    LookupInactive = 0,
                    LookupIndex = 19,
                    LookupAlertOrder = 15,
                    LookupName = "MUSIC"
                },
                new Lookup
                {
                    LookupDescription = "DAMAGED",
                    LookupType = LookupConstants.LIBBOOKCOND,
                    LookupBinary = 10,
                    LookupInactive = 0,
                    LookupIndex = 20,
                    LookupAlertOrder = 15,
                    LookupName = "DAMAGED",
                },
                new Lookup
                {
                    LookupDescription = "POOR",
                    LookupType = LookupConstants.LIBBOOKCOND,
                    LookupBinary = 10,
                    LookupInactive = 0,
                    LookupIndex = 20,
                    LookupAlertOrder = 15,
                    LookupName = "POOR",
                }
,
                new Lookup
                {
                    LookupDescription = "GOODD",
                    LookupType = LookupConstants.LIBBOOKCOND,
                    LookupBinary = 10,
                    LookupInactive = 0,
                    LookupIndex = 20,
                    LookupAlertOrder = 15,
                    LookupName = "GOODD",
                }
,
                new Lookup
                {
                    LookupDescription = "LIKE NEW",
                    LookupType = LookupConstants.LIBBOOKCOND,
                    LookupBinary = 10,
                    LookupInactive = 0,
                    LookupIndex = 20,
                    LookupAlertOrder = 15,
                    LookupName = "LIKE NEW",
                }


            );
            Db.RegistrantLookup.AddRange(
                new RegistrantLookup
                {
                    RegistrantLookupId = 10,
                    CreateBy = 11,
                    CreateDate = DateTime.Now,
                    RegistrantAbbr = "GEN",
                    RegistrantName = "GENDER REGISTRANT",
                    UpdateDate = DateTime.Now,
                    UpdateBy = 11
                },
                new RegistrantLookup
                {
                    RegistrantLookupId = 11,
                    CreateBy = 11,
                    CreateDate = DateTime.Now,
                    RegistrantAbbr = "GANG",
                    RegistrantName = "OTHER",
                    UpdateDate = DateTime.Now,
                    UpdateBy = 12
                });

            Db.LibraryRoom.AddRange(
                new LibraryRoom
                {
                    FacilityId = 1,
                    CreateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateBy = 12,
                    UpdateDate = DateTime.Now,
                    LibraryRoomId = 15,
                    RoomName = "CAT",
                    DeleteFlag = 0
                },
                new LibraryRoom
                {
                    FacilityId = 1,
                    CreateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateBy = 12,
                    UpdateDate = DateTime.Now,
                    LibraryRoomId = 16,
                    RoomName = "RABBIT",
                    DeleteFlag = 0
                }
            );
            Db.LibraryBook.AddRange(
                new LibraryBook
                {
                    LibraryBookId = 10,
                    BookAuthor = "JAIGATHAN",
                    DeleteFlag = 0,
                    CurrentCheckoutInmateId = 101,
                    LibraryRoomId = 15,
                    LibraryRoomLocationId = 11,
                    BookTypeId = 15,
                    CurrentConditionId = 20,
                    BookCategoryId = 16
                },
                new LibraryBook
                {
                    LibraryBookId = 11,
                    BookAuthor = "WILLIAM BLAKE",
                    DeleteFlag = 0,
                    CurrentCheckoutInmateId = 105,
                    LibraryRoomId = 16,
                    LibraryRoomLocationId = 11,
                    BookTypeId = 19,
                    BookCategoryId = 17,
                    CurrentConditionId = 20
                },
                new LibraryBook
                {
                    LibraryBookId = 12,
                    BookAuthor = "JOAN DIDION",
                    DeleteFlag = 0,
                    CurrentCheckoutInmateId = 110,
                    LibraryRoomId = 16,
                    LibraryRoomLocationId = 11,
                    BookCategoryId = 18,
                    BookTypeId = 15,
                    CurrentConditionId = 20

                },
                new LibraryBook
                {
                    LibraryBookId = 13,
                    BookAuthor = "BABUR",
                    DeleteFlag = 0,
                    CurrentCheckoutInmateId = 120,
                    LibraryRoomId = 16,
                    LibraryRoomLocationId = 12,
                    BookTypeId = 21,
                    BookCategoryId = 19,
                    CurrentConditionId = 20
                }
            );

            Db.LibraryRoomLocation.AddRange(
                new LibraryRoomLocation
                {
                    LibraryRoomLocationId = 11,
                    CreateDate = DateTime.Now.AddDays(-15),
                    UpdateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = 0,
                    LibraryRoomId = 16,
                    LocationName = "LIBARY LOC 1"
                },
                new LibraryRoomLocation
                {
                    LibraryRoomLocationId = 12,
                    CreateDate = DateTime.Now.AddDays(-15),
                    UpdateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = 0,
                    LibraryRoomId = 16,
                    LocationName = "LIBARY LOC 2"
                }

                );
            Db.LibraryTrack.AddRange(
                new LibraryTrack
                {
                    LibraryTrackId = 10,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = 0,
                    CheckInConditionId = 6,
                    UpdateDate = DateTime.Now,
                    CheckInDate = DateTime.Now.AddDays(-1),
                    CheckoutDate = DateTime.Now,
                    CheckoutInmateId = 110,
                    LibraryBookId = 12
                }

                );

            Db.IssuedPropertyLookup.AddRange(
                new IssuedPropertyLookup
                {
                    IssuedPropertyLookupId = 5,
                    CreateDate = DateTime.Now.AddDays(-8),
                    UpdateDate = DateTime.Now.AddDays(-3),
                    CreatedBy = 12,
                    UpdatedBy = 13,
                    PropertyName = "THE QURAN – SPANISH"
                },
                new IssuedPropertyLookup
                {
                    IssuedPropertyLookupId = 6,
                    CreateDate = DateTime.Now.AddDays(-8),
                    UpdateDate = DateTime.Now.AddDays(-3),
                    CreatedBy = 11,
                    UpdatedBy = 12,
                    PropertyName = "BIBLE - NEW WORLD TRANSLATION – SPANISH"
                }
            );
            Db.IssuedProperty.AddRange(
                new IssuedProperty
                {
                    IssuedPropertyId = 10,
                    DeleteFlag = 0,
                    ActiveFlag = 1,
                    InmateId = 101,
                    IssuedPropertyLookupId = 5,
                    IssuedCount = 8,
                    IssueNumber = "ISN_001",
                    IssueNote = null
                },
                new IssuedProperty
                {
                    IssuedPropertyId = 11,
                    DeleteFlag = 0,
                    ActiveFlag = 1,
                    InmateId = 100,
                    IssuedPropertyLookupId = 6,
                    IssuedCount = 3,
                    IssueNumber = "ISN_002",
                    IssueNote = null
                });

            Db.ConsularNotifyLookup.AddRange(
                new ConsularNotifyLookup
                {
                    ConsularNotifyLookupId = 10,
                    ConsulateEmail = "dssi@mail.com",
                    AutomateFlag = false,
                    CitizenshipCountry = "INDIA",
                    ConsulateAddress = "TITLE PARK",
                    ConsulateCity = "CHENNAI",
                    ConsulateFax = "(234) 3435-4546",
                    ConsulateInstructions = "GIVE ALL INSTRUCTION",
                    ConsulateState = "TAMIL NADU",
                    ConsulateName = "ALEGESH",
                    CreateDate = DateTime.Now.AddDays(-3),
                    ConsulatePhone = "2345854254",
                    NotificationOptional = true
                },
                new ConsularNotifyLookup
                {
                    ConsularNotifyLookupId = 15,
                    ConsulateEmail = "honar@mail.com",
                    AutomateFlag = true,
                    CitizenshipCountry = "INDIA",
                    ConsulateAddress = "BHARATHI STREET",
                    ConsulateCity = "KADAPA",
                    ConsulateFax = "(451) 4154-4546",
                    ConsulateInstructions = "GIVE ALL INSTRUCTION",
                    ConsulateState = "ANDRA",
                    ConsulateName = "RENOLD",
                    CreateDate = DateTime.Now.AddDays(-2),
                    ConsulatePhone = "457846843",
                    NotificationOptional = false,
                    NotificationRequired = false
                }
            );

            Db.DisciplinaryControlLookup.AddRange(
                new DisciplinaryControlLookup
                {
                    DisciplinaryControlLookupId = 10,
                    DisciplinaryControlLookupLevel = "LEVEL 1",
                    CreateDate = DateTime.Now.AddDays(-10),
                    UpdateDate = DateTime.Now.AddDays(-9),
                    DisciplinaryControlLookupName = "102",
                    DisciplinaryControlLookupType = (int)DisciplinaryLookup.DISCVIOL,
                    FinalSancSentDays = 0,
                    FinalSancFlag = 0,
                    DisciplinaryControlLookupDescription = "GAMBLING IS PROHIBITED"
                },
                new DisciplinaryControlLookup
                {
                    DisciplinaryControlLookupId = 11,
                    DisciplinaryControlLookupLevel = "LEVEL 3",
                    CreateDate = DateTime.Now.AddDays(-10),
                    UpdateDate = DateTime.Now.AddDays(-9),
                    DisciplinaryControlLookupName = "WORK",
                    DisciplinaryControlLookupType = (int)DisciplinaryLookup.DISCWAIV,
                    FinalSancSentDays = 0,
                    FinalSancFlag = 0,
                    DisciplinaryControlLookupDescription = "I DO NOT WAIVE THE 24 HOUR RULE"
                }
            );
        }
    }
}
