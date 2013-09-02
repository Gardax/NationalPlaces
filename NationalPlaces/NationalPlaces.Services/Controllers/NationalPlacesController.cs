using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NationalPlaces.Data;
using NationalPlaces.Models;
using NationalPlaces.Services.Models;

namespace NationalPlaces.Services.Controllers
{
    public class NationalPlacesController : ApiController
    {
        [HttpPost]
        [ActionName("add")]
        public HttpResponseMessage PostPlace(NationalPlaceModel place)
        {
            try
            {
                var context = new NationalPlacesContext();
                using(context)
                {
                    if(place.Name==null || place.Name=="")
                    {
                        throw  new ArgumentNullException("Inavalid place name");
                    }

                    if(place.Description==null || place.Description=="")
                    {
                        throw new ArgumentNullException("Invalid description");
                    }

                    var town=context.Towns.FirstOrDefault(t => t.Name == place.Town);
                    if (town==null)
                    {
                        town=context.Towns.Add(new Town()
                                              {
                                                  Name = place.Town,
                                              });
                        context.SaveChanges();
                    }

                    Place newPlace = new Place()
                                         {
                                             Name = place.Name,
                                             Description = place.Description,
                                             Town = town,
                                             PictureUrl = place.PictureUrl,
                                             Latitude=place.Latitude,
                                             Longitude = place.Longitude
                                         };

                    context.Places.Add(newPlace);
                    context.SaveChanges();
                    var response = this.Request.CreateResponse(HttpStatusCode.Created);
                    return response;
                }
            }
            catch(Exception ex)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                return response;
            }
        }

        [HttpPost]
        [ActionName("getPlacesByTownId")]
        public HttpResponseMessage GetPlacesByTownId(int townId)
        {
            try
            {
                var context = new NationalPlacesContext();
                var allPlaces = from place in context.Towns.FirstOrDefault(t => t.Id == townId).Places
                                select new NationalPlaceModel()
                                           {
                                               Id = place.Id,
                                               Name=place.Name,
                                               PictureUrl = place.PictureUrl
                                           };
                var response = this.Request.CreateResponse(HttpStatusCode.OK, allPlaces);
                return response;
            }
            catch(Exception ex)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                return response;
            }
        }

        [HttpPost]
        [ActionName("getPlaceDetails")]
        public HttpResponseMessage GetPlaceDetails(int placeId)
        {
            try
            {
                var context = new NationalPlacesContext();
                var place = context.Places.FirstOrDefault(p => p.Id == placeId);
                var placeDetails = new NationalPlaceModel()
                                       {
                                           Id = place.Id,
                                           Name = place.Name,
                                           Description = place.Description,
                                           PictureUrl = place.PictureUrl,
                                           Town = place.Town.Name
                                       };

                var response = this.Request.CreateResponse(HttpStatusCode.OK, placeDetails);
                return response;
            }
            catch (Exception ex)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                return response;
            }
        }

        [HttpPost]
        [ActionName("checkIn")]
        public HttpResponseMessage CheckIn(string sessionKey, double latitude, double longitude)
        {
            try
            {
                var context = new NationalPlacesContext();
                var user = context.Users.FirstOrDefault(u => u.SessionKey == sessionKey);
                if (user == null)
                {
                    throw new Exception("You must be logged in to check in.");
                }
                int placeId = GetNearbyPlace(latitude, longitude);
                if (placeId != 0)
                {
                    var place = context.Places.FirstOrDefault(p => p.Id == placeId);
                    place.Users.Add(user);
                    context.SaveChanges();
                    var response = this.Request.CreateResponse(HttpStatusCode.OK,
                                                               "You are checked in from: " + place.Name);
                    return response;
                }
                else
                {
                    var response = this.Request.CreateResponse(HttpStatusCode.BadRequest,
                                                               "You are not close to any place of the application!");
                    return response;
                }

            }
            catch (Exception ex)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
                return response;
            }
        }

        //[HttpGet]
        //[ActionName("nearbyPlaces")]
        public int GetNearbyPlace(double latitude, double longitude)
        {

            var context = new NationalPlacesContext();
            NationalPlaceModel nearbyPlace = new NationalPlaceModel();
            double minDistance = double.MaxValue;
            foreach (var place in context.Places)
            {
                double distance = CheckDistance(place.Latitude, place.Longitude, latitude, longitude);

                if (minDistance > distance)
                {
                    nearbyPlace.Id = place.Id;
                    minDistance = distance;
                }
            }
            //var response = this.Request.CreateResponse(HttpStatusCode.OK, nearbyPlace.Id);
            return nearbyPlace.Id;

        }

        private double CheckDistance(double placeLatitude, double placeLongitude, double userLatitude, double userLongitude)
        {

            double dLat1InRad = placeLatitude * (Math.PI / 180.0);
            double dLong1InRad = placeLongitude * (Math.PI / 180.0);
            double dLat2InRad = userLatitude * (Math.PI / 180.0);
            double dLong2InRad = userLongitude * (Math.PI / 180.0);

            double dLongitude = dLong2InRad - dLong1InRad;
            double dLatitude = dLat2InRad - dLat1InRad;

            // Intermediate result a.
            double a = Math.Pow(Math.Sin(dLatitude / 2.0), 2.0) +
                       Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) *
                       Math.Pow(Math.Sin(dLongitude / 2.0), 2.0);

            // Intermediate result c (great circle distance in Radians).
            double c = 2.0 * Math.Asin(Math.Sqrt(a));

            // Distance.
            // const Double kEarthRadiusMiles = 3956.0;
            const Double kEarthRadiusKms = 6376.5;
            double dDistance = kEarthRadiusKms * c;
            return dDistance;
        }
    }
}
