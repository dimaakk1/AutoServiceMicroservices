namespace AggregatorService.DTO
{
    public class OrderAggregationFilterRequest
    {
        public string? UserId { get; set; }
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? Date { get; set; }
        public int? Page { get; set; }
        public int PageSize { get; set; } = 25;
    }
}
