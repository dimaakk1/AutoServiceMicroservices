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
        private readonly UserService.UserServiceClient _userClient;

        public AggregationService(
            OrderService.OrderServiceClient orderClient,
            ReviewService.ReviewServiceClient reviewClient,
            UserService.UserServiceClient userClient)
        {
            _orderClient = orderClient;
            _reviewClient = reviewClient;
            _userClient = userClient;
        }

        // ======================================================
        // ONE ORDER
        // ======================================================
        public async Task<OrderWithReviewDto> GetOrderWithReviewAsync(int orderId)
        {
            var order = await _orderClient.GetOrderAsync(
                new OrderRequest { OrderId = orderId });

            var user = await _userClient.GetUserAsync(
                new UserRequest { UserId = order.UserId });

            var review = await GetReview(orderId);

            return new OrderWithReviewDto
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                Username = user.Username,
                Email = user.Email,
                Status = order.Status,
                OrderDate = DateTime.Parse(order.OrderDate),

                Items = order.Items.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName, // 🔥 FIX
                    Quantity = i.Quantity,
                    Price = (decimal)i.Price
                }).ToList(),

                Review = review
            };
        }

        // ======================================================
        // ALL ORDERS
        // ======================================================
        public async Task<List<OrderWithReviewDto>> GetAllOrdersWithReviewAsync(OrderAggregationFilterRequest filter)
        {
            var ordersResponse = await _orderClient.GetAllOrdersAsync(
                new OrderFilterRequest
                {
                    UserId = filter.UserId ?? "",
                    Status = filter.Status ?? ""
                });

            var result = new List<OrderWithReviewDto>();

            foreach (var order in ordersResponse.Orders)
            {
                var orderDate = DateTime.Parse(order.OrderDate);

                // 🔥 DATE FILTER (ВАЖЛИВО)
                if (filter.FromDate.HasValue && orderDate < filter.FromDate.Value)
                    continue;

                if (filter.ToDate.HasValue && orderDate > filter.ToDate.Value)
                    continue;

                var user = await _userClient.GetUserAsync(
                    new UserRequest { UserId = order.UserId });

                var review = await GetReview(order.OrderId);

                result.Add(new OrderWithReviewDto
                {
                    OrderId = order.OrderId,
                    UserId = order.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    Status = order.Status,
                    OrderDate = orderDate,

                    Items = order.Items.Select(i => new OrderItemDto
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        Quantity = i.Quantity,
                        Price = (decimal)i.Price
                    }).ToList(),

                    Review = review
                });
            }

            return result;
        }

        // ======================================================
        // ONLY REVIEWED ORDERS
        // ======================================================
        public async Task<List<OrderWithReviewDto>> GetOrdersWithReviewsOnlyAsync()
        {
            var ordersResponse = await _orderClient.GetAllOrdersAsync(
                new OrderFilterRequest
                {
                    UserId = "",
                    Status = ""
                });

            var result = new List<OrderWithReviewDto>();

            foreach (var order in ordersResponse.Orders)
            {
                var review = await GetReview(order.OrderId);

                if (review == null)
                    continue;

                var user = await _userClient.GetUserAsync(
                    new UserRequest { UserId = order.UserId });

                result.Add(new OrderWithReviewDto
                {
                    OrderId = order.OrderId,
                    UserId = order.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    Status = order.Status,
                    OrderDate = DateTime.Parse(order.OrderDate),

                    Items = order.Items.Select(i => new OrderItemDto
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName, // 🔥 FIX
                        Quantity = i.Quantity,
                        Price = (decimal)i.Price
                    }).ToList(),

                    Review = review
                });
            }

            return result;
        }

        // ======================================================
        // MY ORDERS
        // ======================================================
        public async Task<List<OrderWithReviewDto>> GetMyOrdersWithReviewAsync(string userId)
        {
            var ordersResponse = await _orderClient.GetAllOrdersAsync(
                new OrderFilterRequest
                {
                    UserId = userId,
                    Status = ""
                });

            var result = new List<OrderWithReviewDto>();

            foreach (var order in ordersResponse.Orders)
            {
                var user = await _userClient.GetUserAsync(
                    new UserRequest { UserId = order.UserId });

                var review = await GetReview(order.OrderId);

                result.Add(new OrderWithReviewDto
                {
                    OrderId = order.OrderId,
                    UserId = order.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    Status = order.Status,
                    OrderDate = DateTime.Parse(order.OrderDate),

                    Items = order.Items.Select(i => new OrderItemDto
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName, // 🔥 FIX
                        Quantity = i.Quantity,
                        Price = (decimal)i.Price
                    }).ToList(),

                    Review = review
                });
            }

            return result;
        }

        // ======================================================
        // REVIEW HELPER
        // ======================================================
        private async Task<ReviewDto?> GetReview(int orderId)
        {
            try
            {
                using var call = _reviewClient.GetReviewsByOrderId(
                    new ReviewRequest { OrderId = orderId });

                await foreach (var r in call.ResponseStream.ReadAllAsync())
                {
                    return new ReviewDto
                    {
                        Id = r.Id,
                        OrderId = r.OrderId,
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedAt = DateTime.Parse(r.CreatedAt)
                    };
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
