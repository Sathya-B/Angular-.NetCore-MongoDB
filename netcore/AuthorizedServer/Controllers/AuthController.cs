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
using System.Net.Http;
using Newtonsoft;

namespace AuthorizedServer.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        public AuthHelper authHelper = new AuthHelper();
        private IOptions<Audience> _settings;
        private IRTokenRepository _repo;
        public IMongoDatabase _db = MH._client.GetDatabase("Authentication");
        public PasswordHasher<VerificationModel> smsHasher = new PasswordHasher<VerificationModel>();
        public PasswordHasher<RegisterModel> passwordHasher = new PasswordHasher<RegisterModel>();

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
                BsonDocument checkUser;
                string userName;
                string OTP;
                if (data.UserLocation == "IN")
                {
                    userName = data.PhoneNumber;
                }
                else
                {
                    userName = data.Email;
                }
                checkUser = MH.CheckForDatas("UserName", userName, null, null, "Authentication", "Authentication");
                if (checkUser == null)
                {
                    if (data.UserLocation != null)
                    {
                        data.UserName = userName;
                        RegisterModel registerModel = new RegisterModel { UserName = userName, Password = data.Password };
                        data.Password = passwordHasher.HashPassword(registerModel, data.Password);
                        data.OTPExp = DateTime.UtcNow.AddMinutes(2);
                        VerificationModel smsModel = new VerificationModel();
                        smsModel.UserName = userName;
                        if (data.UserLocation == "IN")
                        {
                            Random codeGenerator = new Random();
                            OTP = codeGenerator.Next(0, 1000000).ToString("D6");
                            smsModel.VerificationCode = OTP;
                            SMSHelper.SendSMS(data.PhoneNumber, OTP);
                        }
                        else
                        {
                            OTP = Guid.NewGuid().ToString();
                            string link = GlobalHelper.GetIpConfig() + data.UserName + "/" + OTP + "/no";
                            var result = await EmailHelper.SendEmail(data.FullName, data.Email, link);
                        }
                        data.VerificationCode = smsHasher.HashPassword(smsModel, OTP);
                        data.Status = "Registered";
                        var authCollection = _db.GetCollection<RegisterModel>("Authentication");
                        await authCollection.InsertOneAsync(data);
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
                            Code = "402",
                            Message = "Verification Type Cannot Be Empty",
                            Data = null
                        });
                    }
                }
                else
                {
                    return BadRequest(new ResponseData
                    {
                        Code = "401",
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

        [HttpGet("register/verification/{username}/{otp}")]
        public ActionResult RegisterVerification(string username, string otp)
        {
            try
            {
                var checkUser = MH.CheckForDatas("UserName", username, null, null, "Authentication", "Authentication");
                if (checkUser != null)
                {
                    var verifyUser = BsonSerializer.Deserialize<RegisterModel>(checkUser);
                    if (verifyUser.OTPExp > DateTime.UtcNow)
                    {
                        VerificationModel smsModel = new VerificationModel { UserName = username, VerificationCode = otp };
                        if (smsHasher.VerifyHashedPassword(smsModel, verifyUser.VerificationCode, otp).ToString() == "Success")
                        {
                            var update = Builders<BsonDocument>.Update.Set("Status", "Verified");
                            var filter = Builders<BsonDocument>.Filter.Eq("UserName", username);
                            var result = MH.UpdateSingleObject(filter, "Authentication", "Authentication", update).Result;
                            Parameters parameters = new Parameters { username = username, fullname = verifyUser.FullName };
                            return Ok(Json(authHelper.DoPassword(parameters, _repo, _settings)));
                        }
                        else
                        {
                            return BadRequest(new ResponseData
                            {
                                Code = "402",
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
                        Code = "404",
                        Message = "User Not Found",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AuthController", "RegisterVerification", "RegisterVerification", ex.Message);
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
            try
            {
                BsonDocument checkUser;
                checkUser = MH.CheckForDatas("UserName", user.UserName, null, null, "Authentication", "Authentication");
                if (checkUser != null)
                {
                    var verifyUser = BsonSerializer.Deserialize<RegisterModel>(checkUser);
                    if (verifyUser.Status == "Verified")
                    {
                        RegisterModel registerModel = new RegisterModel();
                        registerModel.UserName = user.UserName;
                        registerModel.Password = user.Password;
                        if (passwordHasher.VerifyHashedPassword(registerModel, verifyUser.Password, user.Password).ToString() == "Success")
                        {
                            Parameters parameters = new Parameters();
                            parameters.username = user.UserName;
                            parameters.fullname = verifyUser.FullName;
                            return Ok(Json(authHelper.DoPassword(parameters, _repo, _settings)));
                        }
                        else
                        {
                            var filter = Builders<BsonDocument>.Filter.Eq("UserName", user.UserName);
                            string response = RecordLoginAttempts(filter);
                            if (response != "Failed")
                                return BadRequest(new ResponseData
                                {
                                    Code = "401",
                                    Message = "Invalid User Infomation" + " & " + response,
                                    Data = null
                                });
                            else
                            {
                                return BadRequest(new ResponseData
                                {
                                    Code = "400",
                                    Message = "Failed",
                                    Data = null
                                });
                            }
                        }
                    }
                    else
                    {
                        return BadRequest(new ResponseData
                        {
                            Code = "402",
                            Message = "User Not Verified",
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
                LoggerDataAccess.CreateLog("AuthController", "Login", "Login", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = null
                });
            }
        }

        public string RecordLoginAttempts(FilterDefinition<BsonDocument> filter)
        {
            try
            {
                var verifyUser = BsonSerializer.Deserialize<RegisterModel>(MH.GetSingleObject(filter, "Authentication", "Authentication").Result);
                if (verifyUser.WrongAttemptCount < 10)
                {
                    var update = Builders<BsonDocument>.Update.Set("WrongAttemptCount", verifyUser.WrongAttemptCount + 1);
                    var result = MH.UpdateSingleObject(filter, "Authentication", "Authentication", update).Result;
                    return "Login Attempt Recorded";
                }
                else
                {
                    var update = Builders<BsonDocument>.Update.Set("Status", "Revoked");
                    var result = MH.UpdateSingleObject(filter, "Authentication", "Authentication", update).Result;
                    return "Account Blocked";
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AuthController", "RecordLoginAttempts", "RecordLoginAttempts", ex.Message);
                return "Failed";
            }
        }

        [HttpPost("forgotpassword")]
        public async Task<ActionResult> ForgotPassword([FromBody]ForgotPasswordModel data)
        {
            try
            {
                string OTP;
                var checkUser = MH.CheckForDatas("UserName", data.UserName, null, null, "Authentication", "Authentication");
                var filter = Builders<BsonDocument>.Filter.Eq("UserName", data.UserName);
                var user = MH.GetSingleObject(filter, "Authentication", "Authentication").Result;
                if (user != null)
                {
                    var userData = BsonSerializer.Deserialize<RegisterModel>(user);
                    VerificationModel smsModel = new VerificationModel();
                    smsModel.UserName = data.UserName;
                    if (data.UserLocation == "IN")
                    {
                        Random codeGenerator = new Random();
                        OTP = codeGenerator.Next(0, 1000000).ToString("D6");
                        smsModel.VerificationCode = OTP;
                        SMSHelper.SendSMS(data.UserName, OTP);
                    }
                    else
                    {
                        OTP = Guid.NewGuid().ToString();
                        string link = GlobalHelper.GetIpConfig() + data.UserName + "/" + OTP + "/yes";
                        await EmailHelper.SendEmail(userData.FullName, data.UserName, link);
                    }
                    var update = Builders<BsonDocument>.Update.Set("Status", "Not Verified").Set("OTPExp", DateTime.UtcNow.AddMinutes(2))
                                                              .Set("VerificationCode", smsHasher.HashPassword(smsModel, OTP));
                    var result = MH.UpdateSingleObject(filter, "Authentication", "Authentication", update).Result;
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
                        Message = "User not found",
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

        [HttpGet("forgotpassword/verification/{username}/{otp}")]
        public ActionResult ForgotPasswordVerification(string username, string otp)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("UserName", username);
                var user = MH.GetSingleObject(filter, "Authentication", "Authentication").Result;
                if (user != null)
                {
                    var verifyUser = BsonSerializer.Deserialize<RegisterModel>(user);
                    if (verifyUser.OTPExp > DateTime.UtcNow)
                    {
                        VerificationModel model = new VerificationModel { UserName = username, VerificationCode = otp };
                        if (smsHasher.VerifyHashedPassword(model, verifyUser.VerificationCode, otp).ToString() == "Success")
                        {
                            var update = Builders<BsonDocument>.Update.Set("Status", "Verified");
                            var result = MH.UpdateSingleObject(filter, "Authentication", "Authentication", update).Result; Parameters parameters = new Parameters();
                            parameters.username = username;
                            parameters.fullname = verifyUser.FullName;
                            var response = authHelper.DoPassword(parameters, _repo, _settings);
                            response.Code = "201";
                            response.Message = "OTP Verified";
                            return Ok(Json(response));
                        }
                        else
                        {
                            return BadRequest(new ResponseData
                            {
                                Code = "401",
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
        public ActionResult ChangePassword([FromBody]LoginModel data)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("UserName", data.UserName);
                var user = MH.GetSingleObject(filter, "Authentication", "Authentication").Result;
                if (user != null)
                {
                    var verifyUser = BsonSerializer.Deserialize<RegisterModel>(user);
                    if (verifyUser.Status == "Verified")
                    {
                        RegisterModel registerModel = new RegisterModel { UserName = data.UserName, Password = data.Password };
                        var update = Builders<BsonDocument>.Update.Set("Password", passwordHasher.HashPassword(registerModel, data.Password));
                        var result = MH.UpdateSingleObject(filter, "Authentication", "Authentication", update).Result;
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
                            Message = "User Not Verified to Change Password",
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
        public ActionResult ChangePasswordWhenLoggedIn([FromBody]ChangePasswordModel data)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("UserName", data.UserName);
                var user = MH.GetSingleObject(filter, "Authentication", "Authentication").Result;
                if (user != null)
                {
                    var verifyUser = BsonSerializer.Deserialize<RegisterModel>(user);
                    RegisterModel registerModel = new RegisterModel { UserName = verifyUser.UserName, Password = data.OldPassword };
                    if (passwordHasher.VerifyHashedPassword(registerModel, verifyUser.Password, data.OldPassword).ToString() == "Success")
                    {
                        var update = Builders<BsonDocument>.Update.Set("Password", passwordHasher.HashPassword(verifyUser, data.NewPassword));
                        var result = MH.UpdateSingleObject(filter, "Authentication", "Authentication", update).Result;
                        return Ok(new ResponseData
                        {
                            Code = "200",
                            Message = "Password Changed Successfully",
                            Data = null
                        });

                    }
                    else
                    {
                        string response = RecordLoginAttempts(filter);
                        if (response != "Failed")
                            return BadRequest(new ResponseData
                            {
                                Code = "401",
                                Message = "Invalid User Infomation" + " & " + response,
                                Data = null
                            });
                        else
                        {
                            return BadRequest(new ResponseData
                            {
                                Code = "400",
                                Message = "Failed",
                                Data = null
                            });
                        }
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
                var user = MH.GetSingleObject(filter, "Authentication", "Authentication").Result;
                if (user != null)
                {
                    var verifyUser = BsonSerializer.Deserialize<RegisterModel>(user);
                    RegisterModel registerModel = new RegisterModel { UserName = data.UserName, Password = data.Password };
                    if (passwordHasher.VerifyHashedPassword(registerModel, verifyUser.Password, data.Password).ToString() == "Success")
                    {
                        var authCollection = _db.GetCollection<RegisterModel>("Authentication");
                        var response = authCollection.DeleteOneAsync(user);
                        return Ok(new ResponseData
                        {
                            Code = "200",
                            Message = "User Deactivated",
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

        [HttpPost("externallogin/google")]
        public async Task<ActionResult> GoogleLogin([FromBody]SocialLoginModel data)
        {
            try
            {
                if (data.Token != null)
                {
                    string textResult;
                    using (var client = new HttpClient())
                    {
                        var uri = new Uri("https://www.googleapis.com/oauth2/v3/tokeninfo?id_token=" + data.Token);

                        var response = await client.GetAsync(uri);

                        textResult = await response.Content.ReadAsStringAsync();
                    }
                    if (textResult.Contains("Invalid Value"))
                    {
                        return BadRequest(new ResponseData
                        {
                            Code = "402",
                            Message = "Invalid token",
                            Data = null
                        });
                    }
                    else
                    {
                        var result = BsonSerializer.Deserialize<GoogleVerificationModel>(textResult);
                        if (result.sub == data.ID)
                        {
                            var checkUser = MH.CheckForDatas("UserName", result.email, null, null, "Authentication", "Authentication");
                            if (checkUser == null)
                            {
                                RegisterModel registerModel = new RegisterModel();
                                registerModel.UserName = result.email;
                                registerModel.SocialId = result.sub;
                                registerModel.FullName = result.name;
                                registerModel.Status = "Verified";
                                registerModel.Email = result.email;
                                var authCollection = _db.GetCollection<RegisterModel>("Authentication");
                                await authCollection.InsertOneAsync(registerModel);
                            }
                            Parameters parameters = new Parameters();
                            parameters.username = result.email;
                            parameters.fullname = result.name; ;
                            return Ok(Json(authHelper.DoPassword(parameters, _repo, _settings)));
                        }
                        else
                        {
                            return BadRequest(new ResponseData
                            {
                                Code = "403",
                                Message = "ID mismatch",
                                Data = null
                            });
                        }
                    }
                }
                else
                {
                    return BadRequest(new ResponseData
                    {
                        Code = "401",
                        Message = "Token is empty",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AuthController", "GoogleLogin", "GoogleLogin", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = null
                });
            }
        }

        [HttpPost("externallogin/facebook")]
        public async Task<ActionResult> FaceBookLogin([FromBody]SocialLoginModel data)
        {
            try
            {
                if (data.Token != null)
                {
                    string textResult;
                    using (var client = new HttpClient())
                    {
                        var uri = new Uri("https://graph.facebook.com/me?locale=en_US&fields=id,name&access_token=" + data.Token);

                        var response = await client.GetAsync(uri);

                        textResult = await response.Content.ReadAsStringAsync();
                    }
                    if (textResult.Contains("An active access token must be used to query information about the current user") || textResult.Contains("Malformed access token"))
                    {
                        return BadRequest(new ResponseData
                        {
                            Code = "402",
                            Message = "Invalid token",
                            Data = null
                        });
                    }
                    else
                    {
                        var result = Newtonsoft.Json.JsonConvert.DeserializeObject<FacebookVerificationModel>(textResult);
                        if (result.id == data.ID)
                        {
                            var checkUser = MH.CheckForDatas("SocialId", data.ID, null, null, "Authentication", "Authentication");
                            if (checkUser == null)
                            {
                                RegisterModel registerModel = new RegisterModel();
                                registerModel.UserName = result.id;
                                registerModel.SocialId = result.id;
                                registerModel.FullName = result.name;
                                registerModel.Status = "Verified";
                                registerModel.Email = data.Email;
                                var authCollection = _db.GetCollection<RegisterModel>("Authentication");
                                await authCollection.InsertOneAsync(registerModel);
                            }
                            Parameters parameters = new Parameters();
                            parameters.username = result.id;
                            parameters.fullname = result.name; ;
                            return Ok(Json(authHelper.DoPassword(parameters, _repo, _settings)));
                        }
                        else
                        {
                            return BadRequest(new ResponseData
                            {
                                Code = "403",
                                Message = "ID mismatch",
                                Data = null
                            });
                        }
                    }
                }
                else
                {
                    return BadRequest(new ResponseData
                    {
                        Code = "401",
                        Message = "Token is empty",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AuthController", "FaceBookLogin", "FaceBookLogin", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = null
                });
            }
        }

        [HttpPost("externallogin/facebook/check")]
        public async Task<ActionResult> FaceBookLoginCheck([FromBody]SocialLoginModel data)
        {
            try
            {
                if (data.Token != null)
                {
                    string textResult;
                    using (var client = new HttpClient())
                    {
                        var uri = new Uri("https://graph.facebook.com/me?locale=en_US&fields=id,name&access_token=" + data.Token);

                        var response = await client.GetAsync(uri);

                        textResult = await response.Content.ReadAsStringAsync();
                    }
                    if (textResult.Contains("An active access token must be used to query information about the current user") || textResult.Contains("Malformed access token"))
                    {
                        return BadRequest(new ResponseData
                        {
                            Code = "402",
                            Message = "Invalid token",
                            Data = null
                        });
                    }
                    else
                    {
                        var result = Newtonsoft.Json.JsonConvert.DeserializeObject<FacebookVerificationModel>(textResult);
                        var checkUser = MH.CheckForDatas("SocialId", result.id, null, null, "Authentication", "Authentication");
                        if (checkUser == null)
                        {
                            return Ok(new ResponseData
                            {
                                Code = "201",
                                Message = "User not found",
                                Data = null
                            });
                        }
                        else
                        {
                            var user = BsonSerializer.Deserialize<RegisterModel>(checkUser);
                            Parameters parameters = new Parameters();
                            parameters.username = result.id;
                            parameters.fullname = user.FullName; ;
                            return Ok(Json(authHelper.DoPassword(parameters, _repo, _settings)));
                        }
                    }
                }
                else
                {
                    return BadRequest(new ResponseData
                    {
                        Code = "401",
                        Message = "Token is empty",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AuthController", "GoogleLogin", "GoogleLogin", ex.Message);
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
