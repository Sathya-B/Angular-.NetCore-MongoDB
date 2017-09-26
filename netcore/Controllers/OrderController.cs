using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arthur_Clive.Data;
using Arthur_Clive.Logger;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MH = Arthur_Clive.Helper.MongoHelper;

namespace Arthur_Clive.Controllers
{
    [Route("api/[controller]")]
    public class OrderController : Controller
    {
        public IMongoDatabase _db = MH._client.GetDatabase("UserInfo");
        public IMongoDatabase order_db = MH._client.GetDatabase("OrderDB");
        public UpdateDefinition<BsonDocument> updateDefinition;

        //    [HttpPost("placeorder")]
        //    public async Task<ActionResult> PlaceOrder([FromBody]OrderInfo data)
        //    {
        //        try
        //        {
        //            var collection = _db.GetCollection<Cart>("Cart");
        //            var filter = Builders<Cart>.Filter.Eq("UserName", data.UserName);
        //            IAsyncCursor<Cart> cursor = await collection.FindAsync(filter);
        //            var products = cursor.ToList();
        //            if (products != null)
        //            {
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LoggerDataAccess.CreateLog("OrderController", "PlaceOrder", "PlaceOrder", ex.Message);
        //            return BadRequest(new ResponseData
        //            {
        //                Code = "400",
        //                Message = "Failed",
        //                Data = ex.Message
        //            });
        //        }
        //    }



        //    [HttpPost("placeorder")]
        //    public async Task<ActionResult> PlaceOrder([FromBody]OrderInfo data)
        //    {
        //        try
        //        {
        //            var collection = _db.GetCollection<Cart>("Cart");
        //            var filter = Builders<Cart>.Filter.Eq("UserName", data.UserName);
        //            IAsyncCursor<Cart> cursor = await collection.FindAsync(filter);
        //            var products = cursor.ToList();
        //            if (products != null)
        //            {
        //                var userCollection = _db.GetCollection<Address>("UserInfo");
        //                var userFilter = Builders<Address>.Filter.Eq("UserName", data.UserName);
        //                IAsyncCursor<Address> userCursor = await userCollection.FindAsync(userFilter);
        //                var userInfo = cursor.ToList();
        //                foreach(var user in userInfo)

