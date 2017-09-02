using System;
using Arthur_Clive.Data;
using Arthur_Clive.DataAccess;
using Arthur_Clive.Logger;
using Microsoft.AspNetCore.Mvc;
using Minio;
using WH = Arthur_Clive.Helper.WebApiHelper;

namespace Arthur_Clive.Controllers
{
    [Route("api/[controller]")]
    public class SubCategoryController : Controller
    {
        ProductDataAccess productDataAccess;
        public MinioClient minio = WH.GetMinioClient();

        public SubCategoryController(ProductDataAccess dataAccess)
        {
            productDataAccess = dataAccess;
        }

        [HttpGet("{productFor}/{productType}")]
        public JsonResult Get(string productFor, string productType)
        {
            try
            {
                var product = productDataAccess.GetProductsForSubCategoryAsync(productFor, productType);
                if (product == null)
                {
                    return Json(new Product());
                }
                else
                {
                    return Json(product);
                }
            }
            catch(Exception ex)
            {
                ApplicationLogger logger =
                    new ApplicationLogger
                    {
                        Controller = "SubCategory",
                        MethodName = "Get",
                        Method = "Get with product_For and product_Type",
                        Description = ex.Message
                    };
                productDataAccess.CreateLog(logger);
                return Json(new Product());
            }
        }

        [HttpGet("{productFor}/{productType}/{productDesign}")]
        public JsonResult Get(string productFor, string productType,string productDesign)
        {
            try
            {
                var product = productDataAccess.GetProductsForSubDivisionByDesignAsync(productFor, productType,productDesign);
                if (product == null)
                {
                    return Json(new Product());
                }
                else
                {
                    return Json(product);
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger logger =
                    new ApplicationLogger
                    {
                        Controller = "SubCategory",
                        MethodName = "Get",
                        Method = "Get with product_For and product_Type",
                        Description = ex.Message
                    };
                productDataAccess.CreateLog(logger);
                return Json(new Product());
            }
        }

        [HttpGet("{productFor}/{productType}/{productDesign}/{productColour}")]
        public JsonResult Get(string productFor, string productType, string productDesign,string productColour)
        {
            try
            {
                var product = productDataAccess.GetProductsForSubDivisionByColourAsync(productFor, productType, productDesign,productColour);
                if (product == null)
                {
                    return Json(new Product());
                }
                else
                {
                    return Json(product);
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger logger =
                    new ApplicationLogger
                    {
                        Controller = "SubCategory",
                        MethodName = "Get",
                        Method = "Get with product_For and product_Type",
                        Description = ex.Message
                    };
                productDataAccess.CreateLog(logger);
                return Json(new Product());
            }
        }

        [HttpGet("{productFor}/{productType}/{productDesign}/{productColour}/{productSize}")]
        public JsonResult Get(string productFor, string productType, string productDesign, string productColour,string productSize)
        {
            try
            {
                var product = productDataAccess.GetProductsForSubDivisionBySizeAsync(productFor, productType, productDesign, productColour,productSize);
                if (product == null)
                {
                    return Json(new Product());
                }
                else
                {
                    return Json(product);
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger logger =
                    new ApplicationLogger
                    {
                        Controller = "SubCategory",
                        MethodName = "Get",
                        Method = "Get with product_For and product_Type",
                        Description = ex.Message
                    };
                productDataAccess.CreateLog(logger);
                return Json(new Product());
            }
        }
    }
}