using System.ComponentModel.DataAnnotations;

namespace Ticket_Booking.Models.BindingModels
{
    public class UserEditProfile
    {
        [Required(ErrorMessage = "Full Name is required")]
        public string FullName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid Phone Number")]
        public string? Phone { get; set; }

        public string? Email { get; set; } 
    }
}