        //                if (userInfo != null)
        //                {
        //                    foreach (var product in products)
        //                    {
        //                        var productFilter = Builders<BsonDocument>.Filter.Eq("Product_SKU", product.ProductSKU);
        //                        var productData = BsonSerializer.Deserialize<Product>(MH.GetSingleObject(productFilter, "ProductDB", "Product").Result);
        //                        if (productData != null)
        //                        {
        //                            if (productData.ProductStock < product.ProductQuantity)
        //                            {
        //                                return BadRequest(new ResponseData
        //                                {
        //                                    Code = "401",
        //                                    Message = "Product Stock Does Not Match the Required Order Quantity",
        //                                    Data = product.ProductSKU
        //                                });
        //                            }
        //                            var update = Builders<BsonDocument>.Update.Set("Product_Stock", productData.ProductStock - product.ProductQuantity);
        //                            var result = MH.UpdateSingleObject(productFilter, "ProductDB", "Product", update).Result;
        //                        }
        //                        else
        //                        {
        //                            return BadRequest(new ResponseData
        //                            {
        //                                Code = "303",
        //                                Message = "Product not Found",
        //                                Data = null
        //                            });
        //                        }
        //                    }
        //                    int i = 0;
        //                    foreach (var address in user.ShippingAddress)
        //                    {
        //                        if (address.Default == true)
        //                        {
        //                            data.ShippingAddress = user.ShippingAddress[i];
        //                        }
        //                        i++;
        //                    }
        //                    int j = 0;
        //                    foreach (var address in user.BillingAddress)
        //                    {
        //                        if (address.Default == true)
        //                        {
        //                            data.BillingAddress = user.BillingAddress[j];
        //                        }
        //                        j++;
        //                    }
        //                    var orderCollection = order_db.GetCollection<OrderInfo>("OrderInfo");
        //                    var orderFilter = Builders<OrderInfo>.Filter.Eq("UserName", data.UserName);
        //                    IAsyncCursor<OrderInfo> orderCursor = await orderCollection.FindAsync(orderFilter);
        //                    var orders = orderCursor.ToList();
        //                    int k = 0;
        //                    if (orders != null)
        //                    {
        //                        foreach (var order in orders)
        //                        {
        //                            k++;
        //                        }
        //                    }
        //                    data.OrderId = k + 1;
        //                    PaymentMethod paymentMethod = new PaymentMethod();
        //                    paymentMethod.Description = data.PaymentMethod;
        //                    List<StatusCode> paymentStatusCode = new List<StatusCode>();
        //                    if (data.PaymentMethod == "Cash On Delivery")
        //                    {
        //                        paymentStatusCode.Add(new StatusCode { StatusId = 1, Description = "Payment Pending", Date = DateTime.UtcNow });
        //                    }
        //                    if (data.PaymentMethod == "Net Banking")
        //                    {
        //                        paymentStatusCode.Add(new StatusCode { StatusId = 2, Description = "Payment Through Net Banking is Initiated", Date = DateTime.UtcNow });
        //                    }
        //                    if (data.PaymentMethod == "Card")
        //                    {
        //                        paymentStatusCode.Add(new StatusCode { StatusId = 2, Description = "Payment Through Credit/Debit/ATM Card is Initiated", Date = DateTime.UtcNow });
        //                    }
        //                    paymentMethod.Status = paymentStatusCode;
        //                    data.PaymentDetails = paymentMethod;
        //                    List<ProductDetails> productDetailsList = new List<ProductDetails>();
        //                    foreach (var product in products)
        //                    {
        //                        ProductDetails productDetails = new ProductDetails();
        //                        productDetails.ProductSKU = product.ProductSKU;
        //                        productDetails.Status = "Order Placed";
        //                        List<StatusCode> statusCodeList = new List<StatusCode>();
        //                        statusCodeList.Add(new StatusCode { StatusId = 1, Description = "Order Placed", Date = DateTime.UtcNow });
        //                        productDetails.StatusCode = statusCodeList;
        //                        productDetails.ProductInCart = product;
        //                        productDetailsList.Add(productDetails);
        //                    }
        //                    data.ProductDetails = productDetailsList;
        //                    await orderCollection.InsertOneAsync(data);
        //                    var query = Query<Cart>.EQ(e => e.UserName, data.UserName);
        //                    foreach (var product in products)
        //                    {
        //                        var cartFilter = Builders<BsonDocument>.Filter.Eq("_id", product.Id);
        //                        MH.DeleteSingleObject(cartFilter, "UserInfo", "Cart");
        //                    }
        //                    return Ok(new ResponseData
        //                    {
        //                        Code = "200",
        //                        Message = "Order Placed",
        //                        Data = null
        //                    });
        //                }
        //                else
        //                {
        //                    return BadRequest(new ResponseData
        //                    {
        //                        Code = "302",
        //                        Message = "User Info not Found",
        //                        Data = null
        //                    });
        //                }
        //            }
        //            else
        //            {
        //                return BadRequest(new ResponseData
        //                {
        //                    Code = "301",
        //                    Message = "Cart is Empty",
        //                    Data = null
        //                });
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LoggerDataAccess.CreateLog("OrderController", "PlaceOrder", "PlaceOrder", ex.Message);
        //            return BadRequest(new ResponseData
        //            {
        //                Code = "400",
        //                Message = "Failed",
        //                Data = ex.Message
        //            });
        //        }
        //    }

        //    [HttpPost("view")]
        //    public async Task<ActionResult> ViewOrders([FromBody]OrderRequest data, string request)
        //    {
        //        try
        //        {
        //            var collection = order_db.GetCollection<OrderInfo>("OrderInfo");
        //            var filter = Builders<OrderInfo>.Filter.Eq("UserName", data.UserName);
        //            IAsyncCursor<OrderInfo> cursor = await collection.FindAsync(filter);
        //            var orders = cursor.ToList();
        //            if (orders != null)
        //            {
        //                if (request == "View")
        //                {
        //                    return Ok(new ResponseData
        //                    {
        //                        Code = "200",
        //                        Message = "Success",
        //                        Data = orders
        //                    });
        //                }
        //            }
        //            return BadRequest(new ResponseData
        //            {
        //                Code = "401",
        //                Message = "No orders Found",
        //                Data = null
        //            });
        //        }
        //        catch (Exception ex)
        //        {
        //            LoggerDataAccess.CreateLog("OrderController", "ViewOrders", "ViewOrders", ex.Message);
        //            return BadRequest(new ResponseData
        //            {
        //                Code = "400",
        //                Message = "Failed",
        //                Data = ex.Message
        //            });
        //        }
        //    }

