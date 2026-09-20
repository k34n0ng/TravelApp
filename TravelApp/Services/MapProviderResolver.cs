using System;
using TravelApp.Models;
using TravelApp.Services;

namespace TravelApp.Services
{
    public class MapProviderResolver : IMapProviderResolver
    {
        private readonly IMapProviderRegistry _registry;

        public MapProviderResolver(IMapProviderRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public IMapProvider Resolve(string? countryCode)
        {
            // Normalize country code
            var normalized = countryCode?.Trim().ToUpperInvariant();

            // Apply selection policy
            MapProviderType providerType;
            switch (normalized)
            {
                case "CN":
                    providerType = MapProviderType.AMap;
                    break;
                case "KR":
                    providerType = MapProviderType.Naver;
                    break;
                default:
                    providerType = MapProviderType.Google;
                    break;
            }

            // Retrieve provider from registry
            return _registry.Get(providerType);
        }
    }
}