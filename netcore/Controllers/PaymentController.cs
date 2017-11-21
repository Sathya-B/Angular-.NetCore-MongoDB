using Microsoft.AspNetCore.Mvc;
using Arthur_Clive.Helper;
using System;
using Arthur_Clive.Data;
using MH = Arthur_Clive.Helper.MongoHelper;
using PU = Arthur_Clive.Helper.PayUHelper;
using Arthur_Clive.Logger;
using System.Linq;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
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
        public IMongoDatabase product_db;
        /// <summary></summary>
        public IMongoCollection<BsonDocument> product_collection;
        /// <summary></summary>
        public IMongoCollection<BsonDocument> orderinfo_collection;
        /// <summary></summary>
        public IMongoDatabase logger_db;
        /// <summary></summary>
        public IMongoCollection<ApplicationLogger> serverlogCollection;

        /// <summary>
        /// 
        /// </summary>
        public PaymentController()
        {
            _client = MH.GetClient();
            product_db = _client.GetDatabase("productDB");
            product_collection = product_db.GetCollection<BsonDocument>("Product");
            userinfo_db = _client.GetDatabase("UserInfo");
            userinfo_collection = userinfo_db.GetCollection<BsonDocument>("UserInfo");
            cartCollection = userinfo_db.GetCollection<Cart>("Cart");
            order_db = _client.GetDatabase("OrderDB");
            orderinfo_collection = order_db.GetCollection<BsonDocument>("OrderInfo");
            logger_db = _client.GetDatabase("ArthurCliveLogDB");
            serverlogCollection = logger_db.GetCollection<ApplicationLogger>("ServerLog");
        }

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
                        var updatePaymentMethod = await MH.UpdateSingleObject(orderinfo_collection, "OrderId", paymentModel.OrderId, null, null, Builders<BsonDocument>.Update.Set("PaymentMethod", paymentResponse["mode"].ToString()));
                        var updatePaymentDetails = await MH.UpdatePaymentDetails(orderinfo_collection, paymentModel.OrderId);
                        var removeCartItems = MH.RemoveCartItems(userinfo_collection, cartCollection, product_collection, paymentModel.OrderId, paymentModel.UserName, paymentModel.Email);
                        var sendGift = GlobalHelper.SendGift(paymentModel.OrderId, orderinfo_collection);
                        var sendProductDetails = EmailHelper.SendEmail_ProductDetails(paymentModel.Email, paymentModel.OrderId, orderinfo_collection);
                        return Redirect(GlobalHelper.ReadXML().Elements("payu").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("redirectsuccess").First().Value);
                    }
                    else
                    {
                        var updatePaymentMethod = await MH.UpdateSingleObject(orderinfo_collection, "OrderId", paymentModel.OrderId, null, null, Builders<BsonDocument>.Update.Set("PaymentMethod", paymentResponse["mode"].ToString()));
                        PaymentMethod paymentDetails = new PaymentMethod();
                        List<StatusCode> statusCodeList = new List<StatusCode>();
                        var orderData = BsonSerializer.Deserialize<OrderInfo>(MH.GetSingleObject(orderinfo_collection, "OrderId", paymentModel.OrderId, null, null).Result);
                        foreach (var detail in orderData.PaymentDetails.Status)
                        {
                            statusCodeList.Add(detail);
                        }
                        statusCodeList.Add(new StatusCode { StatusId = 3, Description = "Payment Failed", Date = DateTime.UtcNow });
                        paymentDetails.Status = statusCodeList;
                        var updatePaymentDetails = await MH.UpdateSingleObject(orderinfo_collection, "OrderId", paymentModel.OrderId, null, null, Builders<BsonDocument>.Update.Set("PaymentDetails", paymentDetails));
                        return Redirect(GlobalHelper.ReadXML().Elements("payu").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("redirectfailure").First().Value);
                    }

                }
                catch (Exception ex)
                {
                    LoggerDataAccess.CreateLog("PaymentController", "PaymentSuccess", ex.Message, serverlogCollection);
                    var updatePaymentMethod = await MH.UpdateSingleObject(orderinfo_collection, "OrderId", paymentModel.OrderId, null, null, Builders<BsonDocument>.Update.Set("PaymentMethod", paymentResponse["mode"].ToString()));
                    PaymentMethod paymentDetails = new PaymentMethod();
                    List<StatusCode> statusCodeList = new List<StatusCode>();
                    var orderData = BsonSerializer.Deserialize<OrderInfo>(MH.GetSingleObject(orderinfo_collection, "OrderId", paymentModel.OrderId, null, null).Result);
                    foreach (var detail in orderData.PaymentDetails.Status)
                    {
                        statusCodeList.Add(detail);
                    }
                    statusCodeList.Add(new StatusCode { StatusId = 3, Description = "Payment Failed", Date = DateTime.UtcNow });
                    paymentDetails.Status = statusCodeList;
                    var updatePaymentDetails = await MH.UpdateSingleObject(orderinfo_collection, "OrderId", paymentModel.OrderId, null, null, Builders<BsonDocument>.Update.Set("PaymentDetails", paymentDetails));
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
                var updatePaymentMethod = await MH.UpdateSingleObject(orderinfo_collection, "OrderId", paymentModel.OrderId, null, null, Builders<BsonDocument>.Update.Set("PaymentMethod", paymentResponse["mode"].ToString()));
                PaymentMethod paymentDetails = new PaymentMethod();
                List<StatusCode> statusCodeList = new List<StatusCode>();
                var orderData = BsonSerializer.Deserialize<OrderInfo>(MH.GetSingleObject(orderinfo_collection, "OrderId", paymentModel.OrderId, null, null).Result);
                foreach (var detail in orderData.PaymentDetails.Status)
                {
                    statusCodeList.Add(detail);
                }
                statusCodeList.Add(new StatusCode { StatusId = 3, Description = "Payment Failed", Date = DateTime.UtcNow });
                paymentDetails.Status = statusCodeList;
                var updatePaymentDetails = await MH.UpdateSingleObject(orderinfo_collection, "OrderId", paymentModel.OrderId, null, null, Builders<BsonDocument>.Update.Set("PaymentDetails", paymentDetails));
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
                var updatePaymentMethod = await MH.UpdateSingleObject(orderinfo_collection, "OrderId", paymentModel.OrderId, null, null, Builders<BsonDocument>.Update.Set("PaymentMethod", paymentResponse["mode"].ToString()));
                PaymentMethod paymentDetails = new PaymentMethod();
                List<StatusCode> statusCodeList = new List<StatusCode>();
                var orderData = BsonSerializer.Deserialize<OrderInfo>(MH.GetSingleObject(orderinfo_collection, "OrderId", paymentModel.OrderId, null, null).Result);
                foreach (var detail in orderData.PaymentDetails.Status)
                {
                    statusCodeList.Add(detail);
                }
                statusCodeList.Add(new StatusCode { StatusId = 4, Description = "Payment Cancelled", Date = DateTime.UtcNow });
                paymentDetails.Status = statusCodeList;
                var updatePaymentDetails = await MH.UpdateSingleObject(orderinfo_collection, "OrderId", paymentModel.OrderId, null, null, Builders<BsonDocument>.Update.Set("PaymentDetails", paymentDetails));
                return Redirect(GlobalHelper.ReadXML().Elements("payu").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("redirectcancelled").First().Value);
            }
            else
            {
                return Redirect(GlobalHelper.ReadXML().Elements("payu").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("redirectcancelled").First().Value);
            }
        }

    }
}