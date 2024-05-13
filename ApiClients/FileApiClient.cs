using AmazingFileVersionControl.Core.DTOs.FileDTOs;
using AmazingFileVersionControl.Core.Infrastructure;
using MongoDB.Bson;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmazingFileVersionControl.ApiClients
{
    public class FileApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public FileApiClient(string baseUrl)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            _httpClient = new HttpClient(handler);
            _baseUrl = baseUrl.TrimEnd('/');
        }

        public void SetToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<string> UploadOwnerFileAsync(FileUploadDTO uploadRequest)
        {
            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(uploadRequest.Name), "Name");
            content.Add(new StringContent(uploadRequest.Project), "Project");
            content.Add(new StringContent(uploadRequest.Type), "Type");

            if (!string.IsNullOrEmpty(uploadRequest.Description))
            {
                content.Add(new StringContent(uploadRequest.Description), "Description");
            }

            if (!string.IsNullOrEmpty(uploadRequest.Owner))
            {
                content.Add(new StringContent(uploadRequest.Owner), "Owner");
            }

            content.Add(new StreamContent(uploadRequest.File.OpenReadStream()), "File", uploadRequest.File.FileName);

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/file/upload", content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();

            return result;
        }

        public async Task<Stream> DownloadOwnerFileAsync(FileQueryDTO queryRequest)
        {
            var url = $"{_baseUrl}/api/file/download?Name={queryRequest.Name}&Project={queryRequest.Project}&Version={queryRequest.Version}";
            if (!string.IsNullOrEmpty(queryRequest.Owner))
            {
                url += $"&Owner={queryRequest.Owner}";
            }

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync();
        }

        public async Task<string> GetOwnerFileInfoAsync(FileQueryDTO queryRequest)
        {
            var url = $"{_baseUrl}/api/file/info?Name={queryRequest.Name}&Project={queryRequest.Project}&Version={queryRequest.Version}";
            if (!string.IsNullOrEmpty(queryRequest.Owner))
            {
                url += $"&Owner={queryRequest.Owner}";
            }

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetOwnerAllFilesInfoAsync(string owner)
        {
            var url = $"{_baseUrl}/api/file/all-info?owner={owner}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task UpdateOwnerFileInfoAsync(FileUpdateDTO updateRequest)
        {
            var content = JsonContent.Create(new
            {
                updateRequest.Name,
                updateRequest.Project,
                updateRequest.Version,
                Owner = updateRequest.Owner,
                UpdatedMetadata = updateRequest.UpdatedMetadata
            });

            var response = await _httpClient.PutAsync($"{_baseUrl}/api/file/update-info", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateOwnerAllFilesInfoAsync(string owner, string updatedMetadataJson)
        {
            var content = JsonContent.Create(new
            {
                Owner = owner,
                UpdatedMetadata = updatedMetadataJson
            });

            var response = await _httpClient.PutAsync($"{_baseUrl}/api/file/update-all-info", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteOwnerFileAsync(FileQueryDTO queryRequest)
        {
            var url = $"{_baseUrl}/api/file/delete?Name={queryRequest.Name}&Project={queryRequest.Project}&Version={queryRequest.Version}";
            if (!string.IsNullOrEmpty(queryRequest.Owner))
            {
                url += $"&Owner={queryRequest.Owner}";
            }

            var response = await _httpClient.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteOwnerAllFilesAsync(string owner)
        {
            var url = $"{_baseUrl}/api/file/delete-all?owner={owner}";
            var response = await _httpClient.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
        }
    }
}
