namespace Ticket_Booking.Models.BindingModels
{
    public class UserOtp
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string? Phone { get; set; }
    }
    
    public class UserOtpModel : UserOtp
    {
        string Otp { get; set; }
    }
}
