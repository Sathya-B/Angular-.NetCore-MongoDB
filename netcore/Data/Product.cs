using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Arthur_Clive.Data
{
    public class Category
    {
        public ObjectId Id { get; set; }
        public string ProductFor { get; set; }
        public string ProductType { get; set; }
        public string MinioObject_URL { get; set; }
        public string Description { get; set; }
    }

    public class Product
    {
        public ObjectId Id { get; set; }
        public string ProductSKU { get; set; }
        public string MinioObject_URL { get; set; }
        public string ProductFor { get; set; }
        public string ProductType { get; set; }
        public string ProductDesign { get; set; }
        public string ProductBrand { get; set; }
        public double ProductPrice { get; set; }
        public double ProductDiscount { get; set; }
        public double ProductDiscountPrice { get; set; }
        public long ProductStock { get; set; }
        public string ProductSize { get; set; }
        public string ProductMaterial { get; set; }
        public double ProductRating { get; set; }
        public Review[] ProductReviews { get; set; }
        public string ProductColour { get; set; }
        public bool RefundApplicable { get; set; }
        public bool ReplacementApplicable { get; set; }
        public string ProductDescription { get; set; }
    }

    public class Review
    {
        public string Name { get; set; }
        public string Comment { get; set; }
    }
}
