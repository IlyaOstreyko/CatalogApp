using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CatalogApp.Infrastructure.Interfaces
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
