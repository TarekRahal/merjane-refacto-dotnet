using System;

namespace Refacto.DotNet.Controllers.Services
{
    public interface IOrderService
    {
        long ProcessOrder(Entities.Order p);
    }
}
