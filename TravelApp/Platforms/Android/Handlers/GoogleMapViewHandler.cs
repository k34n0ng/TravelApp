using System;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Microsoft.Maui.Handlers;
using TravelApp.Controls;

namespace TravelApp.Platforms.Android.Handlers
{
    public class GoogleMapViewHandler : ViewHandler<GoogleMapView, MapView>
    {
        private GoogleMap? _googleMap;
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
            var mapView = new MapView(Context);
            mapView.OnCreate(null);
            mapView.OnResume();
            mapView.GetMapAsync(new MapReadyCallback(OnMapReady));
            return mapView;
        }

        private void OnMapReady(GoogleMap googleMap)
        {
            _googleMap = googleMap;
            UpdateCamera();
            UpdateMarker();
        }

        private void UpdateCamera()
        {
            if (_googleMap != null && VirtualView?.InitialCenter != null)
            {
                var latLng = new LatLng(
                    VirtualView.InitialCenter.Latitude,
                    VirtualView.InitialCenter.Longitude);

                var zoom = (float)VirtualView.InitialZoom;

                _googleMap.MoveCamera(
                    CameraUpdateFactory.NewLatLngZoom(latLng, zoom));
            }
        }

        private void UpdateMarker()
        {
            if (_googleMap == null)
            {
                return;
            }

            if (_currentMarker != null)
            {
                _currentMarker.Remove();
                _currentMarker = null;
            }

            var place = VirtualView?.SelectedPlace;
            if (place?.Location != null)
            {
                var latLng = new LatLng(
                    place.Location.Latitude,
                    place.Location.Longitude);

                var markerOptions = new MarkerOptions()
                    .SetPosition(latLng)
                    .SetTitle(place.Name ?? string.Empty);

                _currentMarker = _googleMap.AddMarker(markerOptions);
            }
        }

        private static void MapInitialCenter(GoogleMapViewHandler handler, GoogleMapView view)
        {
            handler.UpdateCamera();
        }

        private static void MapInitialZoom(GoogleMapViewHandler handler, GoogleMapView view)
        {
            handler.UpdateCamera();
        }

        private static void MapSelectedPlace(GoogleMapViewHandler handler, GoogleMapView view)
        {
            handler.UpdateMarker();
        }

        protected override void DisconnectHandler(MapView platformView)
        {
            if (_currentMarker != null)
            {
                _currentMarker.Remove();
                _currentMarker = null;
            }
            _googleMap = null;
            platformView.OnDestroy();
            base.DisconnectHandler(platformView);
        }

        private class MapReadyCallback : Java.Lang.Object, IOnMapReadyCallback
        {
            private readonly Action<GoogleMap> _onMapReady;

            public MapReadyCallback(Action<GoogleMap> onMapReady)
            {
                _onMapReady = onMapReady;
            }

            public void OnMapReady(GoogleMap googleMap)
            {
                _onMapReady?.Invoke(googleMap);
            }
        }
    }
}