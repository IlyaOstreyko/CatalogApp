using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogApp.Infrastructure.Entities
{
    public class UserEntity
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
    }
}
