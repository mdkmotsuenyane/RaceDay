using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Mvc.Services
{
    public class ApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _contextAccessor;

        public ApiClient(HttpClient http, IHttpContextAccessor contextAccessor)
        {
            _http = http;
            _contextAccessor = contextAccessor;
        }

        private void AttachToken()
        {
            var token = _contextAccessor.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // ---------- Generic HTTP Helpers ----------

        public async Task<T?> GetAsync<T>(string url)
        {
            AttachToken();
            var response = await _http.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<T>();
            }

            // Return null gracefully instead of throwing an exception on 404/401
            return default;
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string url, T data)
        {
            AttachToken();
            return await _http.PostAsJsonAsync(url, data);
        }

        public async Task<HttpResponseMessage> PutAsync<T>(string url, T data)
        {
            AttachToken();
            return await _http.PutAsJsonAsync(url, data);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string url)
        {
            AttachToken();
            return await _http.DeleteAsync(url);
        }

        // ---------- File Upload Helper ----------

        public async Task<HttpResponseMessage> PostFileAsync(string url, MultipartFormDataContent content)
        {
            AttachToken();
            return await _http.PostAsync(url, content);
        }
    }
}
