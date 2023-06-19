using DataAccessLayer.Models.Entities;
using ShareBucket.DataAccessLayer.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UserMicroService.Models.Users
{
    public class UserDataResponse
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

    }
}
