using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NationalPlaces.Models
{
    public class Town
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<Place> Places { get; set; }

        public Town()
        {
            this.Places = new HashSet<Place>();
        }
    }
}
