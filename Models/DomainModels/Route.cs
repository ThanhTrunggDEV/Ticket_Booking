namespace Ticket_Booking.Models.DomainModels
{
    public class Route
    {
        public int Id { get; set; }
        public string FromCity { get; set; } = string.Empty;
        public string ToCity { get; set; } = string.Empty;
        public string OriginAirportCode { get; set; } = string.Empty; 
        public string DestinationAirportCode { get; set; } = string.Empty; 
        public string OriginAirportName { get; set; } = string.Empty; 
        public string DestinationAirportName { get; set; } = string.Empty; 
        public decimal Distance { get; set; }
        public int EstimatedDuration { get; set; } 

       
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    }
}
