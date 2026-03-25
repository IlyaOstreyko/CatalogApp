using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CatalogApp.Infrastructure.Interfaces
{
    public interface IUnitOfWorkWithTransaction : IUnitOfWork
    {
        IDbTransaction? CurrentTransaction { get; }
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    }
}
