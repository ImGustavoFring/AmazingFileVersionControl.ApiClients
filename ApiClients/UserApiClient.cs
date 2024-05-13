using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmazingFileVersionControl.ApiClients
{
    public class UserApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public UserApiClient(string baseUrl)
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

        public async Task<string> GetUserAsync(Guid? userId = null)
        {
            var url = $"{_baseUrl}/api/user/user";
            if (userId.HasValue)
            {
                url += $"?userId={userId}";
            }

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> SearchUsersByLoginAsync(string loginSubstring)
        {
            var url = $"{_baseUrl}/api/user/search-by-login?loginSubstring={loginSubstring}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> SearchUsersByEmailAsync(string emailSubstring)
        {
            var url = $"{_baseUrl}/api/user/search-by-email?emailSubstring={emailSubstring}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task ChangeUserLoginAsync(Guid userId, string newLogin)
        {
            var content = JsonContent.Create(new
            {
                UserId = userId,
                NewLogin = newLogin
            });

            var response = await _httpClient.PutAsync($"{_baseUrl}/api/user/change-login", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task ChangeUserEmailAsync(Guid userId, string newEmail)
        {
            var content = JsonContent.Create(new
            {
                UserId = userId,
                NewEmail = newEmail
            });

            var response = await _httpClient.PutAsync($"{_baseUrl}/api/user/change-email", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task ChangeUserPasswordAsync(Guid userId, string newPassword)
        {
            var content = JsonContent.Create(new
            {
                UserId = userId,
                NewPassword = newPassword
            });

            var response = await _httpClient.PutAsync($"{_baseUrl}/api/user/change-password", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task ChangeUserRoleAsync(Guid userId, string newRole)
        {
            var content = JsonContent.Create(new
            {
                UserId = userId,
                NewRole = newRole
            });

            var response = await _httpClient.PutAsync($"{_baseUrl}/api/user/change-role", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteUserAsync(Guid userId)
        {
            var url = $"{_baseUrl}/api/user/delete?userId={userId}";
            var response = await _httpClient.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
        }
    }
}
