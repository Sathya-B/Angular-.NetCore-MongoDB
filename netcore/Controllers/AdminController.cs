using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arthur_Clive.Data;
using Arthur_Clive.Helper;
using Arthur_Clive.Logger;
using Arthur_Clive.Swagger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Swashbuckle.AspNetCore.Examples;
using MH = Arthur_Clive.Helper.MongoHelper;

namespace Arthur_Clive.Controllers
{
    /// <summary>Controller to subscribe and unsubscribe user and to send email to subscribed users</summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class AdminController : Controller
    {
        /// <summary></summary>
        public MongoClient _client;
        /// <summary></summary>
        public IMongoDatabase subscribe_db;
        /// <summary></summary>
        public IMongoCollection<BsonDocument> subscribeduser_collection;
        /// <summary></summary>
        public IMongoCollection<Subscribe> subscribeduserCollection;
        /// <summary></summary>
        public IMongoDatabase userinfo_db;
        /// <summary></summary>
        public IMongoCollection<BsonDocument> userinfo_collection;
        /// <summary></summary>
        public IMongoDatabase auth_db;
        /// <summary></summary>
        public IMongoCollection<BsonDocument> authentication_collection;
        /// <summary></summary>
        public IMongoDatabase roles_db;
        /// <summary></summary>
        public IMongoCollection<BsonDocument> roles_collection;
        /// <summary></summary>
        public IMongoDatabase logger_db;
        /// <summary></summary>
        public IMongoCollection<ApplicationLogger> serverlogCollection;

        /// <summary>
        /// 
        /// </summary>
        public AdminController()
        {
            _client = MH.GetClient();
            subscribe_db = _client.GetDatabase("SubscribeDB");
            subscribeduser_collection = subscribe_db.GetCollection<BsonDocument>("SubscribedUsers");
            subscribeduserCollection = subscribe_db.GetCollection<Subscribe>("SubscribedUsers");
            userinfo_db = _client.GetDatabase("UserInfo");
            userinfo_collection = userinfo_db.GetCollection<BsonDocument>("UserInfo");
            auth_db = _client.GetDatabase("Authentication");
            authentication_collection = auth_db.GetCollection<BsonDocument>("Authentication");
            roles_db = _client.GetDatabase("RolesDB");
            roles_collection = roles_db.GetCollection<BsonDocument>("Roles");
            logger_db = _client.GetDatabase("ArthurCliveLogDB");
            serverlogCollection = logger_db.GetCollection<ApplicationLogger>("ServerLog");
        }

        /// <summary>Subscribe user</summary>
        /// <param name="emailid">Email of user who needs to be subscribed</param>
        /// <remarks>This api adds user to subscribed user list</remarks>
        /// <response code="200">User subscribed successfully</response>
        /// <response code="404">User not found</response>    
        /// <response code="401">User already subscribed</response>    
        /// <response code="402">UserName is empty</response>
        /// <response code="400">Process ran into an exception</response>   
        [HttpPost("subscribe/{emailid}")]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public async Task<ActionResult> Subscribe(string emailid)
        {
            try
            {
                if (emailid != null)
                {
                    var checkUser = MH.CheckForDatas(subscribeduser_collection, "Email", emailid, null, null);
                    if (checkUser == false)
                    {
                        await subscribeduserCollection.InsertOneAsync(new Subscribe { Email = emailid });
                        var sendEmail = EmailHelper.SendEmail_NewsLetterService(emailid);
                        return Ok(new ResponseData { Code = "200", Message = "Subscribed Succesfully" });
                    }
                    else if (checkUser == false)
                    {
                        return BadRequest(new ResponseData { Code = "401", Message = "User Already Subscribed" });
                    }
                    else
                    {
                        return BadRequest(new ResponseData { Code = "400", Message = "Failed" });
                    }
                }
                else
                {
                    return BadRequest(new ResponseData { Code = "402", Message = "EmailId connot be empty" });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AdminContoller", "Subscribe", ex.Message, serverlogCollection);
                return BadRequest(new ResponseData { Code = "400", Message = "Failed", Data = ex.Message });
            }
        }

        /// <summary>Unsubscribe user</summary>
        /// <param name="emailid">UserName of user who need to get unsubscribed</param>
        /// <remarks>This api removes user from subscribed user list</remarks>
        /// <response code="200">User unsubscribed successfully</response>
        /// <response code="404">User not found</response>    
        /// <response code="402">UserName is empty</response>
        /// <response code="400">Process ran into an exception</response>    
        [HttpDelete("unsubscribe/{emailid}")]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public ActionResult Unsubscribe(string emailid)
        {
            try
            {
                if (emailid != null)
                {
                    var checkUser = MH.CheckForDatas(subscribeduser_collection, "Email", emailid, null, null);
                    if (checkUser == true)
                    {
                        MH.DeleteSingleObject(subscribeduser_collection, "Email", emailid, null, null);
                        return Ok(new ResponseData { Code = "200", Message = "Unsubscribed Succesfully" });
                    }
                    else if (checkUser == false)
                    {
                        return BadRequest(new ResponseData { Code = "404", Message = "No user found" });
                    }
                    else
                    {
                        return BadRequest(new ResponseData { Code = "400", Message = "Failed" });
                    }
                }
                else
                {
                    return BadRequest(new ResponseData { Code = "402", Message = "UserName connot be empty" });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AdminContoller", "Unsubscribe", ex.Message, serverlogCollection);
                return BadRequest(new ResponseData { Code = "400", Message = "Failed", Data = ex.Message });
            }
        }

        /// <summary>Send email to all subscribed users</summary>
        /// <remarks>This api sends message to all subscribed users</remarks>
        /// <param name="message">Message that is to be sent throungh email to the subscribed users by admin</param>
        /// <response code="200">Email successfully sent to all subscribed users</response>
        /// <response code="404">No user found to be subscribed</response> 
        /// <response code="400">Process ran into an exception</response>    
        [HttpPost("sendmessage")]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public async Task<ActionResult> PublicPost([FromBody]string message)
        {
            try
            {
                IAsyncCursor<Subscribe> cursor = await subscribeduserCollection.FindAsync(FilterDefinition<Subscribe>.Empty);
                var users = cursor.ToList();
                if (users.Count > 0)
                {
                    foreach (var user in users)
                    {
                        await EmailHelper.SendEmail_ToSubscribedUsers(user.Email, user.Email, message);
                    }
                    return Ok(new ResponseData { Code = "200", Message = "Email sent to all subscribed users" });
                }
                else
                {
                    return BadRequest(new ResponseData { Code = "404", Message = "There are no subscribed users" });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AdminContoller", "PublicPost", ex.Message, serverlogCollection);
                return BadRequest(new ResponseData { Code = "400", Message = "Failed", Data = ex.Message });
            }
        }

        /// <summary>Upload image to amazon s3 object storage</summary>
        /// <param name="data">Details to image to be uploaded</param>
        /// <response code="200">Image uploaded</response>
        /// <response code="401">Image upload failed</response>   
        /// <response code="404">Image upload data not found</response>   
        /// <response code="400">Process ran into an exception</response>
        [Authorize("Level1Access")]
        [HttpPost("uploadimage")]
        [SwaggerRequestExample(typeof(ImageData), typeof(ImageUploadDetails))]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public ActionResult UploadImageToS3([FromForm]ImageData data)
        {
            try
            {
                if (data != null)
                {
                    var objectName = data.ObjectName + ".jpg";
                    var result = AmazonHelper.UploadImageToS3(data.Image, data.BucketName, objectName);
                    var Image_Url = AmazonHelper.GetS3Object(data.BucketName, objectName);
                    if (result.Result == true)
                    {
                        return Ok(new ResponseData
                        {
                            Code = "200",
                            Message = "Image uploaded",
                            Data = Image_Url
                        });
                    }
                    else
                    {
                        return BadRequest(new ResponseData
                        {
                            Code = "401",
                            Message = "Image upload failed",
                            Data = null
                        });
                    }
                }
                else
                {
                    return BadRequest(new ResponseData
                    {
                        Code = "404",
                        Message = "Image upload data not found",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AdminContoller", "UploadImageToS3", ex.Message, serverlogCollection);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = ex.Message
                });
            }
        }

        /// <summary>Get all the users</summary>
        /// <response code="200">Returns all the users</response>   
        /// <response code="404">No users found</response>   
        /// <response code="400">Process ran into an exception</response>
        [Authorize("Level1Access")]
        [HttpGet("getallusers")]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public ActionResult GetAllUsers()
        {
            try
            {
                var getlist = MH.GetListOfObjects(authentication_collection, null, null, null, null).Result;
                if (getlist != null)
                {
                    List<UserInfomation> userList = new List<UserInfomation>();
                    foreach (var user in getlist)
                    {
                        var userInfo = BsonSerializer.Deserialize<RegisterModel>(user);
                        var billingAddressData = MH.GetSingleObject(userinfo_collection, "UserName", userInfo.UserName, "BillingAddress", true).Result;
                        Address billingAddress = new Address();
                        if (billingAddressData != null)
                        {
                            billingAddress = BsonSerializer.Deserialize<Address>(billingAddressData);
                        }
                        else
                        {
                            billingAddress = null;
                        }
                        var shippingAddressData = MH.GetSingleObject(userinfo_collection, "UserName", userInfo.UserName, "ShippingAddress", true).Result;
                        Address shippingAddress = new Address();
                        if (shippingAddressData != null)
                        {
                            shippingAddress = BsonSerializer.Deserialize<Address>(shippingAddressData);
                        }
                        else
                        {
                            shippingAddress = null;
                        }
                        UserInfomation userInfomation = new UserInfomation
                        {
                            FullName = userInfo.FullName,
                            PhoneNumber = userInfo.PhoneNumber,
                            Email = userInfo.Email,
                            BillingAddress = billingAddress,
                            ShippingAddress = shippingAddress
                        };
                        userList.Add(userInfomation);
                    }
                    return Ok(new ResponseData { Code = "200", Message = "Success", Data = userList });
                }
                else
                {
                    return BadRequest(new ResponseData { Code = "404", Message = "No users found" });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AdminContoller", "GetAllUsers", ex.Message, serverlogCollection);
                return BadRequest(new ResponseData { Code = "400", Message = "Failed", Data = ex.Message });
            }
        }

        /// <summary>Get all the roles added bt admin</summary>
        /// <response code="200">Returns all the roles added by the admin</response>   
        /// <response code="404">No roles found</response>   
        /// <response code="400">Process ran into an exception</response>
        [HttpGet("roles")]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public ActionResult GetAllRoles()
        {
            try
            {
                var getRoles = MH.GetListOfObjects(roles_collection, null, null, null, null).Result;
                if (getRoles != null)
                {
                    List<Roles> rolesList = new List<Roles>();
                    foreach (var role in getRoles)
                    {
                        var roleInfo = BsonSerializer.Deserialize<Roles>(role);
                        rolesList.Add(roleInfo);
                    }
                    return Ok(new ResponseData { Code = "200", Message = "Success", Data = rolesList });
                }
                else
                {
                    return BadRequest(new ResponseData { Code = "404", Message = "No roles found" });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AdminContoller", "GetAllRoles", ex.Message, serverlogCollection);
                return BadRequest(new ResponseData { Code = "400", Message = "Failed", Data = ex.Message });
            }
        }

        /// <summary>Get details of role with the matching role name</summary>
        /// <response code="200">Returns details of role with matching role name added by the admin</response>   
        /// <response code="404">No role found with this name</response>   
        /// <response code="400">Process ran into an exception</response>
        [HttpGet("roles/details/rolename:{rolename}")]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public ActionResult GetRoleDetailByName(string rolename)
        {
            try
            {
                var getRole = MH.GetSingleObject(roles_collection, "RoleName", rolename, null, null).Result;
                if (getRole != null)
                {
                    var roleInfo = BsonSerializer.Deserialize<Roles>(getRole);
                    return Ok(new ResponseData { Code = "200", Message = "Success", Data = roleInfo });
                }
                else
                {
                    return BadRequest(new ResponseData { Code = "404", Message = "No role found with specified role name" });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AdminContoller", "GetRoleDetailByName", ex.Message, serverlogCollection);
                return BadRequest(new ResponseData { Code = "400", Message = "Failed", Data = ex.Message });
            }
        }

        /// <summary>Get roles which have specified level of access</summary>
        /// <response code="200">Returns details of roles with matching level of access</response>   
        /// <response code="404">No role found having the specified level of access</response>   
        /// <response code="400">Process ran into an exception</response>
        [HttpGet("roles/details/levelofaccess:{levelofaccess}")]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public ActionResult GetRolesByLevelOfAccess(string levelofaccess)
        {
            try
            {
                var getRoles = MH.GetListOfObjects(roles_collection, null, null, null, null).Result;
                if (getRoles != null)
                {
                    List<Roles> rolesList = new List<Roles>();
                    foreach (var role in getRoles)
                    {
                        var roleInfo = BsonSerializer.Deserialize<Roles>(role);
                        string accessLevel = "Level" + levelofaccess + "Access";
                        if (roleInfo.LevelOfAccess.Contains(accessLevel))
                        {
                            rolesList.Add(roleInfo);
                        }
                    }
                    return Ok(new ResponseData { Code = "200", Message = "Success", Data = rolesList });
                }
                else
                {
                    return BadRequest(new ResponseData { Code = "404", Message = "No roles found" });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AdminContoller", "GetRolesByLevelOfAccess", ex.Message, serverlogCollection);
                return BadRequest(new ResponseData { Code = "400", Message = "Failed", Data = ex.Message });
            }
        }

        /// <summary>Insert new role</summary>
        /// <param name="data">Details of role to be inserted</param>
        /// <response code="200">Role inserted successfully</response>   
        /// <response code="401">Another role with same name is found</response> 
        /// <response code="402">Another role with same role id is found</response>   
        /// <response code="400">Process ran into an exception</response>
        [HttpPost("roles/insert")]
        [SwaggerRequestExample(typeof(Roles), typeof(RoleDetails))]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public async Task<ActionResult> InsertRole([FromBody]Roles data)
        {
            try
            {
                if (MH.CheckForDatas(roles_collection, "RoleName", data.RoleName, null, null) == true)
                {
                    return BadRequest(new ResponseData { Code = "401", Message = "Another role with same name is found" });
                }
                else if (MH.CheckForDatas(roles_collection, "RoleID", data.RoleID, null, null) == true)
                {
                    return BadRequest(new ResponseData { Code = "402", Message = "Another role with same role id is found" });
                }
                else
                {
                    await roles_db.GetCollection<Roles>("Roles").InsertOneAsync(data);
                    return Ok(new ResponseData { Code = "200", Message = "Role inserted successfully" });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AdminContoller", "InsertRole", ex.Message, serverlogCollection);
                return BadRequest(new ResponseData { Code = "400", Message = "Failed", Data = ex.Message });
            }
        }

        /// <summary>Assign role to user</summary>
        /// <param name="username">UserName of user whoes role needs to be assigned</param>
        /// <param name="rolename">Role name</param>
        /// <response code="200">Role assigned to user.</response>   
        /// <response code="404">User not found</response>   
        /// <response code="400">Process ran into an exception</response>
        [HttpPut("roles/assign/{username}/{rolename}")]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public ActionResult AssignRole(string username, string rolename)
        {
            try
            {
                var checkuser = MH.CheckForDatas(authentication_collection, "UserName", username, null, null);
                if (checkuser == true)
                {
                    var updateRole = MH.UpdateSingleObject(authentication_collection, "UserName", username, null, null, Builders<BsonDocument>.Update.Set("UserRole", rolename));
                    return Ok(new ResponseData { Code = "200", Message = "Role assigned to user" });
                }
                else if (checkuser == false)
                {
                    return BadRequest(new ResponseData { Code = "404", Message = "User not found" });
                }
                else
                {
                    return BadRequest(new ResponseData { Code = "400", Message = "Failed" });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AdminContoller", "InsertRoles", ex.Message, serverlogCollection);
                return BadRequest(new ResponseData { Code = "400", Message = "Failed", Data = ex.Message });
            }
        }
    }
}
