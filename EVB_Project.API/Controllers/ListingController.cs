using EVBTradingContract.Common;
using EVBTradingContract.Request;
using EVBTradingContract.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using System.Net;
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
        public async Task<ActionResult<ApiResponse<List<ListingResponse>>>> GetAllListing([FromQuery] ListingGetRequest request)
        {
            try
            {
                var result = await _listingService.GetAllListingsAsync(request);
                return Ok(new ApiResponse<List<ListingResponse>>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<List<ListingResponse>>
                {
                    Success = false,
                    Data = null
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ListingResponse>>> CreateListing([FromBody] ListingGetRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<ListingResponse>
                {
                    Success = false,
                    Data = null
                });
            }

            try
            {
                var result = await _listingService.CreateListingAsync(request);
                return Ok(new ApiResponse<ListingResponse>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<ListingResponse>
                {
                    Success = false,
                    Data = null
                });
            }
        }

        [HttpGet("{listingId:long}")]
        public async Task<ActionResult<ApiResponse<ListingResponse>>> GetListingById([FromRoute] long listingId)
        {
            try
            {
                var result = await _listingService.GetListingByIdAsync(listingId);
                if (result == null)
                {
                    return NotFound(new ApiResponse<ListingResponse>
                    {
                        Success = false,
                        Data = null
                    });
                }

                return Ok(new ApiResponse<ListingResponse>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<ListingResponse>
                {
                    Success = false,
                    Data = null
                });
            }
        }

        [HttpPut("{listingId:long}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateListing([FromRoute] long listingId, [FromBody] ListingGetRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Data = false
                });
            }

            try
            {
                var result = await _listingService.UpdateListingAsync(listingId, request);
                return Ok(new ApiResponse<bool>
                {
                    Success = result,
                    Data = result
                });
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<bool>
                {
                    Success = false,
                    Data = false
                });
            }
        }

        [HttpDelete("{listingId:long}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteListing([FromRoute] long listingId)
        {
            try
            {
                var result = await _listingService.DeleteListingAsync(listingId);
                return Ok(new ApiResponse<bool>
                {
                    Success = result,
                    Data = result
                });
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<bool>
                {
                    Success = false,
                    Data = false
                });
            }
        }
    }
}