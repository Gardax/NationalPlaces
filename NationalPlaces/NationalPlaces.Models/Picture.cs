using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace NationalPlaces.Models
{
    public class Picture
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string PictureUrl { get; set; }

        [Required]
        public User User { get; set; }

    }
}
