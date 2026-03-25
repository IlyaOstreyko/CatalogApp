using AutoMapper;
using CatalogApp.Infrastructure.Entities;
using CatalogApp.Infrastructure.Interfaces;
using CatalogApp.Shared.Dto;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CatalogApp.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;
        private readonly IMapper _mapper;

        public UserRepository(IDbConnection connection, IDbTransaction transaction, IMapper mapper)
        {
            _connection = connection;
            _transaction = transaction;
            _mapper = mapper;
        }

        public async Task<UserDto?> GetByUsernameAsync(string username)
        {
            const string sql = "SELECT * FROM Users WHERE Username = @username";
            var entity = await _connection.QueryFirstOrDefaultAsync<UserEntity>(sql, new { username }, _transaction);
            return _mapper.Map<UserDto>(entity);
        }
        public async Task<string?> GetPasswordHashByUsernameAsync(string username)
        {
            const string sql = "SELECT PasswordHash FROM Users WHERE Username = @username";
            var hash = await _connection.QueryFirstOrDefaultAsync<string>(sql, new { username }, _transaction);
            return hash;
        }

        public async Task CreateAsync(UserDto dto, string passwordHash)
        {
            const string sql = "INSERT INTO Users (Username, PasswordHash) VALUES (@Username, @PasswordHash)";
            await _connection.ExecuteAsync(sql, new { dto.Username, PasswordHash = passwordHash }, _transaction);
        }
        
            public async Task<bool> CheckUsernameExistsAsync(string username)
        {
            const string sql = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
            var count = await _connection.ExecuteScalarAsync<int>(sql, new { Username = username }, _transaction);
            return count > 0;
        }
        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            const string sql = "SELECT COUNT(1) FROM Users WHERE UserEmail = @Email";
            var count = await _connection.ExecuteScalarAsync<int>(sql, new { Email = email }, _transaction);
            return count > 0;
        }
    }
}
