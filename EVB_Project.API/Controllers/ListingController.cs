using EVBTradingContract.Request;
using EVBTradingContract.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace EVB_Project.API.Controllers
{
    [Route("api/listings")]
    [ApiController]
    [Authorize(Roles = "member")]
    public class ListingController : ControllerBase
    {
        private readonly IListingService _listingService;
        public ListingController(IListingService listingService)
        {
            _listingService = listingService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ListingResponse>>> GetAllListing([FromQuery] ListingGetRequest request)
        {
            var result = await _listingService.GetAllListingsAsync(request);
            return Ok(result);
        }
    }
}
