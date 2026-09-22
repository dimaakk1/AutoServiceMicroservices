using System.Text.Json.Serialization;

namespace AggregatorService.DTO
{
    public class ReviewDto
    {
        [JsonPropertyName("_id")] 
        public string Id { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
