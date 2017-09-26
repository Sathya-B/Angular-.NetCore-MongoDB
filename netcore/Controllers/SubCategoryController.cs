using System;
using Arthur_Clive.Data;
using Arthur_Clive.Helper;
using Arthur_Clive.Logger;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using AH = Arthur_Clive.Helper.AmazonHelper;
using WH = Arthur_Clive.Helper.MinioHelper;
using MH = Arthur_Clive.Helper.MongoHelper;
using System.Threading.Tasks;

namespace Arthur_Clive.Controllers
{
    [Route("api/[controller]")]
    public class SubCategoryController : Controller
    {
        public IMongoDatabase _db = MH._client.GetDatabase("ProductDB");

        [HttpGet("{productFor}/{productType}")]
        public async Task<ActionResult> Get(string productFor, string productType)
        {
            try
            {
                var collection = _db.GetCollection<Product>("Product");
                var filter = Builders<Product>.Filter.Eq("ProductFor", productFor) & Builders<Product>.Filter.Eq("ProductType", productType);
                IAsyncCursor<Product> cursor = await collection.FindAsync(filter);
                var products = cursor.ToList();
                foreach (var product in products)
                {
                    string objectName = product.ProductSKU + ".jpg";
                    //product.MinioObject_URL = WH.GetMinioObject("arthurclive-products", objectName).Result;
                    //product.MinioObject_URL = AH.GetAmazonS3Object("arthurclive-products", objectName);
                    product.MinioObject_URL = AH.GetS3Object("arthurclive-products", objectName);
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
                LoggerDataAccess.CreateLog("SubCategoryController", "Get", "Get Subcategories", ex.Message);
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