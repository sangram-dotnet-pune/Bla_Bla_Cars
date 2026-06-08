using BookingService.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BookingService.Services
{
    public class TripClientService
    {


        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _http;

        public TripClientService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _http = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<Trip?> GetTripByIdAsync(Guid tripId)
            {
            //try
            //{
            //    return await _http.GetFromJsonAsync<TripDto>($"api/trip/{tripId}");
            //}
            //catch
            //{
            //    return null; // Trip Service unavailable
            //}
            AttachToken();
            return await _http.GetFromJsonAsync<Trip>($"api/trip/{tripId}");
        }


        private void AttachToken()
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"]
                            .ToString().Replace("Bearer ", "");
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<bool> UpdateSeatsAsync(Guid tripId, int seats)
        {
            AttachToken();
            var response = await _http.PutAsync($"api/trip/update-seats/{tripId}/{seats}", null);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RestoreSeatsAsync(Guid tripId, int seatsBooked)
        {
            AttachToken();
            var res = await _http.PutAsync($"api/trip/restore-seats/{tripId}/{seatsBooked}", null);
            return res.IsSuccessStatusCode;
        }
    }
}



