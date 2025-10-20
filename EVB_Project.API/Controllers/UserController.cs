using EVBTradingContract.Common;
using EVBTradingContract.Request;
using EVBTradingContract.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services.Interface;
using System.Net;
namespace EVB_Project.API.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PageResponse<UserResponse>>>> GetFiltered(
            [FromQuery] UserFilterRequest request,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _userService.GetAllUsersFiltered(request, page, pageSize);
                return Ok(new ApiResponse<PageResponse<UserResponse>>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<PageResponse<UserResponse>>
                {
                    Success = false,
                    Data = null
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserResponse>>> CreateUser([FromBody] UserCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<UserResponse>
                {
                    Success = false,
                    Data = null
                });
            }

            try
            {
                var result = await _userService.CreateUser(request);
                return Ok(new ApiResponse<UserResponse>
                {
                    Success = true,
                    Data = result.Data
                });
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<UserResponse>
                {
                    Success = false,
                    Data = null
                });
            }
        }

        [HttpGet("{userId:long}")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> GetUserById([FromRoute] Guid userId)
        {
            try
            {
                var result = await _userService.GetUserById(userId);
                if (result == null)
                {
                    return NotFound(new ApiResponse<UserResponse>
                    {
                        Success = false,
                        Data = null
                    });
                }

                return Ok(new ApiResponse<UserResponse>
                {
                    Success = true,
                    Data = result.Data
                });
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<UserResponse>
                {
                    Success = false,
                    Data = null
                });
            }
        }

        [HttpDelete("{userId:long}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteUser([FromRoute] Guid userId)
        {
            try
            {
                var result = await _userService.DeleteUser(userId);
                return Ok(new ApiResponse<bool>
                {
                    Success = result.Success,
                    Data = result.Data
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

        [HttpPut("{userId:long}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateUser([FromRoute] Guid userId, [FromBody] UserRequest request)
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
                var result = await _userService.UpdateUser(userId, request);
                return Ok(new ApiResponse<bool>
                {
                    Success = result.Success,
                    Data = result.Data
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