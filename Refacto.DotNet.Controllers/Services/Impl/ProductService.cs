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

        public void HandleSeasonalProduct(Product p)
        {
            if (p.Available > 0)
            {
                if (DateTime.Now.Date > p.SeasonStartDate && DateTime.Now.Date < p.SeasonEndDate)
                {
                    p.Available -= 1;
                    _ = _ctx.SaveChanges();
                }
                else
                {
                    _ns.SendOutOfStockNotification(p.Name);
                }
            }
            else
            {
                if (DateTime.Now.AddDays(p.LeadTime) > p.SeasonEndDate)
                {
                    _ns.SendOutOfStockNotification(p.Name);
                    p.Available = 0;
                    _ = _ctx.SaveChanges();
                }
                else
                {
                    _ns.SendDelayNotification(p.LeadTime, p.Name);
                }
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

        public void HandleNormalProduct(Product p)
        {
            if (p.Available > 0)
            {
                p.Available -= 1;
                _ = _ctx.SaveChanges();
            }
            else
            {
                _ns.SendDelayNotification(p.LeadTime, p.Name);
            }
        }
    }
}
