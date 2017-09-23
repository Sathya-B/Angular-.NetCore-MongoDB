﻿using System;
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
                if (data.VerificationType == "PhoneNumber")
                {
                    checkUser = MH.CheckForDatas("UserName", data.PhoneNumber, null, null, "Authentication", "Authentication");
                }
                else
                {
                    checkUser = MH.CheckForDatas("UserName", data.Email, null, null, "Authentication", "Authentication");
                }
                if (checkUser == null)
                {
                    var userData = BsonSerializer.Deserialize<RegisterModel>(checkUser);
                    if (data.VerificationType == "PhoneNumber")
                    {
                        data.UserName = data.PhoneNumber;
                    }
                    else
                    {
                        data.UserName = data.Email;
                    }
                    RegisterModel registerModel = new RegisterModel { UserName = data.UserName, Password = data.Password };
                    data.Password = passwordHasher.HashPassword(registerModel, data.Password);
                    VerificationModel smsModel = new VerificationModel();
                    if (data.VerificationType == "PhoneNumber")
                    {
                        smsModel.UserName = data.PhoneNumber;
                    }
                    else
                    {
                        smsModel.UserName = data.Email;
                    }
                    Random codeGenerator = new Random();
                    string OTP = codeGenerator.Next(0, 1000000).ToString("D6");
                    data.VerificationCode = smsHasher.HashPassword(smsModel, OTP);
                    data.OTPExp = DateTime.UtcNow.AddMinutes(5);
                    data.Status = "Registered";
                    var authCollection = _db.GetCollection<RegisterModel>("Authentication");
                    await authCollection.InsertOneAsync(data);
                    if (data.VerificationType == "PhoneNumber")
                    {
                        SMSHelper.SendSMS(data.PhoneNumber, OTP);
                    }
                    else
                    {
                        await EmailHelper.SendEmail(userData.FullName, data.Email, OTP);
                    }
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

        [HttpPost("register/verification")]
        public ActionResult RegisterVerification([FromBody]VerificationModel data)
        {
            try
            {
                var checkUser = MH.CheckForDatas("UserName", data.UserName, null, null, "Authentication", "Authentication");
                if (checkUser != null)
                {
                    var verifyUser = BsonSerializer.Deserialize<RegisterModel>(checkUser);
                    if (verifyUser.OTPExp > DateTime.UtcNow)
                    {
                        VerificationModel smsModel = new VerificationModel { UserName = data.UserName };
                        if (smsHasher.VerifyHashedPassword(smsModel, verifyUser.VerificationCode, data.VerificationCode).ToString() == "Success")
                        {
                            var update = Builders<BsonDocument>.Update.Set("Status", "Verified");
                            var filter = Builders<BsonDocument>.Filter.Eq("UserName", data.UserName);
                            var result = MH.UpdateSingleObject(filter, "Authentication", "Authentication", update).Result;
                            Parameters parameters = new Parameters { UserName = data.UserName };
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
                if (user.VerificationType == "PhoneNumber")
                {
                    checkUser = MH.CheckForDatas("PhoneNumber", user.UserName, null, null, "Authentication", "Authentication");
                }
                else
                {

                    checkUser = MH.CheckForDatas("Email", user.UserName, null, null, "Authentication", "Authentication");
                }
                if (checkUser != null)
                {
                    var verifyUser = BsonSerializer.Deserialize<RegisterModel>(checkUser);
                    RegisterModel registerModel = new RegisterModel();
                    registerModel.UserName = user.UserName;
                    registerModel.Password = user.Password;
                    if (passwordHasher.VerifyHashedPassword(registerModel, verifyUser.Password, user.Password).ToString() == "Success")
                    {
                        Parameters parameters = new Parameters();
                        parameters.UserName = user.UserName;
                        parameters.FullName = verifyUser.FullName;
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
                var checkUser = MH.CheckForDatas("UserName", data.UserName, null, null, "Authentication", "Authentication");
                var filter = Builders<BsonDocument>.Filter.Eq("UserName", data.UserName);
                var user = MH.GetSingleObject(filter, "Authentication", "Authentication").Result;
                if (user != null)
                {
                    var userData = BsonSerializer.Deserialize<RegisterModel>(user);
                    VerificationModel smsModel = new VerificationModel();
                    smsModel.UserName = data.UserName;
                    Random codeGenerator = new Random();
                    string OTP = codeGenerator.Next(0, 1000000).ToString("D6");
                    var update = Builders<BsonDocument>.Update.Set("Status", "Not Verified").Set("OTPExp", DateTime.UtcNow.AddMinutes(4))
                                                              .Set("VerificationCode", smsHasher.HashPassword(smsModel, OTP));
                    var result = MH.UpdateSingleObject(filter, "Authentication", "Authentication", update).Result;
                    if (data.VerificationType == "PhoneNumber")
                    {
                        SMSHelper.SendSMS(data.UserName, OTP);
                    }
                    else
                    {
                        await EmailHelper.SendEmail(userData.FullName, data.UserName, OTP);
                    }
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

        [HttpPost("forgotpassword/verification")]
        public ActionResult ForgotPasswordVerification([FromBody]VerificationModel data)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("UserName", data.UserName);
                var user = MH.GetSingleObject(filter, "Authentication", "Authentication").Result;
                if (user != null)
                {
                    var verifyUser = BsonSerializer.Deserialize<RegisterModel>(user);
                    if (verifyUser.OTPExp > DateTime.UtcNow)
                    {
                        VerificationModel model = new VerificationModel { UserName = data.UserName };
                        if (smsHasher.VerifyHashedPassword(model, verifyUser.VerificationCode, data.VerificationCode).ToString() == "Success")
                        {
                            var update = Builders<BsonDocument>.Update.Set("Status", "Verified");
                            var result = MH.UpdateSingleObject(filter, "Authentication", "Authentication", update).Result; Parameters parameters = new Parameters();
                            parameters.UserName = data.UserName;
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
    }
}
