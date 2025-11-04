using EVBTradingContract.Request;
using EVBTradingContract.Response;
using Mapster;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Mapping
{
    public class OrderMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // Add your mapping configurations here
            // Request → Entity
            config.NewConfig<OrderCreateRequest, Order>();
            // Entity → Response
            config.NewConfig<Order, OrderResponse>();

            config.NewConfig<OrderUpdateStatusRequest, Order>();
        }
    }
}
