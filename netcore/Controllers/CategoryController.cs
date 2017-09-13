using System;
using System.Threading.Tasks;
using Arthur_Clive.Data;
using Arthur_Clive.Helper;
using Arthur_Clive.Logger;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using AH = Arthur_Clive.Helper.AmazonHelper;
using WH = Arthur_Clive.Helper.MinioHelper;
using MH = Arthur_Clive.Helper.MongoHelper;
using MongoDB.Bson;

namespace Arthur_Clive.Controllers
{
    [Route("api/[controller]")]
    public class CategoryController : Controller
    {
        public IMongoDatabase _db = MH._client.GetDatabase("ProductDB");
        public MongoHelper mongoHelper = new MongoHelper();

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            try
            {
                var collection = _db.GetCollection<Category>("Category");
                var filter = FilterDefinition<Category>.Empty;
                IAsyncCursor<Category> cursor = await collection.FindAsync(filter);
                var categories = cursor.ToList();
                foreach (var category in categories)
                {
                    string objectName = category.Product_For + "-" + category.Product_Type + ".jpg";
                    //category.ObjectUrl = WH.GetMinioObject("products", objectName).Result;
                    //category.ObjectUrl = AH.GetAmazonS3Object("arthurclive-products", objectName);
                    category.MinioObject_URL = AH.GetS3Object("arthurclive-products", objectName);
                }
                return Ok(new ResponseData
                {
                    Code = "200",
                    Message = null,
                    Data = categories
                });
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("CategoryController", "Get", "Get", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = null
                });
            }
        }

        #region Unused Post and Delete 

        [HttpPost]
        public async Task<ActionResult> Post([FromBody]Category product)
        {
            try
            {
                string objectName = product.Product_For + "-" + product.Product_Type + ".jpg";
                //product.MinioObject_URL = WH.GetMinioObject("product-category", objectName).Result;
                //product.MinioObject_URL = AH.GetAmazonS3Object("product-category", objectName);
                product.MinioObject_URL = AH.GetS3Object("product-category", objectName);
                var collection = _db.GetCollection<Category>("Category");
                await collection.InsertOneAsync(product);
                return Ok(new ResponseData
                {
                    Code = "200",
                    Message = "Inserted",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("CategoryController", "Post", "Post", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = null
                });
            }
        }

        [HttpDelete("{productFor}/{productType}")]
        public ActionResult Delete(string productFor, string productType)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("Product_For", productFor) & Builders<BsonDocument>.Filter.Eq("Product_Type", productType);
                var product = mongoHelper.GetSingleObject(filter, "ProductDB", "Category").Result;
                if (product != null)
                {
                    var authCollection = _db.GetCollection<Category>("Category");
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
                LoggerDataAccess.CreateLog("CategoryController", "Delete", "Delete", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = null
                });
            }
        }

        #endregion
    }
}
