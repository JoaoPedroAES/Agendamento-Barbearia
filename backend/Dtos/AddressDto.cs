namespace barbearia.api.Dtos
{
    public class AddressDto
    {
        public string Cep { get; set; }
        public string Street { get; set; }
        public string Number { get; set; }
        public string? Complement { get; set; }
        public string Neighborhood { get; set; }
        public string City { get; set; }
        public string State { get; set; }
    }
}
