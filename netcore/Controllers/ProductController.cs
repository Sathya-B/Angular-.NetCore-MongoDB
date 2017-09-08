using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Minio;
using System;
using Microsoft.AspNetCore.Http;
using Arthur_Clive.Data;
using Arthur_Clive.DataAccess;
using WH = Arthur_Clive.Helper.WebApiHelper;
using Arthur_Clive.Logger;
using System.Threading.Tasks;

namespace Arthur_Clive.Controllers
{
    [Route("api/[controller]")]
    public class ProductController : Controller
    {
        public ProductDataAccess productDataAccess;

        public ProductController(ProductDataAccess data)
        {
            productDataAccess = data;
        }

        [HttpGet]
        public JsonResult Get()
        {
            try
            {
                var products = productDataAccess.GetProducts();
                return Json(products);
            }
            catch (Exception ex)
            {
                WH.CreateLog("Product", "Get", "Get",ex.Message);
                return Json(new Product());
            }
        }

        [HttpPost]
        public string Post([FromBody]Product product)
        {
            try
            {
                product.Product_Discount_Price = (product.Product_Price - (product.Product_Price * (product.Product_Discount / 100)));
                product.Product_SKU = product.Product_For + "-" + product.Product_Type + "-" + product.Product_Design + "-" + product.Product_Colour + "-" + product.Product_Size;
                var response = productDataAccess.Create(product);
                return response;
            }
            catch (Exception ex)
            {
                WH.CreateLog("Product", "Post", "Post", ex.Message);
                return "Failed";
            }
        }

        [HttpPost("{productSKU}")]
        public string Post(string productSKU, IFormFile file)
        {
            try
            {
                var stream = file.OpenReadStream();
                var name = file.FileName;
                string objectName = productSKU + ".jpg";
                WH.GetMinioClient().PutObjectAsync("student-maarklist", objectName, stream, file.Length);
                return "Success";
            }
            catch (Exception ex)
            {
                WH.CreateLog("Product", "Post", "Post file to minio", ex.Message);
                return "Failed";
            }
        }

        [HttpPut("{id:length(24)}")]
        public async Task<string> Put(string id, [FromBody]Product product)
        {
            try
            {
                product.Product_Discount_Price = (product.Product_Price - (product.Product_Price * (product.Product_Discount / 100)));
                var recId = new ObjectId(id);
                var response = await productDataAccess.Update(recId, product);
                return response;
            }
            catch (Exception ex)
            {
                WH.CreateLog("Product", "Put", "Put", ex.Message);
                return "Failed";
            }
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<string> Delete(string id)
        {
            try
            {
                var response = await productDataAccess.Remove(new ObjectId(id));
                return response;
            }
            catch (Exception ex)
            {
                ApplicationLogger logger =
                   new ApplicationLogger
                   {
                       Controller = "Product",
                       MethodName = "Delete",
                       Method = "Delete",
                       Description = ex.Message
                   };
                WH.CreateLog("Product", "Delete", "Delete", ex.Message);
                return "Failed";
            }
        }
    }
}
