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
    public class ContractMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<ContractRequest, Contract>();
            config.NewConfig<Contract, ContractResponse>();
        }
    }
}
