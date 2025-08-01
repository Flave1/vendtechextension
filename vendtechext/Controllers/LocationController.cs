using Microsoft.AspNetCore.Mvc;
using vendtechext.BLL.Services;
using vendtechext.Contracts;
using vendtechext.Controllers.Base;
using vendtechext.DAL.Migrations;

namespace vendtechext.Controllers
{
    [ApiController]
    [Route("location-setup/v1")]
    public class LocationSetupController : BaseController
    {
        private readonly ILocationSetupService _locationService;

        public LocationSetupController(ILogger<BaseController> logger, ILocationSetupService locationService) : base(logger)
        {
            _locationService = locationService;
        }


        [HttpGet("countries")]
        public async Task<IActionResult> GetAllCountries()
        {
            var response = await _locationService.GetAllCountriesAsync();
            return Ok(response);
        }

        [HttpGet("cities")]
        public async Task<IActionResult> GetAllCities()
        {
            var response = await _locationService.GetAllCitiesAsync();
            return Ok(response);
        }

        [HttpGet("cities/by-country/{countryId}")]
        public async Task<IActionResult> GetCitiesByCountry(int countryId)
        {
            var response = await _locationService.GetCitiesByCountryAsync(countryId);
            return Ok(response);
        }

        [HttpPost("countries")]
        public async Task<IActionResult> AddCountry([FromBody] CreateCountryDto dto)
        {
            var response = await _locationService.AddCountryAsync(dto);
            return Ok(response);
        }

        [HttpPost("cities")]
        public async Task<IActionResult> AddCity([FromBody] CreateCityDto dto)
        {
            var response = await _locationService.AddCityAsync(dto);
            return Ok(response);
        }

        [HttpDelete("countries/{id}")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var response = await _locationService.DeleteCountryAsync(id);
            return Ok(response);
        }

        [HttpDelete("cities/{id}")]
        public async Task<IActionResult> DeleteCity(int id)
        {
            var response = await _locationService.DeleteCityAsync(id);
            return Ok(response);
        }

        [HttpGet("get-countries-select")]
        public async Task<IActionResult> GetCountriesSelect()
        {
            var result = await _locationService.GetCountriesSelect();
            return Ok(result);
        }

        [HttpGet("get-cities-select/{countryId}")]
        public async Task<IActionResult> GetCountriesSelect(int countryId)
        {
            var result = await _locationService.GetCitiesSelect(countryId);
            return Ok(result);
        }
    }
   

}
