using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogApp.Shared.Auth
{
    public class JwtSettings
    {
        public string Issuer { get; set; } = "CatalogApp";
        public string Audience { get; set; } = "CatalogAppClient";
        public string Secret { get; set; } = null!;
        public int ExpireMinutes { get; set; } = 60 * 24;
    }
}
