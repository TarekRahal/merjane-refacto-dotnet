using System;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Refacto.DotNet.Controllers.Database.Context;
using Refacto.DotNet.Controllers.Entities;
using Refacto.DotNet.Controllers.Services;
using Refacto.DotNet.Controllers.Services.Impl;

namespace Refacto.Dotnet.Controllers.Tests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<AppDbContext> _mockDbContext;
        private readonly Mock<IProductService> _productService;

        public OrderServiceTests()
        {
            _mockDbContext = new Mock<AppDbContext>();
            _ = _mockDbContext.Setup(x => x.Products).ReturnsDbSet(Array.Empty<Product>());
            _productService = new Mock<IProductService>();
        }

        [Fact]
        public void ProcessOrder_ShouldReturnOrderId_WhenOrderIsProcessed()
        {
            // GIVEN
            OrderService orderService = new(_mockDbContext.Object, _productService.Object);
            Order order = new()
            {
                Id = 1,
                Items = new List<Product>
                {
                    new Product
                    {
                        Id = 1,
                        Name = "Product 1",
                        Type = Product.ProductType.NORMAL,
                        Available = 10,
                        LeadTime = 5,
                        ExpiryDate = DateTime.Now.AddDays(10),
                        SeasonStartDate = DateTime.Now.AddDays(-5),
                        SeasonEndDate = DateTime.Now.AddDays(5)
                    }
                }
            };

            // WHEN
            long result = orderService.ProcessOrder(order);

            // THEN
            Assert.Equal(order.Id, result);
        }
    }
}
