namespace TravelApp.Models
{
    public class MapPlace
    {
        public string Name { get; set; } = string.Empty;
        public string? LocalName { get; set; }
        public string? Address { get; set; }
        public string? LocalAddress { get; set; }
        public string? City { get; set; }
        public string? CountryCode { get; set; }
        public string? Category { get; set; }
        public GeoCoordinate? Location { get; set; }
        public MapProviderType Provider { get; set; }
        public string? ProviderPlaceId { get; set; }
        public string? MapDeepLink { get; set; }
    }
}