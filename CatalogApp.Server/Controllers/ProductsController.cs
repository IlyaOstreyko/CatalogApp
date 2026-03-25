using CatalogApp.Infrastructure.Interfaces;
using CatalogApp.Server.Hubs;
using CatalogApp.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace CatalogApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IUnitOfWorkWithTransaction _uow;
        private readonly IHubContext<CatalogHub> _hub;

    public ProductsController(IUnitOfWorkWithTransaction uow, IHubContext<CatalogHub> hub)
    {
        _uow = uow;
        _hub = hub;
    }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _uow.Products.GetAllAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var product = await _uow.Products.GetByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        // GET /api/products/{id}/image
        [HttpGet("{id}/image")]
        public async Task<IActionResult> GetImage(int id)
        {
            var image = await _uow.Products.GetImageAsync(id);
            if (image == null) return NotFound();
            return File(image.Value.Data, image.Value.ContentType);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductDto dto)
        {
            var id = await _uow.Products.CreateAsync(dto);
            _uow.Commit();
            dto.Id = id;

            // Уведомляем всех клиентов о новом товаре
            await _hub.Clients.All.SendAsync("ProductAdded", dto);

            return CreatedAtAction(nameof(Get), new { id }, dto);
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductDto dto)
        {
            dto.Id = id;
            await _uow.Products.UpdateAsync(dto);
            _uow.Commit();

            await _hub.Clients.All.SendAsync("ProductUpdated", dto);

            return NoContent();
        }

        // Create product with image (multipart/form-data)
        [Authorize]
        [HttpPost("create-with-image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateWithImage([FromForm] string productJson, [FromForm] IFormFile? file)
        {
            var dto = JsonSerializer.Deserialize<ProductDto>(productJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (dto == null) return BadRequest();

            await _uow.BeginTransactionAsync();

            try
            {
                byte[]? data = null;
                string? contentType = null;
                string? fileName = null;

                if (file != null)
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    data = ms.ToArray();
                    contentType = file.ContentType;
                    fileName = file.FileName;
                }

                // Сохраняем продукт (внутри CreateWithImageAsync должен быть INSERT и возврат id)
                var id = await _uow.Products.CreateWithImageAsync(dto, data, contentType, fileName);

                // Коммит транзакции — обязательно до уведомления клиентов
                await _uow.CommitAsync();

                // Обновляем DTO полями, которые появились после сохранения
                dto.Id = id;
                dto.HasImage = data != null && data.Length > 0;

                // --- УВЕДОМЛЕНИЕ КЛИЕНТОВ ЧЕРЕЗ SIGNALR ---
                // Отправляем объект DTO (не JSON-строку). Клиент должен ожидать ProductDto.
                try
                {
                    await _hub.Clients.All.SendAsync("ProductAdded", dto);
                }
                catch (Exception sendEx)
                {
                    // Логирование ошибки отправки уведомления (не прерываем основной поток)
                    // Если у тебя есть ILogger<ProductsController>, используй его:
                    // _logger.LogError(sendEx, "Failed to send ProductAdded for id {Id}", dto.Id);
                    // Если логгера нет — можно просто swallow или rethrow в dev-режиме.
                }

                return CreatedAtAction(nameof(Get), new { id }, dto);
            }
            catch
            {
                await _uow.RollbackAsync();
                throw;
            }
        }


        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _uow.Products.DeleteAsync(id);
            _uow.Commit();

            await _hub.Clients.All.SendAsync("ProductDeleted", id);

            return NoContent();
        }
    }
}
