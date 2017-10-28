using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Arthur_Clive.Controllers;
using Arthur_Clive.Data;
using Arthur_Clive.Helper;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace UnitTest_ArthurClive.Controller
{
    class Integrationtest_ArthurCliveController_Helper
    {
        public static ResponceData DeserializedResponceData(dynamic data)
        {
            ActionResultModel deserializedResponce = new ActionResultModel();
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(data));
            DataContractJsonSerializer ser = new DataContractJsonSerializer(deserializedResponce.GetType());
            deserializedResponce = ser.ReadObject(ms) as ActionResultModel;
            ms.Close();
            return deserializedResponce.Value;
        }

        public static ResponceData_CategoryList DeserializedResponceData_CategoryList(dynamic data)
        {
            try
            {
                ActionResultModel_CategoryList deserializedResponce = new ActionResultModel_CategoryList();
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(data));
                DataContractJsonSerializer ser = new DataContractJsonSerializer(deserializedResponce.GetType());
                deserializedResponce = ser.ReadObject(ms) as ActionResultModel_CategoryList;
                ms.Close();
                return deserializedResponce.Value;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        public static IMongoDatabase db = MongoHelper._client.GetDatabase("Authentication");

        public async static void InsertRegiterModeldata(RegisterModel registerModel)
        {
            await db.GetCollection<RegisterModel>("Authentication").InsertOneAsync(registerModel);  
        }

        public async static Task<ActionResult> GetCategories(CategoryController controller)
        {
            var result = await controller.Get() as ActionResult;
            return result;
        }
    }
}
