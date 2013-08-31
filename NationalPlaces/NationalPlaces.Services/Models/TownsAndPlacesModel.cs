using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NationalPlaces.Services.Models
{
    public class TownsAndPlacesModel
    {
        public string TownName { get; set; }

        public IEnumerable<NationalPlaceModel> Places { get; set; }
 
    }
}