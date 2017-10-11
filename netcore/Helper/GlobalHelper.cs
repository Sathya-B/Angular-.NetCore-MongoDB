using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Arthur_Clive.Data;
using MongoDB.Driver;

namespace Arthur_Clive.Helper
{
    /// <summary>Global helper method</summary>
    public class GlobalHelper
    {
        /// <summary>xml file</summary>
        public static XElement ReadXML()
        {
            var dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var xmlStr = File.ReadAllText(Path.Combine(dir, "AmazonKeys.xml"));
            return XElement.Parse(xmlStr);
        }

        /// <summary>Get order list from MongoDB</summary>
        /// <param name="username"></param>
        /// <param name="order_db"></param>
        public async static Task<List<OrderInfo>> GetOrders(string username, IMongoDatabase order_db)
        {
            IAsyncCursor<OrderInfo> cursor = await order_db.GetCollection<OrderInfo>("OrderInfo").FindAsync(Builders<OrderInfo>.Filter.Eq("UserName", username));
            var orders = cursor.ToList();
            return orders;            
        }

        /// <summary>Get product list  from MongoDB</summary>
        /// <param name="productSKU"></param>
        /// <param name="product_db"></param>
        public async static Task<List<Product>> GetProducts(string productSKU, IMongoDatabase product_db)
        {
            IAsyncCursor<Product> productCursor = await product_db.GetCollection<Product>("Product").FindAsync(Builders<Product>.Filter.Eq("ProductSKU", productSKU));
            var products = productCursor.ToList();
            return products;
        }
    }
}
