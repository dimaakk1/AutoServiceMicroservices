using AggregatorService.DTO;

namespace AggregatorService.Services
{
    public interface IAggregationService
    {
        Task<OrderWithReviewDto> GetOrderWithReviewAsync(int orderId);
        Task<List<OrderWithReviewDto>> GetAllOrdersWithReviewAsync(OrderAggregationFilterRequest filter);
        Task<List<OrderWithReviewDto>> GetOrdersWithReviewsOnlyAsync();
        Task<List<OrderWithReviewDto>> GetMyOrdersWithReviewAsync(string userId);
    }

}
