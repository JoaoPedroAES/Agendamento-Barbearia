using System.ComponentModel.DataAnnotations;

namespace barbearia.api.Dtos
{
    public class RegisterCustomerDto
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string? PhoneNumber { get; set; }

        // Campos para o Endereço (API ViaCEP)
        [Required]
        public string Cep { get; set; }
        [Required]
        public string Street { get; set; } // Rua
        [Required]
        public string Number { get; set; }
        public string? Complement { get; set; }
        [Required]
        public string Neighborhood { get; set; } // Bairro
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; } // UF

        public bool CreateAsAdmin { get; set; }
    }
}
