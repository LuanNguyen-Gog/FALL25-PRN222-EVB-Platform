using EVBTradingContract.Common;
using EVBTradingContract.Request;
using EVBTradingContract.Response;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using Repositories.Repository;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Implement
{
    public class ListingService : IListingService
    {
        private readonly ListingRepository _listingRepository;

        public ListingService(ListingRepository listingRepository)
        {
            _listingRepository = listingRepository;
        }

        public async Task<List<ListingResponse>> GetAllListingsAsync(ListingGetRequest request)
        {
            var query = _listingRepository.GetListing(request.Adapt<Listing>());
            var listings = await query.ToListAsync();
            return listings.Adapt<List<ListingResponse>>();
        }

        public async Task<ApiResponse<ListingResponse>> GetListingById(Guid listingId)
        {
            var listing = await _listingRepository.GetByIdAsync(listingId);
            if (listing == null)
            {
                return new ApiResponse<ListingResponse>
                {
                    Success = false,
                    Message = "Listing not found"
                };
            }

            return new ApiResponse<ListingResponse>
            {
                Success = true,
                Data = listing.Adapt<ListingResponse>()
            };
        }

        public async Task<ApiResponse<ListingResponse>> CreateListing(ListingCreateRequest request)
        {
            var listing = request.Adapt<Listing>();
            listing.Id = Guid.NewGuid();
            listing.CreatedAt = DateTime.UtcNow;
            listing.UpdatedAt = DateTime.UtcNow;

            await _listingRepository.CreateAsync(listing);

            return new ApiResponse<ListingResponse>
            {
                Success = true,
                Data = listing.Adapt<ListingResponse>()
            };
        }

        public async Task<ApiResponse<bool>> UpdateListing(Guid listingId, ListingUpdateRequest request)
        {
            var listing = await _listingRepository.GetByIdAsync(listingId);
            if (listing == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Listing not found"
                };
            }

            request.Adapt(listing);
            listing.UpdatedAt = DateTime.UtcNow;

            await _listingRepository.UpdateAsync(listing);

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true
            };
        }

        public async Task<ApiResponse<bool>> DeleteListing(Guid listingId)
        {
            var listing = await _listingRepository.GetByIdAsync(listingId);
            if (listing == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Listing not found"
                };
            }

            await _listingRepository.RemoveAsync(listing);

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true
            };
        }
    }
}