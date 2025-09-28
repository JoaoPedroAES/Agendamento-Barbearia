using System.ComponentModel.DataAnnotations;

namespace barbearia.api.Dtos
{
    public class UpdateProfileDto
    {
        [Required]
        public string FullName { get; set; }
        public string? PhoneNumber { get; set; }

        // Campos do Endereço
        [Required]
        public string Cep { get; set; }
        [Required]
        public string Street { get; set; }
        [Required]
        public string Number { get; set; }
        public string? Complement { get; set; }
        [Required]
        public string Neighborhood { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
    }

}
