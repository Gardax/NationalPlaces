using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NationalPlaces.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string AuthCode { get; set; }

        public string SessionKey { get; set; }

        public string ProfilePictureUrl { get; set; }

        public virtual ICollection<Picture> Pictures { get; set; }

        public virtual ICollection<Place> Places { get; set; } 

        public User()
        {
            this.Pictures = new HashSet<Picture>();
            this.Places=new HashSet<Place>();
        }
    }
}
