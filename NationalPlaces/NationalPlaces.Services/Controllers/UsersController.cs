﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using NationalPlaces.Data;
using NationalPlaces.Services.Models;
using NationalPlaces.Models;

namespace NationalPlaces.Services.Controllers
{
    public class UsersController : ApiController
    {
        public const int MinUsernameLength = 4;
        public const int MaxUsernameLength = 30;
        public const int MinNameLength = 6;
        public const int MaxNameLength = 50;

        private const string ValidUsernameCharacters =
           "qwertyuioplkjhgfdsazxcvbnmQWERTYUIOPLKJHGFDSAZXCVBNM1234567890_.";

        private const string ValidNameCharacters =
            "qwertyuioplkjhgfdsazxcvbnmQWERTYUIOPLKJHGFDSAZXCVBNM1234567890_. -";

        private const string SessionKeyChars =
            "qwertyuioplkjhgfdsazxcvbnmQWERTYUIOPLKJHGFDSAZXCVBNM";

        private static readonly Random rand = new Random();

        private const int SessionKeyLength = 50;

        private const int Sha1Length = 40;

        [HttpPost]
        [ActionName("register")] //api/users/register
        public HttpResponseMessage PostRegisterUser(UserModel model)
        {
            try
            {
                var dbContext = new NationalPlacesContext();
                using (dbContext)
                {
                    this.ValidateUsername(model.Username);
                    this.ValidateName(model.Name);
                    this.ValidateAuthCode(model.AuthCode);

                    var usernameToLower = model.Username.ToLower();
                    var user = dbContext.Users.FirstOrDefault(u => u.Username.ToLower() == usernameToLower);

                    if (user != null)
                    {
                        throw new InvalidOperationException("Username is already taken!");
                    }

                    user = new NationalPlaces.Models.User()
                    {
                        Username = usernameToLower,
                        Name = model.Name,
                        AuthCode = model.AuthCode,
                        ProfilePictureUrl = model.ProfilePictureUrl

                    };

                    dbContext.Users.Add(user);
                    dbContext.SaveChanges();

                    user.SessionKey = this.GenerateSessionKey(user.Id);
                    dbContext.SaveChanges();

                    var loggedModel = new LoggedUserModel()
                    {
                        Name = user.Name,
                        SessionKey = user.SessionKey
                    };

                    var response = this.Request.CreateResponse(HttpStatusCode.Created,
                                              loggedModel);
                    return response;
                }
            }
            catch (Exception ex)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.BadRequest,
                                             ex.Message);
                return response;
            }
        }

        [HttpPost]
        [ActionName("login")]  //api/users/login
        public HttpResponseMessage PostLoginUser(UserModel model)
        {
            try
            {
                ValidateUsername(model.Username);
                ValidateAuthCode(model.AuthCode);

                var context = new NationalPlacesContext();
                using (context)
                {
                    var user = context.Users.FirstOrDefault(u => u.Username == model.Username.ToLower()
                        && u.AuthCode == model.AuthCode);

                    if (user == null)
                    {
                        throw new InvalidOperationException("Invalid username or password");
                    }
                    if (user.SessionKey == null)
                    {
                        user.SessionKey = this.GenerateSessionKey(user.Id);
                        context.SaveChanges();
                    }

                    var loggedModel = new LoggedUserModel()
                    {
                        Name = user.Name,
                        SessionKey = user.SessionKey
                    };

                    var response = this.Request.CreateResponse(HttpStatusCode.Created,
                                        loggedModel);
                    return response;
                }
            }
            catch (Exception ex)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.BadRequest,
                                         ex.Message);
                return response;
            }
        }

        [HttpPut]
        [ActionName("logout")]  //api/users/logout/{sessionKey}
        public HttpResponseMessage PutLogoutUser(string sessionKey)
        {
            try
            {
                var context = new NationalPlacesContext();
                using (context)
                {
                    var user = context.Users.FirstOrDefault(u => u.SessionKey == sessionKey);
                    if (user == null)
                    {
                        throw new ArgumentException("Invalid user authentication.");
                    }

                    user.SessionKey = null;
                    context.SaveChanges();

                    var response = this.Request.CreateResponse(HttpStatusCode.OK);
                    return response;
                }
            }
            catch (Exception ex)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                return response;
            }
        }

        [HttpPost]
        [ActionName("addPicture")]
        public HttpResponseMessage PostAddPicture(string sessionKey, string url)
        {
            try
            {
                var contex = new NationalPlacesContext();
                var user = contex.Users.FirstOrDefault(u => u.SessionKey == sessionKey);
                if (user==null)
                {
                    throw  new Exception("You must be logged in to upload picture.");
                }
                user.Pictures.Add(new Picture()
                                      {
                                          PictureUrl = url
                                      });
                contex.SaveChanges();

                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                return response;

            }
            catch (Exception ex)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                return response;
            }
        }

        [HttpPost]
        [ActionName("addProfilePicture")]
        public HttpResponseMessage addProfilePicture(string sessionKey, string url)
        {
            try
            {
                var contex = new NationalPlacesContext();
                var user = contex.Users.FirstOrDefault(u => u.SessionKey == sessionKey);
                if (user == null)
                {
                    throw new Exception("You must be logged in to upload picture.");
                }
                user.ProfilePictureUrl = url;
                contex.SaveChanges();

                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                return response;

            }
            catch (Exception ex)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                return response;
            }
        }

        [HttpGet]
        [ActionName("getPictures")]
        public HttpResponseMessage GetPictures(string sessionKey)
        {
            try
            {
                var contex = new NationalPlacesContext();
                var user = contex.Users.FirstOrDefault(u => u.SessionKey == sessionKey);
                if (user == null)
                {
                    throw new Exception("You must be logged in to get pictures.");
                }

                var pictures = from picture in user.Pictures
                               select picture.PictureUrl;

                var response = this.Request.CreateResponse(HttpStatusCode.OK, pictures);
                return response;

            }
            catch (Exception ex)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                return response;
            }
        }

        private void ValidateAuthCode(string authCode)
        {
            if (authCode == null || authCode.Length != Sha1Length)
            {
                throw new ArgumentOutOfRangeException("Password should be encrypted");
            }
        }

        private void ValidateName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("Name cannot be null");
            }
            else if (name.Length < MinNameLength)
            {
                throw new ArgumentOutOfRangeException(
                    string.Format("Name must be at least {0} characters long",
                    MinNameLength));
            }
            else if (name.Length > MaxNameLength)
            {
                throw new ArgumentOutOfRangeException(
                    string.Format("Name must be less than {0} characters long",
                    MaxNameLength));
            }
            else if (name.Any(ch => !ValidNameCharacters.Contains(ch)))
            {
                throw new ArgumentOutOfRangeException(
                    "Name must contain only Latin letters, digits .,_");
            }
        }

        private void ValidateUsername(string username)
        {
            if (username == null)
            {
                throw new ArgumentNullException("Username cannot be null");
            }
            else if (username.Length < MinUsernameLength)
            {
                throw new ArgumentOutOfRangeException(
                    string.Format("Username must be at least {0} characters long",
                    MinUsernameLength));
            }
            else if (username.Length > MaxUsernameLength)
            {
                throw new ArgumentOutOfRangeException(
                    string.Format("Username must be less than {0} characters long",
                    MaxUsernameLength));
            }
            else if (username.Any(ch => !ValidUsernameCharacters.Contains(ch)))
            {
                throw new ArgumentOutOfRangeException(
                    "Username must contain only Latin letters, digits .,_");
            }

        }

        private string GenerateSessionKey(int userId)
        {
            StringBuilder skeyBuilder = new StringBuilder(SessionKeyLength);
            skeyBuilder.Append(userId);
            while (skeyBuilder.Length < SessionKeyLength)
            {
                var index = rand.Next(SessionKeyChars.Length);
                skeyBuilder.Append(SessionKeyChars[index]);
            }
            return skeyBuilder.ToString();
        }
    }
}
