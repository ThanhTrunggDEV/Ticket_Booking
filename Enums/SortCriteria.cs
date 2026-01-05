namespace Ticket_Booking.Enums
{
    public enum SortCriteria
    {
        PriceAsc,       // Giá tăng dần
        PriceDesc,      // Giá giảm dần
        DepartureTimeAsc,   // Thời gian khởi hành sớm nhất
        DepartureTimeDesc,  // Thời gian khởi hành muộn nhất
        DurationAsc,    // Thời gian bay ngắn nhất
        DurationDesc,   // Thời gian bay dài nhất
        DistanceAsc,    // Khoảng cách ngắn nhất
        DistanceDesc    // Khoảng cách xa nhất
    }
}
