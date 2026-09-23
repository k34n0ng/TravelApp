using System;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Microsoft.Maui.Handlers;
using TravelApp.Controls;

namespace TravelApp.Platforms.Android.Handlers
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
            var mapView = new MapView(Context);
            mapView.OnCreate(null);
            mapView.OnResume();
            mapView.GetMapAsync(new MapReadyCallback(OnMapReady));
            return mapView;
        }

        private void OnMapReady(GoogleMap googleMap)
        {
            if (VirtualView?.InitialCenter != null)
            {
                var latLng = new LatLng(VirtualView.InitialCenter.Latitude, VirtualView.InitialCenter.Longitude);
                var zoom = (float)VirtualView.InitialZoom;
                googleMap.MoveCamera(CameraUpdateFactory.NewLatLngZoom(latLng, zoom));
            }
        }

        private static void MapInitialCenter(GoogleMapViewHandler handler, GoogleMapView view)
        {
        }

        private static void MapInitialZoom(GoogleMapViewHandler handler, GoogleMapView view)
        {
        }

        protected override void DisconnectHandler(MapView platformView)
        {
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