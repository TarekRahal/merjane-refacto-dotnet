using Refacto.DotNet.Controllers.Database.Context;
using Refacto.DotNet.Controllers.Entities;

namespace Refacto.DotNet.Controllers.Services.Impl
{
    public class ProductService : IProductService
    {
        private readonly INotificationService _ns;
        private readonly AppDbContext _ctx;

        public ProductService(INotificationService ns, AppDbContext ctx)
        {
            _ns = ns;
            _ctx = ctx;
        }

        public void NotifyDelay(int leadTime, Product p)
        {
            p.LeadTime = leadTime;
            _ = _ctx.SaveChanges();
            _ns.SendDelayNotification(leadTime, p.Name);
        }

        public void HandleSeasonalProduct(Product p)
        {
            if (DateTime.Now.AddDays(p.LeadTime) > p.SeasonEndDate)
            {
                _ns.SendOutOfStockNotification(p.Name);
                p.Available = 0;
                _ = _ctx.SaveChanges();
            }
            else if (p.SeasonStartDate > DateTime.Now)
            {
                _ns.SendOutOfStockNotification(p.Name);
                _ = _ctx.SaveChanges();
            }
            else
            {
                NotifyDelay(p.LeadTime, p);
            }
        }

        public void HandleExpiredProduct(Product p)
        {
            if (p.Available > 0 && p.ExpiryDate > DateTime.Now)
            {
                p.Available -= 1;
                _ = _ctx.SaveChanges();
            }
            else
            {
                _ns.SendExpirationNotification(p.Name, (DateTime)p.ExpiryDate);
                p.Available = 0;
                _ = _ctx.SaveChanges();
            }
        }
    }
}
