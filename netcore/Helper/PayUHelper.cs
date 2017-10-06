using System;
using System.Collections;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace Arthur_Clive.Helper
{
    public class PaymentModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProductInfo { get; set; }
        public string Amount { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }
    }

    public class PayUHelper
    {
        public static string Generatehash512(string text)
        {
            byte[] message = Encoding.UTF8.GetBytes(text);
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] hashValue;
            SHA512Managed hashString = new SHA512Managed();
            string hex = "";
            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }
            return hex;
        }

        public static StringBuilder PreparePOSTForm(string url, Hashtable data)
        {
            string formID = "PostForm";
            StringBuilder strForm = new StringBuilder();
            strForm.Append("<form id=\"" + formID + "\" name=\"" + formID + "\" action=\"" + url + "\" method=\"POST\">");
            foreach (DictionaryEntry key in data)
            {
                strForm.Append("<input type=\"hidden\" name=\"" + key.Key + "\" value=\"" + key.Value + "\">");
            }
            strForm.Append("</form>");
            StringBuilder strScript = new StringBuilder();
            strScript.Append("<script language='javascript'>");
            strScript.Append("var v" + formID + " = document." + formID + ";");
            strScript.Append("v" + formID + ".submit();");
            strScript.Append("</script>");
            strForm.Append(strScript);
            return strForm;
        }

        public static string GetTxnId()
        {
            Random random = new Random();
            string strHash = Generatehash512(random.ToString() + DateTime.Now);
            string txnId = strHash.ToString().Substring(0, 20);
            return txnId;
        }
        
        public static string GetHashString(string txnId,PaymentModel model)
        {
            string hashString = "";
            string[] hashSequence = ("key|txnid|amount|productinfo|firstname|email|udf1|udf2|udf3|udf4|udf5|udf6|udf7|udf8|udf9|udf10").Split('|');
            foreach (string hash_var in hashSequence)
            {
                if (hash_var == "key")
                {
                    hashString = hashString + GlobalHelper.ReadXML().Elements("payu").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("key").First().Value;
                    hashString = hashString + '|';
                }
                else if (hash_var == "txnid")
                {
                    hashString = hashString + txnId;
                    hashString = hashString + '|';
                }
                else if (hash_var == "amount")
                {
                    hashString = hashString + Convert.ToDecimal(model.Amount).ToString("g29");
                    hashString = hashString + '|';
                }
                else if (hash_var == "productinfo")
                {
                    hashString = hashString + model.ProductInfo;
                    hashString = hashString + '|';
                }
                else if (hash_var == "firstname")
                {
                    hashString = hashString + model.FirstName;
                    hashString = hashString + '|';
                }
                else if (hash_var == "email")
                {
                    hashString = hashString + model.Email;
                    hashString = hashString + '|';
                }
                else
                {
                    hashString = hashString + "";
                    hashString = hashString + '|';
                }
            }
            hashString += GlobalHelper.ReadXML().Elements("payu").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("saltkey").First().Value;
            return hashString;
        }
    }
}
