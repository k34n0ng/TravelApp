using Microsoft.Extensions.Logging;
using TravelApp.Controls;
using TravelApp.Services;

namespace TravelApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .ConfigureMauiHandlers(handlers =>
                {
#if ANDROID
                    handlers.AddHandler<GoogleMapView, Platforms.Android.Handlers.GoogleMapViewHandler>();
                    handlers.AddHandler<NaverMapView, Platforms.Android.Handlers.NaverMapViewHandler>();
#elif IOS
                    handlers.AddHandler<GoogleMapView, Platforms.iOS.Handlers.GoogleMapViewHandler>();
#endif
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

			// Register map abstraction services
			builder.Services.AddSingleton<IMapProvider, GoogleMapProvider>();
			builder.Services.AddSingleton<IMapProviderRegistry, MapProviderRegistry>();
			builder.Services.AddSingleton<IMapProviderResolver, MapProviderResolver>();

			// Register Pages
			builder.Services.AddTransient<MainPage>();

            return builder.Build();
        }
    }
}
