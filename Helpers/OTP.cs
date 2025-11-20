namespace Ticket_Booking.Helpers
{
    public static class OTP
    {
        public static string GenerateOTP()
        {
            return Random.Shared.Next(100000, 999999).ToString();
        }
    }
}
