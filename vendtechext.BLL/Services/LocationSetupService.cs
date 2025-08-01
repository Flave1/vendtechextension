using Microsoft.EntityFrameworkCore;
using vendtechext.BLL.Exceptions;
using vendtechext.Contracts;
using vendtechext.DAL.Models;
using vendtechext.Helper;

namespace vendtechext.BLL.Services
{
    public interface ILocationSetupService
    {
        Task<APIResponse> GetAllCountriesAsync();
        Task<APIResponse> GetAllCitiesAsync();
        Task<APIResponse> GetCitiesByCountryAsync(int countryId);
        Task<APIResponse> AddCountryAsync(CreateCountryDto dto);
        Task<APIResponse> AddCityAsync(CreateCityDto dto);
        Task<APIResponse> DeleteCountryAsync(int id);
        Task<APIResponse> DeleteCityAsync(int id);
        Task<IList<SelectItem>> GetCitiesSelect(int countryId);
        Task<IList<SelectItem>> GetCountriesSelect();
    }

    public class LocationSetupService : BaseService, ILocationSetupService
    {
        private readonly DataContext _context;

        public LocationSetupService(DataContext context)
        {
            _context = context;
        }

        public async Task<APIResponse> GetAllCountriesAsync()
        {
            var countries = await _context.Countries
                .Select(c => new CountryDto { Id = c.Id, Name = c.Name })
                .ToListAsync();

            return Response.WithStatus("success").WithType(countries).GenerateResponse();
        }

        public async Task<APIResponse> GetAllCitiesAsync()
        {
            var cities = await _context.Cities
                .Select(c => new CityDto { Id = c.Id, Name = c.Name, CountryId = c.CountryId })
                .ToListAsync();

            return Response.WithStatus("success").WithType(cities).GenerateResponse();
        }

        public async Task<APIResponse> GetCitiesByCountryAsync(int countryId)
        {
            var cities = await _context.Cities
                .Where(c => c.CountryId == countryId)
                .Select(c => new CityDto { Id = c.Id, Name = c.Name, CountryId = c.CountryId })
                .ToListAsync();

            return Response.WithStatus("success").WithType(cities).GenerateResponse();
        }

        public async Task<APIResponse> AddCountryAsync(CreateCountryDto dto)
        {
            var country = new Country { Name = dto.Name };
            await _context.Countries.AddAsync(country);
            await _context.SaveChangesAsync();

            return Response.WithStatus("success")
                           .WithMessage("Country added successfully")
                           .GenerateResponse();
        }

        public async Task<APIResponse> AddCityAsync(CreateCityDto dto)
        {
            if (!await _context.Countries.AnyAsync(c => c.Id == dto.CountryId))
                throw new BadRequestException("Invalid Country ID");

            var city = new City { Name = dto.Name, CountryId = dto.CountryId };
            await _context.Cities.AddAsync(city);
            await _context.SaveChangesAsync();

            return Response.WithStatus("success")
                           .WithMessage("City added successfully")
                           .GenerateResponse();
        }

        public async Task<APIResponse> DeleteCountryAsync(int id)
        {
            var country = await _context.Countries.FindAsync(id);
            if (country == null)
                throw new BadRequestException("Country not found");

            _context.Countries.Remove(country);
            await _context.SaveChangesAsync();

            return Response.WithStatus("success")
                           .WithMessage("Country deleted successfully")
                           .GenerateResponse();
        }

        public async Task<APIResponse> DeleteCityAsync(int id)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city == null)
                throw new BadRequestException("City not found");

            _context.Cities.Remove(city);
            await _context.SaveChangesAsync();

            return Response.WithStatus("success")
                           .WithMessage("City deleted successfully")
                           .GenerateResponse();
        }

        public async Task<IList<SelectItem>> GetCountriesSelect()
        {
            var query = _context.Countries.Where(d => d.Deleted == false).OrderBy(s => s.Name);

            var list = await query.Select(a => new SelectItem
            {
                Text = a.Name,
                Value = a.Id.ToString()
            }).ToListAsync();

            return list;
        }

        public async Task<IList<SelectItem>> GetCitiesSelect(int countryId)
        {
            var query = _context.Cities.Where(d => d.Deleted == false && d.CountryId == countryId).OrderBy(s => s.Name);

            var list = await query.Select(a => new SelectItem
            {
                Text = a.Name,
                Value = a.Id.ToString()
            }).ToListAsync();

            return list;
        }
    }

}
