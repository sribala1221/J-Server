using System.ComponentModel.DataAnnotations;

namespace ServerAPI.Authentication
{
    public class LoginVM
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class AdLoginVM
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Domain { get; set; }
    }
    public class SamlLoginVM
    {
        public string UserName { get; set; }
    }

    public class SamlUrlResponse
    {
        public bool Required { get; set; }
        public string IdpUrl { get; set; }
        public string SamlRequestToken { get; set; }
        public string idpLogoutUrl { get; set; }
    }

    public class SamlLogoutRequest
    {
        public string session { get; set; }
    }
}
