using Refacto.DotNet.Controllers.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Refacto.DotNet.Controllers.Services.Impl
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _ctx;
        private readonly IProductService _ps;

        public OrderService(AppDbContext ctx, IProductService ps)
        {
            _ctx = ctx;
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
                        if (p.Available > 0)
                        {
                            p.Available -= 1;
                            _ctx.Entry(p).State = EntityState.Modified;
                            _ = _ctx.SaveChanges();

                        }
                        else
                        {
                            int leadTime = p.LeadTime;
                            if (leadTime > 0)
                            {
                                _ps.NotifyDelay(leadTime, p);
                            }
                        }
                        break;
                    case Entities.Product.ProductType.SEASONAL:
                        if (DateTime.Now.Date > p.SeasonStartDate && DateTime.Now.Date < p.SeasonEndDate && p.Available > 0)
                        {
                            p.Available -= 1;
                            _ = _ctx.SaveChanges();
                        }
                        else
                        {
                            _ps.HandleSeasonalProduct(p);
                        }
                        break;
                    case  Entities.Product.ProductType.EXPIRABLE:
                        if (p.Available > 0 && p.ExpiryDate > DateTime.Now.Date)
                        {
                            p.Available -= 1;
                            _ = _ctx.SaveChanges();
                        }
                        else
                        {
                            _ps.HandleExpiredProduct(p);
                        }
                        break;
                }
            }
            return order.Id;
        }
    }
}
