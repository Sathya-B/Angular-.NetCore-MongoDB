using System;
using System.Collections.Generic;
using System.Text;
using Arthur_Clive.Data;
using Arthur_Clive.Helper;
using MongoDB.Driver;

namespace UnitTest_ArthurClive.Helper
{
    class UnitTestHelper
    {
        public async static void InsertData_SampleCategory()
        {
            IMongoDatabase db = MongoHelper._client.GetDatabase("UnitTestDB");
            var insertData = new Category { MinioObject_URL = "https://s3.ap-south-1.amazonaws.com/product-category/Men-Tshirt.jpg", ProductFor = "Men", ProductType = "Tshirt" };
            var collection = db.GetCollection<Category>("Category");
            await collection.InsertOneAsync(insertData);
        }
    }
}
