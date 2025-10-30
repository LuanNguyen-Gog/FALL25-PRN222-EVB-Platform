using EVBTradingContract.Common;
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

        [HttpGet("{listingId:guid}")]
        public async Task<ActionResult<ApiResponse<ListingResponse>>> GetById([FromRoute] Guid listingId)
        {
            var result = await _listingService.GetListingById(listingId);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ListingResponse>>> Create([FromBody] ListingCreateRequest request)
        {
            var result = await _listingService.CreateListing(request);
            return Ok(result);
        }

        [HttpPut("{listingId:guid}")]
        public async Task<ActionResult<ApiResponse<bool>>> Update([FromRoute] Guid listingId, [FromBody] ListingUpdateRequest request)
        {
            var result = await _listingService.UpdateListing(listingId, request);
            return Ok(result);
        }

        [HttpDelete("{listingId:guid}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete([FromRoute] Guid listingId)
        {
            var result = await _listingService.DeleteListing(listingId);
            return Ok(result);
        }
    }
}