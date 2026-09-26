using CoreLocation;
using Google.Maps;
using Microsoft.Maui.Handlers;
using TravelApp.Controls;

namespace TravelApp.Platforms.iOS.Handlers
{
    public class GoogleMapViewHandler : ViewHandler<GoogleMapView, MapView>
    {
        private Marker? _currentMarker;

        public static IPropertyMapper<GoogleMapView, GoogleMapViewHandler> Mapper =
            new PropertyMapper<GoogleMapView, GoogleMapViewHandler>(ViewMapper)
            {
                [nameof(GoogleMapView.InitialCenter)] = MapInitialCenter,
                [nameof(GoogleMapView.InitialZoom)] = MapInitialZoom,
                [nameof(GoogleMapView.SelectedPlace)] = MapSelectedPlace
            };

        public GoogleMapViewHandler() : base(Mapper)
        {
        }

        public GoogleMapViewHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
        {
        }

        protected override MapView CreatePlatformView()
        {
            var mapView = new MapView();
            UpdateCamera(mapView);
            UpdateMarker(mapView);
            return mapView;
        }

        private void UpdateCamera(MapView mapView)
        {
            if (VirtualView?.InitialCenter != null)
            {
                var coord = new CLLocationCoordinate2D(VirtualView.InitialCenter.Latitude, VirtualView.InitialCenter.Longitude);
                var camera = CameraPosition.FromCamera(coord, (float)VirtualView.InitialZoom);
                mapView.Camera = camera;
            }
        }

        private void UpdateMarker(MapView mapView)
        {
            if (_currentMarker != null)
            {
                _currentMarker.Map = null;
                _currentMarker = null;
            }

            var place = VirtualView?.SelectedPlace;
            if (place?.Location != null)
            {
                var coord = new CLLocationCoordinate2D(place.Location.Latitude, place.Location.Longitude);
                var marker = Marker.FromPosition(coord);
                marker.Title = place.Name ?? string.Empty;
                marker.Map = mapView;
                _currentMarker = marker;
            }
        }

        private static void MapInitialCenter(GoogleMapViewHandler handler, GoogleMapView view)
        {
            if (handler.PlatformView != null)
            {
                handler.UpdateCamera(handler.PlatformView);
            }
        }

        private static void MapInitialZoom(GoogleMapViewHandler handler, GoogleMapView view)
        {
            if (handler.PlatformView != null)
            {
                handler.UpdateCamera(handler.PlatformView);
            }
        }

        private static void MapSelectedPlace(GoogleMapViewHandler handler, GoogleMapView view)
        {
            if (handler.PlatformView != null)
            {
                handler.UpdateMarker(handler.PlatformView);
            }
        }

        protected override void DisconnectHandler(MapView platformView)
        {
            if (_currentMarker != null)
            {
                _currentMarker.Map = null;
                _currentMarker = null;
            }
            base.DisconnectHandler(platformView);
        }
    }
}