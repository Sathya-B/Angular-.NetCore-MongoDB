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
                    string objectName = product.Product_SKU + ".jpg";
                    //product.MinioObject_Url = WH.GetMinioObject("products", objectName).Result;
                    //product.MinioObject_Url = WH.GetAmazonS3Object("arthurclive-products",objectName);
                    product.MinioObject_Url = WH.GetS3Object("arthurclive-products", objectName);
                }
                return products;
            }
            catch (Exception ex)
            {
                WH.CreateLog("ProductDataAccess", "GetProducts", "GetProducts", ex.Message);
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
                WH.CreateLog("ProductDataAccess", "Create", "Create", ex.Message);
                return "Failed";
            }
        }

        public async Task<string> Update(ObjectId id, Product product)
        {
            try
            {
                string objectName = product.Product_SKU + ".jpg";
                //product.MinioObject_Url = WH.GetMinioObject("products", objectName).Result;
                //product.MinioObject_Url = WH.GetAmazonS3Object("arthurclive-products", objectName);
                product.MinioObject_Url = WH.GetS3Object("arthurclive-products", objectName);
                var collection = _db.GetCollection<Product>("Product");
                var Deleteone = await collection.DeleteOneAsync(Builders<Product>.Filter.Eq("Id", id));
                product.Id = id;
                await collection.InsertOneAsync(product);
                return "Updated";
            }
            catch (Exception ex)
            {
                WH.CreateLog("ProductDataAccess", "Update", "Update", ex.Message);
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
                WH.CreateLog("ProductDataAccess", "Remove", "Remove", ex.Message);
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
                    string objectName = product.Product_SKU + ".jpg";
                    //product.MinioObject_Url = WH.GetMinioObject("products", objectName).Result;
                    //product.MinioObject_Url = WH.GetAmazonS3Object("arthurclive-products", objectName);
                    product.MinioObject_Url = WH.GetS3Object("arthurclive-products", objectName);
                }
                return products.ToList();
            }
            catch(Exception ex)
            {
                WH.CreateLog("ProductDataAccess", "GetProductsForSubCategoryAsync", "GetProductsForSubCategoryAsync", ex.Message);
                List<Product> productList = new List<Product>();
                return productList;
            }
        }

        #region Unused Get SubCategories
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
                    string objectName = product.Product_SKU + ".jpg";
                    //product.MinioObject_Url = WH.GetMinioObject("products", objectName).Result;
                    //product.MinioObject_Url = WH.GetAmazonS3Object("arthurclive-products", objectName);
                    product.MinioObject_Url = WH.GetS3Object("arthurclive-products", objectName);
                }
                return products.ToList();
            }
            catch (Exception ex)
            {
                WH.CreateLog("ProductDataAccess", "GetProductsForSubDivisionByDesignAsync", "GetProductsForSubDivisionByDesignAsync", ex.Message);
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
                    string objectName = product.Product_SKU + ".jpg";
                    //product.MinioObject_Url = WH.GetMinioObject("products", objectName).Result;
                    //product.MinioObject_Url = WH.GetAmazonS3Object("arthurclive-products", objectName);
                    product.MinioObject_Url = WH.GetS3Object("arthurclive-products", objectName);
                }
                return products.ToList();
            }
            catch (Exception ex)
            {
                WH.CreateLog("ProductDataAccess", "GetProductsForSubDivisionByColourAsync", "GetProductsForSubDivisionByColourAsync", ex.Message);
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
                    string objectName = product.Product_SKU + ".jpg";
                    //product.MinioObject_Url = WH.GetMinioObject("products", objectName).Result;
                    //product.MinioObject_Url = WH.GetAmazonS3Object("arthurclive-products", objectName);
                    product.MinioObject_Url = WH.GetS3Object("arthurclive-products", objectName);
                }
                return products.ToList();
            }
            catch (Exception ex)
            {
                WH.CreateLog("ProductDataAccess", "GetProductsForSubDivisionBySizeAsync", "GetProductsForSubDivisionBySizeAsync", ex.Message);
                List<Product> productList = new List<Product>();
                return productList;
            }
        }
        #endregion

    }
}
