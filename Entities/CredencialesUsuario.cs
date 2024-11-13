using System.ComponentModel.DataAnnotations;

namespace WarHammer40K.Entities
{
    public class CredencialesUsuario
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public IFormFile? ProfileImage { get; set; }
    }
}
