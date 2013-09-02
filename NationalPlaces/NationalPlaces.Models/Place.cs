using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NationalPlaces.Models
{
    public class Place
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public Town Town { get; set; }

        public string PictureUrl { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public virtual ICollection<User> Users { get; set; }
 
        public Place()
        {
            this.Users=new HashSet<User>();
        }
    }
}
