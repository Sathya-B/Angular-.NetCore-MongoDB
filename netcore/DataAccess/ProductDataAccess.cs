using MongoDB.Driver;
using System.Collections.Generic;
using System;
using System.Linq;
using Minio;
using Arthur_Clive.Data;
using WH = Arthur_Clive.Helper.WebApiHelper;
using Arthur_Clive.Logger;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace Arthur_Clive.DataAccess
{
    public class ProductDataAccess
    {
        public MongoClient _client;
        public MongoServer _server;
        public IMongoDatabase _db;
        public MinioClient minio = WH.GetMinioClient();
        public string presignedUrl;
        public string minioObjName;

        public ProductDataAccess()
        {
            _client = WH.GetClient();
            _db = _client.GetDatabase("ProductDB");
        }

        public async Task<IEnumerable<Product>> GetProducts()
        {
            try
            {
                var collection = _db.GetCollection<Product>("Product");
                var filter = FilterDefinition<Product>.Empty;
                IAsyncCursor<Product> cursor = await collection.FindAsync(filter);
                var products = cursor.ToList();
                foreach (var product in products)
                {
                    var minioObjName = product.Product_SKU + ".jpg";
                    product.MinioObject_Url = WH.GetMinioObject(minio, "products", minioObjName).Result;
                }
                return products;
            }
            catch (Exception ex)
            {
                ApplicationLogger logger =
                     new ApplicationLogger
                     {
                         Controller = "ProductDataAccess",
                         MethodName = "GetProducts",
                         Method = "GetProducts",
                         Description = ex.Message
                     };
                CreateLog(logger);
                List<Product> productList = new List<Product>();
                return productList;
            }
        }

        public string Create(Product product)
        {
            try
            {
                var collection = _db.GetCollection<Product>("Product");
                collection.InsertOneAsync(product);
                return "Created";
            }
            catch (Exception ex)
            {
                ApplicationLogger logger =
                   new ApplicationLogger
                   {
                       Controller = "ProductDataAccess",
                       MethodName = "Create",
                       Method = "Create",
                       Description = ex.Message
                   };
                CreateLog(logger);
                return "Failed";
            }
        }

        public async Task<string> Update(ObjectId id, Product product)
        {
            try
            {
                minioObjName = product.Product_SKU + ".jpg";
                product.MinioObject_Url = WH.GetMinioObject(minio, "products", minioObjName).Result;
                var collection = _db.GetCollection<Product>("Product");
                var Deleteone = await collection.DeleteOneAsync(Builders<Product>.Filter.Eq("Id", id));
                product.Id = id;
                await collection.InsertOneAsync(product);
                return "Updated";
            }
            catch (Exception ex)
            {
                ApplicationLogger logger =
                   new ApplicationLogger
                   {
                       Controller = "ProductDataAccess",
                       MethodName = "Updated",
                       Method = "Updated",
                       Description = ex.Message
                   };
                CreateLog(logger);
                return "Updated";
            }
        }

        public async Task<string> Remove(ObjectId id)
        {
            try
            {
                var collection = _db.GetCollection<Product>("Product");
                var Deleteone = await collection.DeleteOneAsync(Builders<Product>.Filter.Eq("Id", id));
                return "Removed";
            }
            catch (Exception ex)
            {
                ApplicationLogger logger =
                  new ApplicationLogger
                  {
                      Controller = "ProductDataAccess",
                      MethodName = "Failed",
                      Method = "Failed",
                      Description = ex.Message
                  };
                CreateLog(logger);
                return "Failed";
            }
        }

        public async Task<IEnumerable<Product>> GetProductsForSubCategoryAsync(string productFor, string productType)
        {
            try
            {
                var filter = "{ Product_For: '"+ productFor + "' , Product_Type: '" + productType + "'}";
                var collection = _db.GetCollection<Product>("Product");
                IAsyncCursor<Product> cursor = await collection.FindAsync(filter);
                var products = cursor.ToList();
                foreach (var product in products.ToList())
                {
                    var minioObjName = product.Product_SKU + ".jpg";
                    product.MinioObject_Url = WH.GetMinioObject(minio, "products", minioObjName).Result;
                }
                return products.ToList();
            }
            catch(Exception ex)
            {
                ApplicationLogger logger =
                     new ApplicationLogger
                     {
                         Controller = "ProductDataAccess",
                         MethodName = "GetProductsForSubCategory",
                         Method = "GetProductsForSubCategory",
                         Description = ex.Message
                     };
                CreateLog(logger);
                List<Product> productList = new List<Product>();
                return productList;
            }
        }
        
        public async Task<IEnumerable<Product>> GetProductsForSubDivisionByDesignAsync(string productFor, string productType,string productDesign)
        {
            try
            {

                var filter = "{ Product_For: '" + productFor + "' , Product_Type: '" + productType + "', Product_Design: '" + productDesign + "'}";
                var collection = _db.GetCollection<Product>("Product");
                IAsyncCursor<Product> cursor = await collection.FindAsync(filter);
                var products = cursor.ToList();
                foreach (var product in products.ToList())
                {
                    var minioObjName = product.Product_SKU + ".jpg";
                    product.MinioObject_Url = WH.GetMinioObject(minio, "products", minioObjName).Result;
                }
                return products.ToList();
            }
            catch (Exception ex)
            {
                ApplicationLogger logger =
                     new ApplicationLogger
                     {
                         Controller = "ProductDataAccess",
                         MethodName = "GetProductsForSubCategory",
                         Method = "GetProductsForSubCategory",
                         Description = ex.Message
                     };
                CreateLog(logger);
                List<Product> productList = new List<Product>();
                return productList;
            }
        }

        public async Task<IEnumerable<Product>> GetProductsForSubDivisionByColourAsync(string productFor, string productType, string productDesign,string productColour)
        {
            try
            {

                var filter = "{ Product_For: '" + productFor + "' , Product_Type: '" + productType + "', Product_Design: '" + productDesign + "', Product_Colour: '" + productColour + "'}";
                var collection = _db.GetCollection<Product>("Product");
                IAsyncCursor<Product> cursor = await collection.FindAsync(filter);
                var products = cursor.ToList();
                foreach (var product in products.ToList())
                {
                    var minioObjName = product.Product_SKU + ".jpg";
                    product.MinioObject_Url = WH.GetMinioObject(minio, "products", minioObjName).Result;
                }
                return products.ToList();
            }
            catch (Exception ex)
            {
                ApplicationLogger logger =
                     new ApplicationLogger
                     {
                         Controller = "ProductDataAccess",
                         MethodName = "GetProductsForSubCategory",
                         Method = "GetProductsForSubCategory",
                         Description = ex.Message
                     };
                CreateLog(logger);
                List<Product> productList = new List<Product>();
                return productList;
            }
        }

        public async Task<IEnumerable<Product>> GetProductsForSubDivisionBySizeAsync(string productFor, string productType, string productDesign, string productColour,string productSize)
        {
            try
            {

                var filter = "{ Product_For: '" + productFor + "' , Product_Type: '" + productType + "', Product_Design: '" + productDesign + "', Product_Colour: '" + productColour + "', Product_Size: '" + productSize + "'}";
                var collection = _db.GetCollection<Product>("Product");
                IAsyncCursor<Product> cursor = await collection.FindAsync(filter);
                var products = cursor.ToList();
                foreach (var product in products.ToList())
                {
                    var minioObjName = product.Product_SKU + ".jpg";
                    product.MinioObject_Url = WH.GetMinioObject(minio, "products", minioObjName).Result;
                }
                return products.ToList();
            }
            catch (Exception ex)
            {
                ApplicationLogger logger =
                     new ApplicationLogger
                     {
                         Controller = "ProductDataAccess",
                         MethodName = "GetProductsForSubCategory",
                         Method = "GetProductsForSubCategory",
                         Description = ex.Message
                     };
                CreateLog(logger);
                List<Product> productList = new List<Product>();
                return productList;
            }
        }

        public string CreateLog(ApplicationLogger log)
        {
            try
            {
                var collection = _db.GetCollection<ApplicationLogger>("ServerLog");
                collection.InsertOneAsync(log);
                return "Success";
            }
            catch (Exception ex)
            {
                ApplicationLogger logger =
                  new ApplicationLogger
                  {
                      Controller = "ProductDataAccess",
                      MethodName = "CreateLog",
                      Method = "CreateLog",
                      Description = ex.Message
                  };
                CreateLog(logger);
                return "Failed";
            }
        }
    }
}
