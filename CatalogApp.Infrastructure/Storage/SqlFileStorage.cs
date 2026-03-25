using CatalogApp.Infrastructure.Interfaces;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CatalogApp.Infrastructure.Storage
{
    public class SqlFileStorage : IFileStorage
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction? _transaction;

        // Конструктор для DI: получает текущее соединение.
        // При желании можно добавить перегрузку, принимающую IDbTransaction.
        public SqlFileStorage(IDbConnection connection, IDbTransaction? transaction = null)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _transaction = transaction;
        }

        public async Task<string> UploadAsync(
            Stream stream,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name required", nameof(fileName));
            if (string.IsNullOrWhiteSpace(contentType)) throw new ArgumentException("Content type required", nameof(contentType));

            var id = Guid.NewGuid();

            await using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
            var bytes = ms.ToArray();

            const string sql = @"
                INSERT INTO Images (Id, FileName, ContentType, Data, CreatedAt)
                VALUES (@Id, @FileName, @ContentType, @Data, SYSUTCDATETIME())";

            var parameters = new
            {
                Id = id,
                FileName = fileName,
                ContentType = contentType,
                Data = bytes
            };

            var cmd = new CommandDefinition(sql, parameters, _transaction, cancellationToken: cancellationToken);
            await _connection.ExecuteAsync(cmd).ConfigureAwait(false);

            return id.ToString();
        }

        public async Task<Stream> DownloadAsync(string id, CancellationToken cancellationToken = default)
        {
            if (!Guid.TryParse(id, out var guid))
                throw new ArgumentException("Invalid id format", nameof(id));

            const string sql = "SELECT Data, ContentType FROM Images WHERE Id = @Id";

            var cmd = new CommandDefinition(sql, new { Id = guid }, _transaction, cancellationToken: cancellationToken);

            var row = await _connection.QuerySingleOrDefaultAsync<(byte[] Data, string ContentType)?>(cmd).ConfigureAwait(false);

            if (row == null || row.Value.Data == null || row.Value.Data.Length == 0)
                return Stream.Null;

            var ms = new MemoryStream(row.Value.Data, writable: false);
            ms.Position = 0;
            return ms;
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            if (!Guid.TryParse(id, out var guid))
                throw new ArgumentException("Invalid id format", nameof(id));

            const string sql = "DELETE FROM Images WHERE Id = @Id";

            var cmd = new CommandDefinition(sql, new { Id = guid }, _transaction, cancellationToken: cancellationToken);
            await _connection.ExecuteAsync(cmd).ConfigureAwait(false);
        }
    }
}
