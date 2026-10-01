using Android.App;
using Android.OS;
using Android.Widget;
using Android.Content;
using Com.Naver.Maps.Geometry;
using Com.Naver.Maps.Map;
using Com.Naver.Maps.Map.Overlay;
using Microsoft.Maui.Handlers;
using TravelApp.Controls;

namespace TravelApp.Platforms.Android.Handlers
{
    /// <summary>
    /// MAUI handler that renders NaverMapView using the native NAVER Maps Android SDK.
    /// </summary>
    public class NaverMapViewHandler : ViewHandler<NaverMapView, MapView>
    {
        private NaverMap? _naverMap;
        private Marker? _currentMarker;

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

        protected override MapView CreatePlatformView()
{
    var clientId = NaverKeys.GetClientId(Context);

    var ncpKeyClient =
        new NaverMapSdk.NcpKeyClient(clientId);

    NaverMapSdk
        .GetInstance(Context)
        .SetClient(ncpKeyClient);

    var mapView = new MapView(Context);
    mapView.GetMapAsync(new MapReadyCallback(OnMapReady));

    return mapView;
}

        private void OnMapReady(NaverMap naverMap)
        {
            _naverMap = naverMap;
            UpdateCamera();
            UpdateMarker();
        }

        private void UpdateCamera()
        {
            if (_naverMap == null || VirtualView == null)
            {
                return;
            }

            var center = new LatLng(
                VirtualView.InitialCenter.Latitude,
                VirtualView.InitialCenter.Longitude);

            var zoom = (double)VirtualView.InitialZoom;

            _naverMap.MoveCamera(CameraUpdate.ScrollAndZoomTo(center, zoom));
        }

        private void UpdateMarker()
        {
            if (_naverMap == null)
            {
                return;
            }

            if (_currentMarker != null)
            {
                _currentMarker.Map = null;
                _currentMarker = null;
            }

            var place = VirtualView?.SelectedPlace;
            if (place?.Location != null)
            {
                var latLng = new LatLng(
                    place.Location.Latitude,
                    place.Location.Longitude);

                _currentMarker = new Marker(latLng);
				_currentMarker.CaptionText = place.Name ?? string.Empty;
                _currentMarker.Map = _naverMap;
            }
        }

        private static void MapInitialCenter(NaverMapViewHandler handler, NaverMapView view)
        {
            handler.UpdateCamera();
        }

        private static void MapInitialZoom(NaverMapViewHandler handler, NaverMapView view)
        {
            handler.UpdateCamera();
        }

        private static void MapSelectedPlace(NaverMapViewHandler handler, NaverMapView view)
        {
            handler.UpdateMarker();
        }

        protected override void DisconnectHandler(MapView platformView)
        {
            if (_currentMarker != null)
            {
                _currentMarker.Map = null;
                _currentMarker = null;
            }
            _naverMap = null;
            platformView.OnDestroy();
            base.DisconnectHandler(platformView);
        }

        private class MapReadyCallback : Java.Lang.Object, IOnMapReadyCallback
        {
            private readonly Action<NaverMap> _onMapReady;

            public MapReadyCallback(Action<NaverMap> onMapReady)
            {
                _onMapReady = onMapReady;
            }

            public void OnMapReady(NaverMap naverMap)
            {
                _onMapReady?.Invoke(naverMap);
            }
        }
    }

    /// <summary>
    /// NAVER Maps API key configuration.
    /// Placeholder value — set the real value in NaverMaps.Local.props or an environment variable.
    /// Get your Client ID from: https://console.map.naver.com/app
    /// </summary>
    internal static class NaverKeys
{
    private const string MetaDataName =
        "com.merbyte.travelapp.NAVER_MAPS_CLIENT_ID";

    public static string GetClientId(Context context)
    {
        var applicationInfo = context.PackageManager?.GetApplicationInfo(
    context.PackageName!,
    global::Android.Content.PM.PackageInfoFlags.MetaData);

        var clientId = applicationInfo?.MetaData?.GetString(MetaDataName);

        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new InvalidOperationException(
                "NAVER Maps Client ID is not configured.");
        }

        return clientId;
    }
}
}
