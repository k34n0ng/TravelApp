namespace TravelApp.Models
{
    public class MapSearchRequest
    {
        public string Query { get; set; } = string.Empty;
        public string? CountryCode { get; set; }
        public string? City { get; set; }
        public GeoCoordinate? NearLocation { get; set; }
        public int MaxResults { get; set; } = 10;
    }
}