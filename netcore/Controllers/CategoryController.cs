﻿using System;
using System.Threading.Tasks;
using Arthur_Clive.Data;
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
    [Produces("application/json")]
    public class CategoryController : Controller
    {
        public IMongoDatabase _db = MH._client.GetDatabase("ProductDB");

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
                    string objectName = category.ProductFor + "-" + category.ProductType + ".jpg";
                    //category.MinioObject_URL = WH.GetMinioObject("products", objectName).Result;
                    //category.MinioObject_URL = AH.GetAmazonS3Object("product-category", objectName);
                    category.MinioObject_URL = AH.GetS3Object("product-category", objectName);
                }
                return Ok(new ResponseData
                {
                    Code = "200",
                    Message = "Success",
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
                    Data = ex.Message
                });
            }
        }

        #region Unused Post and Delete 
    
        [HttpPost]
        public async Task<ActionResult> Post([FromBody]Category product)
        {
            try
            {
                string objectName = product.ProductFor + "-" + product.ProductType + ".jpg";
                //product.MinioObject_URL = WH.GetMinioObject("products", objectName).Result;
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
                    Data = ex.Message
                });
            }
        }

        [HttpDelete("{productFor}/{productType}")]
        public ActionResult Delete(string productFor, string productType)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("ProductFor", productFor) & Builders<BsonDocument>.Filter.Eq("ProductType", productType);
                var product = MH.GetSingleObject(filter, "ProductDB", "Category").Result;
                if (product != null)
                {
                    var response = MH.DeleteSingleObject(filter, "ProductDB", "Category");
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
