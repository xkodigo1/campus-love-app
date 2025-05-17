using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using campus_love_app.domain.entities;

namespace campus_love_app.domain.ports
{
    public interface ILocationRepository
    {
        List<Country> GetAllCountries();
        List<Region> GetRegionsByCountryId(int countryId);
        List<City> GetCitiesByRegionId(int regionId);
        int GetOrCreateCountry(string countryName);
        int GetOrCreateRegion(string regionName, int countryId);
        int GetOrCreateCity(string cityName, int regionId);
    }
}