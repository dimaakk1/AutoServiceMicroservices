namespace AggregatorService.DTO
{
    public class OrderWithReviewDto
    {
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; }      


        public DateTime OrderDate { get; set; }
        public string Status { get; set; }

        public IEnumerable<OrderItemDto> Items { get; set; }

        public ReviewDto? Review { get; set; }
    }
}
