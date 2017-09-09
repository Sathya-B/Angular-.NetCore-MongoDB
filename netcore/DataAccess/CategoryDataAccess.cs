using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arthur_Clive.Data;
using Minio;
using MongoDB.Driver;
using WH = Arthur_Clive.Helper.WebApiHelper;

namespace Arthur_Clive.DataAccess
{
    public class CategoryDataAccess
    {
        public IMongoDatabase _db = WH._client.GetDatabase("ProductDB");
        
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
                    string objectName = category.Product_For + "-" + category.Product_Type + ".jpg";
                    //category.MinioObject_URL = WH.GetMinioObject("product-category", objectName).Result;
                    //category.MinioObject_URL = WH.GetAmazonS3Object("product-category", objectName);
                    category.MinioObject_URL = WH.GetS3Object("product-category", objectName);
                }
                return categories;
            }
            catch (Exception ex)
            {
                Logger.LoggerDataAccess.CreateLog("CategoryDataAccess", "GetCategories", "GetCategories", ex.Message);
                List<Category> categoryList = new List<Category>();
                return categoryList;
            }
        }

        public string CreateCategory(Category product)
        {
            try
            {
                string objectName = product.Product_For + "-" + product.Product_Type + ".jpg";
                //product.MinioObject_URL = WH.GetMinioObject("product-category", objectName).Result;
                //product.MinioObject_URL = WH.GetAmazonS3Object("product-category", objectName);
                product.MinioObject_URL = WH.GetS3Object("product-category", objectName);
                var collection = _db.GetCollection<Category>("Category");
                collection.InsertOneAsync(product);
                return "Created";
            }
            catch (Exception ex)
            {
                Logger.LoggerDataAccess.CreateLog("CategoryDataAccess", "CreateCategory", "CreateCategory", ex.Message);
                return "Failed";
            }
        }

        #region Update , Delete
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
        #endregion
    }
}
