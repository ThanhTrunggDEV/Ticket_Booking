namespace Ticket_Booking.Helpers
{
    public static class OTP
    {
        private static string _currentOTP = string.Empty;
        public static string GenerateOTP()
        {
            _currentOTP = Random.Shared.Next(100000, 999999).ToString();
            return _currentOTP;
        }
        public static bool ValidateOTP(string otp)
        {
            return otp == _currentOTP;
        }
    }
}
