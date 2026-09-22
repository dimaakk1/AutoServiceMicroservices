using AutoServiceUsers.BLL.Services;
using AutoServiceUsers.BLL.Services.Interfaces;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceUsers.BLL.Grpc
{
    public class UserGrpcService : UserService.UserServiceBase
    {
        private readonly IUserService _userService;

        public UserGrpcService(IUserService userService)
        {
            _userService = userService;
        }

        public override async Task<UserResponse> GetUser(UserRequest request, ServerCallContext context)
        {
            var user = await _userService.GetByIdAsync(request.UserId);

            if (user == null)
            {
                throw new RpcException(new Status(
                    StatusCode.NotFound,
                    "User not found"
                ));
            }

            return new UserResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email
            };
        }
    }
}
