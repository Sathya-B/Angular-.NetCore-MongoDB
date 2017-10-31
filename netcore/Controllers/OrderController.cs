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

namespace Arthur_Clive.Controllers
{
    /// <summary>Controller to view,cancel, return and place orders</summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class OrderController : Controller
    {
        /// <summary></summary>
        public IMongoDatabase _db = MH._client.GetDatabase("UserInfo");
        /// <summary></summary>
        public IMongoDatabase order_db = MH._client.GetDatabase("OrderDB");
        /// <summary></summary>
        public IMongoDatabase product_db = MH._client.GetDatabase("ProductDB");
        /// <summary></summary>
        public UpdateDefinition<BsonDocument> updateDefinition;

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
                IAsyncCursor<Address> userCursor = await _db.GetCollection<Address>("UserInfo").FindAsync(Builders<Address>.Filter.Eq("UserName", username));
                var users = userCursor.ToList();
                if (users.Count > 0)
                {
                    IAsyncCursor<Cart> cartCursor = await _db.GetCollection<Cart>("Cart").FindAsync(Builders<Cart>.Filter.Eq("UserName", username));
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
                            return BadRequest(new ResponseData
                            {
                                Code = "404",
                                Message = "No default address found",
                                Data = null
                            });
                        }
                        List<ProductDetails> productList = new List<ProductDetails>();
                        foreach (var cart in cartDatas)
                        {
                            foreach (var product in MH.GetProducts(cart.ProductSKU, product_db).Result)
                            {
                                if (product.ProductStock < cart.ProductQuantity)
                                {
                                    return BadRequest(new ResponseData
                                    {
                                        Code = "403",
                                        Message = "Order quantity is higher than the product stock.",
                                        Data = null
                                    });
                                }
                                ProductDetails productDetails = new ProductDetails();
                                productDetails.ProductSKU = cart.ProductSKU;
                                productDetails.Status = "Order Placed";
                                List<StatusCode> productStatus = new List<StatusCode>();
                                productStatus.Add(new StatusCode { StatusId = 1, Date = DateTime.UtcNow, Description = "Order Placed" });
                                productDetails.StatusCode = productStatus;
                                productDetails.ProductInCart = cart;
                                productList.Add(productDetails);
                            }
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
                        RegisterModel userInfo = BsonSerializer.Deserialize<RegisterModel>(MH.GetSingleObject(Builders<BsonDocument>.Filter.Eq("UserName", username), "Authentication", "Authentication").Result);
                        Address addressInfo = BsonSerializer.Deserialize<Address>(MH.GetSingleObject(Builders<BsonDocument>.Filter.Eq("UserName", username), "UserInfo", "UserInfo").Result);
                        PaymentModel paymentModel = new PaymentModel { FirstName = userInfo.FullName, UserName = username, LastName = "", ProductInfo = productInfo, Amount = data.TotalAmount.ToString(), Email = userInfo.Email, PhoneNumber = userInfo.PhoneNumber, AddressLine1 = addressInfo.AddressLines, AddressLine2 = addressInfo.Landmark, City = addressInfo.City, State = addressInfo.State, Country = userInfo.UserLocation, ZipCode = addressInfo.PinCode, OrderId = data.OrderId };
                        var hashtableData = PayUHelper.GetHashtableData(paymentModel);
                        return Ok(new ResponseData
                        {
                            Code = "200",
                            Message = "Order Placed",
                            Data = hashtableData
                        });
                    }
                    else
                    {
                        return BadRequest(new ResponseData
                        {
                            Code = "402",
                            Message = "Cart not found",
                            Data = null
                        });
                    }
                }
                else
                {
                    return BadRequest(new ResponseData
                    {
                        Code = "401",
                        Message = "UserInfo not found",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("OrderController", "PlaceOrder", "PlaceOrder", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = ex.Message
                });
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
                var orders = MH.GetOrders(username, order_db);
                if (orders != null)
                {
                    return Ok(new ResponseData
                    {
                        Code = "200",
                        Message = "Success",
                        Data = orders
                    });
                }
                else
                {
                    return BadRequest(new ResponseData
                    {
                        Code = "404",
                        Message = "No orders found",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("OrderController", "GetOrdersOfUser", "GetOrdersOfUser", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = ex.Message
                });
            }
        }

        /// <summary>Get all the orders placed</summary>
        /// <remarks>This api is user to view all the orders</remarks>
        /// <response code="200">Returns the all the orders placed</response>
        /// <response code="404">No orders found</response> 
        /// <response code="400">Process ran into an exception</response> 
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
                    return Ok(new ResponseData
                    {
                        Code = "200",
                        Message = "Success",
                        Data = orders
                    });
                }
                else
                {
                    return BadRequest(new ResponseData
                    {
                        Code = "404",
                        Message = "No orders found",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("OrderController", "GetOrdersOfUser", "GetOrdersOfUser", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = ex.Message
                });
            }
        }

        /// <summary>Update order status</summary>
        /// <param name="orderid">order status update details</param>
        /// <param name="status">order status update details</param>
        /// <response code="200">Order status updated</response>
        /// <response code="401">Order status update failed</response> 
        /// <response code="402">Product details status update failed</response> 
        /// <response code="404">No orders found</response> 
        /// <response code="400">Process ran into an exception</response> 
        [HttpPut("deliverystatus/update/{orderid}/{status}")]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public ActionResult UpdateDeliveryStatus(int orderid, string status)
        {
            try
            {
                var checkData = MH.CheckForDatas("OrderId", orderid, null, null, "OrderDB", "OrderInfo");
                if (checkData != null)
                {
                    var orderDetails = BsonSerializer.Deserialize<OrderInfo>(checkData);
                    var updateOrderStatus = MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("OrderId", orderid), "OrderDB", "OrderInfo", Builders<BsonDocument>.Update.Set("OrderStatus", status)).Result;
                    if (updateOrderStatus == false)
                    {
                        return BadRequest(new ResponseData
                        {
                            Code = "401",
                            Message = "Order status update failed",
                            Data = null
                        });
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
                        if (status == "Order ready to be shipped") { statusId = 2; }
                        else if (status == "Order Shipped") { statusId = 3; }
                        else if (status == "Order Out For Delivery") { statusId = 4; }
                        else if (status == "Order Delivered") { statusId = 5; }
                        else if (status == "Product Replaced") { statusId = 8; }
                        else if (status == "Payment Refunded") { statusId = 10; }
                        statusList.Add(new StatusCode { StatusId = statusId, Date = DateTime.UtcNow, Description = status });
                        product.StatusCode = statusList;
                        productDetails.Add(product);
                    }
                    var updateProductDetails = MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("OrderId", orderid), "OrderDB", "OrderInfo", Builders<BsonDocument>.Update.Set("ProductDetails", productDetails)).Result;
                    if (updateProductDetails == false)
                    {
                        return BadRequest(new ResponseData
                        {
                            Code = "402",
                            Message = "Product details order status update failed",
                            Data = null
                        });
                    }
                    return Ok(new ResponseData
                    {
                        Code = "200",
                        Message = "Status updated",
                        Data = null
                    });
                }
                else
                {
                    return BadRequest(new ResponseData
                    {
                        Code = "404",
                        Message = "Order not found",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("OrderController", "UpdateDeliveryStatus", "UpdateDeliveryStatus", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = ex.Message
                });
            }
        }

        /// <summary>Cancel placed order</summary>
        /// <param name="orderid">id of order placed</param>
        /// <param name="productSKU">SKU of product which needs to be cancelled</param>
        /// <response code="200">Order cancelled</response>
        /// <response code="401">Cancel request cannot be processed as the product is delivered</response> 
        /// <response code="402">Order already cancelled</response> 
        /// <response code="403">Order request cannot be processed</response> 
        /// <response code="404">No orders found</response> 
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
                var checkData = MH.CheckForDatas("OrderId", orderid, null, null, "OrderDB", "OrderInfo");
                if (checkData != null)
                {
                    var orderDetails = BsonSerializer.Deserialize<OrderInfo>(checkData);
                    if (MH.CheckForDatas("ProductSKU", productSKU, null, null, "ProductDB", "Product") != null)
                    {
                        List<ProductDetails> productDetails = new List<ProductDetails>();
                        foreach (var product in orderDetails.ProductDetails)
                        {
                            if (product.ProductSKU == productSKU)
                            {
                                if (product.Status == "Order delivered")
                                {
                                    return BadRequest(new ResponseData
                                    {
                                        Code = "401",
                                        Message = "Cancel request cannot be processed as the product is delivered",
                                        Data = null
                                    });
                                }
                                else if (product.Status == "Order cancelled")
                                {
                                    return BadRequest(new ResponseData
                                    {
                                        Code = "402",
                                        Message = "Order already cancelled",
                                        Data = null
                                    });
                                }
                                else if (product.Status == "Order replaced" || orderDetails.OrderStatus == "Order refunded")
                                {
                                    return BadRequest(new ResponseData
                                    {
                                        Code = "403",
                                        Message = "Cancel request cannot be processed as the order status is " + orderDetails.OrderStatus,
                                        Data = null
                                    });
                                }
                                product.Status = "Order cancelled";
                                List<StatusCode> statusCode = new List<StatusCode>();
                                foreach (var status in product.StatusCode)
                                {
                                    statusCode.Add(status);
                                }
                                statusCode.Add(new StatusCode { Date = DateTime.UtcNow, StatusId = 5, Description = "Order cancelled" });
                                product.StatusCode = statusCode;
                            }
                            productDetails.Add(product);
                        }
                        var updateOrderStatus = MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("OrderId", orderid), "OrderDB", "OrderInfo", Builders<BsonDocument>.Update.Set("OrderStatus", "Order Cancelled")).Result;
                        if (updateOrderStatus == false)
                        {
                            return BadRequest(new ResponseData
                            {
                                Code = "405",
                                Message = "Order status update failed",
                                Data = null
                            });
                        }
                        var updateProductDetails = MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("OrderId", orderid), "OrderDB", "OrderInfo", Builders<BsonDocument>.Update.Set("ProductDetails", productDetails)).Result;
                        if (updateProductDetails == false)
                        {
                            return BadRequest(new ResponseData
                            {
                                Code = "406",
                                Message = "Order cancelled status update failed",
                                Data = null
                            });
                        }
                        //if(orderDetails.PaymentMethod != "COD")
                        //{
                        //Need to refund amount
                        //}
                        return Ok(new ResponseData
                        {
                            Code = "200",
                            Message = "Order cancelled",
                            Data = null
                        });
                    }
                    else
                    {
                        return BadRequest(new ResponseData
                        {
                            Code = "407",
                            Message = "Product not found",
                            Data = null
                        });
                    }
                }
                else
                {
                    return BadRequest(new ResponseData
                    {
                        Code = "404",
                        Message = "Order not found",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("OrderController", "CancelOrder", "CancelOrder", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = ex.Message
                });
            }
        }

        [HttpPut("return/{request}/{orderid}/{productSKU}")]
        public ActionResult ReturnOrder(string request, int orderid, string productSKU)
        {
            if (request == "refund" || request == "replace")
            {
                try
                {
                    var checkData = MH.CheckForDatas("OrderId", orderid, null, null, "OrderDB", "OrderInfo");
                    if (checkData != null)
                    {
                        var orderDetails = BsonSerializer.Deserialize<OrderInfo>(checkData);
                        if (orderDetails.OrderStatus == "Order delivered")
                        {
                            foreach (var product in orderDetails.ProductDetails)
                            {
                                if (product.ProductSKU == productSKU)
                                {
                                    foreach (var status in product.StatusCode)
                                    {
                                        if (status.Description == "Order delivered")
                                        {
                                            if (DateTime.UtcNow > status.Date.AddDays(15))
                                            {
                                                return BadRequest(new ResponseData
                                                {
                                                    Code = "407",
                                                    Message = "Return request cannot be processed as it has been more than 15 days from delivery",
                                                    Data = null
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                            var checkProduct = MH.CheckForDatas("ProductSKU", productSKU, null, null, "ProductDB", "Product");
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
                                                product.Status = "Refund initiated";
                                                List<StatusCode> statusList = new List<StatusCode>();
                                                foreach (var status in product.StatusCode)
                                                {
                                                    statusList.Add(status);
                                                }
                                                statusList.Add(new StatusCode { Date = DateTime.UtcNow, StatusId = 9, Description = "Refund initiated" });
                                            }
                                            productDetails.Add(product);
                                        }
                                        var updateProductDetails = MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("OrderId", orderid), "OrderDB", "OrderInfo", Builders<BsonDocument>.Update.Set("ProductDetails", productDetails)).Result;
                                        if (updateProductDetails == false)
                                        {
                                            return BadRequest(new ResponseData
                                            {
                                                Code = "408",
                                                Message = "Product details status update failed",
                                                Data = null
                                            });
                                        }
                                        //if(orderDetails.PaymentMethod != "COD")
                                        //{
                                        //Need to refund amount
                                        //}
                                        return Ok(new ResponseData
                                        {
                                            Code = "201",
                                            Message = "Payment refund initiated",
                                            Data = null
                                        });
                                    }
                                    else
                                    {
                                        return BadRequest(new ResponseData
                                        {
                                            Code = "403",
                                            Message = "Refund not applicable for this product",
                                            Data = null
                                        });
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
                                                product.Status = "Product replacement initiated";
                                                List<StatusCode> statusList = new List<StatusCode>();
                                                foreach (var status in product.StatusCode)
                                                {
                                                    statusList.Add(status);
                                                }
                                                statusList.Add(new StatusCode { Date = DateTime.UtcNow, StatusId = 7, Description = "Product replacement initiated" });
                                            }
                                            productDetails.Add(product);
                                        }
                                        var updateProductDetails = MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("OrderId", orderid), "OrderDB", "OrderInfo", Builders<BsonDocument>.Update.Set("ProductDetails", productDetails)).Result;
                                        if (updateProductDetails == false)
                                        {
                                            return BadRequest(new ResponseData
                                            {
                                                Code = "408",
                                                Message = "Product details status update failed",
                                                Data = null
                                            });
                                        }
                                        return Ok(new ResponseData
                                        {
                                            Code = "202",
                                            Message = "Product replacement initiated",
                                            Data = null
                                        });
                                    }
                                    else
                                    {
                                        return BadRequest(new ResponseData
                                        {
                                            Code = "405",
                                            Message = "replacement not applicable for this product",
                                            Data = null
                                        });
                                    }
                                }
                            }
                            else
                            {
                                return BadRequest(new ResponseData
                                {
                                    Code = "402",
                                    Message = "product not found",
                                    Data = null
                                });
                            }
                        }
                        else
                        {
                            return BadRequest(new ResponseData
                            {
                                Code = "406",
                                Message = "Return request cannot be processed as the product is not delivered",
                                Data = null
                            });
                        }
                    }
                    else
                    {
                        return BadRequest(new ResponseData
                        {
                            Code = "404",
                            Message = "Order not found",
                            Data = null
                        });
                    }
                }
                catch (Exception ex)
                {
                    LoggerDataAccess.CreateLog("OrderController", "CancelOrder", "CancelOrder", ex.Message);
                    return BadRequest(new ResponseData
                    {
                        Code = "400",
                        Message = "Failed",
                        Data = ex.Message
                    });
                }
            }
            else
            {
                return BadRequest(new ResponseData
                {
                    Code = "401",
                    Message = "Invalid request",
                    Data = null
                });
            }
        }

    }
}