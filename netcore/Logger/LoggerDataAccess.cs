using MongoDB.Bson;
using MongoDB.Driver;
using MH = Arthur_Clive.Helper.MongoHelper;

namespace Arthur_Clive.Logger
{
    /// <summary>Data access for server side logger</summary>
    public class LoggerDataAccess
    {
        /// <summary>Create log for server side error</summary>
        /// <param name="className"></param>
        /// <param name="methodName"></param>
        /// <param name="errorDescription"></param>
        /// <param name="serverlogCollection"></param>
        public static void CreateLog(string className, string methodName, string errorDescription,IMongoCollection<ApplicationLogger> serverlogCollection)
        {
            ApplicationLogger logger =
                new ApplicationLogger
                {
                    Project = "Arthur_Clive",
                    Class = className,
                    Method = methodName,
                    Description = errorDescription
                };
            serverlogCollection.InsertOneAsync(logger);
        }
    }

    /// <summary>Data to be inserted to server side log</summary>
    public class ApplicationLogger
    {
        /// <summary>ObjectId given by MongoDB</summary>
        public ObjectId Id { get; set; }
        /// <summary>Name of controller where error occurs</summary>
        public string Project { get; set; }
        /// <summary>Method where error occurs</summary>
        public string Class { get; set; }
        /// <summary>Method name where error occurs</summary>
        public string Method { get; set; }
        /// <summary>Description of error</summary>
        public string Description { get; set; }
    }
}
