using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;

namespace UnitTest_ArthurClive.Controller
{
    public class ActionResultModel
    {
        public dynamic _t { get; set; }
        public ResponceData Value { get; set; }
        public dynamic Formatters { get; set; }
        public dynamic ContentTypes { get; set; }
        public dynamic DeclaredType { get; set; }
        public int StatusCode { get; set; }
    }

    public class ResponceData
    {
        public dynamic _t { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public dynamic Data { get; set; }
        public dynamic Content { get; set; }
    }

    public class ActionResultModel_CategoryList
    {
        public dynamic _t { get; set; }
        public ResponceData_CategoryList Value { get; set; }
        public dynamic Formatters { get; set; }
        public dynamic ContentTypes { get; set; }
        public dynamic DeclaredType { get; set; }
        public int StatusCode { get; set; }
    }

    public class ResponceData_CategoryList
    {
        public dynamic _t { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public CategoryData Data { get; set; }
        public dynamic Content { get; set; }
    }

    public class CategoryData
    {
        public dynamic _t { get; set; }
        public List<Category> _v { get; set; }
    }

    public class Category
    {
        public dynamic _t { get; set; }
        public dynamic _id { get; set; }
        public string ProductFor { get; set; }
        public string ProductType { get; set; }
        public string MinioObject_URL { get; set; }
        public string Description { get; set; }
    }
}

