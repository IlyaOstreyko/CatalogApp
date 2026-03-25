using CatalogApp.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly IFileStorage _storage;
        private readonly ILogger<FilesController> _logger;

        public FilesController(IFileStorage storage, ILogger<FilesController> logger)
        {
            _storage = storage;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("File missing");

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            using var stream = file.OpenReadStream();
            var url = await _storage.UploadAsync(stream, fileName, file.ContentType);
            return Ok(new { url });
        }
    }
}
