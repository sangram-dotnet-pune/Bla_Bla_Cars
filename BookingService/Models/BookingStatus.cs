namespace BookingService.Models
{
    public enum BookingStatus
    {
        Pending,     // waiting for payment confirmation
        Confirmed,
        Cancelled,
        Failed
    }

}
