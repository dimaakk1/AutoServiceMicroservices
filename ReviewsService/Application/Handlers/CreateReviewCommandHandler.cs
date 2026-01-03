using Application.Commands;
using Application.DTO;
using Application.Grpc;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;
using Grpc.Core;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Cache;

namespace Application.Handlers
{
    public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, ReviewDto>
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

        public async Task<ReviewDto> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderGrpcClient.GetOrderAsync(request.OrderId);

            if (order == null)
            {
                throw new RpcException(
                    new Status(StatusCode.NotFound, $"Order with ID {request.OrderId} not found")
                );
            }

            var review = new Review(
                order.CustomerId,
                request.OrderId,
                new Rating(request.Rating),
                request.Comment
            );

            await _repository.AddAsync(review);

            // 🔥 Інвалідуємо кеш агрегатора
            await _cache.InvalidateAsync($"orderwithreview:{request.OrderId}");

            return _mapper.Map<ReviewDto>(review);
        }
    }

}
