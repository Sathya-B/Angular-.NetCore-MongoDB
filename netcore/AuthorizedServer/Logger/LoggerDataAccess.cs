using MongoDB.Driver;
using WH = AuthorizedServer.Helper.MongoHelper;

namespace AuthorizedServer.Logger
{
    public class LoggerDataAccess
    {
        public static IMongoDatabase _db = WH._client.GetDatabase("ArthurCliveLogDB");

        public static void CreateLog(string controllerName, string methodName, string method, string errorDescription)
        {
            ApplicationLogger logger =
                new ApplicationLogger
                {
                    Controller = controllerName,
                    MethodName = methodName,
                    Method = method,
                    Description = errorDescription
                };
            var collection = _db.GetCollection<ApplicationLogger>("testLog");
            collection.InsertOneAsync(logger);
        }
    }
}
