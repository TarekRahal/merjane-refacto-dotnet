using Moq;
using Refacto.DotNet.Controllers.Database.Context;
using Refacto.DotNet.Controllers.Entities;
using Refacto.DotNet.Controllers.Services;
using Refacto.DotNet.Controllers.Services.Impl;

namespace Refacto.Dotnet.Controllers.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<AppDbContext> _mockDbContext;

        public ProductServiceTests()
        {
            _mockNotificationService = new Mock<INotificationService>();
            _mockDbContext = new Mock<AppDbContext>();
        }

        [Fact]
        public void HandleSeasonalProduct_ShouldDecreaseAvailability_WhenProductIsInSeasonAndAvailable()
        {
            ProductService productService = new(_mockNotificationService.Object, _mockDbContext.Object);
            Product product = new()
            {
                Name = "Seasonal Product",
                LeadTime = 5,
                Available = 10,
                SeasonStartDate = DateTime.Now.AddDays(-1),
                SeasonEndDate = DateTime.Now.AddDays(10)
            };
            productService.HandleSeasonalProduct(product);
            Assert.Equal(9, product.Available);
        }

        [Fact]
        public void HandleSeasonalProduct_ShouldNotifyDelay_WhenProductIsOutOfStockAndDelayIsInSeason()
        {
            ProductService productService = new(_mockNotificationService.Object, _mockDbContext.Object);
            Product product = new()
            {
                Name = "Seasonal Product",
                LeadTime = 5,
                Available = 0,
                SeasonStartDate = DateTime.Now.AddDays(-1),
                SeasonEndDate = DateTime.Now.AddDays(10)
            };
            productService.HandleSeasonalProduct(product);
            _mockNotificationService.Verify(service => service.SendDelayNotification(product.LeadTime, product.Name), Times.Once());
        }

        [Fact]
        public void HandleSeasonalProduct_ShouldNotifyOutOfStock_WhenProductUnavailableAndDelayIsOutOfSeason()
        {
            ProductService productService = new(_mockNotificationService.Object, _mockDbContext.Object);
            Product product = new()
            {
                Name = "Seasonal Product",
                LeadTime = 5,
                Available = 0,
                SeasonStartDate = DateTime.Now.AddDays(-20),
                SeasonEndDate = DateTime.Now.AddDays(2)
            };
            productService.HandleSeasonalProduct(product);
            Assert.Equal(0, product.Available);
            _mockDbContext.Verify(ctx => ctx.SaveChanges(), Times.Once());
            _mockNotificationService.Verify(service => service.SendOutOfStockNotification(product.Name), Times.Once());
        }

        [Fact]
        public void HandleSeasonalProduct_ShouldNotifyOutOfStock_WhenProductIsNotInSeasonYet()
        {
            ProductService productService = new(_mockNotificationService.Object, _mockDbContext.Object);
            Product product = new()
            {
                Name = "Seasonal Product",
                LeadTime = 5,
                Available = 10,
                SeasonStartDate = DateTime.Now.AddDays(10),
                SeasonEndDate = DateTime.Now.AddDays(20)
            };
            productService.HandleSeasonalProduct(product);
            Assert.Equal(10, product.Available);
            _mockDbContext.Verify(ctx => ctx.SaveChanges(), Times.Never());
            _mockNotificationService.Verify(service => service.SendOutOfStockNotification(product.Name), Times.Once());
        }

        [Fact]
        public void HandleSeasonalProduct_ShouldNotifyOutOfStock_WhenProductIsNoLongerInSeason()
        {
            ProductService productService = new(_mockNotificationService.Object, _mockDbContext.Object);
            Product product = new()
            {
                Name = "Seasonal Product",
                LeadTime = 5,
                Available = 10,
                SeasonStartDate = DateTime.Now.AddDays(-20),
                SeasonEndDate = DateTime.Now.AddDays(-1)
            };
            productService.HandleSeasonalProduct(product);
            Assert.Equal(10, product.Available);
            _mockDbContext.Verify(ctx => ctx.SaveChanges(), Times.Never());
            _mockNotificationService.Verify(service => service.SendOutOfStockNotification(product.Name), Times.Once());
        }

        [Fact]
        public void HandleExpiredProduct_ShouldDecreaseAvailability_WhenProductIsNotExpiredAndAvailable()
        {
            ProductService productService = new(_mockNotificationService.Object, _mockDbContext.Object);
            Product product = new()
            {
                Name = "Expirable Product",
                LeadTime = 5,
                Available = 10,
                ExpiryDate = DateTime.Now.AddDays(10)
            };
            _mockDbContext.Verify(ctx => ctx.SaveChanges(), Times.Never());
            productService.HandleExpiredProduct(product);
            Assert.Equal(9, product.Available);
        }

        [Fact]
        public void HandleExpiredProduct_ShouldNotifyExpiration_WhenProductIsExpired()
        {
            ProductService productService = new(_mockNotificationService.Object, _mockDbContext.Object);
            Product product = new()
            {
                Name = "Expirable Product",
                LeadTime = 5,
                Available = 10,
                ExpiryDate = DateTime.Now.AddDays(-1)
            };
            productService.HandleExpiredProduct(product);
            Assert.Equal(0, product.Available);
            _mockDbContext.Verify(ctx => ctx.SaveChanges(), Times.Once());
            _mockNotificationService.Verify(service => service.SendExpirationNotification(product.Name, (DateTime)product.ExpiryDate), Times.Once());
        }

        [Fact]
        public void HandleNormalProduct_ShouldDecreaseAvailability_WhenProductIsAvailable()
        {
            ProductService productService = new(_mockNotificationService.Object, _mockDbContext.Object);
            Product product = new()
            {
                Name = "Normal Product",
                LeadTime = 5,
                Available = 10
            };
            productService.HandleNormalProduct(product);
            Assert.Equal(9, product.Available);
            _mockDbContext.Verify(ctx => ctx.SaveChanges(), Times.Once());
        }

        [Fact]
        public void HandleNormalProduct_ShouldNotifyDelay_WhenProductIsNotAvailable()
        {
            ProductService productService = new(_mockNotificationService.Object, _mockDbContext.Object);
            Product product = new()
            {
                Name = "Normal Product",
                LeadTime = 5,
                Available = 0
            };
            productService.HandleNormalProduct(product);
            _mockDbContext.Verify(ctx => ctx.SaveChanges(), Times.Never());
            _mockNotificationService.Verify(service => service.SendDelayNotification(product.LeadTime, product.Name), Times.Once());
        }
    }
}
