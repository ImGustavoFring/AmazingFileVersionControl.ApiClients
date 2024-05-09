using AmazingFileVersionControl.Core.DTOs.AuthDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmazingFileVersionControl.ApiClients.ApiClients
{
    public class AuthApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public AuthApiClient(string baseUrl)
        {
            _httpClient = new HttpClient();
            _baseUrl = baseUrl.TrimEnd('/');
        }

        public async Task<string> RegisterAsync(RegisterDTO registerRequest)
        {
            var registerContent = new StringContent(JsonSerializer.Serialize(registerRequest),
                Encoding.UTF8, "application/json");

            var registerResponse = await _httpClient.PostAsync($"{_baseUrl}/register", registerContent);

            if (registerResponse.IsSuccessStatusCode)
            {
                var registerResult = await registerResponse.Content.ReadAsStringAsync();
                return registerResult;
            }

            else
            {
                var error = await registerResponse.Content.ReadAsStringAsync();
                throw new Exception($"Registration failed. Error: {error}");
            }
        }

        public async Task<string> LoginAsync(LoginDTO loginRequest)
        {
            var loginContent = new StringContent(JsonSerializer.Serialize(loginRequest),
                Encoding.UTF8, "application/json");

            var loginResponse = await _httpClient.PostAsync($"{_baseUrl}/login", loginContent);

            if (loginResponse.IsSuccessStatusCode)
            {
                var loginResult = await loginResponse.Content.ReadAsStringAsync();
                return loginResult;
            }
            
            else
            {
                var error = await loginResponse.Content.ReadAsStringAsync();
                throw new Exception($"Login failed. Error: {error}");
            }
        }
    }
}
