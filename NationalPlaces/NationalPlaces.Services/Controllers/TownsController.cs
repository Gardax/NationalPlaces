using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NationalPlaces.Data;
using NationalPlaces.Services.Models;

namespace NationalPlaces.Services.Controllers
{
    public class TownsController : ApiController
    {
        [HttpGet]
        [ActionName("getAll")]
        public HttpResponseMessage GetAll()
        {
            try
            {
                var context = new NationalPlacesContext();
                var allTowns = from town in context.Towns
                               select new TownModel()
                                          {
                                              Id = town.Id,
                                              Name = town.Name,
                                              CountOfPlaces = town.Places.Count
                                          };
                var response = this.Request.CreateResponse(HttpStatusCode.OK, allTowns);
                return response;
            }
            catch(Exception ex)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.BadRequest,
                                             ex.Message);
                return response;
            }
        }

        [HttpGet]
        [ActionName("getTownsAndPlaces")]
        public HttpResponseMessage GetTownsAndPlaces()
        {
            try
            {
                var context = new NationalPlacesContext();
                var allTowns = from town in context.Towns
                               select new TownsAndPlacesModel()
                               {
                                   TownName = town.Name,
                                   Places = from place in town.Places
                                            select new NationalPlaceModel()
                                                       {
                                                           Id = place.Id,
                                                           Name = place.Name,
                                                           PictureUrl = place.PictureUrl
                                                       }
                               };
                var response = this.Request.CreateResponse(HttpStatusCode.OK, allTowns);
                return response;
            }
            catch (Exception ex)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.BadRequest,
                                             ex.Message);
                return response;
            }
        }
    }
}
