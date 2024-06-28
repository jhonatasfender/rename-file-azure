using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Npgsql;

namespace RenameFilesAzure
{
    internal static class Program
    {
        private const string AzureBlobStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=?;AccountKey=?;EndpointSuffix=core.windows.net";
        private const string BlobContainerName = "?";
        private const string PostgresConnectionString = "Host=?;Database=?;Port=5432;Username=?@b?;Password=?;SslMode=Require;Trust Server Certificate=true";

        private static BlobContainerClient? _blobContainerClient;

        public static async Task Main(string[] args)
        {
            var blobServiceClient = new BlobServiceClient(AzureBlobStorageConnectionString);
            _blobContainerClient = blobServiceClient.GetBlobContainerClient(BlobContainerName);

            await ProcessBlobsAsync();
        }

        private static async Task ProcessBlobsAsync()
        {
            var records = await FetchRecordsFromDatabaseAsync();
            await RenameAndMoveBlobsAsync(records);
        }

        private static async Task<List<(int Id, string ArquivoUrl)>> FetchRecordsFromDatabaseAsync()
        {
            await using var connection = new NpgsqlConnection(PostgresConnectionString);
            await connection.OpenAsync();

            const string selectSql = "SELECT \"Id\", \"ArquivoUrl\" FROM public.\"Bilhetes\" WHERE \"ArquivoUrl\" IS NOT NULL ORDER BY \"Id\" DESC";
            await using var selectCommand = new NpgsqlCommand(selectSql, connection);
            await using var reader = await selectCommand.ExecuteReaderAsync();

            var records = new List<(int Id, string ArquivoUrl)>();
            while (await reader.ReadAsync())
            {
                records.Add((reader.GetInt32(0), reader.GetString(1)));
            }

            return records;
        }

        private static async Task RenameAndMoveBlobsAsync(List<(int Id, string ArquivoUrl)> records)
        {
            foreach (var (id, arquivoUrl) in records)
            {
                var oldBlobName = Path.GetFileName(arquivoUrl);
                var newBlobName = GenerateNewBlobName(oldBlobName);

                if (_blobContainerClient == null)
                {
                    continue;
                }

                var oldBlobClient = _blobContainerClient.GetBlobClient(oldBlobName);
                if (!await oldBlobClient.ExistsAsync())
                {
                    continue;
                }

                var newBlobClient = _blobContainerClient.GetBlobClient(newBlobName);
                await newBlobClient.StartCopyFromUriAsync(oldBlobClient.Uri);

                if (!await WaitForBlobCopyCompletionAsync(newBlobClient))
                {
                    Console.WriteLine($"Failed to copy blob '{oldBlobName}'.");
                    continue;
                }

                await oldBlobClient.DeleteIfExistsAsync();
                await UpdateDatabaseRecordAsync(id, arquivoUrl, newBlobName);
            }
        }

        private static async Task<bool> WaitForBlobCopyCompletionAsync(BlobClient blobClient)
        {
            BlobProperties properties;
            do
            {
                await Task.Delay(1000);
                properties = await blobClient.GetPropertiesAsync();
            } while (properties.CopyStatus == CopyStatus.Pending);

            return properties.CopyStatus == CopyStatus.Success;
        }

        private static async Task UpdateDatabaseRecordAsync(int id, string oldFileUrl, string newBlobName)
        {
            var newFileUrl = oldFileUrl.Replace(Path.GetFileName(oldFileUrl), newBlobName);

            await using var connection = new NpgsqlConnection(PostgresConnectionString);
            await connection.OpenAsync();

            const string updateSql = @"UPDATE ""Bilhetes"" SET ""ArquivoUrl"" = @newFileUrl WHERE ""ArquivoUrl"" = @oldFileUrl";
            await using var updateCommand = new NpgsqlCommand(updateSql, connection);
            updateCommand.Parameters.AddWithValue("@newFileUrl", newFileUrl);
            updateCommand.Parameters.AddWithValue("@oldFileUrl", oldFileUrl);

            var rowsAffected = await updateCommand.ExecuteNonQueryAsync();
            Console.WriteLine($"Updated {rowsAffected} records for blob '{oldFileUrl}' to '{newFileUrl}'.");
        }

        private static string GenerateNewBlobName(string oldBlobName)
        {
            return oldBlobName.Replace("apolice", "bilhete");
        }
    }
}