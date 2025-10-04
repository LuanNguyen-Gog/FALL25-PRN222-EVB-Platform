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
    public class BatteryMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // Request → Entity
            config.NewConfig<BatteryFilterRequest, Battery>();
            // Entity → Response
            config.NewConfig<Battery, BatteryResponse>();
        }
    }
}
