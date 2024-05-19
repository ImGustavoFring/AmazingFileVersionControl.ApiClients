using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AmazingFileVersionControl.Core.DTOs.FileDTOs;
using Microsoft.AspNetCore.WebUtilities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.GridFS;
using Newtonsoft.Json;

namespace AmazingFileVersionControl.ApiClients
{
    public class FileClient
    {
        private readonly HttpClient _httpClient;

        public FileClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ObjectId> UploadFileAsync(FileUploadDTO fileUploadDto)
        {
            using var form = new MultipartFormDataContent();

            // Add file to the form
            using var fileStream = fileUploadDto.File.OpenReadStream();
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "File",
                FileName = fileUploadDto.Name
            };
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(fileUploadDto.Type);

            form.Add(fileContent, "File", fileUploadDto.Name);

            // Add other data
            form.Add(new StringContent(fileUploadDto.Name), "Name");
            form.Add(new StringContent(fileUploadDto.Type), "Type");
            form.Add(new StringContent(fileUploadDto.Project), "Project");

            if (!string.IsNullOrEmpty(fileUploadDto.Description))
                form.Add(new StringContent(fileUploadDto.Description), "Description");
            if (!string.IsNullOrEmpty(fileUploadDto.Owner))
                form.Add(new StringContent(fileUploadDto.Owner), "Owner");
            if (fileUploadDto.Version.HasValue)
                form.Add(new StringContent(fileUploadDto.Version.Value.ToString()), "Version");

            var response = await _httpClient.PostAsync("api/file/upload", form);
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                throw new Exception($"File upload failed. Status code: {response.StatusCode}, Error: {errorResponse}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<UploadFileResponse>(responseContent);
            return ObjectId.Parse(result.FileId);
        }



        public async Task<(Stream, GridFSFileInfo)> DownloadFileWithMetadataAsync(FileQueryDTO fileQueryDto)
        {
            var query = $"?Name={fileQueryDto.Name}&Type={fileQueryDto.Type}&Project={fileQueryDto.Project}&Owner={fileQueryDto.Owner}&Version={fileQueryDto.Version}";
            var response = await _httpClient.GetAsync($"api/file/download{query}");

            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            var metadataJson = response.Headers.GetValues("File-Metadata").FirstOrDefault();

            if (string.IsNullOrEmpty(metadataJson))
            {
                throw new InvalidOperationException("Response does not contain metadata.");
            }

            var fileInfo = BsonSerializer.Deserialize<GridFSFileInfo>(metadataJson);

            return (stream, fileInfo);
        }

        public async Task<GridFSFileInfo> GetFileInfoByVersionAsync(FileQueryDTO fileQueryDto)
        {
            var query = $"?Name={fileQueryDto.Name}&Type={fileQueryDto.Type}&Project={fileQueryDto.Project}&Version={fileQueryDto.Version}&Owner={fileQueryDto.Owner}";
            var response = await _httpClient.GetAsync($"api/file/info/version{query}");

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return BsonSerializer.Deserialize<GridFSFileInfo>(responseContent);
        }

        public async Task<List<GridFSFileInfo>> GetFileInfoAsync(FileQueryDTO fileQueryDto)
        {
            var query = $"?Name={fileQueryDto.Name}&Type={fileQueryDto.Type}&Project={fileQueryDto.Project}&Owner={fileQueryDto.Owner}";
            var response = await _httpClient.GetAsync($"api/file/info{query}");

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return BsonSerializer.Deserialize<List<GridFSFileInfo>>(responseContent);
        }

        public async Task<List<GridFSFileInfo>> GetProjectFilesInfoAsync(string project, string owner)
        {
            var query = $"?Project={project}&Owner={owner}";
            var response = await _httpClient.GetAsync($"api/file/project/info{query}");

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return BsonSerializer.Deserialize<List<GridFSFileInfo>>(responseContent);
        }

        public async Task<List<GridFSFileInfo>> GetAllFilesInfoAsync(string owner)
        {
            var query = $"?Owner={owner}";
            var response = await _httpClient.GetAsync($"api/file/all/info{query}");

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return BsonSerializer.Deserialize<List<GridFSFileInfo>>(responseContent);
        }

        public async Task UpdateFileInfoByVersionAsync(FileUpdateDTO fileUpdateDto)
        {
            var content = new StringContent(JsonConvert.SerializeObject(fileUpdateDto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync("api/file/update/version", content);

            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateFileInfoAsync(FileUpdateDTO fileUpdateDto)
        {
            var content = new StringContent(JsonConvert.SerializeObject(fileUpdateDto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync("api/file/update", content);

            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateFileInfoByProjectAsync(UpdateAllFilesDTO updateAllFilesDto)
        {
            var content = new StringContent(JsonConvert.SerializeObject(updateAllFilesDto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync("api/file/update/project", content);

            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateAllFilesInfoAsync(UpdateAllFilesDTO updateAllFilesDto)
        {
            var content = new StringContent(JsonConvert.SerializeObject(updateAllFilesDto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync("api/file/update/all", content);

            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteFileByVersionAsync(FileQueryDTO fileQueryDto)
        {
            var query = $"?Name={fileQueryDto.Name}&Type={fileQueryDto.Type}&Project={fileQueryDto.Project}&Version={fileQueryDto.Version}&Owner={fileQueryDto.Owner}";
            var response = await _httpClient.DeleteAsync($"api/file/delete/version{query}");

            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteFileAsync(FileQueryDTO fileQueryDto)
        {
            var query = $"?Name={fileQueryDto.Name}&Type={fileQueryDto.Type}&Project={fileQueryDto.Project}&Owner={fileQueryDto.Owner}";
            var response = await _httpClient.DeleteAsync($"api/file/delete{query}");

            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteProjectFilesAsync(string project, string owner)
        {
            var query = $"?Project={project}&Owner={owner}";
            var response = await _httpClient.DeleteAsync($"api/file/delete/project{query}");

            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAllFilesAsync(string owner)
        {
            var query = $"?Owner={owner}";
            var response = await _httpClient.DeleteAsync($"api/file/delete/all{query}");

            response.EnsureSuccessStatusCode();
        }
    }

    public class UploadFileResponse
    {
        public string FileId { get; set; }
    }
}
