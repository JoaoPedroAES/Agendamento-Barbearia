using System.ComponentModel.DataAnnotations.Schema;

namespace barbearia.api.Models
{
    public class WorkSchedule
    {
        public int Id { get; set; }

        public int BarberId { get; set; }
        [ForeignKey("BarberId")]
        public virtual Barber Barber { get; set; }

        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }  
        public TimeSpan BreakStartTime { get; set; }
        public TimeSpan BreakEndTime { get; set; }
    }
}
