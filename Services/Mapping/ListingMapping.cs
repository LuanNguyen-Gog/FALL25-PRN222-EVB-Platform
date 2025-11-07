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
    public class ListingMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // Request → Entity
            config.NewConfig<ListingGetRequest, Listing>()
                .IgnoreNonMapped(true);

            // Entity → Response
            config.NewConfig<Listing, ListingResponse>()
                .Map(dest => dest.ListingId, src => src.Id)
                .Map(dest => dest.Status, src => src.Status.ToString()) // nếu Status là enum
                .IgnoreNonMapped(true)
                .Compile();
        }
    }
}
