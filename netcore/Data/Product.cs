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

    public class Product
    {
        public ObjectId Id { get; set; }

        [BsonElement("Product_SKU")]
        public string Product_SKU { get; set; }

        [BsonElement("MinioObjectUrl")]
        public string MinioObject_Url { get; set; }

        [BsonElement("Product_For")]
        public string Product_For { get; set; }

        [BsonElement("Product_Type")]
        public string Product_Type { get; set; }

        [BsonElement("Product_Design")]
        public string Product_Design { get; set; }

        [BsonElement("Product_Brand")]
        public string Product_Brand { get; set; }

        [BsonElement("Product_Price")]
        public double Product_Price { get; set; }

        [BsonElement("Product_Discount")]
        public double Product_Discount { get; set; }

        [BsonElement("Discount_Price")]
        public double Product_Discount_Price { get; set; }

        [BsonElement("Product_Stock")]
        public long Product_Stock { get; set; }

        [BsonElement("Product_Size")]
        public string Product_Size { get; set; }

        [BsonElement("Detail_Material")]
        public string Product_Material { get; set; }

        [BsonElement("Product_Rating")]
        public double Product_Rating { get; set; }

        [BsonElement("Product_Reviews")]
        public Review[] Product_Reviews { get; set; }

        [BsonElement("Product_Colour")]
        public string Product_Colour { get; set; }

        [BsonElement("Refund_Applicable")]
        public bool Refund_Applicable { get; set; }

        [BsonElement("Replacement_Applicable")]
        public bool Replacement_Applicable { get; set; }

        [BsonElement("Product_Description")]
        public string Product_Description { get; set; }
    }

    public class Review
    {
        public string Name { get; set; }
        public string Comment { get; set; }
    }
}
