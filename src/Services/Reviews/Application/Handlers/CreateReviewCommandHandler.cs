using Application.Commands;
using Application.DTO;
using Application.Grpc;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;
using Grpc.Core;
using MediatR;
using Application.Cache;

namespace Application.Handlers
{
    public class CreateReviewCommandHandler
        : IRequestHandler<CreateReviewCommand, ReviewDto>
    {
        private readonly IReviewRepository _repository;
        private readonly IMapper _mapper;
        private readonly OrderGrpcClient _orderGrpcClient;
        private readonly TwoLevelCacheService<ReviewDto> _cache;

        public CreateReviewCommandHandler(
            IReviewRepository repository,
            IMapper mapper,
            OrderGrpcClient orderGrpcClient,
            TwoLevelCacheService<ReviewDto> cache)
        {
            _repository = repository;
            _mapper = mapper;
            _orderGrpcClient = orderGrpcClient;
            _cache = cache;
        }

        public async Task<ReviewDto> Handle(
    CreateReviewCommand request,
    CancellationToken cancellationToken)
        {
            var order = await _orderGrpcClient
                .GetOrderAsync(request.OrderId);

            if (order == null)
                throw new RpcException(
                    new Status(StatusCode.NotFound,
                    $"Order with ID {request.OrderId} not found"));

            var review = new Review(
                request.OrderId,
                new Rating(request.Rating),
                request.Comment
            );

            await _repository.AddAsync(review);

            // ✅ ONLY REVIEW SERVICE CACHE
            await _cache.InvalidateAsync("reviews:all");
            await _cache.InvalidateAsync($"reviews:order:{request.OrderId}");
            await _cache.InvalidateAsync($"review:{review.Id}");

            return _mapper.Map<ReviewDto>(review);
        }
    }
}