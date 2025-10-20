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
        Task<ListingResponse> GetListingByIdAsync(long listingId);
        Task<ListingResponse> CreateListingAsync(ListingGetRequest request);
        Task<bool> UpdateListingAsync(long listingId, ListingGetRequest request);
        Task<bool> DeleteListingAsync(long listingId);
    }
}
