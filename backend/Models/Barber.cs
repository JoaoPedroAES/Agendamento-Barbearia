using System.ComponentModel.DataAnnotations.Schema;

namespace barbearia.api.Models
{
    public class Barber
    {
        public int Id { get; set; }
        public string Bio { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool HasAcceptedTerms { get; set; } = false;
        public string ApplicationUserId { get; set; }

        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser UserAccount { get; set; }
    }
}
