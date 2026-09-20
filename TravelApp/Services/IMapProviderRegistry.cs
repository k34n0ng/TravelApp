using System.Collections.Generic;
using TravelApp.Models;

namespace TravelApp.Services
{
    public interface IMapProviderRegistry
    {
        IMapProvider Get(MapProviderType providerType);
        IReadOnlyList<IMapProvider> GetAll();
    }
}