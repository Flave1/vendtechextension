using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vendtechext.Contracts
{
    public class CountryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class CreateCountryDto
    {
        public string Name { get; set; }
    }

    public class CityDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CountryId { get; set; }
    }

    public class CreateCityDto
    {
        public string Name { get; set; }
        public int CountryId { get; set; }
    }

}
