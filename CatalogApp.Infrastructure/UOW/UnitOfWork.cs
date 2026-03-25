using AutoMapper;
using CatalogApp.Infrastructure.Interfaces;
using CatalogApp.Infrastructure.Repositories;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CatalogApp.Infrastructure.UOW
{
    public class UnitOfWork : IUnitOfWorkWithTransaction
    {
        private readonly IDbConnection _connection;
        private readonly IMapper _mapper;
        private IDbTransaction? _transaction;
        private bool _disposed;

        private IProductRepository? _products;
        public IProductRepository Products =>
            _products ??= new ProductRepository(_connection, _transaction, _mapper);

        private IUserRepository? _users;
        public IUserRepository Users =>
            _users ??= new UserRepository(_connection, _transaction, _mapper);

        public UnitOfWork(IDbConnection connection, IMapper mapper)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public IDbTransaction? CurrentTransaction => _transaction;

        public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null) return Task.CompletedTask;

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            _transaction = _connection.BeginTransaction();
            return Task.CompletedTask;
        }

        public void Commit()
        {
            if (_transaction == null) return;
            try
            {
                _transaction.Commit();
            }
            catch
            {
                _transaction.Rollback();
                throw;
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            Commit();
            return Task.CompletedTask;
        }

        public void Rollback()
        {
            if (_transaction == null) return;
            try
            {
                _transaction.Rollback();
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            Rollback();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _transaction?.Dispose();
                _transaction = null;
                // НЕ диспозим _connection — это управляется DI
            }

            _disposed = true;
        }
    }
}
