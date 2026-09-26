using Microsoft.Maui.Controls;
using TravelApp.Models;

namespace TravelApp.Controls
{
    public class GoogleMapView : View
    {
        public static readonly BindableProperty InitialCenterProperty =
            BindableProperty.Create(
                nameof(InitialCenter),
                typeof(GeoCoordinate),
                typeof(GoogleMapView),
                default(GeoCoordinate));

        public static readonly BindableProperty InitialZoomProperty =
            BindableProperty.Create(
                nameof(InitialZoom),
                typeof(double),
                typeof(GoogleMapView),
                10.0);

        public static readonly BindableProperty SelectedPlaceProperty =
            BindableProperty.Create(
                nameof(SelectedPlace),
                typeof(MapPlace),
                typeof(GoogleMapView),
                default(MapPlace));

        public GeoCoordinate? InitialCenter
        {
            get => (GeoCoordinate?)GetValue(InitialCenterProperty);
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