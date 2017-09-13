using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Arthur_Clive.Data;
using Arthur_Clive.Helper;
using Arthur_Clive.Logger;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using AH = Arthur_Clive.Helper.AmazonHelper;
using WH = Arthur_Clive.Helper.MinioHelper;
using MH = Arthur_Clive.Helper.MongoHelper;
using MongoDB.Bson;
using Arthur_Clive.Controllers;
using System.Collections.Generic;

namespace UnitTest_ArthurClive
{
    [TestClass]
    public class ControllerUnitTest
    {
        [TestMethod]
        public void Get()
        {
            //Arrange
            CategoryController controller = new CategoryController();
            //Act
            var result = controller.Get() as Task<ActionResult>;
            var viewResult = result.Result;
            //Assert
            Assert.IsNotNull(viewResult);
        }
    }
}
