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
                                             PictureUrl = place.PictureUrl
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
        public HttpResponseMessage CheckIn(string sessionKey, int placeId)
        {
            try
            {
                var context = new NationalPlacesContext();
                var user = context.Users.FirstOrDefault(u => u.SessionKey == sessionKey);
                if(user==null)
                {
                    throw new Exception("You must be logged in to check in.");
                }

                var place = context.Places.FirstOrDefault(p => p.Id == placeId);
                place.Users.Add(user);
                context.SaveChanges();

                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                return response;
            }
            catch (Exception ex)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                return response;
            }
        }
    }
}
