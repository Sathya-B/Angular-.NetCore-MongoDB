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
using Swashbuckle.AspNetCore.Examples;
using Arthur_Clive.Swagger;
namespace Arthur_Clive.Controllers
{
    /// <summary>Controller to refresh address, cart and wishlist, to get products in cart and wishlist and to get userinfo of user</summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        /// <summary></summary>
        public MongoClient _client;
        /// <summary></summary>
        public IMongoDatabase userinfo_db;
        /// <summary></summary>
        public IMongoCollection<Address> userinfo_collection;
        /// <summary></summary>
        public IMongoCollection<Cart> cart_collection;
        /// <summary></summary>
        public IMongoCollection<WishList> wishlist_collection;
        /// <summary></summary>
        public UpdateDefinition<BsonDocument> updateDefinition;
        /// <summary></summary>
        public IMongoDatabase logger_db;
        /// <summary></summary>
        public IMongoCollection<ApplicationLogger> serverlogCollection;

        /// <summary></summary>
        public UserController()
        {
            _client = MH.GetClient();
            userinfo_db = _client.GetDatabase("UserInfo");
            userinfo_collection = userinfo_db.GetCollection<Address>("UserInfo");
            cart_collection = userinfo_db.GetCollection<Cart>("Cart");
            wishlist_collection = userinfo_db.GetCollection<WishList>("WishList");
            logger_db = _client.GetDatabase("ArthurCliveLogDB");
            serverlogCollection = logger_db.GetCollection<ApplicationLogger>("ServerLog");
        }

        /// <summary>Get default address of user</summary>
        /// <param name="username">UserName of user</param>
        /// <remarks>This api is user to get default address of an user</remarks>
        /// <response code="200">Returns the default address of the user</response>
        /// <response code="400">Process ran into an exception</response>  
        [HttpGet("userinfo/{username}")]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public async Task<ActionResult> GetDefaultAddressOfUser(string username)
        {
            try
            {
                IAsyncCursor<Address> cursor = await userinfo_collection.FindAsync(Builders<Address>.Filter.Eq("UserName", username) & Builders<Address>.Filter.Eq("DefaultAddress", true));
                var userInfo = cursor.ToList();
                return Ok(new ResponseData { Code = "200", Message = "Success", Data = userInfo });
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("UserController", "GetDefaultAddressOfUser", ex.Message, serverlogCollection);
                return BadRequest(new ResponseData { Code = "400", Message = "Failed", Data = ex.Message }); ;
            }
        }

        /// <summary>Refresh address details of user</summary>
        /// <remarks>This api is uses to refresh userinfo of a user</remarks>
        /// <param name="data">List of address to be updated</param>
        /// <param name="username">UserName of user</param>
        /// <response code="200">Address List of user is refreshed</response>
        /// <response code="400">Process ran into an exception</response>    
        [HttpPost("userinfo/{username}")]
        [SwaggerRequestExample(typeof(AddressList), typeof(AddressDetail))]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public async Task<ActionResult> RefreshUserInfo([FromBody]AddressList data, string username)
        {
            try
            {
                var result = userinfo_collection.DeleteManyAsync(Builders<Address>.Filter.Eq("UserName", username)).Result;
                if (data.ListOfAddress.Count > 0)
                {
                    data.ListOfAddress.ToList().ForEach(c => c.UserName = username);
                    await userinfo_collection.InsertManyAsync(data.ListOfAddress);
                }
                return Ok(new ResponseData { Code = "200", Message = "Inserted" });
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("UserController", "RefreshUserInfo", ex.Message, serverlogCollection);
                return BadRequest(new ResponseData { Code = "400", Message = "Failed", Data = ex.Message });
            }
        }

