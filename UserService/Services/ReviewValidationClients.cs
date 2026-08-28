using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace UserService.Services;

public record TripReviewValidation(Guid TripId, Guid DriverId, string Status);
public record BookingReviewValidation(Guid BookingId, Guid TripId, Guid PassengerId, Guid DriverId, string Status, string TripStatus);
public record ReviewParticipant(Guid BookingId, Guid PassengerId, Guid DriverId, string PassengerName, string Status);
public record ReviewParticipants(Guid TripId, Guid DriverId, string TripStatus, IReadOnlyList<ReviewParticipant> Bookings);

public class TripReviewClient(HttpClient http)
{
    public async Task<(TripReviewValidation? Value, HttpStatusCode Status)> GetAsync(Guid tripId, string token, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/trip/review-validation/{tripId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return (null, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<TripReviewValidation>(cancellationToken: cancellationToken), response.StatusCode);
    }
}

public class BookingReviewClient(HttpClient http)
{
    public async Task<(BookingReviewValidation? Value, HttpStatusCode Status)> GetAsync(Guid bookingId, string token, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/booking/review-validation/{bookingId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return (null, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<BookingReviewValidation>(cancellationToken: cancellationToken), response.StatusCode);
    }

    public async Task<(ReviewParticipants? Value, HttpStatusCode Status)> GetParticipantsAsync(Guid tripId, string token, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/booking/review-participants/{tripId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return (null, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ReviewParticipants>(cancellationToken: cancellationToken), response.StatusCode);
    }
}
