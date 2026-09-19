namespace EdenRequest.Api.DTO
{
    public class FinnishAddressResultDto
    {
        public string? Label { get; set; }
        public string? Street { get; set; }
        public string? HouseNumber { get; set; }
        public string? PostalCode { get; set; }
        public string? City { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
