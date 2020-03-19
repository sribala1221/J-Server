using GenerateTables.Models;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
   
        public class FulcrumEnrollRequest
        {
            public int personId { get; set; }
            public string biolocation { get; set; }
            public string type { get; set; }
            public string[] templates { get; set; }
        }
        public class FulcrumEnrollRequestParams
        {
            public int personId { get; set; }
            public string biolocation { get; set; }
            public string taxonomy { get; set; }
            public FulcrumEnrollRequestTemplate[] templates { get; set; }
        }
        public class FulcrumEnrollRequestTemplate
        {
            public string type { get; set; }
            public string template { get; set; }
        }
        public class FulcrumEnrollResponse
        {
            public string status { get; set; }
        }
        public class FulcrumGeneralizeRequest
        {
            public string biolocation { get; set; }
            public string[] templates { get; set; }
        }
        public class FulcrumGeneralizeResponse
        {
            public string result { get; set; }
        }

        public class FulcrumIdentifyRequest
        {
            public string biolocation { get; set; }
            public string taxonomy { get; set; }
            public FulcrumIdentifyRequestTemplate[] templates { get; set; }
        }
        public class FulcrumIdentifyRequestTemplate
        {
            public string type { get; set; }
            public string template { get; set; }
            public string templateBase64 { get; set; }
            public int quality { get; set; }
        }
        public class FulcrumIdentifyResponse
        {
            public int? recordId { get; set; }
            public string status { get; set; }
            public string bioLocation { get; set; }
            public int? matchId { get; set; }
            public int? matchScore { get; set; }
            public string personId { get; set; }
        }

        public class FulcrumVerifyRequest
        {
            public string biolocation { get; set; }
            public int personId { get; set; }
            public string taxonomy { get; set; }
            public FulcrumVerifyTemplate template { get; set; }
        }
        public class FulcrumVerifyTemplate
        {
            public int quality { get; set; }
            public string template { get; set; }
            public string type { get; set; }
        }
        public class FulcrumVerifyResponse
        {
            public bool result { get; set; }
        }

        public class FulcrumDeleteRequest
        {
            public int personId { get; set; }
            public string biolocation { get; set; }
            public string taxonomy { get; set; }
        }
}
