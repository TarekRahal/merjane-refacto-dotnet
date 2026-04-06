using System;

namespace Refacto.DotNet.Controllers.Services
{
    public interface IProductService
    {
        void HandleSeasonalProduct(Entities.Product p);
        void HandleExpiredProduct(Entities.Product p);
        void HandleNormalProduct(Entities.Product p);
    }
}
