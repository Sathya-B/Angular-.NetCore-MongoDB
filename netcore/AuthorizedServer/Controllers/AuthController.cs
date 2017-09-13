using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AuthorizedServer.Models;
using MongoDB.Driver;
using MH = AuthorizedServer.Helper.MongoHelper;
using Microsoft.AspNetCore.Identity;
using AuthorizedServer.Helper;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using AuthorizedServer.Repositories;
using Microsoft.Extensions.Options;
using AuthorizedServer.Logger;
using Microsoft.AspNetCore.Authorization;

namespace AuthorizedServer.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        public MongoHelper helper = new MongoHelper();
        public AuthHelper authHelper = new AuthHelper();
        private IOptions<Audience> _settings;
        private IRTokenRepository _repo;
        public IMongoDatabase _db = MH._client.GetDatabase("Authentication");
        public PasswordHasher<SmsVerificationModel> smsHasher = new PasswordHasher<SmsVerificationModel>();
        public PasswordHasher<RegisterModel> registerHasher = new PasswordHasher<RegisterModel>();
        public PasswordHasher<LoginModel> loginHasher = new PasswordHasher<LoginModel>();

        public AuthController(IOptions<Audience> settings, IRTokenRepository repo)
        {
            this._settings = settings;
            this._repo = repo;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody]RegisterModel data)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("PhoneNumber", data.PhoneNumber);
                var verifyUser = helper.GetSingleObject(filter, "Authentication", "Authentication").Result;
                if (verifyUser == null)
                {
                    data.Password = registerHasher.HashPassword(data, data.Password);
                    SmsVerificationModel smsModel = new SmsVerificationModel();
                    smsModel.PhoneNumber = data.PhoneNumber;
                    Random codeGenerator = new Random();
                    string OTP = codeGenerator.Next(0, 1000000).ToString("D6");
                    data.VerificationCode = smsHasher.HashPassword(smsModel, OTP);
                    data.OTPExp = DateTime.UtcNow.AddMinutes(4);
                    data.Status = "Registered";
                    var authCollection = _db.GetCollection<RegisterModel>("Authentication");
                    await authCollection.InsertOneAsync(data);
                    SMSHelper.SendSMS(data.PhoneNumber, OTP);
                    return Ok(new ResponseData
                    {
                        Code = "200",
                        Message = "User Registered",
                        Data = null
                    });
                }
                else
                {
                    return BadRequest(new ResponseData
                    {
                        Code = "400",
                        Message = "User Already Registered",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AuthController", "Register", "Register", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = null
                });
            }
        }

        [HttpPost("register/smsverification")]
        public ActionResult SmsVerification([FromBody]SmsVerificationModel data)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("PhoneNumber", data.PhoneNumber);
                var user = helper.GetSingleObject(filter, "Authentication", "Authentication").Result;
                if (user != null)
                {
                    var verifyUser = BsonSerializer.Deserialize<RegisterModel>(user);
                    SmsVerificationModel smsModel = new SmsVerificationModel();
                    smsModel.PhoneNumber = data.PhoneNumber;
                    if (verifyUser.OTPExp > DateTime.UtcNow)
                    {
                        if (smsHasher.VerifyHashedPassword(smsModel, verifyUser.VerificationCode, data.VerificationCode).ToString() == "Success")
                        {
                            var update = Builders<BsonDocument>.Update.Set("Status", "Verified");
                            var result = helper.UpdateSingleObject(filter, "Authentication", "Authentication", update).Result;
                            Parameters parameters = new Parameters();
                            parameters.username = data.PhoneNumber;
                            return Ok(Json(authHelper.DoPassword(parameters, _repo, _settings)));
                        }
                        else
                        {
                            return BadRequest(new ResponseData
                            {
                                Code = "400",
                                Message = "OTP Invalid",
                                Data = null
                            });
                        }
                    }
                    else
                    {
                        return BadRequest(new ResponseData
                        {
                            Code = "401",
                            Message = "OTP Expired",
                            Data = null
                        });
                    }
                }
                else
                {
                    return BadRequest(new ResponseData
                    {
                        Code = "400",
                        Message = "User Not Found",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AuthController", "SMSVerification", "SMSVerification", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = null
                });
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
                    var filter = Builders<BsonDocument>.Filter.Eq("PhoneNumber", user.UserName);
                    var verifyUser = BsonSerializer.Deserialize<RegisterModel>(helper.GetSingleObject(filter, "Authentication", "Authentication").Result);
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
                        string response = LoginAttempts(filter);
                        return BadRequest(new ResponseData
                        {
                            Code = "400",
                            Message = "Invalid User Infomation" + " & " + response,
                            Data = null
                        });
                    }

                }
                catch (Exception ex)
                {
                    LoggerDataAccess.CreateLog("AuthController", "Login", "Login", ex.Message);
                    return BadRequest(new ResponseData
                    {
                        Code = "400",
                        Message = "Failed",
                        Data = null
                    });
                }
            }
        }

        public string LoginAttempts(FilterDefinition<BsonDocument> filter)
        {
            try
            {
                var verifyUser = BsonSerializer.Deserialize<RegisterModel>(helper.GetSingleObject(filter, "Authentication", "Authentication").Result);
                if (verifyUser.WrongAttemptCount < 10)
                {
                    var update = Builders<BsonDocument>.Update.Set("WrongAttemptCount", verifyUser.WrongAttemptCount + 1);
                    var result = helper.UpdateSingleObject(filter, "Authentication", "Authentication", update).Result;
                    return "Login Attempt Recorded";
                }
                else
                {
                    var update = Builders<BsonDocument>.Update.Set("Status", "Revoked");
                    var result = helper.UpdateSingleObject(filter, "Authentication", "Authentication", update).Result;
                    return "Account Blocked";
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AuthController", "LoginAttempts", "LoginAttempts", ex.Message);
                return "Failed";
            }
        }

        [HttpPost("forgotpassword")]
        public ActionResult ForgotPassword([FromBody]ForgotPasswordModel data)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("PhoneNumber", data.PhoneNumber);
                var user = helper.GetSingleObject(filter, "Authentication", "Authentication").Result;
                if (user != null)
                {
                    SmsVerificationModel smsModel = new SmsVerificationModel();
                    smsModel.PhoneNumber = data.PhoneNumber;
                    Random codeGenerator = new Random();
                    string OTP = codeGenerator.Next(0, 1000000).ToString("D6");
                    var update = Builders<BsonDocument>.Update.Set("Status", "Not Verified").Set("OTPExp", DateTime.UtcNow.AddMinutes(4))
                                                              .Set("VerificationCode", smsHasher.HashPassword(smsModel, OTP));
                    var result = helper.UpdateSingleObject(filter, "Authentication", "Authentication", update).Result;
                    SMSHelper.SendSMS(data.PhoneNumber, OTP);
                    return Ok(new ResponseData
                    {
                        Code = "200",
                        Message = "Success",
                        Data = null
                    });
                }
                else
                {
                    return BadRequest(new ResponseData
                    {
                        Code = "404",
                        Message = "User Not Found",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AuthController", "ForgetPassword", "ForgetPassword", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = null
                });
            }
        }

        [HttpPost("forgotpassword/smsverification")]
        public ActionResult ForgotPasswordVerification([FromBody]SmsVerificationModel data)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("PhoneNumber", data.PhoneNumber);
                var user = helper.GetSingleObject(filter, "Authentication", "Authentication").Result;
                if (user != null)
                {
                    var verifyUser = BsonSerializer.Deserialize<RegisterModel>(user);
                    if (verifyUser.OTPExp > DateTime.UtcNow)
                    {
                        SmsVerificationModel smsModel = new SmsVerificationModel();
                        smsModel.PhoneNumber = data.PhoneNumber;
                        if (smsHasher.VerifyHashedPassword(smsModel, verifyUser.VerificationCode, data.VerificationCode).ToString() == "Success")
                        {
                            var update = Builders<BsonDocument>.Update.Set("Status", "Verified");
                            var result = helper.UpdateSingleObject(filter, "Authentication", "Authentication", update).Result; Parameters parameters = new Parameters();
                            parameters.username = data.PhoneNumber;
                            var response = authHelper.DoPassword(parameters, _repo, _settings);
                            response.Code = "201";
                            response.Message = "OTP Verified";
                            return Ok(Json(response));
                        }
                        else
                        {
                            return BadRequest(new ResponseData
                            {
                                Code = "400",
                                Message = "Invalied OTP",
                                Data = null
                            });
                        }
                    }
                    else
                    {
                        return BadRequest(new ResponseData
                        {
                            Code = "402",
                            Message = "OTP Expired",
                            Data = null
                        });
                    }
                }
                else
                {
                    return BadRequest(new ResponseData
                    {
                        Code = "404",
                        Message = "User Not Found",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AuthController", "ForgotPasswordVerification", "ForgotPasswordVerification", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = null
                });
            }
        }

        [HttpPost("forgotpassword/changepassword")]
        //[Authorize]
        public ActionResult ChangePassword([FromBody]RegisterModel data)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("PhoneNumber", data.PhoneNumber);
                var user = helper.GetSingleObject(filter, "Authentication", "Authentication").Result;
                if (user != null)
                {
                    var verifyUser = BsonSerializer.Deserialize<RegisterModel>(user);
                    if (verifyUser.Status == "Verified")
                    {
                        var update = Builders<BsonDocument>.Update.Set("Password", registerHasher.HashPassword(data, data.Password));
                        var result = helper.UpdateSingleObject(filter, "Authentication", "Authentication", update).Result;
                        return Ok(new ResponseData
                        {
                            Code = "200",
                            Message = "Password Changed Successfully",
                            Data = null
                        });
                    }
                    else
                    {
                        return BadRequest(new ResponseData
                        {
                            Code = "400",
                            Message = "User Not Verified to Chanage Password",
                            Data = null
                        });
                    }
                }
                else
                {
                    return BadRequest(new ResponseData
                    {
                        Code = "400",
                        Message = "User Not Found",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AuthController", "ChangePassword", "ChangePassword", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = null
                });
            }
        }

        [HttpPost("changepassword")]
        //[Authorize]
        public ActionResult ChangePasswordWhenLoggedIn([FromBody]ChangePassword data)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("PhoneNumber", data.PhoneNumber);
                var user = helper.GetSingleObject(filter, "Authentication", "Authentication").Result;
                if (user != null)
                {
                    var verifyUser = BsonSerializer.Deserialize<RegisterModel>(user);
                    SmsVerificationModel smsModel = new SmsVerificationModel();
                    smsModel.PhoneNumber = verifyUser.PhoneNumber;
                    if (smsHasher.VerifyHashedPassword(smsModel, verifyUser.Password, data.CurrentPassword).ToString() == "Success")
                    {
                        var update = Builders<BsonDocument>.Update.Set("Password", registerHasher.HashPassword(verifyUser, data.NewPassword));
                        var result = helper.UpdateSingleObject(filter, "Authentication", "Authentication", update).Result;
                        return Ok(new ResponseData
                        {
                            Code = "200",
                            Message = "Password Changed Successfully",
                            Data = null
                        });

                    }
                    else
                    {
                        return BadRequest(new ResponseData
                        {
                            Code = "401",
                            Message = "Invalid Password",
                            Data = null
                        });
                    }
                }
                else
                {
                    return BadRequest(new ResponseData
                    {
                        Code = "404",
                        Message = "User Not Found",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AuthController", "ChangePasswordWhenLoggedIn", "ChangePasswordWhenLoggedIn", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = null
                });
            }
        }

        [HttpPost("deactivateaccount")]
        public ActionResult DeactivateAccount([FromBody]LoginModel data)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("PhoneNumber", data.UserName);
                var user = helper.GetSingleObject(filter, "Authentication", "Authentication").Result;
                if (user != null)
                {
                    var verifyUser = BsonSerializer.Deserialize<RegisterModel>(user);
                    LoginModel loginModel = new LoginModel();
                    loginModel.UserName = data.UserName;
                    if (loginHasher.VerifyHashedPassword(loginModel, verifyUser.Password, data.Password).ToString() == "Success")
                    {
                        var authCollection = _db.GetCollection<RegisterModel>("Authentication");
                        var response = authCollection.DeleteOneAsync(user);
                        return Ok(new ResponseData
                        {
                            Code = "200",
                            Message = "user Deactivated",
                            Data = null
                        });
                    }
                    else
                    {
                        return BadRequest(new ResponseData
                        {
                            Code = "401",
                            Message = "Invalid UserName or Password",
                            Data = null
                        });
                    }
                }
                else
                {
                    return BadRequest(new ResponseData
                    {
                        Code = "404",
                        Message = "User Not Found",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AuthController", "DeactivateAccount", "DeactivateAccount", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = null
                });
            }
        }
    }
}
