using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arthur_Clive.Data;
using Arthur_Clive.Logger;
using Minio;
using MongoDB.Driver;
using WH = Arthur_Clive.Helper.WebApiHelper;

namespace Arthur_Clive.DataAccess
{
    public class CategoryDataAccess
    {
        public MongoClient _client;
        public MongoServer _server;
        public IMongoDatabase _db;
        public MinioClient minio = WH.GetMinioClient();
        public string presignedUrl;
        public string minioObjName;

        public CategoryDataAccess()
        {
            _client = WH.GetClient();
            _db = _client.GetDatabase("ProductDB");
        }

        public async Task<IEnumerable<Category>> GetCategories()
        {
            try
            {
                var collection = _db.GetCollection<Category>("Category");
                var filter = FilterDefinition<Category>.Empty;
                IAsyncCursor<Category> cursor = await collection.FindAsync(filter);
                    var categories = cursor.ToList();
                foreach (var category in categories)
                {
                    var minioObjName = category.Product_For + "-" + category.Product_Type + ".jpg";
                   // category.MinioObject_URL = WH.GetMinioObject(minio, "product-category", minioObjName).Result;
                   category.MinioObject_URL = WH.GetS3Object("product-category", minioObjName);
                }
                return categories;
            }
            catch(Exception ex)
            {
                ApplicationLogger logger =
                     new ApplicationLogger
                     {
                         Controller = "CategoryDataAccess",
                         MethodName = "GetCategories",
                         Method = "GetCategories",
                         Description = ex.Message
                     };
                CreateLog(logger);
                List<Category> categoryList = new List<Category>();
                return categoryList;
            }
        }

        public string CreateCategory(Category product)
        {
            try
            {
                minioObjName = product.Product_For + "-" + product.Product_Type + ".jpg";
                product.MinioObject_URL = WH.GetMinioObject(minio, "product-category", minioObjName).Result;
                var collection = _db.GetCollection<Category>("Category");
                collection.InsertOneAsync(product);
                return "Created";
            }
            catch (Exception ex)
            {
                ApplicationLogger logger =
                    new ApplicationLogger
                    {
                        Controller = "CategoryDataAccess",
                        MethodName = "CreateCategory",
                        Method = "CreateCategory",
                        Description = ex.Message
                    };
                CreateLog(logger);
                return "Failed";
            }
        }

        //public void Update(ObjectId id, Product p)
        //{
        //    try
        //    {
        //        p.Id = id;
        //        var res = Query<Product>.EQ(pd => pd.Id, id);
        //        var operation = Update<Product>.Replace(p);
        //        _db.GetCollection<Product>("Product").Update(res, operation);
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}

        //public void Remove(ObjectId id)
        //{
        //    try
        //    {
        //        var res = Query<Product>.EQ(e => e.Id, id);
        //        var operation = _db.GetCollection<Product>("Product").Remove(res);
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}

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
                        Controller = "CategoryDataAccess",
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
