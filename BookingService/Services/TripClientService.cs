using BookingService.Models;
using System.Net.Http.Json;

namespace BookingService.Services
{
    public class TripClientService
    {

       
      
            private readonly HttpClient _http;

            public TripClientService(HttpClient http)
            {
                _http = http;
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
            return await _http.GetFromJsonAsync<Trip>($"api/trip/{tripId}");
        }

        public async Task<bool> UpdateSeatsAsync(Guid tripId, int seats)
        {
            var response = await _http.PutAsync($"api/trip/update-seats/{tripId}/{seats}", null);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RestoreSeatsAsync(Guid tripId, int seatsBooked)
        {
            var res = await _http.PutAsync($"api/trip/restore-seats/{tripId}/{seatsBooked}", null);
            return res.IsSuccessStatusCode;
        }
    }
}



