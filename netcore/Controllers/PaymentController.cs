using Microsoft.AspNetCore.Mvc;
using Arthur_Clive.Helper;
using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections;
using System.Net;

namespace Arthur_Clive.Controllers
{
    [Route("api/[controller]")]
    public class PaymentController : Controller
    {
        [HttpPost]
        public ActionResult Payment([FromBody]PayentModel model)
        {
            try
            {
                Random random = new Random();
                string strHash = PayUHelper.GenerateHash(random.ToString() + DateTime.Now);
                string txnId = strHash.ToString().Substring(0, 20);
                string[] hashSequence = ("key|txnid|amount|productinfo|firstname|email|udf1|udf2|udf3|udf4|udf5|udf6|udf7|udf8|udf9|udf10").Split('|'); 
                string hash_string = "";
                foreach (string hash_var in hashSequence)
                {
                    if (hash_var == "key")
                    {
                        hash_string = hash_string + GlobalHelper.ReadXML().Elements("payu").Where(x => x.Element("current").Value.Equals("Test")).Descendants("merchantkey").First().Value;
                        hash_string = hash_string + '|';
                    }
                    else if (hash_var == "txnid")
                    {
                        hash_string = hash_string + txnId;
                        hash_string = hash_string + '|';
                    }
                    else if (hash_var == "amount")
                    {
                        hash_string = hash_string + model.Amount;
                        hash_string = hash_string + '|';
                    }
                    else if(hash_var == "productinfo")
                    {
                        hash_string = hash_string + model.ProductInfo;
                        hash_string = hash_string + '|';
                    }
                    else if (hash_var == "firstname")
                    {
                        hash_string = hash_string + model.ProductInfo;
                        hash_string = hash_string + '|';
                    }
                    else if (hash_var == "email")
                    {
                        hash_string = hash_string + model.Email;
                        hash_string = hash_string + '|';
                    }
                    else
                    {
                        hash_string = hash_string + "";
                        hash_string = hash_string + '|';
                    }
                }
                hash_string += GlobalHelper.ReadXML().Elements("payu").Where(x => x.Element("current").Value.Equals("Test")).Descendants("saltkey").First().Value;
                string hash = PayUHelper.GenerateHash(hash_string).ToLower();
                string action = GlobalHelper.ReadXML().Elements("payu").Where(x => x.Element("current").Value.Equals("Test")).Descendants("url").First().Value + "/_payment";
                Hashtable data = new Hashtable(); 
                data.Add("hash", hash);
                data.Add("txnid", txnId);
                data.Add("key", GlobalHelper.ReadXML().Elements("payu").Where(x => x.Element("current").Value.Equals("Test")).Descendants("merchantkey").First().Value);
                data.Add("amount", model.Amount);
                data.Add("firstname", model.FullName);
                data.Add("email", model.Email);
                data.Add("phone", model.PhoneNumber);
                data.Add("productinfo", model.ProductInfo);
                data.Add("surl", model.SuccessURL);
                data.Add("furl", model.FailureURL);
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
                data.Add("service_provider", "");
                var form = PayUHelper.PreparePOSTForm(action, data);
                return Ok(form);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
