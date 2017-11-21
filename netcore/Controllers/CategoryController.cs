using System;
using System.Threading.Tasks;
using Arthur_Clive.Data;
using Arthur_Clive.Logger;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using AH = Arthur_Clive.Helper.AmazonHelper;
using WH = Arthur_Clive.Helper.MinioHelper;
using MH = Arthur_Clive.Helper.MongoHelper;
using MongoDB.Bson;
using Swashbuckle.AspNetCore.Examples;
using Arthur_Clive.Swagger;
using MongoDB.Bson.Serialization;
using Microsoft.AspNetCore.Authorization;

namespace Arthur_Clive.Controllers
{
    /// <summary>Controller to get, post and delete product category</summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class CategoryController : Controller
    {
        /// <summary></summary>
        public MongoClient _client;
        /// <summary></summary>
        public IMongoDatabase product_db;
        /// <summary></summary>
        public IMongoCollection<BsonDocument> category_collection;
        /// <summary></summary>
        public IMongoCollection<Category> categoryCollection;
        /// <summary></summary>
        public IMongoDatabase logger_db;
        /// <summary></summary>
        public IMongoCollection<ApplicationLogger> serverlogCollection;

        /// <summary>
        /// 
        /// </summary>
        public CategoryController()
        {
            _client = MH.GetClient();
            product_db = _client.GetDatabase("ProductDB");
            category_collection = product_db.GetCollection<BsonDocument>("Category");
            categoryCollection = product_db.GetCollection<Category>("Category");
            logger_db = _client.GetDatabase("ArthurCliveLogDB");
            serverlogCollection = logger_db.GetCollection<ApplicationLogger>("ServerLog");
        }

        /// <summary>Get categories</summary>
        /// <remarks>This api gets all the categories</remarks>
        /// <response code="200">Returns categories</response>
        /// <response code="404">No categories found</response> 
        /// <response code="400">Process ran into an exception</response>  
        [HttpGet]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public async Task<ActionResult> Get()
        {
            try
            {
                IAsyncCursor<Category> cursor = await categoryCollection.FindAsync(FilterDefinition<Category>.Empty);
                var categories = cursor.ToList();
                if (categories.Count > 0)
                {
                    foreach (var category in categories)
                    {
                        string objectName = category.ProductFor + "-" + category.ProductType + ".jpg";
                        //category.MinioObject_URL = WH.GetMinioObject("products", objectName).Result;
                        //category.MinioObject_URL = AH.GetAmazonS3Object("product-category", objectName);
                        category.MinioObject_URL = AH.GetS3Object("product-category", objectName);
                    }
                    return Ok(new ResponseData { Code = "200", Message = "Success", Data = categories });
                }
                else
                {
                    return BadRequest(new ResponseData { Code = "404", Message = "No categories found" });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("CategoryController", "Get", ex.Message, serverlogCollection);
                return BadRequest(new ResponseData { Code = "400", Message = "Failed", Data = ex.Message });
            }
        }

        /// <summary>Insert new category</summary>
        /// <remarks>This api inserts a new category</remarks>
        /// <param name="category">Category to be inserted</param>
        /// <response code="200">Category inserted successfully</response>
        /// <response code="401">Category with same product for and product type is found</response>  
        /// <response code="400">Process ran into an exception</response>  
        [Authorize("Level1Access")]
        [HttpPost]
        [SwaggerRequestExample(typeof(Category), typeof(InsertCategoryDetails))]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public async Task<ActionResult> Post([FromBody]Category category)
        {
            try
            {
                var check = MH.CheckForDatas(category_collection, "ProductFor", category.ProductFor, "ProductType", category.ProductType);
                if (check == false)
                {
                    string objectName = category.ProductFor + "-" + category.ProductType + ".jpg";
                    //product.MinioObject_URL = WH.GetMinioObject("products", objectName).Result;
                    //product.MinioObject_URL = AH.GetAmazonS3Object("product-category", objectName);
                    category.MinioObject_URL = AH.GetS3Object("product-category", objectName);
                    await categoryCollection.InsertOneAsync(category);
                    return Ok(new ResponseData { Code = "200", Message = "Inserted" });
                }
                else if (check == true)
                {
                    return BadRequest(new ResponseData { Code = "401", Message = "Category with same product for and product type is found" });
                }
                else
                {
                    return BadRequest(new ResponseData { Code = "400", Message = "Failed" });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("CategoryController", "Post", ex.Message, serverlogCollection);
                return BadRequest(new ResponseData { Code = "400", Message = "Failed", Data = ex.Message });
            }
        }

        /// <summary>Delete a category</summary>
        /// <param name="productFor">Whom is the product for</param>
        /// <param name="productType">Type of product</param>
        /// <remarks>This api deletes a category</remarks>
        /// <response code="200">Category deleted</response>
        /// <response code="404">Category not found</response> 
        /// <response code="400">Process ran into an exception</response>  
        [Authorize("Level1Access")]
        [HttpDelete("{productFor}/{productType}")]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public ActionResult Delete(string productFor, string productType)
        {
            try
            {
                var product = MH.GetSingleObject(category_collection, "ProductFor", productFor, "ProductType", productType).Result;
                if (product != null)
                {
                    var response = MH.DeleteSingleObject(category_collection, "ProductFor", productFor, "ProductType", productType);
                    return Ok(new ResponseData { Code = "200", Message = "Deleted" });
                }
                else
                {
                    return BadRequest(new ResponseData { Code = "404", Message = "Category Not Found" });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("CategoryController", "Delete", ex.Message, serverlogCollection);
                return BadRequest(new ResponseData { Code = "400", Message = "Failed", Data = ex.Message });
            }
        }

        /// <summary>Update category details</summary>
        /// <param name="data">Details of category</param>
        /// <param name="productFor">For whom is the category</param>
        /// <param name="productType">Type of category</param>
        /// <response code="200">Category updated successfully</response>
        /// <response code="404">No category found</response>   
        /// <response code="400">Process ran into an exception</response>   
        [Authorize("Level1Access")]
        [HttpPut("{productFor}/{productType}")]
        [SwaggerRequestExample(typeof(UpdateCategory), typeof(UpdateCategoryDetails))]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public async Task<ActionResult> Update([FromBody]UpdateCategory data, string productFor, string productType)
        {
            try
            {
                var checkData = MH.GetSingleObject(category_collection, "ProductFor", productFor, "ProductType", productType).Result;
                if (checkData != null)
                {
                    var objectId = BsonSerializer.Deserialize<Category>(checkData).Id;
                    if (data.ProductFor != null)
                    {
                        var objectName = data.ProductFor + "-" + BsonSerializer.Deserialize<Category>(MH.GetSingleObject(category_collection, "_id", objectId, null, null).Result).ProductType;
                        await MH.UpdateCategoryDetails(category_collection, BsonSerializer.Deserialize<Category>(checkData).Id, productFor, productType, data.ProductFor, "ProductFor", objectName + ".jpg");
                    }
                    if (data.ProductType != null)
                    {
                        var objectName = BsonSerializer.Deserialize<Category>(MH.GetSingleObject(category_collection, "_id", objectId, null, null).Result).ProductFor + "-" + data.ProductType;
                        await MH.UpdateCategoryDetails(category_collection, BsonSerializer.Deserialize<Category>(checkData).Id, productFor, productType, data.ProductType, "ProductType", objectName + ".jpg");
                    }
                    if (data.Description != null)
                    {
                        var update = await MH.UpdateSingleObject(category_collection, "_id", objectId, null, null, Builders<BsonDocument>.Update.Set("Description", data.Description));
                    }
                    return Ok(new ResponseData { Code = "200", Message = "Updated" });
                }
                else
                {
                    return BadRequest(new ResponseData { Code = "404", Message = "Category not found" });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("CategoryController", "Update", ex.Message, serverlogCollection);
                return BadRequest(new ResponseData { Code = "400", Message = "Failed", Data = ex.Message });
            }
        }
    }
}
