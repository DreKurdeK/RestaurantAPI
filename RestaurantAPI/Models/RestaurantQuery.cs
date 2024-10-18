namespace RestaurantAPI.Models
{
    public class RestaurantQuery
    {
        public string? SearchPhrase { get; set; } = null;
        public string? SortBy { get; set; } = null;
        public SortDirection SortDirection { get; set; } = SortDirection.ASC;
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
    }

}
