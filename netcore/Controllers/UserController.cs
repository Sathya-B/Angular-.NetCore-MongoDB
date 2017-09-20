using System;
using System.Threading.Tasks;
using Arthur_Clive.Data;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Arthur_Clive.Helper;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Arthur_Clive.Logger;
using AH = Arthur_Clive.Helper.AmazonHelper;
using MH = Arthur_Clive.Helper.MongoHelper;
using System.Collections.Generic;
using System.Linq;

namespace Arthur_Clive.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        public IMongoDatabase _db = MH._client.GetDatabase("UserInfo");
        public MongoHelper mongoHelper = new MongoHelper();
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
                        Code = "400",
                        Message = "Billing or Shipping Address Not Avaliable",
                        Data = null
                    });
                }
                else
                {
                    var filter = Builders<BsonDocument>.Filter.Eq("PhoneNumber", data.PhoneNumber);
                    var userInfo = mongoHelper.GetSingleObject(filter, "UserInfo", "UserInfo").Result;
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
                        await mongoHelper.UpdateSingleObject(filter, "UserInfo", "UserInfo", updateDefinition);
                        string response = await Update(oldData, data);
                        return Ok(new ResponseData
                        {
                            Code = "200",
                            Message = response,
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
                var filter = Builders<BsonDocument>.Filter.Eq("PhoneNumber", data.PhoneNumber);
                await mongoHelper.UpdateSingleObject(filter, "UserInfo", "UserInfo", updateDefinition);
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

        [HttpGet("userinfo")]
        public ActionResult GetOneUser([FromBody]string phonenumber)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("PhoneNumber", phonenumber);
                var userInfo = BsonSerializer.Deserialize<UserInfo>(mongoHelper.GetSingleObject(filter, "UserInfo", "UserInfo").Result);
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
                //List<Cart> cartItems = new List<Cart>();
                //foreach (Cart cartItem in data.ListOfProducts)
                //{
                //    var productFilter = Builders<BsonDocument>.Filter.Eq("ProductSKU", cartItem.ProductSKU);
                //    var productCollection = mongoHelper.GetSingleObject(productFilter, "ProductDB", "Product").Result;
                //    var product = BsonSerializer.Deserialize<Product>(productCollection);
                //    cartItem.UserName = username;
                //    string objectName = cartItem.ProductSKU + ".jpg";
                //    //cartItem.ObjectUrl = WH.GetMinioObject("products", objectName).Result;
                //    //cartItem.ObjectUrl = AH.GetAmazonS3Object("arthurclive-products", objectName);
                //    cartItem.MinioObject_URL = AH.GetS3Object("arthurclive-products", objectName);
                //    cartItem.ProductFor = product.ProductFor;
                //    cartItem.ProductType = product.ProductType;
                //    cartItem.ProductDesign = product.ProductDesign;
                //    cartItem.ProductBrand = product.ProductBrand;
                //    cartItem.ProductPrice = product.ProductPrice;
                //    cartItem.ProductDiscount = product.ProductDiscount;
                //    cartItem.ProductDiscountPrice = product.ProductDiscountPrice;
                //    cartItem.ProductSize = product.ProductSize;
                //    cartItem.ProductColour = product.ProductColour;
                //    cartItem.ProductDescription = product.ProductDescription;
                //    cartItems.Add(cartItem);
                //}
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
                    Message = null,
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
                //List<WishList> wishlistItems = new List<WishList>();
                //foreach (WishList item in data.ListOfProducts)
                //{
                //    var filter = Builders<BsonDocument>.Filter.Eq("ProductSKU", item.ProductSKU);
                //    var product = BsonSerializer.Deserialize<Product>(mongoHelper.GetSingleObject(filter, "ProductDB", "Product").Result);
                //    item.UserName = username;
                //    string objectName = item.ProductSKU + ".jpg";
                //    //item.ObjectUrl = WH.GetMinioObject("products", objectName).Result;
                //    //item.ObjectUrl = AH.GetAmazonS3Object("arthurclive-products", objectName);
                //    item.MinioObject_URL = AH.GetS3Object("arthurclive-products", objectName);
                //    item.ProductFor = product.ProductFor;
                //    item.ProductType = product.ProductType;
                //    item.ProductDesign = product.ProductDesign;
                //    item.ProductBrand = product.ProductBrand;
                //    item.ProductPrice = product.ProductPrice;
                //    item.ProductDiscount = product.ProductDiscount;
                //    item.ProductDiscountPrice = product.ProductDiscountPrice;
                //    item.ProductSize = product.ProductSize;
                //    item.ProductColour = product.ProductColour;
                //    item.ProductDescription = product.ProductDescription;
                //    wishlistItems.Add(item);
                //}
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
                    Message = null,
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
