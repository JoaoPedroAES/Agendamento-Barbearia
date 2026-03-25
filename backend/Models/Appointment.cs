using System.ComponentModel.DataAnnotations.Schema;

namespace barbearia.api.Models
{
    public enum AppointmentStatus
    {
        Scheduled,
        Completed,
        CancelledByCustomer,
        CancelledByAdmin
    }
    public class Appointment
    {
        public int Id { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public decimal TotalPrice { get; set; }

        
        public string CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual ApplicationUser Customer { get; set; }

        
        public int BarberId { get; set; }
        [ForeignKey("BarberId")]
        public virtual Barber Barber { get; set; }

        
        public virtual ICollection<Service> Services { get; set; }
    }
}
