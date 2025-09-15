using System.ComponentModel.DataAnnotations;

namespace barbearia.api.Dtos
{
    public class UpdateBarberDto
    {
        [Required]
        public string FullName { get; set; }
        public string Bio { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
