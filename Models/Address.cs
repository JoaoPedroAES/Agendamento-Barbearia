using System.ComponentModel.DataAnnotations.Schema;

namespace barbearia.api.Models
{
    public class Address
    {
        public int Id { get; set; }
        public string Cep { get; set; }
        public string Street { get; set; }
        public string Number { get; set; }
        public string? Complement { get; set; }
        public string Neighborhood { get; set; }
        public string City { get; set; }
        public string State { get; set; }

        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser User { get; set; }
    }
}
