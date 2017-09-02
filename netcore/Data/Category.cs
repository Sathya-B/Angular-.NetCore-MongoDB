using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Arthur_Clive.Data
{
    public class Category
    {
        public ObjectId Id { get; set; }

        [BsonElement("Product_For")]
        public string Product_For { get; set; }

        [BsonElement("Product_Type")]
        public string Product_Type { get; set; }

        [BsonElement("MinioObject_URL")]
        public string MinioObject_URL { get; set; }

        [BsonElement("Description")]
        public string Description { get; set; }
    }
}
