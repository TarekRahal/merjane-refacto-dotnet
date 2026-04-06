using Refacto.DotNet.Controllers.Database.Context;

namespace Refacto.DotNet.Controllers.Services.Impl
{
    public class OrderService : IOrderService
    {
        private readonly IProductService _ps;

        public OrderService(AppDbContext ctx, IProductService ps)
        {
            _ps = ps;
        }
        public long ProcessOrder(Entities.Order order)
        {
            ICollection<Entities.Product>? products = order.Items;

            if (products == null)
            {
                throw new ArgumentException("Order must contain at least one product.");
            }


            foreach (Entities.Product p in products)
            {
                switch (p.Type)
                {
                    case Entities.Product.ProductType.NORMAL:
                        _ps.HandleNormalProduct(p);
                        break;
                    case Entities.Product.ProductType.SEASONAL:
                        _ps.HandleSeasonalProduct(p);
                        break;
                    case  Entities.Product.ProductType.EXPIRABLE:
                        _ps.HandleExpiredProduct(p);
                        break;
                }
            }
            return order.Id;
        }
    }
}
