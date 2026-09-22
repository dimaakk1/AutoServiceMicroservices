namespace AggregatorService.DTO
{
    public class OrderAggregationFilterRequest
    {
        public string? UserId { get; set; }
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
