using AggregatorService.DTO;
using Grpc.Core;
using AggregatorService.Cache;

namespace AggregatorService.Services
{
    /*public class AggregationService : IAggregationService
    {
        private readonly HttpClient _ordersClient;
        private readonly HttpClient _reviewsClient;

        public AggregationService(IHttpClientFactory factory)
        {
            _ordersClient = factory.CreateClient("orders");
            _reviewsClient = factory.CreateClient("reviews");
        }

        public async Task<OrderWithReviewDto> GetOrderWithReviewAsync(int orderId)
        {
            var order = await _ordersClient.GetFromJsonAsync<OrderWithReviewDto>($"api/Orders/Order/{orderId}");
            if (order == null)
                throw new Exception("Order not found");

            var reviews = await _reviewsClient.GetFromJsonAsync<List<ReviewDto>>($"api/Reviews/order/{orderId}");
            var review = reviews?.FirstOrDefault();

            order.Review = review;

            return order;
        }
    }*/

    public class AggregationService : IAggregationService
    {
        private readonly OrderService.OrderServiceClient _orderClient;
        private readonly ReviewService.ReviewServiceClient _reviewClient;
        private readonly TwoLevelCacheService<OrderWithReviewDto> _cache;

        public AggregationService(
            OrderService.OrderServiceClient orderClient,
            ReviewService.ReviewServiceClient reviewClient,
            TwoLevelCacheService<OrderWithReviewDto> cache)
        {
            _orderClient = orderClient;
            _reviewClient = reviewClient;
            _cache = cache;
        }

        public async Task<OrderWithReviewDto> GetOrderWithReviewAsync(int orderId)
        {
            var key = $"orderwithreview:{orderId}";

            return await _cache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    var orderResponse = await _orderClient.GetOrderAsync(new OrderRequest { OrderId = orderId });

                    ReviewDto? review = null;
                    using var call = _reviewClient.GetReviewsByOrderId(new ReviewRequest { OrderId = orderId });
                    await foreach (var r in call.ResponseStream.ReadAllAsync())
                    {
                        review = new ReviewDto
                        {
                            Id = r.Id,
                            CustomerId = r.CustomerId,
                            OrderId = r.OrderId,
                            Rating = r.Rating,
                            Comment = r.Comment,
                            CreatedAt = DateTime.Parse(r.CreatedAt)
                        };
                        break;
                    }

                    return new OrderWithReviewDto
                    {
                        OrderId = orderResponse.OrderId,
                        CustomerId = orderResponse.CustomerId,
                        Status = orderResponse.Status,
                        OrderDate = DateTime.Parse(orderResponse.OrderDate),
                        Items = orderResponse.Items.Select(i => new OrderItemDto
                        {
                            ProductId = i.ProductId,
                            Quantity = i.Quantity,
                            Price = (decimal)i.Price
                        }).ToList(),
                        Review = review
                    };
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? throw new Exception("Order with review not found");
        }

        public async Task InvalidateOrderCacheAsync(int orderId)
        {
            await _cache.InvalidateAsync($"orderwithreview:{orderId}");
        }
    }

}
