using Microsoft.Maui.Controls;
using TravelApp.Models;
using TravelApp.Services;

namespace TravelApp.Controls;

public class MapHostView : ContentView
{
    private readonly IMapProviderResolver _resolver;

    public static readonly BindableProperty PlaceProperty =
        BindableProperty.Create(
            nameof(Place),
            typeof(MapPlace),
            typeof(MapHostView),
            null,
            BindingMode.OneWay,
            propertyChanged: OnPlaceChanged);

    public MapPlace? Place
    {
        get => (MapPlace?)GetValue(PlaceProperty);
        set => SetValue(PlaceProperty, value);
    }

    public MapHostView()
    {
        _resolver = Application.Current?.Handler?.MauiContext?.Services
            .GetService<IMapProviderResolver>()
            ?? throw new InvalidOperationException(
                "IMapProviderResolver not registered.");
    }

    private static void OnPlaceChanged(
        BindableObject bindable,
        object? oldValue,
        object? newValue)
    {
        var host = (MapHostView)bindable;
        host.UpdateMapControl((MapPlace?)newValue);
    }

    private void UpdateMapControl(MapPlace? place)
    {
        if (place == null)
        {
            Content = null;
            return;
        }

        try
        {
            View mapControl = place.Provider switch
            {
                MapProviderType.Google => new GoogleMapView(),
                MapProviderType.Naver => new NaverMapView(),
                _ => throw new NotSupportedException(
                    $"Provider not supported by MapHostView: {place.Provider}")
            };

            if (mapControl is GoogleMapView googleMap)
            {
                googleMap.InitialCenter = place.Location;
                googleMap.SelectedPlace = place;
            }
            else if (mapControl is NaverMapView naverMap)
            {
                naverMap.InitialCenter = place.Location;
                naverMap.SelectedPlace = place;
            }

            Content = mapControl;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"MapHostView failed to create map for provider '{place.Provider}': {ex}");

            Content = null;
        }
    }
}