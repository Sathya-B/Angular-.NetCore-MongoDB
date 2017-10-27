using Microsoft.AspNetCore.Mvc;
using Arthur_Clive.Helper;
using System;
using Arthur_Clive.Data;
using MH = Arthur_Clive.Helper.MongoHelper;
using PU = Arthur_Clive.Helper.PayUHelper;
using Arthur_Clive.Logger;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Arthur_Clive.Swagger;
using MongoDB.Driver;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Arthur_Clive.Controllers
{
    /// <summary>Controller to make payment using PayUMoney and get return responce</summary>
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class PaymentController : Controller
    {
        /// <summary></summary>
        public IMongoDatabase _db = MH._client.GetDatabase("UserInfo");
        /// <summary></summary>
        public IMongoDatabase product_db = MH._client.GetDatabase("ProductDB");
        /// <summary></summary>
        public IMongoDatabase order_db = MH._client.GetDatabase("OrderDB");

        /// <summary>Success responce from the PayU payment gateway</summary>
        /// <param name="paymentResponse">Responce data from PayU</param>
        [HttpPost("success")]
        public async Task<ActionResult> PaymentSuccess(IFormCollection paymentResponse)
        {
            if (paymentResponse != null)
            {
                PaymentModel paymentModel = new PaymentModel
                {
                    Email = paymentResponse["email"],
                    OrderId = Convert.ToInt16(paymentResponse["udf1"]),
                    UserName = paymentResponse["udf2"],
                    ProductInfo = paymentResponse["productinfo"],
                    FirstName = paymentResponse["firstname"],
                    Amount = paymentResponse["amount"],
                };
                try
                {
                    if (PU.Generatehash512(PU.GetReverseHashString(paymentResponse["txnid"], paymentModel)) == paymentResponse["hash"])
                    {
                        var updatePaymentMethod = await MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("OrderId", paymentModel.OrderId), "OrderDB", "OrderInfo", Builders<BsonDocument>.Update.Set("PaymentMethod", paymentResponse["mode"].ToString()));
                        PaymentMethod paymentDetails = new PaymentMethod();
                        List<StatusCode> statusCodeList = new List<StatusCode>();
                        var orderData = BsonSerializer.Deserialize<OrderInfo>(MH.GetSingleObject(Builders<BsonDocument>.Filter.Eq("OrderId", paymentModel.OrderId), "OrderDB", "OrderInfo").Result);
                        foreach (var detail in orderData.PaymentDetails.Status)
                        {
                            statusCodeList.Add(detail);
                        }
                        statusCodeList.Add(new StatusCode { StatusId = 2, Description = "Payment received", Date = DateTime.UtcNow });
                        paymentDetails.Status = statusCodeList;
                        var updatePaymentDetails = await MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("OrderId", paymentModel.OrderId), "OrderDB", "OrderInfo", Builders<BsonDocument>.Update.Set("PaymentDetails", paymentDetails));
                        IAsyncCursor<Cart> cartCursor = await _db.GetCollection<Cart>("Cart").FindAsync(Builders<Cart>.Filter.Eq("UserName", paymentModel.UserName));
                        var cartDatas = cartCursor.ToList();
                        foreach (var cart in cartDatas)
                        {
                            foreach (var product in MH.GetProducts(cart.ProductSKU, product_db).Result)
                            {
                                long updateQuantity = product.ProductStock - cart.ProductQuantity;
                                if (product.ProductStock - cart.ProductQuantity < 0)
                                {
                                    updateQuantity = 0;
                                    var emailResponce = EmailHelper.SendEmailToAdmin(paymentModel.UserName.ToString(),paymentModel.Email.ToString(), cart.ProductSKU, cart.ProductQuantity, product.ProductStock, paymentModel.OrderId).Result;
                                }
                                var result = MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("ProductSKU", cart.ProductSKU), "ProductDB", "Product", Builders<BsonDocument>.Update.Set("ProductStock", updateQuantity)).Result;
                            }
                        }
                        var removeCartItems = _db.GetCollection<Cart>("Cart").DeleteMany(Builders<Cart>.Filter.Eq("UserName", paymentModel.UserName));
                        return Redirect(GlobalHelper.ReadXML().Elements("payu").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("redirectsuccess").First().Value);
                    }
                    else
                    {
                        var updatePaymentMethod = await MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("OrderId", paymentModel.OrderId), "OrderDB", "OrderInfo", Builders<BsonDocument>.Update.Set("PaymentMethod", paymentResponse["mode"].ToString()));
                        PaymentMethod paymentDetails = new PaymentMethod();
                        List<StatusCode> statusCodeList = new List<StatusCode>();
                        var orderData = BsonSerializer.Deserialize<OrderInfo>(MH.GetSingleObject(Builders<BsonDocument>.Filter.Eq("OrderId", paymentModel.OrderId), "OrderDB", "OrderInfo").Result);
                        foreach (var detail in orderData.PaymentDetails.Status)
                        {
                            statusCodeList.Add(detail);
                        }
                        statusCodeList.Add(new StatusCode { StatusId = 3, Description = "Payment failed", Date = DateTime.UtcNow });
                        paymentDetails.Status = statusCodeList;
                        var updatePaymentDetails = await MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("OrderId", paymentModel.OrderId), "OrderDB", "OrderInfo", Builders<BsonDocument>.Update.Set("PaymentDetails", paymentDetails));
                        return Redirect(GlobalHelper.ReadXML().Elements("payu").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("redirectfailure").First().Value);
                    }

                }
                catch (Exception ex)
                {
                    LoggerDataAccess.CreateLog("PaymentController", "PaymentSuccess", "PaymentSuccess", ex.Message);
                    var updatePaymentMethod = await MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("OrderId", paymentModel.OrderId), "OrderDB", "OrderInfo", Builders<BsonDocument>.Update.Set("PaymentMethod", paymentResponse["mode"].ToString()));
                    PaymentMethod paymentDetails = new PaymentMethod();
                    List<StatusCode> statusCodeList = new List<StatusCode>();
                    var orderData = BsonSerializer.Deserialize<OrderInfo>(MH.GetSingleObject(Builders<BsonDocument>.Filter.Eq("OrderId", paymentModel.OrderId), "OrderDB", "OrderInfo").Result);
                    foreach (var detail in orderData.PaymentDetails.Status)
                    {
                        statusCodeList.Add(detail);
                    }
                    statusCodeList.Add(new StatusCode { StatusId = 3, Description = "Payment failed", Date = DateTime.UtcNow });
                    paymentDetails.Status = statusCodeList;
                    var updatePaymentDetails = await MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("OrderId", paymentModel.OrderId), "OrderDB", "OrderInfo", Builders<BsonDocument>.Update.Set("PaymentDetails", paymentDetails));
                    return Redirect(GlobalHelper.ReadXML().Elements("payu").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("redirectfailure").First().Value);
                }
            }
            else
            {
                return Redirect(GlobalHelper.ReadXML().Elements("payu").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("redirectfailure").First().Value);
            }

        }

        /// <summary>Failure responce from the PayU payment gateway</summary>
        /// <param name="paymentResponse">Responce data from PayU</param>
        [HttpPost("failed")]
        public async Task<ActionResult> PaymentFailed(IFormCollection paymentResponse)
        {
            if (paymentResponse != null)
            {
                string responseHash = paymentResponse["hash"];
                PaymentModel paymentModel = new PaymentModel
                {
                    Email = paymentResponse["email"],
                    OrderId = Convert.ToInt16(paymentResponse["udf1"]),
                    UserName = paymentResponse["udf2"],
                    ProductInfo = paymentResponse["productinfo"],
                    FirstName = paymentResponse["firstname"],
                    Amount = paymentResponse["amount"],
                };
                var updatePaymentMethod = await MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("OrderId", paymentModel.OrderId), "OrderDB", "OrderInfo", Builders<BsonDocument>.Update.Set("PaymentMethod", paymentResponse["mode"].ToString()));
                PaymentMethod paymentDetails = new PaymentMethod();
                List<StatusCode> statusCodeList = new List<StatusCode>();
                var orderData = BsonSerializer.Deserialize<OrderInfo>(MH.GetSingleObject(Builders<BsonDocument>.Filter.Eq("OrderId", paymentModel.OrderId), "OrderDB", "OrderInfo").Result);
                foreach (var detail in orderData.PaymentDetails.Status)
                {
                    statusCodeList.Add(detail);
                }
                statusCodeList.Add(new StatusCode { StatusId = 3, Description = "Payment failed", Date = DateTime.UtcNow });
                paymentDetails.Status = statusCodeList;
                var updatePaymentDetails = await MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("OrderId", paymentModel.OrderId), "OrderDB", "OrderInfo", Builders<BsonDocument>.Update.Set("PaymentDetails", paymentDetails));
                return Redirect(GlobalHelper.ReadXML().Elements("payu").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("redirectfailure").First().Value);
            }
            else
            {
                return Redirect(GlobalHelper.ReadXML().Elements("payu").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("redirectfailure").First().Value);
            }
        }

        /// <summary>Cancel responce from the PayU payment gateway</summary>
        /// <param name="paymentResponse">Responce data from PayU</param>
        [HttpPost("cancel")]
        public async Task<ActionResult> Paymentcancelled(IFormCollection paymentResponse)
        {
            if (paymentResponse != null)
            {
                string responseHash = paymentResponse["hash"];
                PaymentModel paymentModel = new PaymentModel
                {
                    Email = paymentResponse["email"],
                    OrderId = Convert.ToInt16(paymentResponse["udf1"]),
                    UserName = paymentResponse["udf2"],
                    ProductInfo = paymentResponse["productinfo"],
                    FirstName = paymentResponse["firstname"],
                    Amount = paymentResponse["amount"],
                };
                var updatePaymentMethod = await MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("OrderId", paymentModel.OrderId), "OrderDB", "OrderInfo", Builders<BsonDocument>.Update.Set("PaymentMethod", paymentResponse["mode"].ToString()));
                PaymentMethod paymentDetails = new PaymentMethod();
                List<StatusCode> statusCodeList = new List<StatusCode>();
                var orderData = BsonSerializer.Deserialize<OrderInfo>(MH.GetSingleObject(Builders<BsonDocument>.Filter.Eq("OrderId", paymentModel.OrderId), "OrderDB", "OrderInfo").Result);
                foreach (var detail in orderData.PaymentDetails.Status)
                {
                    statusCodeList.Add(detail);
                }
                statusCodeList.Add(new StatusCode { StatusId = 4, Description = "Payment cancelled", Date = DateTime.UtcNow });
                paymentDetails.Status = statusCodeList;
                var updatePaymentDetails = await MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("OrderId", paymentModel.OrderId), "OrderDB", "OrderInfo", Builders<BsonDocument>.Update.Set("PaymentDetails", paymentDetails));
                return Redirect(GlobalHelper.ReadXML().Elements("payu").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("redirectcancelled").First().Value);
            }
            else
            {
                return Redirect(GlobalHelper.ReadXML().Elements("payu").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("redirectcancelled").First().Value);
            }
        }

    }
}
