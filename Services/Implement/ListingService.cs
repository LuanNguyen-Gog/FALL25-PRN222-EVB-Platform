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
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Services.Implement
{
    public class ListingService : IListingService
    {
        private readonly ListingRepository _listingRepository;
        public ListingService(ListingRepository listingRepository)
        {
            _listingRepository = listingRepository;
        }
        public async Task<List<ListingResponse>> GetAllListingsAsync(ListingGetRequest request/*, int maxItems = 1000*/)
        {
            var entity = request.Adapt<Listing>();
            var query = _listingRepository.GetListing(entity);
            var items = await query
                //.Take(maxItems) // chặn quá tải
        .ProjectToType<ListingResponse>() // Mapster: select đúng cột cần ở trong ListingResponse
        .ToListAsync();
            return items;
        }
    }
}
