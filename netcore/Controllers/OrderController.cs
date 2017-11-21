using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arthur_Clive.Data;
using Arthur_Clive.Logger;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MH = Arthur_Clive.Helper.MongoHelper;
using Swashbuckle.AspNetCore.Examples;
using Arthur_Clive.Swagger;
using Arthur_Clive.Helper;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Arthur_Clive.Controllers
{
    /// <summary>Controller to view,cancel, return and place orders</summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class OrderController : Controller
    {
        /// <summary></summary>
        public MongoClient _client;
        /// <summary></summary>
        public IMongoDatabase userinfo_db;
        /// <summary></summary>
        public IMongoCollection<BsonDocument> userinfo_collection;
        /// <summary></summary>
        public IMongoCollection<Cart> cartCollection;
        /// <summary></summary>
        public IMongoDatabase order_db;
        /// <summary></summary>
        public IMongoCollection<BsonDocument> orderinfo_collection;
        /// <summary></summary>
        public IMongoDatabase product_db;
        /// <summary></summary>
        public IMongoCollection<BsonDocument> product_collection;
        /// <summary></summary>
        public IMongoDatabase auth_db;
        /// <summary></summary>
        public IMongoCollection<BsonDocument> authentication_collection;
        /// <summary></summary>
        public UpdateDefinition<BsonDocument> updateDefinition;
        /// <summary></summary>
        public IMongoDatabase logger_db;
        /// <summary></summary>
        public IMongoCollection<ApplicationLogger> serverlogCollection;

        /// <summary>
        /// 
        /// </summary>
        public OrderController()
        {
            _client = MH.GetClient();
            userinfo_db = _client.GetDatabase("UserInfo");
            userinfo_collection = userinfo_db.GetCollection<BsonDocument>("UserInfo");
            cartCollection = userinfo_db.GetCollection<Cart>("Cart");
            order_db = _client.GetDatabase("OrderDB");
            orderinfo_collection = order_db.GetCollection<BsonDocument>("OrderInfo");
            product_db = _client.GetDatabase("ProductDB");
            product_collection = product_db.GetCollection<BsonDocument>("Product");
            auth_db = _client.GetDatabase("Authentication");
            authentication_collection = auth_db.GetCollection<BsonDocument>("Authentication");
            logger_db = _client.GetDatabase("ArthurCliveLogDB");
            serverlogCollection = logger_db.GetCollection<ApplicationLogger>("ServerLog");
        }

        /// <summary>Order products added to the cart</summary>
        /// <remarks>This api is used to place an order</remarks>
        /// <param name="data">Info needed to place order</param>
        /// <param name="username">UserName of user who needs to place order</param>
        /// <response code="200">Order placed successfully</response>
        /// <response code="401">UserInfo not found</response> 
        /// <response code="402">Cart not found</response> 
        /// <response code="403">Order quantity is higher than the product stock</response> 
        /// <response code="404">Default address not found</response> 
        /// <response code="400">Process ran into an exception</response> 
        [HttpPost("placeorder/{username}")]
        [SwaggerRequestExample(typeof(OrderInfo), typeof(OrderDetails))]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public async Task<ActionResult> PlaceOrder([FromBody]OrderInfo data, string username)
        {
            try
            {
                List<Address> users = new List<Address>();
                foreach (var user in MH.GetListOfObjects(userinfo_collection, "UserName", username, null, null).Result)
                {
                    users.Add(BsonSerializer.Deserialize<Address>(user));
                }
                if (users.Count > 0)
                {
                    IAsyncCursor<Cart> cartCursor = await cartCollection.FindAsync(Builders<Cart>.Filter.Eq("UserName", username));
                    var cartDatas = cartCursor.ToList();
                    if (cartDatas.Count > 0)
                    {
                        var ordersCount = order_db.GetCollection<OrderInfo>("OrderInfo").Find(Builders<OrderInfo>.Filter.Empty).Count();
                        data.OrderId = ordersCount + 1;
                        data.UserName = username;
                        data.PaymentMethod = "Nil";
                        PaymentMethod paymentMethod = new PaymentMethod();
                        List<StatusCode> paymentStatus = new List<StatusCode>();
                        paymentStatus.Add(new StatusCode { Date = DateTime.UtcNow, StatusId = 1, Description = "Payment Initiated" });
                        paymentMethod.Status = paymentStatus;
                        data.PaymentDetails = paymentMethod;
                        List<Address> addressList = new List<Address>();
                        foreach (var address in users)
                        {
                            if (address.DefaultAddress == true)
                            {
                                addressList.Add(address);
                            }
                        }
                        data.Address = addressList;
                        if (data.Address.Count == 0)
                        {
                            return BadRequest(new ResponseData { Code = "404", Message = "No default address found" });
                        }
                        List<ProductDetails> productList = new List<ProductDetails>();
                        foreach (var cart in cartDatas)
                        {
                            var product = BsonSerializer.Deserialize<Product>(MH.GetSingleObject(product_collection, "ProductSKU", cart.ProductSKU, null, null).Result);
                            if (product.ProductStock < cart.ProductQuantity)
                            {
                                return BadRequest(new ResponseData { Code = "403", Message = "Order quantity is higher than the product stock." });
                            }
                            ProductDetails productDetails = new ProductDetails();
                            productDetails.ProductSKU = cart.ProductSKU;
                            productDetails.Status = "Order Placed";
                            productDetails.Reviewed = false;
                            List<StatusCode> productStatus = new List<StatusCode>();
                            productStatus.Add(new StatusCode { StatusId = 1, Date = DateTime.UtcNow, Description = "Order Placed" });
                            productDetails.StatusCode = productStatus;
                            productDetails.ProductInCart = cart;
                            productList.Add(productDetails);
                        }
                        data.ProductDetails = productList;
                        data.OrderStatus = "Order Placed";
                        await order_db.GetCollection<OrderInfo>("OrderInfo").InsertOneAsync(data);
                        List<string> productInfoList = new List<string>();
                        foreach (var cart in cartDatas)
                        {
                            productInfoList.Add(cart.ProductSKU);
                        }
                        string productInfo = String.Join(":", productInfoList);
                        RegisterModel userInfo = BsonSerializer.Deserialize<RegisterModel>(MH.GetSingleObject(authentication_collection, "UserName", username, null, null).Result);
                        Address addressInfo = BsonSerializer.Deserialize<Address>(MH.GetSingleObject(userinfo_collection, "UserName", username, null, null).Result);
                        PaymentModel paymentModel = new PaymentModel { FirstName = userInfo.FullName, UserName = username, LastName = "", ProductInfo = productInfo, Amount = data.TotalAmount.ToString(), Email = userInfo.Email, PhoneNumber = userInfo.PhoneNumber, AddressLine1 = addressInfo.AddressLines, AddressLine2 = addressInfo.Landmark, City = addressInfo.City, State = addressInfo.State, Country = userInfo.UserLocation, ZipCode = addressInfo.PinCode, OrderId = data.OrderId };
                        if (data.TotalAmount == 0)
                        {
                            var updatePaymentMethod = await MH.UpdateSingleObject(orderinfo_collection, "OrderId", paymentModel.OrderId, null, null, Builders<BsonDocument>.Update.Set("PaymentMethod", "Coupon"));
                            var updatePaymentDetails = await MH.UpdatePaymentDetails(orderinfo_collection, paymentModel.OrderId);
                            var removecartItems = await MH.RemoveCartItems(orderinfo_collection, cartCollection, product_collection, paymentModel.OrderId, paymentModel.UserName, paymentModel.Email);
                            var sendGift = GlobalHelper.SendGift(paymentModel.OrderId, orderinfo_collection);
                            return Ok(new ResponseData { Code = "201", Message = "Payment success" });
                        }
                        else
                        {
                            var hashtableData = PayUHelper.GetHashtableData(paymentModel);
                            return Ok(new ResponseData { Code = "200", Message = "Order Placed", Data = hashtableData });
                        }
                    }
                    else
                    {
                        return BadRequest(new ResponseData { Code = "402", Message = "Cart not found" });
                    }
                }
                else
                {
                    return BadRequest(new ResponseData { Code = "401", Message = "UserInfo not found" });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("OrderController", "PlaceOrder", ex.Message, serverlogCollection);
                return BadRequest(new ResponseData { Code = "400", Message = "Failed", Data = ex.Message });
            }
        }

        /// <summary>Get orders by order id</summary>
        /// <param name="orderid">Id of order</param>
        /// <response code="200">Returns order that matches the order id</response>
        /// <response code="404">No orders found</response> 
        /// <response code="400">Process ran into an exception</response> 
        [HttpGet("viewsingleorder/{orderid}")]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public ActionResult GetOrderById(int orderid)
        {
            try
            {
                var checkData = MH.GetSingleObject(orderinfo_collection, "OrderId", orderid, null, null).Result;
                if (checkData != null)
                {
                    return Ok(new ResponseData
                    {
                        Code = "200",
                        Message = "Success",
                        Data = BsonSerializer.Deserialize<OrderInfo>(checkData)
                    });
                }
                else
                {
                    return BadRequest(new ResponseData { Code = "404", Message = "No orders found" });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("OrderController", "GetOrdersOfUser", ex.Message, serverlogCollection);
                return BadRequest(new ResponseData { Code = "400", Message = "Failed", Data = ex.Message });
            }
        }

        /// <summary>Get orders placed by a user</summary>
        /// <param name="username">UserName of user whos orders need to be found</param>
        /// <remarks>This api is user to view all the orders placed by the user</remarks>
        /// <response code="200">Returns all the orders placed by the user</response>
        /// <response code="404">No orders found</response> 
        /// <response code="400">Process ran into an exception</response> 
        [ProducesResponseType(typeof(ResponseData), 200)]
        [HttpGet("vieworder/{username}")]
        public ActionResult GetOrdersOfUser(string username)
        {
            try
            {
                var orderList = MH.GetListOfObjects(orderinfo_collection, "UserName", username, null, null).Result;
                if (orderList != null)
                {
                    List<OrderInfo> orders = new List<OrderInfo>();
                    foreach (var order in orderList)
                    {
                        orders.Add(BsonSerializer.Deserialize<OrderInfo>(order));
                    }
                    return Ok(new ResponseData { Code = "200", Message = "Success", Data = orders });
                }
                else
                {
                    return BadRequest(new ResponseData { Code = "404", Message = "No orders found" });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("OrderController", "GetOrdersOfUser", ex.Message, serverlogCollection);
                return BadRequest(new ResponseData { Code = "400", Message = "Failed", Data = ex.Message });
            }
        }

        /// <summary>Get all the orders placed</summary>
        /// <remarks>This api is user to view all the orders</remarks>
        /// <response code="200">Returns the all the orders placed</response>
        /// <response code="404">No orders found</response> 
        /// <response code="400">Process ran into an exception</response> 
        //[Authorize("Level1Access")]
        [HttpGet("viewallorders")]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public async Task<ActionResult> GetAllOrders()
        {
            try
            {
                IAsyncCursor<OrderInfo> cursor = await order_db.GetCollection<OrderInfo>("OrderInfo").FindAsync(Builders<OrderInfo>.Filter.Empty);
                var orders = cursor.ToList();
                if (orders != null)
                {
                    return Ok(new ResponseData { Code = "200", Message = "Success", Data = orders });
                }
                else
                {
                    return BadRequest(new ResponseData { Code = "404", Message = "No orders found" });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("OrderController", "GetOrdersOfUser", ex.Message, serverlogCollection);
                return BadRequest(new ResponseData { Code = "400", Message = "Failed", Data = ex.Message });
            }
        }

        /// <summary>Update order status</summary>
        /// <param name="orderid">id of order</param>
        /// <param name="status">order status update details</param>
        /// <response code="200">Order status updated</response>
        /// <response code="401">Order status update failed</response> 
        /// <response code="402">Product details status update failed</response> 
        /// <response code="404">Order not found</response> 
        /// <response code="400">Process ran into an exception</response> 
        //[Authorize("Level1Access")]
        [HttpPut("deliverystatus/update/{orderid}/{status}")]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public ActionResult UpdateDeliveryStatus(int orderid, string status)
        {
            try
            {
                var checkData = MH.GetSingleObject(orderinfo_collection, "OrderId", orderid, null, null).Result;
                if (checkData != null)
                {
                    var orderDetails = BsonSerializer.Deserialize<OrderInfo>(checkData);
                    if (status != "Order Replaced" || status != "Order Refunded")
                    {
                        var updateOrderStatus = MH.UpdateSingleObject(orderinfo_collection, "OrderId", orderid, null, null, Builders<BsonDocument>.Update.Set("OrderStatus", status)).Result;
                        if (updateOrderStatus == false)
                        {
                            return BadRequest(new ResponseData { Code = "401", Message = "Order status update failed" });
                        }
                    }
                    List<ProductDetails> productDetails = new List<ProductDetails>();
                    foreach (var product in orderDetails.ProductDetails)
                    {
                        product.Status = status;
                        List<StatusCode> statusList = new List<Data.StatusCode>();
                        foreach (var data in product.StatusCode)
                        {
                            statusList.Add(data);
                        }
                        int statusId = 0;
                        if (status == "Packing In Progress") { statusId = 2; }
                        else if (status == "Order Shipped") { statusId = 3; }
                        else if (status == "Order Delivered") { statusId = 5; }
                        else if (status == "Order Replaced") { statusId = 7; }
                        else if (status == "Order Refunded") { statusId = 9; }
                        statusList.Add(new StatusCode { StatusId = statusId, Date = DateTime.UtcNow, Description = status });
                        product.StatusCode = statusList;
                        productDetails.Add(product);
                    }
                    var updateProductDetails = MH.UpdateSingleObject(orderinfo_collection, "OrderId", orderid, null, null, Builders<BsonDocument>.Update.Set("ProductDetails", productDetails)).Result;
                    if (updateProductDetails == false)
                    {
                        return BadRequest(new ResponseData { Code = "402", Message = "Product details order status update failed" });
                    }
                    return Ok(new ResponseData { Code = "200", Message = "Status updated" });
                }
                else
                {
                    return BadRequest(new ResponseData { Code = "404", Message = "Order not found" });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("OrderController", "UpdateDeliveryStatus", ex.Message, serverlogCollection);
                return BadRequest(new ResponseData { Code = "400", Message = "Failed", Data = ex.Message });
            }
        }

        /// <summary>Cancel placed order</summary>
        /// <param name="orderid">id of order placed</param>
        /// <param name="productSKU">SKU of product which needs to be cancelled</param>
        /// <response code="200">Order cancelled</response>
        /// <response code="401">Cancel request cannot be processed as the product is delivered</response> 
        /// <response code="402">Order already cancelled</response> 
        /// <response code="403">Order request cannot be processed</response> 
        /// <response code="404">Order not found</response> 
        /// <response code="405">Order status update failed</response> 
        /// <response code="406">Product order status update failed</response> 
        /// <response code="407">Product not found</response> 
        /// <response code="400">Process ran into an exception</response> 
        [HttpPut("cancel/{orderid}/{productSKU}")]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public ActionResult CancelOrder(int orderid, string productSKU)
        {
            try
            {
                var checkData = MH.GetSingleObject(orderinfo_collection, "OrderId", orderid, null, null).Result;
                if (checkData != null)
                {
                    var orderDetails = BsonSerializer.Deserialize<OrderInfo>(checkData);
                    if (MH.CheckForDatas(product_collection, "ProductSKU", productSKU, null, null) != null)
                    {
                        List<ProductDetails> productDetails = new List<ProductDetails>();
                        foreach (var product in orderDetails.ProductDetails)
                        {
                            if (product.ProductSKU == productSKU)
                            {
                                if (product.Status == "Order Delivered")
                                {
                                    return BadRequest(new ResponseData { Code = "401", Message = "Cancel request cannot be processed as the product is delivered" });
                                }
                                else if (product.Status == "Order Cancelled")
                                {
                                    return BadRequest(new ResponseData
                                    {
                                        Code = "402",
                                        Message = "Order already cancelled"
                                    });
                                }
                                else if (product.Status == "Order Replaced" || orderDetails.OrderStatus == "Order Refunded")
                                {
                                    return BadRequest(new ResponseData
                                    {
                                        Code = "403",
                                        Message = "Cancel request cannot be processed as the order status is " + orderDetails.OrderStatus,
                                        Data = null
                                    });
                                }
                                product.Status = "Order Cancelled";
                                List<StatusCode> statusCode = new List<StatusCode>();
                                foreach (var status in product.StatusCode)
                                {
                                    statusCode.Add(status);
                                }
                                statusCode.Add(new StatusCode { Date = DateTime.UtcNow, StatusId = 4, Description = "Order Cancelled" });
                                product.StatusCode = statusCode;
                            }
                            productDetails.Add(product);
                        }
                        var updateOrderStatus = MH.UpdateSingleObject(orderinfo_collection, "OrderId", orderid, null, null, Builders<BsonDocument>.Update.Set("OrderStatus", "Order Cancelled")).Result;
                        if (updateOrderStatus == false)
                        {
                            return BadRequest(new ResponseData { Code = "405", Message = "Order status update failed" });
                        }
                        var updateProductDetails = MH.UpdateSingleObject(orderinfo_collection, "OrderId", orderid, null, null, Builders<BsonDocument>.Update.Set("ProductDetails", productDetails)).Result;
                        if (updateProductDetails == false)
                        {
                            return BadRequest(new ResponseData { Code = "406", Message = "Order cancelled status update failed" });
                        }
                        //if(orderDetails.PaymentMethod != "COD")
                        //{
                        //Need to refund amount
                        //}
                        return Ok(new ResponseData { Code = "200", Message = "Order cancelled" });
                    }
                    else
                    {
                        return BadRequest(new ResponseData { Code = "407", Message = "Product not found" });
                    }
                }
                else
                {
                    return BadRequest(new ResponseData { Code = "404", Message = "Order not found" });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("OrderController", "CancelOrder", ex.Message, serverlogCollection);
                return BadRequest(new ResponseData { Code = "400", Message = "Failed", Data = ex.Message });
            }
        }

        /// <summary>Return a product</summary>
        /// <param name="request">return request</param>
        /// <param name="orderid">Id of oder to be returned</param>
        /// <param name="productSKU">SKU of product to be returned</param>
        /// <response code="201">Payment refund initiated</response>
        /// <response code="202">Product replacemnet initiated</response>
        /// <response code="401">Invalid request</response> 
        /// <response code="402">Product not found</response> 
        /// <response code="403">Refund not applicable for this product</response> 
        /// <response code="404">Order not found</response> 
        /// <response code="405">Replacement not applicable for this product</response> 
        /// <response code="406">"Return request cannot be processed as the product is not delivered</response> 
        /// <response code="407">Return request cannot be processed as it has been more than 15 days from delivery</response> 
        /// <response code="408">Product details status update failed</response>
        /// <response code="400">Process ran into an exception</response> 
        [HttpPut("return/{request}/{orderid}/{productSKU}")]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public ActionResult ReturnOrder(string request, int orderid, string productSKU)
        {
            if (request == "refund" || request == "replace")
            {
                try
                {
                    var checkData = MH.GetSingleObject(orderinfo_collection, "OrderId", orderid, null, null).Result;
                    if (checkData != null)
                    {
                        var orderDetails = BsonSerializer.Deserialize<OrderInfo>(checkData);
                        if (orderDetails.OrderStatus == "Order Delivered")
                        {
                            foreach (var product in orderDetails.ProductDetails)
                            {
                                if (product.ProductSKU == productSKU)
                                {
                                    foreach (var status in product.StatusCode)
                                    {
                                        if (status.Description == "Order Delivered")
                                        {
                                            if (DateTime.UtcNow > status.Date.AddDays(15))
                                            {
                                                return BadRequest(new ResponseData
                                                {
                                                    Code = "407",
                                                    Message = "Return request cannot be processed as it has been more than 15 days from delivery"
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                            var checkProduct = MH.CheckForDatas(product_collection, "ProductSKU", productSKU, null, null);
                            if (checkProduct != null)
                            {
                                var productData = BsonSerializer.Deserialize<Product>(checkData);
                                if (request == "refund")
                                {
                                    if (productData.RefundApplicable == true)
                                    {
                                        List<ProductDetails> productDetails = new List<ProductDetails>();
                                        foreach (var product in orderDetails.ProductDetails)
                                        {
                                            if (product.ProductSKU == productSKU)
                                            {
                                                product.Status = "Order Refund Initiated";
                                                List<StatusCode> statusList = new List<StatusCode>();
                                                foreach (var status in product.StatusCode)
                                                {
                                                    statusList.Add(status);
                                                }
                                                statusList.Add(new StatusCode { Date = DateTime.UtcNow, StatusId = 8, Description = "Order Refund Initiated" });
                                            }
                                            productDetails.Add(product);
                                        }
                                        var updateProductDetails = MH.UpdateSingleObject(product_collection, "OrderId", orderid, null, null, Builders<BsonDocument>.Update.Set("ProductDetails", productDetails)).Result;
                                        if (updateProductDetails == false)
                                        {
                                            return BadRequest(new ResponseData { Code = "408", Message = "Product details status update failed" });
                                        }
                                        //if(orderDetails.PaymentMethod != "COD")
                                        //{
                                        //Need to refund amount
                                        //}
                                        return Ok(new ResponseData { Code = "201", Message = "Payment Refund Initiated" });
                                    }
                                    else
                                    {
                                        return BadRequest(new ResponseData { Code = "403", Message = "Refund not applicable for this product" });
                                    }
                                }
                                else
                                {
                                    if (productData.ReplacementApplicable == true)
                                    {
                                        List<ProductDetails> productDetails = new List<ProductDetails>();
                                        foreach (var product in orderDetails.ProductDetails)
                                        {
                                            if (product.ProductSKU == productSKU)
                                            {
                                                product.Status = "Order Replacement Initiated";
                                                List<StatusCode> statusList = new List<StatusCode>();
                                                foreach (var status in product.StatusCode)
                                                {
                                                    statusList.Add(status);
                                                }
                                                statusList.Add(new StatusCode { Date = DateTime.UtcNow, StatusId = 6, Description = "Order Replacement Initiated" });
                                            }
                                            productDetails.Add(product);
                                        }
                                        var updateProductDetails = MH.UpdateSingleObject(orderinfo_collection, "OrderId", orderid, null, null, Builders<BsonDocument>.Update.Set("ProductDetails", productDetails)).Result;
                                        if (updateProductDetails == false)
                                        {
                                            return BadRequest(new ResponseData { Code = "408", Message = "Product details status update failed" });
                                        }
                                        return Ok(new ResponseData { Code = "202", Message = "Product replacement initiated" });
                                    }
                                    else
                                    {
                                        return BadRequest(new ResponseData { Code = "405", Message = "Replacement not applicable for this product" });
                                    }
                                }
                            }
                            else
                            {
                                return BadRequest(new ResponseData { Code = "402", Message = "Product not found" });
                            }
                        }
                        else
                        {
                            return BadRequest(new ResponseData { Code = "406", Message = "Return request cannot be processed as the product is not delivered" });
                        }
                    }
                    else
                    {
                        return BadRequest(new ResponseData { Code = "404", Message = "Order not found" });
                    }
                }
                catch (Exception ex)
                {
                    LoggerDataAccess.CreateLog("OrderController", "CancelOrder", ex.Message, serverlogCollection);
                    return BadRequest(new ResponseData { Code = "400", Message = "Failed", Data = ex.Message });
                }
            }
            else
            {
                return BadRequest(new ResponseData { Code = "401", Message = "Invalid request" });
            }
        }

    }
}