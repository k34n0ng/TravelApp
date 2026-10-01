using CoreLocation;
using Foundation;
using Microsoft.Maui.Handlers;
using NMapsMap;
using TravelApp.Controls;

namespace TravelApp.Platforms.iOS.Handlers
{
    /// <summary>
    /// MAUI handler that renders NaverMapView using the native NAVER Maps iOS SDK.
    /// </summary>
    public class NaverMapViewHandler : ViewHandler<NaverMapView, NMFMapView>
    {
        private NMFMarker? _currentMarker;

        public NaverMapViewHandler() : base(Mapper)
        {
        }

        public NaverMapViewHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
        {
        }

        public static IPropertyMapper<NaverMapView, NaverMapViewHandler> Mapper =
            new PropertyMapper<NaverMapView, NaverMapViewHandler>(ViewMapper)
            {
                [nameof(NaverMapView.InitialCenter)] = MapInitialCenter,
                [nameof(NaverMapView.InitialZoom)] = MapInitialZoom,
                [nameof(NaverMapView.SelectedPlace)] = MapSelectedPlace,
            };

        protected override NMFMapView CreatePlatformView()
        {
            // Initialize NAVER SDK client if not already set
            NMFAuthManager.Shared.Client = new NMFNcpKeyClient(
                NaverKeys.NaverMapsIosApiKey, beta: false);

            var mapView = new NMFMapView();
            UpdateCamera(mapView);
            UpdateMarker(mapView);
            return mapView;
        }

        private void UpdateCamera(NMFMapView mapView)
        {
            if (VirtualView?.InitialCenter == null)
            {
                return;
            }

            var coord = new CLLocationCoordinate2D(
                VirtualView.InitialCenter.Latitude,
                VirtualView.InitialCenter.Longitude);

            mapView.SetCenter(coord, animated: false);
            mapView.ZoomLevel = (nint)(float)VirtualView.InitialZoom;
        }

        private void UpdateMarker(NMFMapView mapView)
        {
            if (_currentMarker != null)
            {
                _currentMarker.Map = null;
                _currentMarker = null;
            }

            var place = VirtualView?.SelectedPlace;
            if (place?.Location != null)
            {
                var coord = new CLLocationCoordinate2D(
                    place.Location.Latitude,
                    place.Location.Longitude);

                var marker = NMFMarker.FromPosition(coord);
                marker.Title = place.Name ?? string.Empty;
                marker.CaptionText = place.LocalName ?? string.Empty;
                marker.Map = mapView;
                _currentMarker = marker;
            }
        }

        private static void MapInitialCenter(NaverMapViewHandler handler, NaverMapView view)
        {
            if (handler.PlatformView != null)
            {
                handler.UpdateCamera(handler.PlatformView);
            }
        }

        private static void MapInitialZoom(NaverMapViewHandler handler, NaverMapView view)
        {
            if (handler.PlatformView != null)
            {
                handler.UpdateCamera(handler.PlatformView);
            }
        }

        private static void MapSelectedPlace(NaverMapViewHandler handler, NaverMapView view)
        {
            if (handler.PlatformView != null)
            {
                handler.UpdateMarker(handler.PlatformView);
            }
        }

        protected override void DisconnectHandler(NMFMapView platformView)
        {
            if (_currentMarker != null)
            {
                _currentMarker.Map = null;
                _currentMarker = null;
            }
            base.DisconnectHandler(platformView);
        }
    }

    /// <summary>
    /// NAVER Maps API key configuration for iOS.
    /// Placeholder value — set the real value in NaverMaps.Local.props or an environment variable.
    /// Get your Client ID from: https://console.map.naver.com/app
    /// </summary>
    internal static class NaverKeys
    {
        public static string NaverMapsIosApiKey
            => Environment.GetEnvironmentVariable("NAVER_MAPS_IOS_API_KEY")
                ?? "placeholder_naver_client_id";
    }
}
