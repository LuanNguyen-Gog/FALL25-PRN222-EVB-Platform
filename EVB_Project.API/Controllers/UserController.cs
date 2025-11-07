using EVBTradingContract.Common;
using EVBTradingContract.Request;
using EVBTradingContract.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services.Interface;


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
        public async Task<ActionResult<PageResponse<User>>> GetFiltered([FromQuery] UserFilterRequest request, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _userService.GetAllUsersFiltered(request, page, pageSize);
            return Ok(result);

        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserResponse>>> CreateUser([FromBody] UserRequest request)
        {
            var result = await _userService.CreateUser(request);
            return Ok(result);
        }
        [HttpGet]
        [Route("{userId}")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> GetUserById([FromRoute] Guid userId)
        {
            var result = await _userService.GetUserById(userId);
            if (result == null) return NotFound();
            return Ok(result);
        }
        [HttpDelete]
        [Route("{userId}")]
        public async Task<ApiResponse<bool>> DeleteUser([FromRoute] Guid userId)
        {
            return await _userService.DeleteUser(userId);
        }
        [HttpPut]
        [Route("{userId}")]
        public async Task<ApiResponse<bool>> UpdateUser([FromRoute] Guid userId, [FromBody] UserRequest request)
        {
            return await _userService.UpdateUser(userId, request);
        }
    }
}
