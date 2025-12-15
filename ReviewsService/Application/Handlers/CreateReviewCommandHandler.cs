using Application.Commands;
using Application.DTO;
using Application.Grpc;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers
{
    public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, ReviewDto>
    {
        private readonly IReviewRepository _repository;
        private readonly IMapper _mapper;
        private readonly OrderGrpcClient _orderGrpcClient;


        public CreateReviewCommandHandler(IReviewRepository repository, IMapper mapper, OrderGrpcClient orderGrpcClient)
        {
            _repository = repository;
            _mapper = mapper;
            _orderGrpcClient = orderGrpcClient;
        }

        public async Task<ReviewDto> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            // Перевірка існування замовлення через gRPC
            var exists = await _orderGrpcClient.ExistsAsync(request.OrderId);
            if (!exists)
            {
                throw new Exception($"Order with ID {request.OrderId} does not exist.");
            }

            var review = new Review(request.CustomerId, request.OrderId, new Rating(request.Rating), request.Comment);
            await _repository.AddAsync(review);

            return _mapper.Map<ReviewDto>(review);
        }
    }
}
