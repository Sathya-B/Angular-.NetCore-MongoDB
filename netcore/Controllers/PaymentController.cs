using Microsoft.AspNetCore.Mvc;
using Arthur_Clive.Helper;
using System;
using System.Collections;
using System.Text;
using Arthur_Clive.Data;
using PU = Arthur_Clive.Helper.PayUHelper;
using Arthur_Clive.Logger;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;

namespace Arthur_Clive.Controllers
{
    [Route("api/[controller]")]
    public class PaymentController : Controller
    {
        [HttpPost]
        public ActionResult MakePayment([FromBody]PaymentModel model)
        {
            try
            {
                string SuccessUrl = "http://localhost:5001/api/payment/success";
                string FailureUrl = "http://localhost:5001/api/payment/failed";
                string txnId = PU.GetTxnId();
                string hashString = PU.GetHashString(txnId,model);
                string hash = PU.Generatehash512(hashString).ToLower();
                string action = GlobalHelper.ReadXML().Elements("payu").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("url").First().Value + "/_payment";
                Hashtable data = new Hashtable();
                data.Add("hash", hash);
                data.Add("txnid", txnId);
                data.Add("key", "gtKFFx");
                string AmountForm = Convert.ToDecimal(model.Amount).ToString("g29");
                data.Add("amount", AmountForm);
                data.Add("firstname", model.FirstName);
                data.Add("email", model.Email);
                data.Add("phone", model.PhoneNumber);
                data.Add("productinfo", model.ProductInfo);
                data.Add("surl", SuccessUrl);
                data.Add("furl", FailureUrl);
                data.Add("lastname", model.LastName);
                data.Add("curl", "");
                data.Add("address1", model.AddressLine1);
                data.Add("address2", model.AddressLine2);
                data.Add("city", model.City);
                data.Add("state", model.State);
                data.Add("country", model.Country);
                data.Add("zipcode", model.ZipCode);
                data.Add("udf1", "");
                data.Add("udf2", "");
                data.Add("udf3", "");
                data.Add("udf4", "");
                data.Add("udf5", "");
                data.Add("pg", "");
                data.Add("service_provider", "PayUMoney");
                StringBuilder strForm = PU.PreparePOSTForm(action, data);
                var form = PU.PreparePOSTForm(action, data);
                return Ok(new ResponseData
                {
                    Code = "200",
                    Form = form,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("PaymentController", "MakePayment", "MakePayment", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = null
                });
            }
        }

        [HttpPost("{responce}")]
        public ActionResult Return([FromBody]FormCollection data,string responce)
        {
            try
            {
                //string[] hashSequence = PU.SplitHashSequence();
                return Ok(new ResponseData
                {
                    Code = "200",
                    Form = null,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("PaymentController", "Return", "Return", ex.Message);
                return BadRequest(new ResponseData
                {
                    Code = "400",
                    Message = "Failed",
                    Data = null
                });
            }
        }
    }
}
