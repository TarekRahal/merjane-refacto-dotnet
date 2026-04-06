using System;

namespace Refacto.DotNet.Controllers.Services
{
    public interface IProductService
    {
        void NotifyDelay(int leadTime, Entities.Product p);
        void HandleSeasonalProduct(Entities.Product p);
        void HandleExpiredProduct(Entities.Product p);
    }
}
