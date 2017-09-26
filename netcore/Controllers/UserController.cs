using System;
using System.Threading.Tasks;
using Arthur_Clive.Data;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Arthur_Clive.Logger;
using AH = Arthur_Clive.Helper.AmazonHelper;
using WH = Arthur_Clive.Helper.MinioHelper;
using MH = Arthur_Clive.Helper.MongoHelper;
using System.Linq;

namespace Arthur_Clive.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        public IMongoDatabase _db = MH._client.GetDatabase("UserInfo");
        public UpdateDefinition<BsonDocument> updateDefinition;

        [HttpPost("userinfo/{username}")]
        public async Task<ActionResult> RefreshUserInfo([FromBody]UserInfoList data,string username)
        {
            try
            {
                var addressFilter = Builders<Address>.Filter.Eq("UserName", username);
                var addressCollection = _db.GetCollection<Address>("UserInfo");
                var result = addressCollection.DeleteManyAsync(addressFilter).Result;
                data.ListOfAddress.ToList().ForEach(c => c.UserName = username);
                var authCollection = _db.GetCollection<Address>("UserInfo");
                await authCollection.InsertManyAsync(data.ListOfAddress);
                return Ok(new ResponseData
                {
                    Code = "200",
                    Message = "Inserted",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("UserController", "RefreshUserInfo", "RefreshUserInfo", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = ex.Message
                });
            }
        }
        
        [HttpGet("userinfo/address/{username}")]
        public async Task<ActionResult> GetAddressOfUser(string username)
        {
            try
            {
                var userCollection = _db.GetCollection<Address>("UserInfo");
                var filter = Builders<Address>.Filter.Eq("UserName", username);
                IAsyncCursor<Address> cursor = await userCollection.FindAsync(filter);
                var userInfo = cursor.ToList();
                return Ok(new ResponseData
                {
                    Code = "200",
                    Message = "Success",
                    Data = userInfo
                });
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("UserController", "GetAddressOfUser", "GetAddressOfUser", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = null
                }); 
            }
        }

        [HttpGet("userinfo/{username}")]
        public async Task<ActionResult> GetDefaultAddressOfUser(string username)
        {
            try
            {
                var userCollection = _db.GetCollection<Address>("UserInfo");
                var filter = Builders<Address>.Filter.Eq("UserName", username) & Builders<Address>.Filter.Eq("DefaultAddress", true);
                IAsyncCursor<Address> cursor = await userCollection.FindAsync(filter);
                var userInfo = cursor.ToList();
                return Ok(new ResponseData
                {
                    Code = "200",
                    Message = "Success",
                    Data = userInfo
                });
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("UserController", "GetDefaultAddressOfUser", "GetDefaultAddressOfUser", ex.Message);
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
                    Data = ex.Message
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
                    Data = ex.Message
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
                    Data = ex.Message
                });
            }
        }

    }
}
