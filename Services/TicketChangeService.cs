using Ticket_Booking.Enums;
using Ticket_Booking.Models.DomainModels;
using Ticket_Booking.Repositories;

namespace Ticket_Booking.Services
{

    public interface ITicketChangeService
    {
        Task<(bool allowed, string? message, decimal changeFee)> CheckChangeEligibilityAsync(Ticket ticket);
        Task<decimal> CalculateChangeFeeAsync(Ticket ticket);
        Task<decimal> CalculatePriceDifferenceAsync(Ticket originalTicket, Trip newTrip, SeatClass? newSeatClass);
        Task<(decimal totalDue, decimal refundAmount)> CalculateTotalChangeAmountAsync(
            Ticket originalTicket, Trip newTrip, SeatClass? newSeatClass);
    }

    public class TicketChangeService : ITicketChangeService
    {
        private readonly TripRepository _tripRepository;
        private const int MIN_HOURS_BEFORE_DEPARTURE = 3;  // Minimum hours before departure to allow changes

        public TicketChangeService(TripRepository tripRepository)
        {
            _tripRepository = tripRepository;
        }

        public async Task<(bool allowed, string? message, decimal changeFee)> CheckChangeEligibilityAsync(Ticket ticket)
        {
            // Check if ticket is cancelled
            if (ticket.IsCancelled)
            {
                return (false, "Vé đã bị hủy, không thể đổi vé.", 0);
            }

            // Check if ticket is already checked in
            if (ticket.IsCheckedIn)
            {
                return (false, "Vé đã check-in, không thể đổi vé. Vui lòng liên hệ quầy tại sân bay.", 0);
            }

            // Get the original trip
            var originalTrip = await _tripRepository.GetByIdAsync(ticket.TripId);
            if (originalTrip == null)
            {
                return (false, "Không tìm thấy thông tin chuyến bay.", 0);
            }

            // Check time before departure
            var hoursBeforeDeparture = (originalTrip.DepartureTime - DateTime.UtcNow).TotalHours;
            if (hoursBeforeDeparture < MIN_HOURS_BEFORE_DEPARTURE)
            {
                return (false, 
                    $"Không thể đổi vé. Phải đổi vé trước ít nhất {MIN_HOURS_BEFORE_DEPARTURE} giờ so với giờ khởi hành.", 
                    0);
            }

            // Check if trip has already departed
            if (originalTrip.DepartureTime <= DateTime.UtcNow)
            {
                return (false, "Chuyến bay đã khởi hành, không thể đổi vé.", 0);
            }

            // Calculate change fee based on airline and seat class
            var changeFee = await CalculateChangeFeeAsync(ticket);

            return (true, null, changeFee);
        }


        public async Task<decimal> CalculateChangeFeeAsync(Ticket ticket)
        {
            var trip = await _tripRepository.GetByIdAsync(ticket.TripId);
            if (trip == null) return 0;

            var airlineName = trip.Company?.Name?.ToLower() ?? "";
            var seatClass = ticket.SeatClass;

            // Base change fees in USD (can be converted to VND later)
            decimal changeFee = 0;

            // Vietnam Airlines policy
            if (airlineName.Contains("vietnam airlines"))
            {
                switch (seatClass)
                {
                    case SeatClass.Economy:

                        changeFee = 15m;  // Default for Economy Standard
                        break;
                    case SeatClass.Business:
                        
                        changeFee = 15m;
                        break;
                    case SeatClass.FirstClass:
                        changeFee = 0m;
                        break;
                }
            }
            // VietJet Air policy
            else if (airlineName.Contains("vietjet"))
            {
                switch (seatClass)
                {
                    case SeatClass.Economy:
                        changeFee = 15m;  // ~350,000 VND
                        break;
                    case SeatClass.Business:
                        changeFee = 35m;  // ~800,000 VND
                        break;
                    case SeatClass.FirstClass:
                        changeFee = 0m;  // Skyboss: Free
                        break;
                }
            }
            // Bamboo Airways policy
            else if (airlineName.Contains("bamboo"))
            {
                switch (seatClass)
                {
                    case SeatClass.Economy:
                        changeFee = 10m;  // ~270,000 VND
                        break;
                    case SeatClass.Business:
                        changeFee = 10m;
                        break;
                    case SeatClass.FirstClass:
                        changeFee = 0m;
                        break;
                }
            }
            // Jetstar Pacific policy (similar to VietJet)
            else if (airlineName.Contains("jetstar"))
            {
                switch (seatClass)
                {
                    case SeatClass.Economy:
                        changeFee = 15m;
                        break;
                    case SeatClass.Business:
                        changeFee = 20m;
                        break;
                    case SeatClass.FirstClass:
                        changeFee = 0m;
                        break;
                }
            }
            // Default policy
            else
            {
                changeFee = 15m;  // Default change fee
            }

            return changeFee;
        }

        /// <summary>
        /// Calculates price difference between original ticket and new ticket
        /// </summary>
        public async Task<decimal> CalculatePriceDifferenceAsync(Ticket originalTicket, Trip newTrip, SeatClass? newSeatClass)
        {
            // Use original seat class if new seat class is not specified
            var targetSeatClass = newSeatClass ?? originalTicket.SeatClass;

            // Get original trip price
            var originalTrip = await _tripRepository.GetByIdAsync(originalTicket.TripId);
            if (originalTrip == null) return 0;

            decimal originalPrice = GetPriceBySeatClass(originalTrip, originalTicket.SeatClass);
            decimal newPrice = GetPriceBySeatClass(newTrip, targetSeatClass);

            return newPrice - originalPrice;
        }

        /// <summary>
        /// Calculates total amount due (change fee + price difference)
        /// Note: If new ticket is cheaper, no refund is given - customer only pays change fee
        /// </summary>
        public async Task<(decimal totalDue, decimal refundAmount)> CalculateTotalChangeAmountAsync(
            Ticket originalTicket, Trip newTrip, SeatClass? newSeatClass)
        {
            var changeFee = await CalculateChangeFeeAsync(originalTicket);
            var priceDifference = await CalculatePriceDifferenceAsync(originalTicket, newTrip, newSeatClass);

            decimal totalDue = 0;
            decimal refundAmount = 0;  // No refunds - always 0

            if (priceDifference > 0)
            {
                // New ticket is more expensive: customer pays change fee + difference
                totalDue = changeFee + priceDifference;
            }
            else
            {
                // New ticket is cheaper or same price: customer only pays change fee (no refund)
                totalDue = changeFee;
            }

            return (totalDue, refundAmount);
        }

        private decimal GetPriceBySeatClass(Trip trip, SeatClass seatClass)
        {
            return seatClass switch
            {
                SeatClass.Economy => trip.EconomyPrice,
                SeatClass.Business => trip.BusinessPrice,
                SeatClass.FirstClass => trip.FirstClassPrice,
                _ => trip.EconomyPrice
            };
        }
    }
}