        //    [HttpPost("{request}")]
        //    public async Task<ActionResult> StatusUpdate([FromBody]OrderRequest data, string request)
        //    {
        //        try
        //        {
        //            var collection = order_db.GetCollection<OrderInfo>("OrderInfo");
        //            var filter = Builders<OrderInfo>.Filter.Eq("UserName", data.UserName);
        //            IAsyncCursor<OrderInfo> cursor = await collection.FindAsync(filter);
        //            var orders = cursor.ToList();
        //            if (orders != null)
        //            {
        //                if (request == "Cancel" || request == "Refund" || request == "Return")
        //                {
        //                    foreach (var order in orders)
        //                    {
        //                        if (order.OrderId == data.OrderId)
        //                        {
        //                            if (request == "Cancel")
        //                            {
        //                                List<ProductDetails> productDetailsList = new List<ProductDetails>();
        //                                foreach (var product in order.ProductDetails)
        //                                {
        //                                    if (product.Status == "Canceled")
        //                                    {
        //                                        return BadRequest(new ResponseData
        //                                        {
        //                                            Code = "303",
        //                                            Message = "Order Already Canceled",
        //                                            Data = order
        //                                        });
        //                                    }
        //                                    if (product.Status == "Order Placed")
        //                                    {
        //                                        if (product.ProductSKU == data.ProductSKU)
        //                                        {
        //                                            ProductDetails productDetails = new ProductDetails();
        //                                            productDetails.Status = "Canceled";
        //                                            productDetails.ProductInCart = product.ProductInCart;
        //                                            List<StatusCode> statusCodeList = new List<StatusCode>();
        //                                            StatusCode statusCode = new StatusCode();
        //                                            foreach (var status in product.StatusCode)
        //                                            {
        //                                                statusCodeList.Add(status);
        //                                            }
        //                                            statusCode.StatusId = 2;
        //                                            statusCode.Description = "Canceled";
        //                                            statusCode.Date = DateTime.UtcNow;
        //                                            statusCodeList.Add(statusCode);
        //                                            productDetails.StatusCode = statusCodeList;
        //                                            productDetailsList.Add(productDetails);
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        productDetailsList.Add(product);
        //                                    }
        //                                }
        //                                updateDefinition = Builders<BsonDocument>.Update.Set("ProductDetails", productDetailsList);
        //                                var updateFilter = Builders<BsonDocument>.Filter.Eq("UserName", data.UserName) & Builders<BsonDocument>.Filter.Eq("OrderId", data.OrderId);
        //                                await MH.UpdateSingleObject(updateFilter, "OrderDB", "OrderInfo", updateDefinition);
        //                                if (order.PaymentMethod == "Cash On Delivery")
        //                                {
        //                                    return Ok(new ResponseData
        //                                    {
        //                                        Code = "201",
        //                                        Message = "Order Canceled",
        //                                        Data = orders
        //                                    });
        //                                }
        //                                return Ok(new ResponseData
        //                                {
        //                                    Code = "202",
        //                                    Message = "Payment Refund Initiated",
        //                                    Data = orders
        //                                });
        //                            }
        //                            if (request == "Return")
        //                            {
        //                                List<ProductDetails> productDetailsList = new List<ProductDetails>();
        //                                foreach (var product in order.ProductDetails)
        //                                {
        //                                    if (product.Status == "Delivered")
        //                                    {
        //                                        var productFilter = Builders<BsonDocument>.Filter.Eq("Product_SKU", data.ProductSKU);
        //                                        var productData = BsonSerializer.Deserialize<Product>(MH.GetSingleObject(productFilter, "ProductDB", "Product").Result);
        //                                        if (request == "Refund")
        //                                        {
        //                                            if (productData.RefundApplicable == true)
        //                                            {
        //                                                if (product.ProductSKU == data.ProductSKU)
        //                                                {
        //                                                    ProductDetails productDetails = new ProductDetails();
        //                                                    productDetails.Status = "Refund Initiated";
        //                                                    productDetails.ProductInCart = product.ProductInCart;
        //                                                    List<StatusCode> statusCodeList = new List<StatusCode>();
        //                                                    StatusCode statusCode = new StatusCode();
        //                                                    foreach (var status in product.StatusCode)
        //                                                    {
        //                                                        statusCodeList.Add(status);
        //                                                    }
        //                                                    statusCode.StatusId = 4;
        //                                                    statusCode.Description = "Refund Initiated";
        //                                                    statusCode.Date = DateTime.UtcNow;
        //                                                    statusCodeList.Add(statusCode);
        //                                                    productDetails.StatusCode = statusCodeList;
        //                                                    productDetailsList.Add(productDetails);
        //                                                }

