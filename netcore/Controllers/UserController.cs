using System;
using System.Threading.Tasks;
using Arthur_Clive.Data;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Arthur_Clive.Logger;
using AH = Arthur_Clive.Helper.AmazonHelper;
using MH = Arthur_Clive.Helper.MongoHelper;
using System.Linq;

namespace Arthur_Clive.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        public IMongoDatabase _db = MH._client.GetDatabase("UserInfo");
        public UpdateDefinition<BsonDocument> updateDefinition;

        [HttpPost("userinfo")]
        public async Task<ActionResult> Post([FromBody]UserInfo data)
        {
            try
            {
                if (data.ShippingAddress == null || data.BillingAddress == null)
                {
                    return BadRequest(new ResponseData
                    {
                        Code = "401",
                        Message = "Billing or Shipping Address Not Avaliable",
                        Data = null
                    });
                }
                else
                {
                    var filter = Builders<BsonDocument>.Filter.Eq("UserName", data.UserName);
                    var userInfo = MH.GetSingleObject(filter, "UserInfo", "UserInfo").Result;
                    if (userInfo == null)
                    {
                        var userCollection = _db.GetCollection<UserInfo>("UserInfo");
                        if (data.BillingAddress[0].Default == true)
                        {
                            data.BillingAddress[0].Default = true;
                            data.BillingAddress[0].AddressId = 1;
                        }
                        if (data.ShippingAddress != null)
                        {
                            data.ShippingAddress[0].Default = true;
                            data.ShippingAddress[0].AddressId = 1;
                        }
                        await userCollection.InsertOneAsync(data);
                        return Ok(new ResponseData
                        {
                            Code = "200",
                            Message = "Inserted",
                            Data = null
                        });
                    }
                    else
                    {
                        var oldData = BsonSerializer.Deserialize<UserInfo>(userInfo);
                        if (data.BillingAddress != null)
                        {
                            if (data.BillingAddress[0].Default == true)
                            {
                                foreach (var array in oldData.BillingAddress)
                                {
                                    array.Default = false;
                                }
                                updateDefinition = Builders<BsonDocument>.Update.Set("BillingAddress", oldData.BillingAddress);
                            }
                        }
                        if (data.ShippingAddress != null)
                        {
                            if (data.ShippingAddress[0].Default == true)
                            {
                                foreach (var array in oldData.ShippingAddress)
                                {
                                    array.Default = false;
                                }
                                updateDefinition = Builders<BsonDocument>.Update.Set("ShippingAddress", oldData.ShippingAddress);
                            }

                        }
                        if (data.BillingAddress != null && data.ShippingAddress != null)
                        {
                            if (data.BillingAddress[0].Default == true && data.ShippingAddress[0].Default == true)
                            {
                                updateDefinition = Builders<BsonDocument>.Update.Set("BillingAddress", oldData.BillingAddress).Set("ShippingAddress", oldData.ShippingAddress);
                            }
                        }
                        await MH.UpdateSingleObject(filter, "UserInfo", "UserInfo", updateDefinition);
                        string response = await Update(oldData, data);
                        return Ok(new ResponseData
                        {
                            Code = "200",
                            Message = "User updated" + response,
                            Data = null
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("UserController", "Post", "Post", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = null
                });
            }
        }

        public async Task<string> Update(UserInfo data, UserInfo updateData)
        {
            try
            {
                if (updateData.BillingAddress != null)
                {
                    updateData.BillingAddress[0].AddressId = data.BillingAddress.Count + 1;
                    data.BillingAddress.Add(updateData.BillingAddress[0]);
                    updateDefinition = Builders<BsonDocument>.Update.Set("BillingAddress", data.BillingAddress);
                }
                if (updateData.ShippingAddress != null)
                {
                    updateData.ShippingAddress[0].AddressId = data.ShippingAddress.Count + 1;
                    data.ShippingAddress.Add(updateData.ShippingAddress[0]);
                    updateDefinition = Builders<BsonDocument>.Update.Set("ShippingAddress", data.ShippingAddress);
                }
                if (updateData.BillingAddress != null && updateData.ShippingAddress != null)
                {
                    updateDefinition = Builders<BsonDocument>.Update.Set("BillingAddress", data.BillingAddress).Set("ShippingAddress", data.ShippingAddress);
                }
                var filter = Builders<BsonDocument>.Filter.Eq("UserName", data.UserName);
                await MH.UpdateSingleObject(filter, "UserInfo", "UserInfo", updateDefinition);
                return "Updated";
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("UserController", "Update", "Update", ex.Message);
                return "Failed";
            }
        }

        [HttpGet("userinfo")]
        public async Task<ActionResult> Get()
        {
            try
            {
                var userCollection = _db.GetCollection<UserInfo>("UserInfo");
                var filter = FilterDefinition<UserInfo>.Empty;
                IAsyncCursor<UserInfo> cursor = await userCollection.FindAsync(filter);
                var userInfo = cursor.ToList();
                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("UserController", "Get", "Get", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = null
                }); ;
            }
        }

        [HttpGet("userinfo/{username}")]
        public ActionResult GetOneUser(string username)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("UserName", username);
                var userInfo = BsonSerializer.Deserialize<UserInfo>(MH.GetSingleObject(filter, "UserInfo", "UserInfo").Result);
                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("UserController", "GetOneUser", "GetOneUser", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = null
                }); ;
            }
        }

        [HttpPost("cart/{username}")]
        public async Task<ActionResult> RefreshCart([FromBody]CartList data, string username)
        {
            try
            {
                var cartFilter = Builders<Cart>.Filter.Eq("UserName", username);
                var cartCollection = _db.GetCollection<Cart>("Cart");
                var result = cartCollection.DeleteManyAsync(cartFilter).Result;
                data.ListOfProducts.ToList().ForEach(c => c.UserName = username);

                var authCollection = _db.GetCollection<Cart>("Cart");
                await authCollection.InsertManyAsync(data.ListOfProducts);
                return Ok(new ResponseData
                {
                    Code = "200",
                    Message = "Inserted",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AuthController", "RefreshCart", "RefreshCart", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = ex.Message
                });
            }
        }

        [HttpGet("cart/{username}")]
        public async Task<ActionResult> GetProductsInCart(string username)
        {
            try
            {
                var collection = _db.GetCollection<Cart>("Cart");
                var filter = Builders<Cart>.Filter.Eq("UserName", username);
                IAsyncCursor<Cart> cursor = await collection.FindAsync(filter);
                var products = cursor.ToList();
                foreach (var data in products)
                {
                    string objectName = data.ProductSKU + ".jpg";
                    //data.ObjectUrl = WH.GetMinioObject("products", objectName).Result;
                    //data.ObjectUrl = AH.GetAmazonS3Object("arthurclive-products", objectName);
                    data.MinioObject_URL = AH.GetS3Object("arthurclive-products", objectName);
                }
                return Ok(new ResponseData
                {
                    Code = "200",
                    Message = "Success",
                    Data = products
                });
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AuthController", "GetProductsInCart", "GetProductsInCart", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = null
                });
            }
        }

        [HttpPost("wishlist/{username}")]
        public async Task<ActionResult> RefreshWishList([FromBody]WishlistList data, string username)
        {
            try
            {
                var wishlistFilter = Builders<WishList>.Filter.Eq("UserName", username);
                var wishlistCollection = _db.GetCollection<WishList>("WishList");
                var result = wishlistCollection.DeleteManyAsync(wishlistFilter).Result;
                data.ListOfProducts.ToList().ForEach(c => c.UserName = username);
                var authCollection = _db.GetCollection<WishList>("WishList");
                await authCollection.InsertManyAsync(data.ListOfProducts);
                return Ok(new ResponseData
                {
                    Code = "200",
                    Message = "Inserted",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AuthController", "RefreshWishList", "RefreshWishList", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = null
                });
            }
        }

        [HttpGet("wishlist/{username}")]
        public async Task<ActionResult> GetProductsInWishList(string username)
        {
            try
            {
                var collection = _db.GetCollection<WishList>("WishList");
                var filter = Builders<WishList>.Filter.Eq("UserName", username);
                IAsyncCursor<WishList> cursor = await collection.FindAsync(filter);
                var products = cursor.ToList();
                foreach (var data in products)
                {
                    string objectName = data.ProductSKU + ".jpg";
                    //data.ObjectUrl = WH.GetMinioObject("products", objectName).Result;
                    //data.ObjectUrl = AH.GetAmazonS3Object("arthurclive-products", objectName);
                    data.MinioObject_URL = AH.GetS3Object("arthurclive-products", objectName);
                }
                return Ok(new ResponseData
                {
                    Code = "200",
                    Message = "Success",
                    Data = products
                });
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AuthController", "GetProductsInWishList", "GetProductsInWishList", ex.Message);
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
