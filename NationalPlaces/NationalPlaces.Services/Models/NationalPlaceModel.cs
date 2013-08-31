using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NationalPlaces.Services.Models
{
    public class NationalPlaceModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Town { get; set; }

        public string PictureUrl { get; set; }
    }
}