using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AuthorizedServer.Models;
using MongoDB.Driver;
using WH = AuthorizedServer.Helper.MongoHelper;
using Microsoft.AspNetCore.Identity;
using AuthorizedServer.Helper;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using AuthorizedServer.Repositories;
using Microsoft.Extensions.Options;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AuthorizedServer.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        //some config in the appsettings.json
        private IOptions<Audience> _settings;
        //repository to handler the sqlite database
        private IRTokenRepository _repo;

        public IMongoDatabase _db;

        AuthHelper authHelper = new AuthHelper();

        public AuthController(IOptions<Audience> settings, IRTokenRepository repo)
        {
            _db = WH._client.GetDatabase("Authentication");
            this._settings = settings;
            this._repo = repo;
        }


        [HttpPost("register")]
        public async Task<bool> Register([FromBody]RegisterModel data)
        {
            var authCollection = _db.GetCollection<RegisterModel>("Authentication");
            try
            {
                PasswordHasher<RegisterModel> passHasher = new PasswordHasher<RegisterModel>();
                data.Password = passHasher.HashPassword(data, data.Password);


                PasswordHasher<SmsVerificationModel> hasher = new PasswordHasher<SmsVerificationModel>();
                SmsVerificationModel smsModel = new SmsVerificationModel();
                smsModel.PhoneNumber = data.PhoneNumber;
                //Generate OTP Code
                Random codeGenerator = new Random();
                string OTP = codeGenerator.Next(0, 1000000).ToString("D6");
                data.VerificationCode = hasher.HashPassword(smsModel, OTP);

                data.OTPExp = DateTime.Now.AddMinutes(4);
                data.Status = "Registered";
                await authCollection.InsertOneAsync(data);

                SMSHelper.SendSMS("+91" + data.PhoneNumber, OTP);

                return true;
            }
            catch
            {
                return false;
            }
        }

        [HttpPost("login")]
        public ActionResult Login([FromBody]LoginModel user)
        {
            if (user == null)
            {
                return Json(new ResponseData
                {
                    Code = "901",
                    Message = "null of parameters",
                    Data = null
                });
            }
            else
            {
                try
                {
                    MongoHelper helper = new MongoHelper();
                    var filter = Builders<BsonDocument>.Filter.Eq("PhoneNumber", user.UserName);
                    var verifyUser = BsonSerializer.Deserialize<RegisterModel>(helper.GetSingleObject(filter, "Authentication", "Authentication").Result);

                    PasswordHasher<LoginModel> loginHasher = new PasswordHasher<LoginModel>();
                    LoginModel loginModel = new LoginModel();
                    loginModel.UserName = user.UserName;
                    if (loginHasher.VerifyHashedPassword(loginModel, verifyUser.Password, user.Password).ToString() == "Success")
                    {
                        Parameters parameters = new Parameters();
                        parameters.username = user.UserName;
                        return Ok(Json(authHelper.DoPassword(parameters, _repo, _settings)));
                    }
                    else
                    {
                        return Ok(new ResponseData
                        {
                            Code = "902",
                            Message = "invalid user infomation",
                            Data = null
                        });
                    }

                }
                catch (Exception ex)
                {
                    return Ok("Failed");
                }
            }
        }

        [HttpPost("register/smsverification")]
        public IActionResult SmsVerification([FromBody]SmsVerificationModel data)
        {
            try
            {
                MongoHelper helper = new MongoHelper();
                var filter = Builders<BsonDocument>.Filter.Eq("PhoneNumber", data.PhoneNumber);                
                var verifyUser = BsonSerializer.Deserialize<RegisterModel>(helper.GetSingleObject(filter, "Authentication", "Authentication").Result);

                PasswordHasher<SmsVerificationModel> hasher = new PasswordHasher<SmsVerificationModel>();
                SmsVerificationModel smsModel = new SmsVerificationModel();
                smsModel.PhoneNumber = data.PhoneNumber;

                if (verifyUser.OTPExp < DateTime.Now)
                {            
                    if (hasher.VerifyHashedPassword(smsModel, verifyUser.VerificationCode, data.VerificationCode).ToString() == "Success")
                    {
                        var update = Builders<BsonDocument>.Update.Set("Status", "Verified");
                        var result = helper.UpdateSingleObject(filter, "Authentication", "Authentication", update).Result;

                        Parameters parameters = new Parameters();
                        parameters.username = data.PhoneNumber;
                        return Ok(Json(authHelper.DoPassword(parameters, _repo, _settings)));
                    }
                    else
                    {
                        return BadRequest("OTP Invalid");
                    }
                }
                else{
                    return BadRequest("OTP Expired");
                }
            }
            catch (Exception ex)
            {
                return Ok("Failed");
            }
        }
    }
}
