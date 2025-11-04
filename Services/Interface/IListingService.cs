using EVBTradingContract.Common;
using EVBTradingContract.Request;
using EVBTradingContract.Response;
using System;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IListingService
    {
        Task<List<ListingResponse>> GetAllListingsAsync(ListingGetRequest request);
        Task<ApiResponse<ListingResponse>> GetListingById(Guid listingId);
        Task<ApiResponse<ListingResponse>> CreateListing(ListingCreateRequest request);
        Task<ApiResponse<bool>> UpdateListing(Guid listingId, ListingUpdateRequest request);
        Task<ApiResponse<bool>> DeleteListing(Guid listingId);
    }
}