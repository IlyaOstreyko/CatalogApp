using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogApp.Infrastructure.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }
        IUserRepository Users { get; }

        // синхронные версии (оставляем для совместимости)
        void Commit();
        void Rollback();

        // асинхронные версии (рекомендуются)
        Task CommitAsync(CancellationToken cancellationToken = default);
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}

