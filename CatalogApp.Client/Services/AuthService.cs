using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogApp.Client.Services
{
    public class AuthService : IAuthService
    {
        public string? Token { get; private set; }

        public void SetToken(string token) => Token = token;
    }
}
