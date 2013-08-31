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
                                                  Name = place.Name,
                                              });
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
    }
}
