using System;
using System.Collections.Generic;
using System.Linq;
using TravelApp.Models;

namespace TravelApp.Services
{
    public class MapProviderRegistry : IMapProviderRegistry
    {
        private readonly Dictionary<MapProviderType, IMapProvider> _providers;

        public MapProviderRegistry(IEnumerable<IMapProvider> providers)
		{
			_providers = providers.ToDictionary(
				p => p.ProviderType,
				p => p);
		}

        public IMapProvider Get(MapProviderType providerType)
        {
            if (_providers.TryGetValue(providerType, out var provider))
            {
                return provider;
            }
            throw new InvalidOperationException(
                $"No map provider registered for type: {providerType}");
        }

        public IReadOnlyList<IMapProvider> GetAll()
        {
            return _providers.Values.ToList();
        }
	
    }
}