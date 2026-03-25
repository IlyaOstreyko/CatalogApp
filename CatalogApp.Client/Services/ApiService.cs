using CatalogApp.Shared.Api;
using CatalogApp.Shared.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CatalogApp.Client.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;

        public ApiService(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _http = new HttpClient();
        }

        public async Task<ProductDto?> CreateProductWithImageAsync(ProductDto product, string? filePath, string? token = null)
        {
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var form = new MultipartFormDataContent();

            var productJson = JsonSerializer.Serialize(product);
            form.Add(new StringContent(productJson, Encoding.UTF8, "application/json"), "productJson");

            if (!string.IsNullOrEmpty(filePath))
            {
                var fs = File.OpenRead(filePath);
                var fileContent = new StreamContent(fs);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                form.Add(fileContent, "file", Path.GetFileName(filePath));
            }

            var resp = await _http.PostAsync($"{_baseUrl}/api/products/create-with-image", form);
            if (!resp.IsSuccessStatusCode) return null;
            var body = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ProductDto>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<Stream?> GetProductImageAsync(int productId)
        {
            var resp = await _http.GetAsync($"{_baseUrl}/api/products/{productId}/image");
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadAsStreamAsync();
        }

        public async Task<string?> LoginAsync(LoginRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                using var resp = await _http.PostAsync($"{_baseUrl}/api/auth/login", content).ConfigureAwait(false);

                if (!resp.IsSuccessStatusCode)
                {
                    // Можно логировать код и тело ответа
                    return null;
                }

                using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
                return doc.RootElement.GetProperty("token").GetString();
            }
            catch (HttpRequestException ex)
            {
                // Сетевая ошибка (сервер недоступен, отказ соединения и т.д.)
                // Логирование: ex.Message
                return null;
            }
            catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
            {
                // Таймаут
                return null;
            }
            catch (Exception)
            {
                // Неожиданная ошибка
                return null;
            }
        }

        public async Task RegisterAsync(RegisterRequest request)
        {
            var json = JsonSerializer.Serialize(request);
            var resp = await _http.PostAsync($"{_baseUrl}/api/auth/register", new StringContent(json, Encoding.UTF8, "application/json"));
            resp.EnsureSuccessStatusCode();
        }

        public async Task<IEnumerable<ProductDto>> GetProductsAsync(string? token = null)
        {
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var resp = await _http.GetAsync($"{_baseUrl}/api/products");
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<ProductDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? Enumerable.Empty<ProductDto>();
        }

        public async Task<ProductDto?> CreateProductAsync(ProductDto product, string? token = null)
        {
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var json = JsonSerializer.Serialize(product);
            var resp = await _http.PostAsync($"{_baseUrl}/api/products", new StringContent(json, Encoding.UTF8, "application/json"));
            if (!resp.IsSuccessStatusCode) return null;
            var body = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ProductDto>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<string?> UploadFileAsync(string filePath, string? token = null)
        {
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var form = new MultipartFormDataContent();
            using var fs = File.OpenRead(filePath);
            var fileContent = new StreamContent(fs);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            form.Add(fileContent, "file", Path.GetFileName(filePath));

            var resp = await _http.PostAsync($"{_baseUrl}/api/files/upload", form);
            if (!resp.IsSuccessStatusCode) return null;
            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            return doc.RootElement.GetProperty("url").GetString();
        }
        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            var url = $"{_baseUrl}/api/auth/email-exists?email={Uri.EscapeDataString(email)}";

            var resp = await _http.GetAsync(url);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<bool>(json);
        }
        public async Task<bool> CheckUsernameExistsAsync(string Username)
        {
            var url = $"{_baseUrl}/api/auth/username-exists?username={Uri.EscapeDataString(Username)}";

            var resp = await _http.GetAsync(url);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<bool>(json);
        }
    }
}
