using System.Collections.Generic;
using MongoDB.Bson;

namespace Arthur_Clive.Data
{
    public class Subscribe
    {
        public ObjectId Id { get; set; }
        public string UserName { get; set; }
    }

    public class UserInfoList
    {
        public List<Address> ListOfAddress { get; set; }
    }

    public class Address
    {
        public ObjectId Id { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string AddressLines { get; set; }
        public string PostOffice { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PinCode { get; set; }
        public string Landmark { get; set; }
        public bool BillingAddress { get; set; }
        public bool ShippingAddress { get; set; }
        public bool DefaultAddress { get; set; }
    }

    public class CartList
    {
        public List<Cart> ListOfProducts { get; set; }
    }

    public class WishlistList
    {
        public List<WishList> ListOfProducts { get; set; }
    }

    public class Cart
    {
        public ObjectId Id { get; set; }
        public string UserName { get; set; }
        public string ProductSKU { get; set; }
        public string MinioObject_URL { get; set; }
        public string ProductFor { get; set; }
        public string ProductType { get; set; }
        public string ProductDesign { get; set; }
        public string ProductBrand { get; set; }
        public double ProductPrice { get; set; }
        public double ProductDiscount { get; set; }
        public double ProductDiscountPrice { get; set; }
        public long ProductQuantity { get; set; }
        public string ProductSize { get; set; }
        public string ProductColour { get; set; }
        public string ProductDescription { get; set; }
    }

    public class WishList
    {
        public ObjectId Id { get; set; }
        public string UserName { get; set; }
        public string ProductSKU { get; set; }
        public string MinioObject_URL { get; set; }
        public string ProductFor { get; set; }
        public string ProductType { get; set; }
        public string ProductDesign { get; set; }
        public string ProductBrand { get; set; }
        public double ProductPrice { get; set; }
        public double ProductDiscount { get; set; }
        public double ProductDiscountPrice { get; set; }
        public string ProductSize { get; set; }
        public string ProductColour { get; set; }
        public string ProductDescription { get; set; }
    }
}
