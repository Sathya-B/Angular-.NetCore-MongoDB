using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuthorizedServer.Logger
{
    public class ApplicationLogger
    {
        public ObjectId Id { get; set; }

        [BsonElement("Controller")]
        public string Controller { get; set; }

        [BsonElement("Method")]
        public string Method { get; set; }

        [BsonElement("MethodName")]
        public string MethodName { get; set; }

        [BsonElement("Description")]
        public string Description { get; set; }
    }
}
