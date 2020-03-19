using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ServerAPI.Authorization;

namespace ServerAPI.Controllers {
    [Route("[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly IPersonIdentityService _personIdentity;
        private readonly IPersonPhotoService _personPhoto;
        private readonly IPersonAddressService _personAddress;
        private readonly IPersonAkaService _personAka;
        private readonly IPersonCharService _personChar;
        private readonly IPersonDnaService _personDna;
        private readonly IPersonDescriptorService _personDescriptor;
        private readonly IPersonTestingService _personTesting;
        private readonly IPersonContactService _personContact;
        private readonly IPersonProfileService _personProfile;
        private readonly IPersonMilitaryService _personMilitary;
        private readonly IPersonService _personService;
       
        public PersonController(IPersonIdentityService personIdentity, IPersonPhotoService personPhoto,
            IPersonAddressService personAddress, IPersonAkaService personAka,
            IPersonCharService personChar,
            IPersonDnaService personDna, IPersonDescriptorService personDescriptor,
            IPersonTestingService personTesting, IPersonContactService personContact,
            IPersonProfileService personProfile, IPersonMilitaryService personMilitary, IPersonService personService)
        {
            _personIdentity = personIdentity;
            _personPhoto = personPhoto;
            _personAddress = personAddress;
            _personAka = personAka;
            _personChar = personChar;
            _personDna = personDna;
            _personDescriptor = personDescriptor;
            _personTesting = personTesting;
            _personContact = personContact;
            _personProfile = personProfile;
            _personMilitary = personMilitary;
            _personService = personService;
        }

        [HttpGet("GetPersonDetails")]
        public IActionResult GetPersonDetails(int personId) => Ok(_personIdentity.GetPersonDetails(personId));

        [HttpGet("GetInmateDetails")]
        public IActionResult GetInmateDetails(int inmateId) => Ok(_personService.GetInmateDetails(inmateId));

        [FuncPermission(432, FuncPermissionType.Access)]
        [HttpGet("GetNamePopupDetails")]
        public IActionResult GetNamePopupDetails(int personId, int inmateId) => 
            Ok(_personIdentity.GetNamePopupDetails(personId, inmateId));

        [FuncPermission(432, FuncPermissionType.Edit)]
        [HttpPost("InsertNamePopupDetails")]
        public async Task<IActionResult> InsertNamePopupDetails([FromBody] PersonDetail personDetail) => 
            Ok(await _personIdentity.InsertNamePopupDetails(personDetail));

        [HttpGet("GetPersonSavedHistory")]
        public IActionResult GetPersonSavedHistory(int personId) => Ok(_personIdentity.GetPersonSavedHistory(personId));

        [HttpGet("GetCitizenshipList")]
        public IActionResult GetCitizenshipList(int personId) => Ok(_personIdentity.GetCitizenshipList(personId));

        [HttpGet("GetPersonnelCitizenshipHistory")]
        public IActionResult GetPersonnelCitizenshipHistory(int personId, int personCitizenshipId) => 
            Ok(_personIdentity.GetPersonnelCitizenshipHistory(personId, personCitizenshipId));

        [HttpGet("GetPersonPhoto")]
        public IActionResult GetPersonPhoto(int personId, bool deleteFlag = false) => 
            Ok(_personPhoto.GetPersonPhoto(personId, deleteFlag));

        [FuncPermission(438, FuncPermissionType.Add)]
        [HttpPost("InsertUpdatePersonPhoto")]
        public async Task<IActionResult> InsertUpdatePersonPhoto([FromBody] PersonPhoto personPhoto) => 
            Ok(await _personPhoto.InsertUpdatePersonPhoto(personPhoto));
        [FuncPermission(438, FuncPermissionType.Edit)]
        [HttpPost("UpdatePersonPhotos")]
        public async Task<IActionResult> UpdatePersonPhotos([FromBody] PersonPhoto personPhoto) => 
            Ok(await _personPhoto.InsertUpdatePersonPhoto(personPhoto));

        [FuncPermission(438, FuncPermissionType.Edit)]
        [HttpGet("GetPersonPhotoDescriptor")]
        public IActionResult GetPersonPhotoDescriptor(int personId) => Ok(_personPhoto.GetPersonPhotoDescriptor(personId));

        [HttpPost("InsertUpdatePersonCitizenship")]
        public async Task<IActionResult> InsertUpdatePersonCitizenship([FromBody] PersonCitizenshipVm citizenship) => 
            Ok(await _personIdentity.InsertUpdatePersonCitizenship(citizenship));

        [HttpPost("DeleteUndoPersonCitizenship")]
        public async Task<IActionResult> DeleteUndoPersonCitizenship([FromBody] PersonCitizenshipVm citizenshipDetail) => 
            Ok(await _personIdentity.DeleteUndoPersonCitizenship(citizenshipDetail));

        [FuncPermission(431, FuncPermissionType.Edit)]
        [HttpPost("InsertUpdatePersonDetails")]
        public async Task<IActionResult> InsertUpdatePersonDetails([FromBody] PersonIdentity person) => 
            Ok(await _personIdentity.InsertUpdatePersonDetails(person));

        [AllowAnonymous]
        [HttpPost("InsertUpdatePerson")]
        public async Task<IActionResult> InsertUpdatePerson([FromBody] PersonVm person) => 
            Ok(await _personIdentity.InsertUpdatePerson(person));

        [HttpGet("GetPersonAddressDetails")]
        public IActionResult GetPersonAddressDetails(int personId) => Ok(_personAddress.GetPersonAddressDetails(personId));

        [FuncPermission(431, FuncPermissionType.Add)]
        [HttpPost("InsertAddressDetails")]
        public async Task<IActionResult> InsertAddressDetails(
            [FromBody] PersonAddressDetails personAddressDetails) => 
            Ok(await _personAddress.InsertUpdateAddressDetails(personAddressDetails));

        [FuncPermission(431, FuncPermissionType.Edit)]
        [HttpPost("UpdateAddressDetails")]
        public async Task<IActionResult> UpdateAddressDetails(
            [FromBody] PersonAddressDetails personAddressDetails) => 
            Ok(await _personAddress.InsertUpdateAddressDetails(personAddressDetails));

        [HttpGet("GetPersonAkaDetails")]
        public IActionResult GetPersonAkaDetails(int personId) => Ok(_personAka.GetAkaDetails(personId));

        [HttpGet("GetPersonAkaHistory")]
        public IActionResult GetPersonAkaHistory(int akaId) => Ok(_personAka.GetPersonAkaHistory(akaId));

        [FuncPermission(431, FuncPermissionType.Delete)]
        [HttpPut("DeletePersonAka")]
        public async Task<IActionResult> DeletePersonAka([FromBody] AkaVm aka) => Ok(await _personAka.DeleteUndoPersonAka(aka));

        [HttpPut("UndoPersonAka")]
        public async Task<IActionResult> UndoPersonAka([FromBody] AkaVm aka) => Ok(await _personAka.DeleteUndoPersonAka(aka));

        [FuncPermission(431, FuncPermissionType.Add)]
        [HttpPost("InsertPersonAka")]
        public async Task<IActionResult> InsertPersonAka([FromBody] AkaVm aka) => Ok(await _personAka.InsertUpdatePersonAka(aka));

        [FuncPermission(431, FuncPermissionType.Edit)]
        [HttpPut("UpdatePersonAka")]
        public async Task<IActionResult> UpdatePersonAka([FromBody] AkaVm aka) => Ok(await _personAka.InsertUpdatePersonAka(aka));

        [HttpGet("GetPersonCharDetail")]
        public IActionResult GetPersonCharDetail(int personId) => Ok(_personChar.GetCharDetails(personId));

        [FuncPermission(431, FuncPermissionType.Edit)]
        [HttpPost("InsertPersonChar")]
        public async Task<IActionResult> InsertPersonChar([FromBody] PersonCharVm personchar) => 
            Ok(await _personChar.InsertUpdatePersonChar(personchar));

        [FuncPermission(431, FuncPermissionType.Edit)]
        [HttpPost("UpdatePersonChar")]
        public async Task<IActionResult> UpdatePersonChar([FromBody] PersonCharVm personchar) => 
            Ok(await _personChar.InsertUpdatePersonChar(personchar));

        [AllowAnonymous]
        [HttpPost("InsertUpdatePersonCharFromInterfaceEngineStartPrebookFromKPF")]
        public async Task<IActionResult> InsertUpdatePersonCharFromInterfaceEngineStartPrebookFromKPF([FromBody] PersonCharVm personchar) => 
            Ok(await _personChar.InsertUpdatePersonCharFromInterfaceEngineStartPrebookFromKPF(personchar));

        [HttpGet("GetPersonCharHistory")]
        public IActionResult GetPersonCharHistory(int charId) => Ok(_personChar.GetPersonCharHistory(charId));

        [HttpGet("GetDnaDetails")]
        public IActionResult GetDnaDetails(int personId) => Ok(_personDna.GetDnaDetails(personId));

        [FuncPermission(440, FuncPermissionType.Add)]
        [HttpPost("InsertPersonDna")]
        public async Task<IActionResult> InsertPersonDna([FromBody] DnaVm dna) => Ok(await _personDna.InsertUpdatePersonDna(dna));

        [FuncPermission(440, FuncPermissionType.Edit)]
        [HttpPost("UpdatePersonDna")]
        public async Task<IActionResult> UpdatePersonDna([FromBody] DnaVm dna) => Ok(await _personDna.InsertUpdatePersonDna(dna));

        [HttpPut("InsertPersonMilitary")]
        [FuncPermission(431, FuncPermissionType.Add)]
        public IActionResult InsertPersonMilitary() => Ok();

        [HttpPut("UpdatePersonMilitary")]
        [FuncPermission(431, FuncPermissionType.Edit)]
        public IActionResult UpdatePersonMilitary() => Ok();

        [HttpGet("GetDnaHistoryDetails")]
        public IActionResult GetDnaHistoryDetails(int dnaId) => Ok(_personDna.GetDnaHistoryDetails(dnaId));

        [HttpGet("GetIdentifierDetails")]
        public IActionResult GetIdentifierDetails(int personId, bool isDelete) => 
            Ok(_personDescriptor.GetIdentifierDetails(personId, isDelete));
        [FuncPermission(437, FuncPermissionType.Add)]
        [HttpGet("GetDescriptorLookupDetails")]
        public IActionResult GetDescriptorLookupDetails(string[] bodyMap, int personId) => 
            Ok(_personDescriptor.GetDescriptorLookupDetails(bodyMap, personId));

        [FuncPermission(437, FuncPermissionType.Edit)]
        [HttpGet("DescriptorEdit")]
        public IActionResult DescriptorEdit() => NoContent();

        [HttpPost("InsertUpdateDescriptor")]
        public async Task<IActionResult> InsertUpdateDescriptor([FromBody] PersonDescriptorVm desc) => 
            Ok(await _personDescriptor.InsertUpdateDescriptor(desc));
        [FuncPermission(437, FuncPermissionType.Delete)]
        [HttpPost("DeleteUndoDescriptor")]
        public async Task<IActionResult> DeleteUndoDescriptor([FromBody] PersonDescriptorVm descriptor) => 
            Ok(await _personDescriptor.DeleteUndoDescriptor(descriptor));

        [HttpGet("GetTestingDetails")]
        public IActionResult GetTestingDetails(int personId) => Ok(_personTesting.GetTestingDetails(personId));

        [FuncPermission(441, FuncPermissionType.Add)]
        [HttpPost("InsertTestingDetails")]
        public async Task<IActionResult> InsertTestingDetails([FromBody] TestingVm testingDet) =>
            Ok(await _personTesting.InsertUpdateTestingDetails(testingDet));

        [FuncPermission(441, FuncPermissionType.Edit)]
        [HttpPost("UpdateTestingDetails")]
        public async Task<IActionResult> UpdateTestingDetails([FromBody] TestingVm testingDet) => 
            Ok(await _personTesting.InsertUpdateTestingDetails(testingDet));

        [HttpGet("GetTestingHistoryDetails")]
        public IActionResult GetTestingHistoryDetails(int testingId) => Ok(_personTesting.GetTestingHistoryDetails(testingId));

        [HttpGet("GetContactDetails")]
        public IActionResult GetContactDetails(int typePersonId) => Ok(_personContact.GetContactDetails(typePersonId));

        [FuncPermission(446, FuncPermissionType.Add)]
        [HttpPost("InsertContactAttempt")]
        public async Task<IActionResult> InsertContactAttempt([FromBody] ContactAttemptVm contactAttempt) => 
            Ok(await _personContact.InsertUpdateContactAttempt(contactAttempt));

        [FuncPermission(446, FuncPermissionType.Edit)]
        [HttpPost("UpdateContactAttempt")]
        public async Task<IActionResult> UpdateContactAttempt([FromBody] ContactAttemptVm contactAttempt) => 
            Ok(await _personContact.InsertUpdateContactAttempt(contactAttempt));

        [FuncPermission(446, FuncPermissionType.Delete)]
        [HttpPost("DeleteContactAttempt")]
        public async Task<IActionResult> DeleteContactAttempt([FromBody] int contactAttemptId) => 
            Ok(await _personContact.DeleteUndoContactAttempt(contactAttemptId));

        [HttpPost("UndoContactAttempt")]
        public async Task<IActionResult> UndoContactAttempt([FromBody] int contactAttemptId) => 
            Ok(await _personContact.DeleteUndoContactAttempt(contactAttemptId));

        [FuncPermission(431, FuncPermissionType.Add)]
        [HttpPost("InsertContact")]
        public async Task<IActionResult> InsertContact([FromBody] ContactVm contact) => 
            Ok(await _personContact.InsertUpdateContact(contact));

        [FuncPermission(431, FuncPermissionType.Edit)]
        [HttpPost("UpdateContact")]
        public async Task<IActionResult> UpdateContact([FromBody] ContactVm contact) => 
            Ok(await _personContact.InsertUpdateContact(contact));

        [FuncPermission(431, FuncPermissionType.Delete)]
        [HttpPost("DeleteContact")]
        public async Task<IActionResult> DeleteContact([FromBody] ContactVm contactDet) => 
            Ok(await _personContact.DeleteUndoContact(contactDet));

        [HttpPost("UndoContact")]
        public async Task<IActionResult> UndoContact([FromBody] ContactVm contactDet) => 
            Ok(await _personContact.DeleteUndoContact(contactDet));

        [HttpGet("GetContactCreateUpdateDetails")]
        public IActionResult GetContactCreateUpdateDetails(int contactId) => 
            Ok(_personContact.GetContactCreateUpdateDetails(contactId));

        [HttpGet("GetSkillAndTradedetails")]
        public IActionResult GetSkillAndTradedetails(int personId) => Ok(_personProfile.GetSkillAndTradedetails(personId));

        [HttpPost("InsertSkillAndTradeDetails")]
        public async Task<IActionResult> InsertSkillAndTradeDetails([FromBody] PersonProfileVm personProfile) => 
            Ok(await _personProfile.InsertSkillAndTradeDetails(personProfile));

        [HttpGet("GetProgramAndClass")]
        public IActionResult GetProgramAndClass(int inmateId, int facilityId) => 
            Ok(_personProfile.GetProgramAndClass(inmateId, facilityId));

        [HttpPost("ValidateProgam")]
        public IActionResult ValidateProgam([FromBody] ProgramDetails program) => Ok(_personProfile.ValidateProgam(program));

        [HttpGet("GetProfileDetails")]
        public IActionResult GetProfileDetails(int personId, int inmateId) => 
            Ok(_personProfile.GetProfileDetails(personId, inmateId));

        [FuncPermission(431, FuncPermissionType.Edit)]
        [HttpPost("InsertUpdatePersonProfile")]
        public async Task<IActionResult> InsertUpdatePersonProfile([FromBody] PersonProfileVm personProfile) => 
            Ok(await _personProfile.InsertUpdatePersonProfile(personProfile));

        [HttpGet("GetWorkCrewRequestDetails")]
        public IActionResult GetWorkCrewRequestDetails(int facilityId, int inmateId) => 
            Ok(_personProfile.GetWorkCrewRequestDetails(facilityId, inmateId));

        [HttpPost("UpdateInmateNumber")]
        public async Task<IActionResult> UpdateInmateNumber([FromBody] AkaVm akaDetail) => 
            Ok(await _personIdentity.UpdateInmateNumber(akaDetail.InmateId, akaDetail.AkaInmateNumber, akaDetail.AkaHistoryList));

        [HttpPost("InsertWorkCrowAndFurloughRequest")]
        public async Task<IActionResult> InsertWorkCrowAndFurloughRequest(
            [FromBody] WorkCrowAndFurloughRequest workCrowAndFurlough) => 
            Ok(await _personProfile.InsertWorkCrowAndFurloughRequest(workCrowAndFurlough));

        [HttpGet("GetPersonMilitaryDetails")]
        public IActionResult GetPersonMilitaryDetails(int personId) => 
            Ok(_personMilitary.GetPersonMilitaryDetails(personId));

        [HttpPost("InsertUpdatePersonMilitary")]
        public async Task<IActionResult> InsertUpdatePersonMilitary([FromBody] PersonMilitaryVm personMilitary) => 
            Ok(await _personMilitary.InsertUpdatePersonMilitary(personMilitary));

        [HttpPut("DeletePersonMilitary")]
        public async Task<IActionResult> DeletePersonMilitary([FromBody] PersonMilitaryVm personMilitary) =>
            Ok(await _personMilitary.DeletePersonMilitary(personMilitary));

        [HttpGet("GetMilitaryHistoryDetails")]
        public IActionResult GetMilitaryHistoryDetails(int militaryId) => 
            Ok(_personMilitary.GetMilitaryHistoryDetails(militaryId));

        //To get Images From Temp Folder
        [HttpGet("GetTempImages")]
        public IActionResult GetTempImages(string facilityName) => Ok(_personPhoto.GetTempImages(facilityName));

        //Upload Image To Temp Folder 
        [HttpPost("UploadTempPhoto")]
        public IActionResult UploadTempPhoto([FromQuery]string facilityName) => HttpContext.Request.Form.Files.Count == 0
                ? BadRequest("file not uploaded!")
                : (IActionResult)Ok(_personPhoto.UploadTempPhoto(HttpContext.Request.Form.Files[0], facilityName));

        //Delete Image From Temp Folder
        [HttpGet("DeleteTempPhoto")]
        public IActionResult DeleteTempPhoto(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest("fileName Required!");
            _personPhoto.DeleteTempPhoto(fileName);
            return Ok();
        }

        // Delete Person Photo Details.
        [FuncPermission(438, FuncPermissionType.Delete)]
        [HttpPost("DeleteOrUndoPhoto")]
        public async Task<IActionResult> DeleteOrUndoPhoto([FromBody] DeleteParams deleteParams) => 
            Ok(await _personPhoto.DeleteOrUndoPhoto(deleteParams));

        //Name preference Search
        [HttpGet("NamePreference")]
        public IActionResult NamePreference(int personId) => Ok(_personProfile.NamePreference(personId));

        [HttpGet("GetIsPersonSealed")]
        public IActionResult GetIsPersonSealed(int personId) => Ok(_personService.IsPersonSealed(personId));

        [HttpGet("GetDescriptorLookup")]
        public IActionResult GetDescriptorLookup() => Ok(_personDescriptor.PersonDescriptorLookup());

        [HttpGet("GetCustomFields")]
        public IActionResult GetCustomFields(int userControlId, int? PersonId) => 
            Ok(_personIdentity.GetCustomFields(userControlId, PersonId));

        [FuncPermission(433, FuncPermissionType.Edit)]
        [HttpPut("UpdatePersonAkas")]
        public IActionResult UpdatePersonAka() => Ok();

        [FuncPermission(431, FuncPermissionType.Add)]
        [HttpPost("CreatePerson")]
        public IActionResult CreatePerson() => Ok();

        [FuncPermission(431, FuncPermissionType.Access)]
        [HttpGet("getHistoryDetails")]
        public IActionResult GetHistoryDetails() => Ok();

        [FuncPermission(438, FuncPermissionType.Edit)]
        [HttpPut("UpdatePersonPhoto")]
        public IActionResult UpdatePersonPhoto() => Ok();

    }
}
