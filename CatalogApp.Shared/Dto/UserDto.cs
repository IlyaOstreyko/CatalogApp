using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogApp.Shared.Dto
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
    }
}
