namespace barbearia.api.Dtos
{
    public class UserProfileDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public AddressDto Address { get; set; }
    }
}
