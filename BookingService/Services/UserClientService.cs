using System.Net.Http.Headers;
using System.Text.Json;

namespace BookingService.Services
{
    public class UserClientService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _http;

      
        public UserClientService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _http = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<UserDetailsDto?> GetCurrentUserAsync(Guid userId, string token)
        {
            if (userId == Guid.Empty)
                return null;

            _http.DefaultRequestHeaders.Authorization =
       new AuthenticationHeaderValue("Bearer", token);
            var response = await _http.GetAsync($"auth/me/{userId}");
            if (!response.IsSuccessStatusCode)
                return null;

            await using var contentStream = await response.Content.ReadAsStreamAsync();
            var user = await JsonSerializer.DeserializeAsync<UserDetailsDto>(contentStream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return user;
        }

        public class UserDetailsDto
        {
            public Guid UserId { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public string? PhoneNumber { get; set; }
        }
    }
}
