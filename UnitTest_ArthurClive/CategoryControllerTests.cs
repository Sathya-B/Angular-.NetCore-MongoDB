using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Arthur_Clive.Data;
using Arthur_Clive.Helper;
using Arthur_Clive.Logger;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using AH = Arthur_Clive.Helper.AmazonHelper;
using WH = Arthur_Clive.Helper.MinioHelper;
using MH = Arthur_Clive.Helper.MongoHelper;
using MongoDB.Bson;
using Arthur_Clive;
using Arthur_Clive.Controllers;

namespace UnitTest_ArthurClive
{
    [TestClass]
    public class CategoryControllerTests
    {
        [TestMethod]
        public void Get()
        {
            CategoryController categoryController = new CategoryController();

            //var result = categoryController.Get() as  
        }
    }
}