        /// <summary>Get products in cart of user</summary>
        /// <param name="username">UserName of user</param>
        /// <remarks>This api is user to get products in cart of an user</remarks>
        /// <response code="200">Returns the products in cart</response>
        /// <response code="400">Process ran into an exception</response> 
        [HttpGet("cart/{username}")]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public async Task<ActionResult> GetProductsInCart(string username)
        {
            try
            {
                IAsyncCursor<Cart> cursor = await cart_collection.FindAsync(Builders<Cart>.Filter.Eq("UserName", username));
                var products = cursor.ToList();
                foreach (var data in products)
                {
                    string objectName = data.ProductSKU + ".jpg";
                    //data.ObjectUrl = WH.GetMinioObject("products", objectName).Result;
                    //data.ObjectUrl = AH.GetAmazonS3Object("arthurclive-products", objectName);
                    data.MinioObject_URL = AH.GetS3Object("arthurclive-products", objectName);
                }
                return Ok(new ResponseData { Code = "200", Message = "Success", Data = products });
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("UserController", "GetProductsInCart", ex.Message, serverlogCollection);
                return BadRequest(new ResponseData { Code = "400", Message = "Failed", Data = ex.Message });
            }
        }

        /// <summary>Refresh products in cart</summary>
        /// <remarks>This api is used to refresh products in cart of the user</remarks>
        /// <param name="data">List of cart details to be updated</param>
        /// <param name="username">UserName of user</param>
        /// <response code="200">Cart of user is refreshed succeddfully</response>
        /// <response code="400">Process ran into an exception</response> 
        [HttpPost("cart/{username}")]
        [SwaggerRequestExample(typeof(CartList), typeof(CartDetail))]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public async Task<ActionResult> RefreshCart([FromBody]CartList data, string username)
        {
            try
            {
                var result = cart_collection.DeleteManyAsync(Builders<Cart>.Filter.Eq("UserName", username)).Result;
                if (data.ListOfProducts.Count > 0)
                {
                    data.ListOfProducts.ToList().ForEach(c => c.UserName = username);
                    await cart_collection.InsertManyAsync(data.ListOfProducts);
                }
                return Ok(new ResponseData { Code = "200", Message = "Inserted", Data = null });
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("UserController", "RefreshCart", ex.Message, serverlogCollection);
                return BadRequest(new ResponseData { Code = "400", Message = "Failed", Data = ex.Message });
            }
        }

        /// <summary>Get products in wishlist of user</summary>
        /// <param name="username">UserName of user</param>
        /// <remarks>This api is user to get products in wishlist of an user</remarks>
        /// <response code="200">Returns the products in wishlist</response>
        /// <response code="400">Process ran into an exception</response> 
        [HttpGet("wishlist/{username}")]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public async Task<ActionResult> GetProductsInWishList(string username)
        {
            try
            {
                IAsyncCursor<WishList> cursor = await wishlist_collection.FindAsync(Builders<WishList>.Filter.Eq("UserName", username));
                var products = cursor.ToList();
                foreach (var data in products)
                {
                    string objectName = data.ProductSKU + ".jpg";
                    //data.ObjectUrl = WH.GetMinioObject("products", objectName).Result;
                    //data.ObjectUrl = AH.GetAmazonS3Object("arthurclive-products", objectName);
                    data.MinioObject_URL = AH.GetS3Object("arthurclive-products", objectName);
                }
                return Ok(new ResponseData { Code = "200", Message = "Success", Data = products });
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("UserController", "GetProductsInWishList", ex.Message, serverlogCollection);
                return BadRequest(new ResponseData { Code = "400", Message = "Failed", Data = ex.Message });
            }
        }

        /// <summary>Refresh products in wishlist</summary>
        /// <remarks>This api is used to refresh products in wishlist of the user</remarks>
        /// <param name="data">List of wishlist details to be updated</param>
        /// <param name="username">UserName of user</param>
        /// <response code="200">Wishlist of user is refreshed successfully</response>
        /// <response code="400">Process ran into an exception</response> 
        [HttpPost("wishlist/{username}")]
        [SwaggerRequestExample(typeof(WishlistList), typeof(WishlistDetail))]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public async Task<ActionResult> RefreshWishList([FromBody]WishlistList data, string username)
        {
            try
            {
                var result = wishlist_collection.DeleteManyAsync(Builders<WishList>.Filter.Eq("UserName", username)).Result;
                if (data.ListOfProducts.Count > 0)
                {
                    data.ListOfProducts.ToList().ForEach(c => c.UserName = username);
                    await wishlist_collection.InsertManyAsync(data.ListOfProducts);
                }
                return Ok(new ResponseData { Code = "200", Message = "Inserted" });
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("UserController", "RefreshWishList", ex.Message, serverlogCollection);
                return BadRequest(new ResponseData { Code = "400", Message = "Failed", Data = ex.Message });
            }
        }

    }
}
