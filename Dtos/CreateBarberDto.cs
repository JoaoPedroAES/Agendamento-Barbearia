using System.ComponentModel.DataAnnotations;

namespace barbearia.api.Dtos
{
    public class CreateBarberDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
        
        [Required]
        public string FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string Bio { get; set; }
    }
}
