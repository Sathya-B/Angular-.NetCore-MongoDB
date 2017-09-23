using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Minio;
using System;
using Arthur_Clive.Data;
using Arthur_Clive.Logger;
using System.Threading.Tasks;
using MongoDB.Driver;
using Arthur_Clive.Helper;
using AH = Arthur_Clive.Helper.AmazonHelper;
using MH = Arthur_Clive.Helper.MongoHelper;

namespace Arthur_Clive.Controllers
{
    [Route("api/[controller]")]
    public class ProductController : Controller
    {
        public IMongoDatabase _db = MH._client.GetDatabase("ProductDB");

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            try
            {
                var collection = _db.GetCollection<Product>("Product");
                var filter = FilterDefinition<Product>.Empty;
                IAsyncCursor<Product> cursor = await collection.FindAsync(filter);
                var products = cursor.ToList();
                foreach (var data in products)
                {
                    string objectName = data.ProductSKU + ".jpg";
                    //data.ObjectURL = WH.GetMinioObject("products", objectName).Result;
                    //data.ObjectURL = AH.GetAmazonS3Object("arthurclive-products", objectName);
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
                LoggerDataAccess.CreateLog("ProductController", "Get", "Get", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = ex.Message
                });
            }
        }

        #region Unused Post and Delete

        [HttpPost]
        public async Task<ActionResult> Post([FromBody]Product product)
        {
            try
            {
                product.ProductDiscountPrice = (product.ProductPrice - (product.ProductPrice * (product.ProductDiscount / 100)));
                product.ProductSKU = product.ProductFor + "-" + product.ProductType + "-" + product.ProductDesign + "-" + product.ProductColour + "-" + product.ProductSize;
                string objectName = product.ProductSKU + ".jpg";
                //product.MinioObject_URL = WH.GetMinioObject("arthurclive-products", objectName).Result;
                //product.MinioObject_URL = AH.GetAmazonS3Object("arthurclive-products", objectName);
                product.MinioObject_URL = AH.GetS3Object("arthurclive-products", objectName);
                var collection = _db.GetCollection<Product>("Product");
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
                LoggerDataAccess.CreateLog("ProductController", "Post", "Post", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = ex.Message
                });
            }
        }

        [HttpDelete("{productSKU}")]
        public ActionResult Delete(string productSKU)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("Product_SKU", productSKU);
                var product = MH.GetSingleObject(filter, "ProductDB", "Product").Result;
                if (product != null)
                {
                    var authCollection = _db.GetCollection<Product>("Product");
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
                        Code = "404",
                        Message = "Product Not Found",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("ProductController", "Delete", "Delete", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = ex.Message
                });
            }
        }

        #endregion
    }
}
