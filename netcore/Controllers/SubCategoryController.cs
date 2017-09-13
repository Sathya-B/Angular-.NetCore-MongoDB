using System;
using Arthur_Clive.Data;
using Arthur_Clive.Helper;
using Arthur_Clive.Logger;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using WH = Arthur_Clive.Helper.MinioHelper;
using AH = Arthur_Clive.Helper.AmazonHelper;
using MH = Arthur_Clive.Helper.MongoHelper;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;

namespace Arthur_Clive.Controllers
{
    [Route("api/[controller]")]
    public class SubCategoryController : Controller
    {
        public IMongoDatabase _db = MH._client.GetDatabase("ProductDB");
        public MongoHelper mongoHelper = new MongoHelper();

        [HttpGet("{productFor}/{productType}")]
        public ActionResult Get(string productFor, string productType)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("Product_For", productFor) & Builders<BsonDocument>.Filter.Eq("Product_Type", productType);
                var userInfo = BsonSerializer.Deserialize<UserInfo>(mongoHelper.GetSingleObject(filter, "ProductDB", "Product").Result);
                return Ok(new ResponseData
                {
                    Code = "200",
                    Message = "Success",
                    Data = userInfo
                });
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("SubCategoryController", "Get", "Get Subcategories", ex.Message);
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