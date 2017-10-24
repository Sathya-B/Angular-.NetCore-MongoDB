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
using GH = Arthur_Clive.Helper.GlobalHelper;
using Swashbuckle.AspNetCore.Examples;
using Arthur_Clive.Swagger;
using Arthur_Clive.Helper;

namespace Arthur_Clive.Controllers
{
    /// <summary>Controller to view,cancle, return and place orders</summary>
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
                        var ordersCollection = order_db.GetCollection<OrderInfo>("OrderInfo");
                        var ordersCount = ordersCollection.Find(Builders<OrderInfo>.Filter.Empty).Count();
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
                                productStatus.Add(new StatusCode { StatusId = 1, Date = DateTime.UtcNow, Description = "OrderPlaced" });
                                productDetails.StatusCode = productStatus;
                                productDetails.ProductInCart = cart;
                                productList.Add(productDetails);
                            }
                        }
                        data.ProductDetails = productList;
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

        /// <summary>Cancle order placed by a user</summary>
        /// <remarks>This api is user ro cancle a order placed by a user</remarks>
        /// <param name="data">Info of order which needs to be cancled</param>
        /// <param name="username">UserName of user who needs to cancle an order</param>
        /// <param name="productSKU">SKU of product whos order needs to be cancled</param>
        /// <response code="200">Order successfully cancled</response>
        /// <response code="404">No orders found</response> 
        /// <response code="401">Order cancle request failed</response> 
        /// <response code="400">Process ran into an exception</response> 
        [HttpPost("cancel/{username}/{productSKU}")]
        [SwaggerRequestExample(typeof(OrderInfo), typeof(OrderRequestDetails))]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public async Task<ActionResult> CancelOrder([FromBody]OrderInfo data, string username, string productSKU)
        {
            try
            {
                var orders = MH.GetOrders(username, order_db).Result;
                if (orders == null)
                {
                    return BadRequest(new ResponseData
                    {
                        Code = "404",
                        Message = "No orders found",
                        Data = null
                    });
                }
                else
                {
                    IAsyncCursor<OrderInfo> orderCursor = await order_db.GetCollection<OrderInfo>("OrderInfo").FindAsync(Builders<OrderInfo>.Filter.Eq("UserName", username) & Builders<OrderInfo>.Filter.Eq("OrderId", data.OrderId));
                    var order = orderCursor.FirstOrDefaultAsync().Result;
                    if (order == null)
                    {
                        return BadRequest(new ResponseData
                        {
                            Code = "404",
                            Message = "Order Not Found",
                            Data = null
                        });
                    }
                    foreach (var productDetails in order.ProductDetails)
                    {
                        if (productDetails.Status == "Cancled" || productDetails.Status == "Refunded" || productDetails.Status == "Replaced" || productDetails.Status == "Delivered")
                        {
                            return BadRequest(new ResponseData
                            {
                                Code = "401",
                                Message = "Order Cancle Request Failed",
                                Data = null
                            });
                        }
                        else
                        {
                            PaymentMethod paymentMethod = new PaymentMethod();
                            List<StatusCode> paymentList = new List<StatusCode>();
                            foreach (var status in order.PaymentDetails.Status)
                            {
                                paymentList.Add(status);
                            }
                            StatusCode paymentStatus = new StatusCode { StatusId = 3, Date = DateTime.UtcNow, Description = "Payment refund initiated" };
                            paymentList.Add(paymentStatus);
                            paymentMethod.Status = paymentList;
                            var result = MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("OrderId", data.OrderId), "OrderDB", "OrderInfo", Builders<BsonDocument>.Update.Set("PaymentDetails", paymentMethod)).Result;
                            List<ProductDetails> productDetailsList = new List<ProductDetails>();
                            foreach (var product in order.ProductDetails)
                            {
                                if (product.ProductSKU == productSKU)
                                {
                                    ProductDetails details = new ProductDetails();
                                    details.ProductSKU = productSKU;
                                    details.Status = "Order cancled";
                                    List<StatusCode> productStatusList = new List<StatusCode>();
                                    foreach (var status in product.StatusCode) { productStatusList.Add(status); }
                                    StatusCode productStatus = new StatusCode { StatusId = 3, Date = DateTime.UtcNow, Description = "Order cancled" };
                                    productStatusList.Add(productStatus);
                                    details.StatusCode = productStatusList;
                                    details.ProductInCart = product.ProductInCart;
                                    productDetailsList.Add(details);
                                }
                                else
                                {
                                    productDetailsList.Add(product);
                                }
                            }
                            var responce = MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("OrderId", data.OrderId), "OrderDB", "OrderInfo", Builders<BsonDocument>.Update.Set("ProductDetails", productDetailsList)).Result;
                        }
                    }
                    IAsyncCursor<OrderInfo> orderCursorAfterUpdate = await order_db.GetCollection<OrderInfo>("OrderInfo").FindAsync(Builders<OrderInfo>.Filter.Eq("UserName", username) & Builders<OrderInfo>.Filter.Eq("OrderId", data.OrderId));
                    var orderAfterUpdate = orderCursorAfterUpdate.FirstOrDefaultAsync().Result;
                    foreach (var product in orderAfterUpdate.ProductDetails)
                    {
                        if (product.ProductSKU == productSKU)
                        {
                            IAsyncCursor<Product> productCursor = await product_db.GetCollection<Product>("Product").FindAsync(Builders<Product>.Filter.Eq("ProductSKU", productSKU));
                            var productData = productCursor.FirstOrDefaultAsync().Result;
                            var responce = MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("ProductSKU", productSKU), "ProductDB", "Product", Builders<BsonDocument>.Update.Set("ProductStock", productData.ProductStock + product.ProductInCart.ProductQuantity)).Result;
                        }
                    }
                    return Ok(new ResponseData
                    {
                        Code = "200",
                        Message = "Success",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("OrderController", "CancleOrder", "CancleOrder", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = ex.Message
                });
            }
        }

        /// <summary>Return a product placed by a user</summary>
        /// <remarks>This api is user to return a product to get refund or replacement for the product</remarks>
        /// <param name="data">Data of product which needs to be retuned</param>
        /// <param name="request">Request needed to process return of product</param>
        /// <param name="username">UserName of user who need to return a product</param>
        /// <param name="productSKU">SKU of product which needs to be returned</param>
        /// <returns></returns>
        /// <response code="200">Return request successfully initiated for this product</response>
        /// <response code="404">No orders found</response> 
        /// <response code="401">Product return request failed</response> 
        /// <response code="402">Refund not appliccable for this product</response> 
        /// <response code="403">Replacement not aplicable for this product</response> 
        /// <response code="405">Resuest is not valid</response> 
        /// <response code="400">Process ran into an exception</response> 
        [HttpPost("{request}/{username}/{productSKU}")]
        [SwaggerRequestExample(typeof(OrderInfo), typeof(OrderDetails))]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public async Task<ActionResult> ReturnProduct([FromBody]OrderInfo data, string request, string username, string productSKU)
        {
            try
            {
                if (request == "refund" || request == "replace")
                {
                    var orders = MH.GetOrders(username, order_db).Result;
                    if (orders == null)
                    {
                        return BadRequest(new ResponseData
                        {
                            Code = "404",
                            Message = "No orders found",
                            Data = null
                        });
                    }
                    else
                    {
                        IAsyncCursor<OrderInfo> orderCursor = await order_db.GetCollection<OrderInfo>("OrderInfo").FindAsync(Builders<OrderInfo>.Filter.Eq("OrderId", data.OrderId));
                        var order = orderCursor.FirstOrDefaultAsync().Result;
                        if (order == null)
                        {
                            return BadRequest(new ResponseData
                            {
                                Code = "404",
                                Message = "Order Not Found",
                                Data = null
                            });
                        }
                        foreach (var productDetails in order.ProductDetails)
                        {
                            if (productDetails.ProductSKU == productSKU)
                            {
                                var productData = BsonSerializer.Deserialize<Product>(MH.GetSingleObject(Builders<BsonDocument>.Filter.Eq("ProductSKU", productSKU), "ProductDB", "Product").Result);
                                if (productDetails.Status != "Order delivered")
                                {
                                    return BadRequest(new ResponseData
                                    {
                                        Code = "401",
                                        Message = "Order Return Request Failed",
                                        Data = null
                                    });
                                }
                                else
                                {
                                    if (request == "refund")
                                    {
                                        if (productData.RefundApplicable == false)
                                        {
                                            return BadRequest(new ResponseData
                                            {
                                                Code = "402",
                                                Message = "Refund not applicable for this product",
                                                Data = null
                                            });
                                        }
                                    }
                                    if (request == "replace")
                                    {
                                        if (productData.ReplacementApplicable == false)
                                        {
                                            return BadRequest(new ResponseData
                                            {
                                                Code = "403",
                                                Message = "Replacement not applicable for this product",
                                                Data = null
                                            });
                                        }
                                    }
                                    if (request == "refund")
                                    {
                                        PaymentMethod paymentMethod = new PaymentMethod();
                                        List<StatusCode> paymentList = new List<StatusCode>();
                                        foreach (var status in order.PaymentDetails.Status)
                                        {
                                            paymentList.Add(status);
                                        }
                                        StatusCode paymentStatus = new StatusCode { StatusId = 3, Date = DateTime.UtcNow, Description = "Payment refund initiated" };
                                        paymentList.Add(paymentStatus);
                                        paymentMethod.Status = paymentList;
                                        var result = MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("OrderId", data.OrderId), "OrderDB", "OrderInfo", Builders<BsonDocument>.Update.Set("PaymentDetails", paymentMethod)).Result;
                                    }
                                    List<ProductDetails> productDetailsList = new List<ProductDetails>();
                                    foreach (var product in order.ProductDetails)
                                    {
                                        if (product.ProductSKU == productSKU)
                                        {
                                            ProductDetails details = new ProductDetails();
                                            details.ProductSKU = productSKU;
                                            details.Status = "Refund Initiated";
                                            List<StatusCode> productStatusList = new List<StatusCode>();
                                            foreach (var status in product.StatusCode){ productStatusList.Add(status); }
                                            StatusCode productStatus = new StatusCode();
                                            productStatus.Date = DateTime.UtcNow;
                                            if (request == "refund")
                                            {
                                                productStatus.StatusId = 6;
                                                productStatus.Description = "Order refund initiated";
                                            }
                                            if (request == "replace")
                                            {
                                                productStatus.StatusId = 4;
                                                productStatus.Description = "Order replacement initiated";
                                            }
                                            productStatusList.Add(productStatus);
                                            details.StatusCode = productStatusList;
                                            details.ProductInCart = product.ProductInCart;
                                            productDetailsList.Add(details);
                                        }
                                        else
                                        {
                                            productDetailsList.Add(product);
                                        }
                                    }
                                    var responce = MH.UpdateSingleObject( Builders<BsonDocument>.Filter.Eq("OrderId", data.OrderId), "OrderDB", "OrderInfo", Builders<BsonDocument>.Update.Set("ProductDetails", productDetailsList)).Result;
                                }
                            }
                        }
                        return Ok(new ResponseData
                        {
                            Code = "200",
                            Message = "Success",
                            Data = null
                        });
                    }
                }
                else
                {
                    return BadRequest(new ResponseData
                    {
                        Code = "405",
                        Message = "Invalid Request",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("OrderController", "ReturnProduct", "ReturnProduct", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = ex.Message
                });
            }
        }

        /// <summary>
        /// Update status of order placed by a user
        /// </summary>
        /// <remarks>This api is used to update status of a product</remarks>
        /// <param name="data">Data required to update status</param>
        /// <param name="username">UserName of user whoes status needs to be updated</param>
        /// <param name="productSKU">SKU of product whoes status needs to be updated</param>
        /// <returns></returns>
        /// <response code="200">Status for product is updated successfully</response>
        /// <response code="404">Order not found</response> 
        /// <response code="401">Status is empty</response> 
        /// <response code="400">Process ran into an exception</response> 
        [HttpPost("updatestatus/{username}/{productSKU}")]
        [SwaggerRequestExample(typeof(StatusUpdate), typeof(StatusUpdateDetails))]
        [ProducesResponseType(typeof(ResponseData), 200)]
        public async Task<ActionResult> UpdateOrder([FromBody]StatusUpdate data, string username, string productSKU)
        {
            try
            {
                if (data.Status == null)
                {
                    return BadRequest(new ResponseData
                    {
                        Code = "401",
                        Message = "Status cannot be empty",
                        Data = null
                    });
                }
                IAsyncCursor<OrderInfo> orderCursor = await order_db.GetCollection<OrderInfo>("OrderInfo").FindAsync(Builders<OrderInfo>.Filter.Eq("OrderId", data.OrderId));
                var order = orderCursor.FirstOrDefaultAsync().Result;
                if (order == null)
                {
                    return BadRequest(new ResponseData
                    {
                        Code = "404",
                        Message = "Order Not Found",
                        Data = null
                    });
                }
                PaymentMethod paymentMethod = new PaymentMethod();
                List<StatusCode> statusList = new List<StatusCode>();
                List<ProductDetails> productList = new List<ProductDetails>();
                if (data.Status == "Delivered")
                {
                    int i = 1;
                    foreach (var status in order.PaymentDetails.Status)
                    {
                        statusList.Add(status);
                        i++;
                    }
                    if (order.PaymentMethod == "COD")
                    {
                        statusList.Add(new StatusCode { StatusId = i, Date = DateTime.UtcNow, Description = "Payment Received" });
                    }
                    paymentMethod.Status = statusList;

                    foreach (var product in order.ProductDetails)
                    {
                        if (product.ProductSKU == productSKU)
                        {
                            ProductDetails details = new ProductDetails();
                            details.ProductSKU = productSKU;
                            details.Status = order.PaymentMethod;
                            List<StatusCode> productStatusList = new List<StatusCode>();
                            int j = 1;
                            foreach (var status in product.StatusCode)
                            {
                                productStatusList.Add(status);
                                j++;
                            }
                            StatusCode productStatus = new StatusCode { StatusId = j, Date = DateTime.UtcNow, Description = data.Status };
                            productStatusList.Add(productStatus);
                            details.StatusCode = productStatusList;
                            details.ProductInCart = product.ProductInCart;
                            productList.Add(details);
                        }
                        else
                        {
                            productList.Add(product);
                        }
                    }
                }
                else if (data.Status == "Refunded" || data.Status == "Replaced" || data.Status == "Refund Failed" || data.Status == "Replacement Failed"
                            || data.Status == "Refund Cancle" || data.Status == "Replacement Cancled")
                {
                    int i = 1;
                    foreach (var status in order.PaymentDetails.Status)
                    {
                        statusList.Add(status);
                        i++;
                    }
                    if (order.PaymentMethod == "Cash On Delivery")
                    {
                        if (data.Status == "Refunded")
                        {
                            statusList.Add(new StatusCode { StatusId = i, Date = DateTime.UtcNow, Description = "Payment Refund Initiated" });
                        }
                    }
                    paymentMethod.Status = statusList;

                    foreach (var product in order.ProductDetails)
                    {
                        if (product.ProductSKU == productSKU)
                        {
                            ProductDetails details = new ProductDetails();
                            details.ProductSKU = productSKU;
                            details.Status = order.PaymentMethod;
                            List<StatusCode> productStatusList = new List<StatusCode>();
                            int j = 1;
                            foreach (var status in product.StatusCode)
                            {
                                productStatusList.Add(status);
                                j++;
                            }
                            StatusCode productStatus = new StatusCode { StatusId = j, Date = DateTime.UtcNow, Description = data.Status };
                            productStatusList.Add(productStatus);
                            details.StatusCode = productStatusList;
                            details.ProductInCart = product.ProductInCart;
                            productList.Add(details);
                        }
                        else
                        {
                            productList.Add(product);
                        }
                    }
                }
                var paymentUpdate = Builders<BsonDocument>.Update.Set("PaymentDetails", paymentMethod);
                var result = MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("UserName", username) & Builders<BsonDocument>.Filter.Eq("OrderId", data.OrderId), "OrderDB", "OrderInfo", paymentUpdate).Result;
                var productUpdate = Builders<BsonDocument>.Update.Set("ProductDetails", productList);
                var responce = MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("UserName", username) & Builders<BsonDocument>.Filter.Eq("OrderId", data.OrderId), "OrderDB", "OrderInfo", productUpdate).Result;
                return Ok(new ResponseData
                {
                    Code = "200",
                    Message = "Success",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("OrderController", "UpdateOrder", "UpdateOrder", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = ex.Message
                });
            }
        }

    }
}