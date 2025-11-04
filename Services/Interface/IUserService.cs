using EVBTradingContract.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EVBTradingContract.Request;
using EVBTradingContract.Response;

namespace Services.Interface
{
    public interface IUserService
    {
        Task<PageResponse<UserResponse>> GetAllUsersFiltered(UserFilterRequest request, int page, int pageSize);
        Task<ApiResponse<UserResponse>> CreateUser(UserRequest request);
        Task<ApiResponse<UserResponse?>> GetUserById(Guid userId);
        Task<ApiResponse<bool>> DeleteUser(Guid userId);
        Task<ApiResponse<bool>> UpdateUser(Guid userId, UserRequest user);
    }
}
