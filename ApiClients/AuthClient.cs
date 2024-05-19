using AmazingFileVersionControl.Core.DTOs.AuthDTOs;
using AmazingFileVersionControl.ApiClients.Helpers;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AmazingFileVersionControl.ApiClients
{
    public class AuthClient
    {
        private readonly HttpClient _httpClient;

        public AuthClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> RegisterAsync(RegisterDTO registerDto)
        {
            var content = new StringContent(JsonConvert.SerializeObject(registerDto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/Auth/register", content);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return TokenHelper.ExtractToken(responseContent);
        }

        public async Task<string> LoginAsync(LoginDto loginDto)
        {
            var content = new StringContent(JsonConvert.SerializeObject(loginDto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/Auth/login", content);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return TokenHelper.ExtractToken(responseContent);
        }
    }
}
