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
using WH = Arthur_Clive.Helper.MinioHelper;
using MH = Arthur_Clive.Helper.MongoHelper;

namespace Arthur_Clive.Controllers
{
    /// <summary>Controller to get, post and delete products</summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class ProductController : Controller
    {
        /// <summary></summary>
        public IMongoDatabase _db = MH._client.GetDatabase("ProductDB");

        /// <summary>Get all the products </summary>
        /// <remarks>This api is used to get all the products</remarks>
        /// <response code="200">Returns success message</response>
        /// <response code="404">No products found</response> 
        /// <response code="400">Process ran into an exception</response>   
        [HttpGet]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public async Task<ActionResult> Get()
        {
            try
            {
                var collection = _db.GetCollection<Product>("Product");
                var filter = FilterDefinition<Product>.Empty;
                IAsyncCursor<Product> cursor = await collection.FindAsync(filter);
                var products = cursor.ToList();
                if (products.Count > 0)
                {
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
                else
                {
                    return BadRequest(new ResponseData
                    {
                        Code = "404",
                        Message = "No products found",
                        Data = null
                    });
                }
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

        /// <summary>Get products with filter</summary>
        /// <param name="productFor">For whom is the product</param>
        /// <param name="productType">type of product</param>
        /// <param name="productDesign">Design on product</param>
        /// <response code="200">Returns success message</response>
        /// <response code="404">No products found</response> 
        /// <response code="400">Process ran into an exception</response> 
        [HttpGet("{productFor}/{productType}/{productDesign}")]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public async Task<ActionResult> GetProductByFilter(string productFor,string productType,string productDesign)
        {
            try
            {
                var collection = _db.GetCollection<Product>("Product");
                var filter = Builders<Product>.Filter.Eq("ProductFor", productFor) & Builders<Product>.Filter.Eq("ProductType", productType) & Builders<Product>.Filter.Eq("ProductDesign",productDesign);
                IAsyncCursor<Product> cursor = await collection.FindAsync(filter);
                var products = cursor.ToList();
                if (products.Count > 0)
                {
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
                else
                {
                    return BadRequest(new ResponseData
                    {
                        Code = "404",
                        Message = "No products found",
                        Data = null
                    });
                }
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

        /// <summary>Insert a new products </summary>
        /// <remarks>This api is used to insert a new product</remarks>
        /// <param name="product">Details of product to be inserted</param>
        /// <response code="200">Product inserted successfully</response>
        /// <response code="400">Process ran into an exception</response>   
        [HttpPost]
        [ProducesResponseType(typeof(ResponseData), 200)]
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

        /// <summary>Delete a product</summary>
        /// <param name="productSKU">SKU of product to be deleted</param>
        /// <remarks>This api is used to delete a product</remarks>
        /// <response code="200">Returns delete product details and success message</response>
        /// <response code="404">No product found</response>   
        /// <response code="400">Process ran into an exception</response>   
        [HttpDelete("{productSKU}")]
        [ProducesResponseType(typeof(ResponseData), 200)]
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
    }
}
