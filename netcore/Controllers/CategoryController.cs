using System;
using Arthur_Clive.Data;
using Arthur_Clive.DataAccess;
using Arthur_Clive.Logger;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using WH = Arthur_Clive.Helper.WebApiHelper;

namespace Arthur_Clive.Controllers
{
    [Route("api/[controller]")]
    public class CategoryController : Controller
    {
        public CategoryDataAccess categoryDataAccess;

        public CategoryController(CategoryDataAccess dataAccess)
        {
            categoryDataAccess = dataAccess;
        }

        [HttpGet]
        public JsonResult Get()
        {
            try
            {
                var product = categoryDataAccess.GetCategories();
                return Json(product);
            }
            catch (Exception ex)
            {
                WH.CreateLog("Category", "Get", "Get", ex.Message);
                return Json(new Category());
            }
        }

        //[HttpPost]
        //public string Post([FromBody]Category product)
        //{
        //    try
        //    {
        //        categoryDataAccess.CreateCategory(product);
        //        return "Success";
        //    }
        //    catch (Exception ex)
        //    {
        //        ApplicationLogger logger =
        //            new ApplicationLogger
        //            {
        //                Controller = "Category",
        //                MethodName = "Post",
        //                Method = "Post",
        //                Description = ex.ToString()
        //            };
        //        loggerDataAccess.CreateLog(logger);
        //        return "Failed";
        //    }
        //}

        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //} 
    }
}
