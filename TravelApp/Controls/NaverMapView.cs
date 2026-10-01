using Microsoft.Maui.Controls;
using TravelApp.Models;

namespace TravelApp.Controls
{
    /// <summary>
    /// MAUI map view control for NAVER Maps.
    /// Mirrors the GoogleMapView pattern to remain consistent with the existing architecture.
    /// </summary>
    public class NaverMapView : View
    {
        public static readonly BindableProperty InitialCenterProperty =
            BindableProperty.Create(
                nameof(InitialCenter),
                typeof(GeoCoordinate),
                typeof(NaverMapView),
                new GeoCoordinate(37.5796, 126.9770));

        public static readonly BindableProperty InitialZoomProperty =
            BindableProperty.Create(
                nameof(InitialZoom),
                typeof(double),
                typeof(NaverMapView),
                10.0);

        public static readonly BindableProperty SelectedPlaceProperty =
            BindableProperty.Create(
                nameof(SelectedPlace),
                typeof(MapPlace),
                typeof(NaverMapView),
                default(MapPlace));

        public GeoCoordinate InitialCenter
        {
            get => (GeoCoordinate)GetValue(InitialCenterProperty);
            set => SetValue(InitialCenterProperty, value);
        }

        public double InitialZoom
        {
            get => (double)GetValue(InitialZoomProperty);
            set => SetValue(InitialZoomProperty, value);
        }

        public MapPlace? SelectedPlace
        {
            get => (MapPlace?)GetValue(SelectedPlaceProperty);
            set => SetValue(SelectedPlaceProperty, value);
        }
    }
}
