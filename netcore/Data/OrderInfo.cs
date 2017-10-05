using System;
using System.Collections.Generic;
using MongoDB.Bson;

namespace Arthur_Clive.Data
{
    public class OrderInfo
    {
        public ObjectId Id { get; set; }
        public int OrderId { get; set; }
        public string UserName { get; set; }
        public string PaymentMethod { get; set; }
        public PaymentMethod PaymentDetails { get; set; }
        public List<Address> Address { get; set; }
        public List<ProductDetails> ProductDetails { get; set; }
    }

    public class ProductDetails
    {
        public string ProductSKU { get; set; }
        public string Status { get; set; }
        public List<StatusCode> StatusCode { get; set; }
        public Cart ProductInCart { get; set; }
    }

    public class StatusCode
    {
        public int StatusId { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
    }

    public class PaymentMethod
    {
        public string Method { get; set; }
        public List<StatusCode> Status { get; set; }
    }

    public class OrderRequest
    {
        public ObjectId Id { get; set; }
        public int OrderId { get; set; }
        public string UserName { get; set; }
        public string ProductSKU { get; set; }
    }

    public class StatusUpdate
    {
        public string Status { get; set; }
        public int OrderId { get; set; }
    }
}
