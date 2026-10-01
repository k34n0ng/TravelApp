using TravelApp.Controls;
using TravelApp.Models;

namespace TravelApp.Views;

public partial class NaverMapTestPage : ContentPage
{
    public NaverMapTestPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Adds/updates a marker at Gyeongbokgung Palace (경복궁).
    /// WGS84: 37.5796, 126.9770.
    /// </summary>
    private void OnAddMarkerClicked(object? sender, EventArgs e)
    {
        NaverMapControl.SelectedPlace = new MapPlace
        {
            Name = "Gyeongbokgung Palace",
            LocalName = "경복궁",
            Address = "161 Sajik-ro, Jongno-gu, Seoul, South Korea",
            City = "Seoul",
            CountryCode = "KR",
            Category = "landmark",
            Location = new GeoCoordinate(37.5796, 126.9770),
            Provider = MapProviderType.Naver,
            ProviderPlaceId = "naver_gyeongbokgung_kr"
        };
    }
}