        //                                                else
        //                                                {
        //                                                    productDetailsList.Add(product);
        //                                                }
        //                                            }
        //                                            else
        //                                            {
        //                                                return BadRequest(new ResponseData
        //                                                {
        //                                                    Code = "304",
        //                                                    Message = "Refund Not Applicable",
        //                                                    Data = productData
        //                                                });
        //                                            }
        //                                        }
        //                                        if (request == "Replace")
        //                                        {
        //                                            if (productData.ReplacementApplicable == true)
        //                                            {
        //                                                if (product.ProductSKU == data.ProductSKU)
        //                                                {
        //                                                    ProductDetails productDetails = new ProductDetails();
        //                                                    productDetails.Status = "Return Initiated";
        //                                                    productDetails.ProductInCart = product.ProductInCart;
        //                                                    List<StatusCode> statusCodeList = new List<StatusCode>();
        //                                                    StatusCode statusCode = new StatusCode();
        //                                                    foreach (var status in product.StatusCode)
        //                                                    {
        //                                                        statusCodeList.Add(status);
        //                                                    }
        //                                                    statusCode.StatusId = 7;
        //                                                    statusCode.Description = "Return Initiated";
        //                                                    statusCode.Date = DateTime.UtcNow;
        //                                                    statusCodeList.Add(statusCode);
        //                                                    productDetails.StatusCode = statusCodeList;
        //                                                    productDetailsList.Add(productDetails);
        //                                                }

        //                                                else
        //                                                {
        //                                                    productDetailsList.Add(product);
        //                                                }
        //                                            }
        //                                            else
        //                                            {
        //                                                return BadRequest(new ResponseData
        //                                                {
        //                                                    Code = "305",
        //                                                    Message = "Replacement Not Applicable",
        //                                                    Data = productData
        //                                                });
        //                                            }
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        return BadRequest(new ResponseData
        //                                        {
        //                                            Code = "303",
        //                                            Message = "Request Cannot be Processed as The Product is not Delivered Yet",
        //                                            Data = null
        //                                        });
        //                                    }
        //                                }
        //                                updateDefinition = Builders<BsonDocument>.Update.Set("ProductDetails", productDetailsList);
        //                                var updateFilter = Builders<BsonDocument>.Filter.Eq("UserName", data.UserName) & Builders<BsonDocument>.Filter.Eq("OrderId", data.OrderId);
        //                                await MH.UpdateSingleObject(updateFilter, "OrderDB", "OrderInfo", updateDefinition);
        //                                if (request == "Refund")
        //                                {
        //                                    return Ok(new ResponseData
        //                                    {
        //                                        Code = "202",
        //                                        Message = "Refund Initiated",
        //                                        Data = null
        //                                    });
        //                                }
        //                                if (request == "Return")
        //                                {
        //                                    return Ok(new ResponseData
        //                                    {
        //                                        Code = "203",
        //                                        Message = "Return Initiated",
        //                                        Data = null
        //                                    });
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    return BadRequest(new ResponseData
        //                    {
        //                        Code = "302",
        //                        Message = "Request Not Valid",
        //                        Data = null
        //                    });
        //                }
        //            }
        //            return BadRequest(new ResponseData
        //            {
        //                Code = "301",
        //                Message = "No orders Found",
        //                Data = null
        //            });
        //        }
        //        catch (Exception ex)
        //        {
        //            LoggerDataAccess.CreateLog("OrderController", "UpdateStatus", "UpdateStatus", ex.Message);
        //            return BadRequest(new ResponseData
        //            {
        //                Code = "400",
        //                Message = "Failed",
        //                Data = null
        //            });
        //        }
        //    }
    }
}
