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
        public async Task<ActionResult> InsertToCart([FromBody]Cart data, string username)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("ProductSKU", data.ProductSKU);
                var product = BsonSerializer.Deserialize<Product>(mongoHelper.GetSingleObject(filter, "ProductDB", "Product").Result);
                var cartFilter = Builders<BsonDocument>.Filter.Eq("UserName", username) & Builders<BsonDocument>.Filter.Eq("ProductSKU", data.ProductSKU);
                var cart = mongoHelper.GetSingleObject(filter, "UserInfo", "Cart").Result;
                if (product == null)
                {
                    data.UserName = username;
                    string objectName = data.ProductSKU + ".jpg";
                    //data.ObjectUrl = WH.GetMinioObject("products", objectName).Result;
                    //data.ObjectUrl = AH.GetAmazonS3Object("arthurclive-products", objectName);
                    data.MinioObject_URL = AH.GetS3Object("arthurclive-products", objectName);
                    data.ProductFor = product.ProductFor;
                    data.ProductType = product.ProductType;
                    data.ProductDesign = product.ProductDesign;
                    data.ProductBrand = product.ProductBrand;
                    data.ProductPrice = product.ProductPrice;
                    data.ProductDiscount = product.ProductDiscount;
                    data.ProductDiscountPrice = product.ProductDiscountPrice;
                    data.ProductSize = product.ProductSize;
                    data.ProductColour = product.ProductColour;
                    data.ProductDescription = product.ProductDescription;
                    var authCollection = _db.GetCollection<Cart>("Cart");
                    await authCollection.InsertOneAsync(data);
                }
                else
                {
                    var cartData = BsonSerializer.Deserialize<Cart>(cart);
                    long previousProductQuanitity = cartData.ProductQuantity;
                    var update = Builders<BsonDocument>.Update.Set("ProductQuantity", previousProductQuanitity + data.ProductQuantity);
                    var result = mongoHelper.UpdateSingleObject(filter, "UserInfo", "Cart", update).Result;
                }
                return Ok(new ResponseData
                {
                    Code = "200",
                    Message = "Inserted",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AuthController", "InsertToCart", "InsertToCart", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = null
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

        [HttpPut("cart/update/{username}")]
        public ActionResult UpdateProductInCart([FromBody]Cart data, string username)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("UserName", username) & Builders<BsonDocument>.Filter.Eq("ProductSKU", data.ProductSKU);
                var product = mongoHelper.GetSingleObject(filter, "UserInfo", "Cart").Result;
                if (product != null)
                {
                    var productData = BsonSerializer.Deserialize<Cart>(product);
                    if (data.ProductQuantity != 0)
                    {
                        var update = Builders<BsonDocument>.Update.Set("ProductQuantity", data.ProductQuantity);
                        var result = mongoHelper.UpdateSingleObject(filter, "UserInfo", "Cart", update).Result;
                    }
                    var updatedProduct = BsonSerializer.Deserialize<Cart>(mongoHelper.GetSingleObject(filter, "UserInfo", "Cart").Result);
                    string objectName = data.ProductSKU + ".jpg";
                    //var ObjectUrl = WH.GetMinioObject("products", objectName).Result;
                    //var ObjectUrl = AH.GetAmazonS3Object("arthurclive-products", objectName);
                    var ObjectUrl = AH.GetS3Object("arthurclive-products", objectName);
                    var updateDefinition = Builders<BsonDocument>.Update.Set("ProductSKU", data.ProductSKU).Set("ObjectUrl", ObjectUrl);
                    var response = mongoHelper.UpdateSingleObject(filter, "UserInfo", "Cart", updateDefinition).Result;
                    return Ok(new ResponseData
                    {
                        Code = "200",
                        Message = "Updated",
                        Data = null
                    });
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
                LoggerDataAccess.CreateLog("AuthController", "UpdateProductInCart", "UpdateProductInCart", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = null
                });
            }
        }

        [HttpDelete("cart/delete/{username}")]
        public ActionResult DeleteProductInCart([FromBody]Cart data, string username)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("UserName", username) & Builders<BsonDocument>.Filter.Eq("ProductSKU", data.ProductSKU);
                var product = mongoHelper.GetSingleObject(filter, "UserInfo", "Cart").Result;
                if (product != null)
                {
                    var authCollection = _db.GetCollection<Cart>("Cart");
                    var response = authCollection.DeleteOneAsync(product);
                    return Ok(new ResponseData
                    {
                        Code = "200",
                        Message = "Deleted",
                        Data = null
                    });
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
                LoggerDataAccess.CreateLog("AuthController", "DeleteProductInCart", "DeleteProductInCart", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = null
                });
            }
        }

        [HttpPost("wishlist/{username}")]
        public async Task<ActionResult> InsertToWishList([FromBody]WishList data, string username)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("ProductSKU", data.ProductSKU);
                var product = BsonSerializer.Deserialize<Product>(mongoHelper.GetSingleObject(filter, "ProductDB", "Product").Result);
                data.UserName = username;
                string objectName = data.ProductSKU + ".jpg";
                //data.ObjectUrl = WH.GetMinioObject("products", objectName).Result;
                //data.ObjectUrl = AH.GetAmazonS3Object("arthurclive-products", objectName);
                data.MinioObject_URL = AH.GetS3Object("arthurclive-products", objectName);
                data.ProductFor = product.ProductFor;
                data.ProductType = product.ProductType;
                data.ProductDesign = product.ProductDesign;
                data.ProductBrand = product.ProductBrand;
                data.ProductPrice = product.ProductPrice;
                data.ProductDiscount = product.ProductDiscount;
                data.ProductDiscountPrice = product.ProductDiscountPrice;
                data.ProductSize = product.ProductSize;
                data.ProductColour = product.ProductColour;
                data.ProductDescription = product.ProductDescription;
                var authCollection = _db.GetCollection<WishList>("WishList");
                await authCollection.InsertOneAsync(data);
                return Ok(new ResponseData
                {
                    Code = "200",
                    Message = "Inserted",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("AuthController", "InsertToWishList", "InsertToWishList", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = null
                });
            }
        }

        [HttpGet("wishlist/{username}")]
        public async Task<ActionResult> GetProductsInWishList()
        {
            try
            {
                var collection = _db.GetCollection<WishList>("WishList");
                var filter = FilterDefinition<WishList>.Empty;
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

        [HttpDelete("wishlist/delete/{username}")]
        public ActionResult DeleteProductInWishList([FromBody]WishList data, string username)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("UserName", username) & Builders<BsonDocument>.Filter.Eq("ProductSKU", data.ProductSKU);
                var product = mongoHelper.GetSingleObject(filter, "UserInfo", "WishList").Result;
                if (product != null)
                {
                    var authCollection = _db.GetCollection<WishList>("WishList");
                    var response = authCollection.DeleteOneAsync(product);
                    return Ok(new ResponseData
                    {
                        Code = "200",
                        Message = "Deleted",
                        Data = null
                    });
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
                LoggerDataAccess.CreateLog("AuthController", "DeleteProductInWishList", "DeleteProductInWishList", ex.Message);
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
