using Arthur_Clive.Data;
using Swashbuckle.AspNetCore.Examples;

namespace Arthur_Clive.Swagger
{
    #region ProductController
    /// <summary></summary>
    public class InsertProduct : IExamplesProvider
    {
        /// <summary></summary>
        public object GetExamples()
        {
            return new Product
            {
                ProductFor = "Girls",
                ProductType = "Tshirt",
                ProductDesign = "LionFace",
                ProductBrand = "Arthur Clive",
                ProductPrice = 395.0,
                ProductDiscount = 0.0,
                ProductStock = 10,
                ProductSize = "9_10Y",
                ProductColour = "Black",
                ReplacementApplicable = true,
                ProductDescription = "“Om” signifies divine knowledge, eternal peace and spirituality. An absolute fashion icon on its own, the print makes it a versatile wear. Team up with trendy jackets and denims, certainly brings about a smart street-style sensibility.",
                RefundApplicable = true,
                ProductMaterial = "Cotton"
            };
        }
    }

    /// <summary></summary>
    public class UpdateProduct : IExamplesProvider
    {
        /// <summary></summary>
        public object GetExamples()
        {
            return new Product
            {
                ProductFor = "Men",
                ProductType = "Shirt",
                ProductDesign = "Om",
                ProductBrand = "Arthur Clive",
                ProductPrice = 695.0,
                ProductDiscount = 10.0,
                ProductStock = 15,
                ProductSize = "11_12Y",
                ProductColour = "White",
                ReplacementApplicable = false,
                ProductDescription = "Updated description",
                RefundApplicable = false,
                ProductMaterial = "Flax",
                ProductRating = 4.5,
                ProductReviews = new Review[] { new Review { Name = "Sample1", Comment = "Good" }, new Review { Name = "Sample2", Comment = "Bad" } }
            };
        }
    }

    #endregion

    #region CategoryController

    /// <summary></summary>
    public class InsertCategory : IExamplesProvider
    {
        /// <summary></summary>
        public object GetExamples()
        {
            return new Category
            {
                ProductFor = "Women",
                ProductType = "Tops",
                Description = "Tops for women"
            };
        }
    }

    /// <summary></summary>
    public class UpdateCategory : IExamplesProvider
    {
        /// <summary></summary>
        public object GetExamples()
        {
            return new Category
            {
                ProductFor = "Men",
                ProductType = "Shirt",
                Description = "Shirts for men"
            };
        }
    }

    #endregion

    #region UserInfoController

    /// <summary></summary>
    public class AddressDetail : IExamplesProvider
    {
        /// <summary></summary>
        public object GetExamples()
        {
            return new UserInfoList()
            {
                ListOfAddress = {
                    new Address
                    {
                    UserName = "sample@gmail.com",
                    Name = "Sample",
                    PhoneNumber = "12341234",
                    AddressLines = "GSK street",
                    PostOffice = "Saravanampati",
                    City = "Coimbatore",
                    State = "TamilNadu",
                    PinCode = "641035",
                    Landmark = "Near KGISL",
                    BillingAddress = true,
                    ShippingAddress = false,
                    DefaultAddress = true
                },
                    new Address
                    {
                        UserName = "sample@gmail.com",
                Name = "Sample",
                PhoneNumber = "12341234",
                AddressLines = "GSK street",
                PostOffice = "Saravanampati",
                City = "Coimbatore",
                State = "TamilNadu",
                PinCode = "641035",
                Landmark = "Near KGISL",
                BillingAddress = false,
                ShippingAddress = true,
                DefaultAddress = true
                    }
                }
            };
        }
    }

    #endregion
}
