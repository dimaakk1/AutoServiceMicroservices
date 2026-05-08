using AggregatorService.DTO;

namespace AggregatorService.Services
{
    public interface IAggregationService
    {
        Task<OrderWithReviewDto> GetOrderWithReviewAsync(int orderId);
        Task<List<OrderWithReviewDto>> GetAllOrdersWithReviewAsync(string? userId);
        Task<List<OrderWithReviewDto>> GetMyOrdersWithReviewAsync(string userId);
        Task<List<OrderWithReviewDto>> GetOrdersWithReviewsOnlyAsync();
    }

}
