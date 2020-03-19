using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class PersonAkaVm{
        public int AkaId{get;set;}
        public string AkaFirstName{get;set;}
        public string AkaLastName{get;set;}
        public string AkaMiddleName{get;set;}
        public string personGangName{get;set;}
    }

    public class AkaDetails{
       public List<PersonAkaVm> personAka {get;set;}
       public List<KeyValuePair<string,int>> personGangName{get;set;}
    }

    public class PersonAkaHeader{
        public PersonAkaVm PersonHeaderDetails {get;set;}
       public string PersonPronoun{get;set;}
    }
}