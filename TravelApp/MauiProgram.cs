using Microsoft.Extensions.Logging;
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
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

			// Register map abstraction services
			builder.Services.AddSingleton<IMapProviderRegistry, MapProviderRegistry>();
			builder.Services.AddSingleton<IMapProviderResolver, MapProviderResolver>();
            return builder.Build();
        }
    }
}
