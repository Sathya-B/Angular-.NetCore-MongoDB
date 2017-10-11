using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace Arthur_Clive.Data
{
    /// <summary>Contains the info of the order made</summary>
    public class OrderInfo
    {
        /// <summary>ObjectId give by MongoDB</summary>
        public ObjectId Id { get; set; }
        /// <summary>Id given to the placed order</summary>
        [Required]
        [DefaultValue("1")]
        public int OrderId { get; set; }
        /// <summary>Email or PhoneNumber of the user</summary>
        public string UserName { get; set; }
        /// <summary>Payment method prefered by the user</summary>
        [Required]
        [DefaultValue("Cash On Delivery")]
        public string PaymentMethod { get; set; }
        /// <summary>Payment method and status of the payment</summary>
        public PaymentMethod PaymentDetails { get; set; }
        /// <summary>Address details of user</summary>
        public List<Address> Address { get; set; }
        /// <summary>Product details of the order</summary>
        public List<ProductDetails> ProductDetails { get; set; }
    }

    /// <summary>Contails the details related to ordered product</summary>
    public class ProductDetails
    {
        /// <summary>SKU given to the product</summary>
        public string ProductSKU { get; set; }
        /// <summary>Delivery status of the product</summary>
        public string Status { get; set; }
        /// <summary>Code given to the delivery status</summary>
        public List<StatusCode> StatusCode { get; set; }
        /// <summary>Product details</summary>
        public Cart ProductInCart { get; set; }
    }

    /// <summary>Contails the status details</summary>
    public class StatusCode
    {
        /// <summary>Id given to the status</summary>
        public int StatusId { get; set; }
        /// <summary>Description of the status</summary>
        public string Description { get; set; }
        /// <summary>Date and time when the status is registered</summary>
        public DateTime Date { get; set; }
    }

    /// <summary>Contails payment details for the product</summary>
    public class PaymentMethod
    {
        /// <summary>Method of the payment</summary>
        public string Method { get; set; }
        /// <summary>Status of the payment</summary>
        public List<StatusCode> Status { get; set; }
    }

    /// <summary>Contails the request details for the product</summary>
    public class OrderRequest
    {
        /// <summary>ObjectId give by MongoDB</summary>
        public ObjectId Id { get; set; }
        /// <summary>Id given to the order</summary>
        [Required]
        [DefaultValue("1")]
        public int OrderId { get; set; }
        /// <summary>UserName of the user who make the order</summary>
        [Required]
        [DefaultValue("sample@gmail.com")]
        public string UserName { get; set; }
        /// <summary>SKU of the ordered product</summary>
        [Required]
        [DefaultValue("Men-Tshirt-Om-Black-S")]
        public string ProductSKU { get; set; }
    }

    /// <summary>Contails update details for the order</summary>
    public class StatusUpdate
    {
        /// <summary>Status to be updated</summary>
        [Required]
        [DefaultValue("Enter your status to be updated")]
        public string Status { get; set; }
        /// <summary>Id of the order for which the status is to be updated</summary>
        [Required]
        [DefaultValue("1")]
        public int OrderId { get; set; }
    }
}
