using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NationalPlaces.Services.Models
{
    public class UserModel
    {
        public string Username { get; set; }

        public string Name { get; set; }

        public string AuthCode { get; set; }

        public string SessionKey { get; set; }

        public string ProfilePictureUrl { get; set; }

    }
}