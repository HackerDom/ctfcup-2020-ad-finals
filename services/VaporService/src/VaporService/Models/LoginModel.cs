using System.ComponentModel.DataAnnotations;

namespace VaporService.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "No name")] public string Name { get; set; }

        [Required(ErrorMessage = "No password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}