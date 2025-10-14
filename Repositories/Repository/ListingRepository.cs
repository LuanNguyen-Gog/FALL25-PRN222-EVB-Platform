using Repositories.Basic;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repository
{
    public class ListingRepository : GenericRepo<Listing>
    {
        public ListingRepository(DBContext.EVBatteryTradingContext context) : base(context)
        {
        }
        public IQueryable<Listing> GetListing(Listing listing)
        {
            var query = _context.Listings.AsQueryable();
            if (listing.ListingId > 0)
                query = query.Where(b => b.ListingId == listing.ListingId);
            if (listing.SellerId > 0)
                query = query.Where(b => b.SellerId == listing.SellerId);
            if (listing.VehicleId > 0)
                query = query.Where(b => b.VehicleId == listing.VehicleId);
            if (listing.BatteryId > 0)
                query = query.Where(b => b.BatteryId == listing.BatteryId);
            if (!string.IsNullOrEmpty(listing.Title))
                query = query.Where(b => b.Title == listing.Title);
            if (!string.IsNullOrEmpty(listing.Description))
                query = query.Where(b => b.Description == listing.Description);
            if (listing.PriceVnd.HasValue)
                query = query.Where(b => b.PriceVnd == listing.PriceVnd);
            if (listing.AiSuggestedPriceVnd.HasValue)
                query = query.Where(b => b.AiSuggestedPriceVnd == listing.AiSuggestedPriceVnd);
            if (listing.Status.HasValue)
                query = query.Where(b => b.Status == listing.Status);
            if (listing.ApprovedBy.HasValue)
                query = query.Where(b => b.ApprovedBy == listing.ApprovedBy);
            return query.OrderBy(b => b.ListingId);
            //cần phải .ToList() ở service để thực thi câu query
        }
    }
}
