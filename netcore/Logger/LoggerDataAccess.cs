﻿using MongoDB.Driver;
using MH = Arthur_Clive.Helper.MongoHelper;

namespace Arthur_Clive.Logger
{
    public class LoggerDataAccess
    {
        public static IMongoDatabase _db = MH._client.GetDatabase("ArthurCliveLogDB");

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
            var collection = _db.GetCollection<ApplicationLogger>("ServerLog");
            collection.InsertOneAsync(logger);
        }
    }
}
