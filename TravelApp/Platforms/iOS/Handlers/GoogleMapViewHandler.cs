using CoreLocation;
using Google.Maps;
using Microsoft.Maui.Handlers;
using TravelApp.Controls;

namespace TravelApp.Platforms.iOS.Handlers
{
    public class GoogleMapViewHandler : ViewHandler<GoogleMapView, MapView>
    {
        public static IPropertyMapper<GoogleMapView, GoogleMapViewHandler> Mapper =
            new PropertyMapper<GoogleMapView, GoogleMapViewHandler>(ViewMapper)
            {
                [nameof(GoogleMapView.InitialCenter)] = MapInitialCenter,
                [nameof(GoogleMapView.InitialZoom)] = MapInitialZoom
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
    }
}