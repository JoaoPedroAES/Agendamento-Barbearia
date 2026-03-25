namespace barbearia.api.Dtos
{
    public class CreateAppointmentDto
    {
        public int BarberId { get; set; }
        public DateTime StartDateTime { get; set; }
        public List<int> ServiceIds { get; set; }
    }
}
