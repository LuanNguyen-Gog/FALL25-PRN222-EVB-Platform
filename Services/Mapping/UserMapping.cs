using EVBTradingContract.Request;
using EVBTradingContract.Response;
using Mapster;
using Repositories.Models;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Mapping
{
    public class UserMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // FilterRequest → Entity
            config.NewConfig<UserFilterRequest, User>();

            // Entity → Response
            config.NewConfig<User, UserResponse>();

            // Request → Entity
            config.NewConfig<UserRequest, User>();
        }
    }
}
