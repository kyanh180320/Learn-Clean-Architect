using Application.DTOs.Request;
using Application.DTOs.Response;
using Domain.Common.Extension;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mapping
{
    public class UserMapping
    {
        public UserResponse ToReponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedAt = DateTime.Now.ToLongTimestamp(),
            };
        }
        public User ToEntity ( RegisterUserRequest request){
            return new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                FullName = request.FullName,
                IsActive = true,
                CreatedAt = DateTime.Now.ToLongTimestamp(),

            };
        }
    }
}
