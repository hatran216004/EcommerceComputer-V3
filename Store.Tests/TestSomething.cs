using Microsoft.VisualStudio.TestTools.UnitTesting;
using Store_EF.Models;
using Store_EF.Models.Extensions;
using System;

namespace Store.Tests
{
    [TestClass]
    public class TestSomething
    {
        [TestMethod]
        public void FormattedPrice()
        {
            Product product = new Product()
            {
                ProductId = 1,
                Title = "Laptop",
                Price = 100000,
                Stock = 1,
                Description = "Test",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            Console.WriteLine(product.FormattedPrice());
            Console.WriteLine(product.FormattedPrice(quantity: 5));
        }
    }
}
