using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogApp.Client.Services
{
    public interface IAuthService
    {
        string? Token { get; }
    }
}
