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
                        if (data.BillingAddress[0].Default == "true")
                        {
                            data.BillingAddress[0].Default = "true";
                            data.BillingAddress[0].AddressId = 1;
                        }
                        if (data.ShippingAddress != null)
                        {
                            data.ShippingAddress[0].Default = "true";
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
                            if (data.BillingAddress[0].Default == "true")
                            {
                                foreach (var array in oldData.BillingAddress)
                                {
                                    array.Default = "false";
                                }
                                updateDefinition = Builders<BsonDocument>.Update.Set("BillingAddress", oldData.BillingAddress);
                            }
                        }
                        if (data.ShippingAddress != null)
                        {
                            if (data.ShippingAddress[0].Default == "true")
                            {
                                foreach (var array in oldData.ShippingAddress)
                                {
                                    array.Default = "false";
                                }
                                updateDefinition = Builders<BsonDocument>.Update.Set("ShippingAddress", oldData.ShippingAddress);
                            }

                        }
                        if (data.BillingAddress != null && data.ShippingAddress != null)
                        {
                            if (data.BillingAddress[0].Default == "true" && data.ShippingAddress[0].Default == "true")
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

        [HttpPost("cart")]
        public async Task<ActionResult> InsertToCart([FromBody]Cart data)
        {
            try
            {
                data.ProductSKU = data.ProductFor + "-" + data.ProductType + "-" + data.ProductDesign + "-" + data.ProductColour + "-" + data.ProductSize;
                string objectName = data.ProductSKU + ".jpg";
                //data.ObjectUrl = WH.GetMinioObject("products", objectName).Result;
                //data.ObjectUrl = AH.GetAmazonS3Object("arthurclive-products", objectName);
                data.ObjectUrl = AH.GetS3Object("arthurclive-products", objectName);
                data.ProductDiscountPrice = (data.ProductPrice - (data.ProductPrice * (data.ProductDiscount / 100)));
                var authCollection = _db.GetCollection<Cart>("Cart");
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
                LoggerDataAccess.CreateLog("AuthController", "InsertToCart", "InsertToCart", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = null
                });
            }
        }

        [HttpGet("cart")]
        public async Task<ActionResult> GetProductsInCart()
        {
            try
            {
                var collection = _db.GetCollection<Cart>("Cart");
                var filter = FilterDefinition<Cart>.Empty;
                IAsyncCursor<Cart> cursor = await collection.FindAsync(filter);
                var products = cursor.ToList();
                foreach (var data in products)
                {
                    string objectName = data.ProductSKU + ".jpg";
                    //data.ObjectUrl = WH.GetMinioObject("products", objectName).Result;
                    //data.ObjectUrl = AH.GetAmazonS3Object("arthurclive-products", objectName);
                    data.ObjectUrl = AH.GetS3Object("arthurclive-products", objectName);
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

        [HttpPut("cart/update")]
        public ActionResult UpdateProductInCart([FromBody]Cart data)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("ProductSKU", data.ProductSKU);
                var product = mongoHelper.GetSingleObject(filter, "UserInfo", "Cart").Result;
                if (product != null)
                {
                    var productData = BsonSerializer.Deserialize<Cart>(product);
                    if (productData.ProductDesign != null)
                    {
                        var update = Builders<BsonDocument>.Update.Set("ProductDesign", data.ProductDesign);
                        var result = mongoHelper.UpdateSingleObject(filter, "UserInfo", "Cart", update).Result;
                    }
                    if (productData.ProductQuantity != 0)
                    {
                        var update = Builders<BsonDocument>.Update.Set("ProductQuantity", data.ProductQuantity);
                        var result = mongoHelper.UpdateSingleObject(filter, "UserInfo", "Cart", update).Result;
                    }
                    if (productData.ProductSize != null)
                    {
                        var update = Builders<BsonDocument>.Update.Set("ProductSize", data.ProductSize);
                        var result = mongoHelper.UpdateSingleObject(filter, "UserInfo", "Cart", update).Result;
                    }
                    if (productData.ProductColour != null)
                    {
                        var update = Builders<BsonDocument>.Update.Set("ProductColour", data.ProductColour);
                        var result = mongoHelper.UpdateSingleObject(filter, "UserInfo", "Cart", update).Result;
                    }
                    var updatedProduct = BsonSerializer.Deserialize<Cart>(mongoHelper.GetSingleObject(filter, "UserInfo", "Cart").Result);
                    var productSKU = updatedProduct.ProductFor + "-" + updatedProduct.ProductType + "-" + updatedProduct.ProductDesign + "-" + updatedProduct.ProductColour + "-" + updatedProduct.ProductSize;
                    string objectName = productSKU + ".jpg";
                    //var ObjectUrl = WH.GetMinioObject("products", objectName).Result;
                    //var ObjectUrl = AH.GetAmazonS3Object("arthurclive-products", objectName);
                    var ObjectUrl = AH.GetS3Object("arthurclive-products", objectName);
                    var updateDefinition = Builders<BsonDocument>.Update.Set("ProductSKU", productSKU).Set("ObjectUrl", ObjectUrl);
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

        [HttpPost("cart/delete")]
        public ActionResult DeleteProductInCart([FromBody]Cart data)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("ProductSKU", data.ProductSKU);
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

        [HttpPost("wishlist")]
        public async Task<ActionResult> InsertToWishList([FromBody]WishList data)
        {
            try
            {
                data.ProductSKU = data.ProductFor + "-" + data.ProductType + "-" + data.ProductDesign + "-" + data.ProductColour + "-" + data.ProductSize;
                string objectName = data.ProductSKU + ".jpg";
                //data.ObjectUrl = WH.GetMinioObject("products", objectName).Result;
                //data.ObjectUrl = AH.GetAmazonS3Object("arthurclive-products", objectName);
                data.ObjectUrl = AH.GetS3Object("arthurclive-products", objectName);
                data.ProductDiscountPrice = (data.ProductPrice - (data.ProductPrice * (data.ProductDiscount / 100)));
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

        [HttpGet("wishlist")]
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
                    data.ObjectUrl = AH.GetS3Object("arthurclive-products", objectName);
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

        [HttpPut("wishlist/update")]
        public ActionResult UpdateProductInWishList([FromBody]Cart data)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("ProductSKU", data.ProductSKU);
                var product = mongoHelper.GetSingleObject(filter, "UserInfo", "WishList").Result;
                if (product != null)
                {
                    var productData = BsonSerializer.Deserialize<Cart>(product);
                    if (productData.ProductDesign != null)
                    {
                        var update = Builders<BsonDocument>.Update.Set("ProductDesign", data.ProductDesign);
                        var result = mongoHelper.UpdateSingleObject(filter, "UserInfo", "WishList", update).Result;
                    }
                    if (productData.ProductSize != null)
                    {
                        var update = Builders<BsonDocument>.Update.Set("ProductSize", data.ProductSize);
                        var result = mongoHelper.UpdateSingleObject(filter, "UserInfo", "WishList", update).Result;
                    }
                    if (productData.ProductColour != null)
                    {
                        var update = Builders<BsonDocument>.Update.Set("ProductColour", data.ProductColour);
                        var result = mongoHelper.UpdateSingleObject(filter, "UserInfo", "WishList", update).Result;
                    }
                    var updatedProduct = BsonSerializer.Deserialize<Cart>(mongoHelper.GetSingleObject(filter, "UserInfo", "WishList").Result);
                    var productSKU = updatedProduct.ProductFor + "-" + updatedProduct.ProductType + "-" + updatedProduct.ProductDesign + "-" + updatedProduct.ProductColour + "-" + updatedProduct.ProductSize;
                    string objectName = productSKU + ".jpg";
                    //var ObjectUrl = WH.GetMinioObject("products", objectName).Result;
                    //var ObjectUrl = AH.GetAmazonS3Object("arthurclive-products", objectName);
                    var ObjectUrl = AH.GetS3Object("arthurclive-products", objectName);
                    var updateDefinition = Builders<BsonDocument>.Update.Set("ProductSKU", productSKU).Set("ObjectUrl", ObjectUrl);
                    var response = mongoHelper.UpdateSingleObject(filter, "UserInfo", "WishList", updateDefinition).Result;
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
                LoggerDataAccess.CreateLog("AuthController", "UpdateProductInWishList", "UpdateProductInWishList", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = null
                });
            }
        }

        [HttpPost("wishlist/delete")]
        public ActionResult DeleteProductInWishList([FromBody]WishList data)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("ProductSKU", data.ProductSKU);
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
