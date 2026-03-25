using CatalogApp.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogApp.Infrastructure.Interfaces
{
    public interface IUserRepository
    {
        Task<UserDto?> GetByUsernameAsync(string username);
        Task<string?> GetPasswordHashByUsernameAsync(string username);
        Task CreateAsync(UserDto userDto, string passwordHash);
        Task<bool> CheckUsernameExistsAsync(string username);
        
        Task<bool> CheckEmailExistsAsync(string email);
    }
}
