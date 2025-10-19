using EVBTradingContract.Common;
using EVBTradingContract.Request;
using EVBTradingContract.Response;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using Repositories.Repository;
using Services.Interface;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implement
{
    public class UserService : IUserService
    {
        private readonly UserRepository _userRepository;
        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<PageResponse<UserResponse>> GetAllUsersFiltered(UserFilterRequest request, int page, int pageSize)
        {
            var entity = request.Adapt<User>();
            var query = _userRepository.GetFiltered(entity);

            var totalCount = await query.CountAsync();
            var item = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PageResponse<UserResponse>()
            {
                Items = item.Adapt<List<UserResponse>>(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        public async Task<ApiResponse<UserResponse>> CreateUser(UserCreateRequest request)
        {
            try
            {
                var entity = request.Adapt<User>();
                var createdEntity = await _userRepository.CreateAsync(entity);
                entity.Status = Repositories.Enum.Enum.UserStatus.Active;
                return new ApiResponse<UserResponse>()
                {
                    Success = true,
                    Message = "User created successfully",
                    Data = entity.Adapt<UserResponse>()
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserResponse>()
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }
        public async Task<ApiResponse<UserResponse?>> GetUserById(Guid userId)
        {
            try
            {
                var entity = await _userRepository.GetByIdAsync(userId);
                if (entity == null) return null;
                return new ApiResponse<UserResponse?>()
                {
                    Success = true,
                    Message = "User retrieved successfully",
                    Data = entity.Adapt<UserResponse>()
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserResponse?>()
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }
        public async Task<ApiResponse<bool>> DeleteUser(Guid userId)
        {
            try
            {
                var entity = await _userRepository.GetByIdAsync(userId);
                if (entity == null) throw new Exception("User not found");
                await _userRepository.RemoveAsync(entity);
                return new ApiResponse<bool>()
                {
                    Success = true,
                    Message = "User deleted successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>()
                {
                    Success = false,
                    Message = ex.Message,
                    Data = false
                };
            }
        }
        public async Task<ApiResponse<bool>> UpdateUser(Guid userId, UserRequest user)
        {
            try
            {
                var existingUser = await _userRepository.GetByIdAsync(userId);
                if (existingUser == null) throw new Exception("User not found");
                var updatedUser = user.Adapt<User>();
                updatedUser.Id = userId; // Ensure the ID remains unchanged
                await _userRepository.UpdateAsync(updatedUser);
                return new ApiResponse<bool>()
                {
                    Success = true,
                    Message = "User updated successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>()
                {
                    Success = false,
                    Message = ex.Message,
                    Data = false
                };
            }
        }
    }
}
