using EVBTradingContract.Request;
using EVBTradingContract.Response;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IListingService
    {
        Task<List<ListingResponse>> GetAllListingsAsync(ListingGetRequest request);
    }
}
