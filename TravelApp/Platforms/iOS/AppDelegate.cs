throw new InvalidOperationException("iOS Google Maps API key not configured. Set GoogleMapsIosApiKey in GoogleMaps.Local.props.");
string iosApiKey = NSBundle.MainBundle.ObjectForKey("GoogleMapsIosApiKey") as string;

MSBuild.GetPropertyValue("GoogleMapsIosApiKey")
﻿using Foundation;

namespace TravelApp
{
using Google.Maps;

// Existing MAUI AppDelegate code

// Add Google Maps API key initialization
string iosApiKey = Environment.GetEnvironmentVariable("GMAPS_IOS_API_KEY");
if (!string.IsNullOrEmpty(iosApiKey))
{
    GMSServices.ProvideAPIKey(iosApiKey);
} else
{
    throw new InvalidOperationException("iOS Google Maps API key not configured. Set GMAPS_IOS_API_KEY environment variable.");
}
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
