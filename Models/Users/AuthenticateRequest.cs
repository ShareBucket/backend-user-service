using System.ComponentModel.DataAnnotations;

namespace ShareBucket.UserMicroService.Models.Users
{
    public class AuthenticateRequest
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}