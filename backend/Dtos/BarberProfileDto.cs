namespace barbearia.api.Dtos
{
    public class BarberProfileDto
    {
        public int BarberId { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Bio { get; set; }
        public bool HasAcceptedTerms { get; set; }
    }
}
