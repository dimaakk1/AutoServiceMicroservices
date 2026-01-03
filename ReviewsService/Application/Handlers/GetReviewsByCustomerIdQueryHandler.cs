using Application.DTO;
using Application.Queries;
using AutoMapper;
using Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Cache;

namespace Application.Handlers
{
    public class GetReviewsByCustomerIdQueryHandler
    : IRequestHandler<GetReviewsByCustomerIdQuery, IEnumerable<ReviewDto>>
    {
        private readonly IReviewRepository _repository;
        private readonly IMapper _mapper;
        private readonly TwoLevelCacheService<IEnumerable<ReviewDto>> _cache;

        public GetReviewsByCustomerIdQueryHandler(
            IReviewRepository repository,
            IMapper mapper,
            TwoLevelCacheService<IEnumerable<ReviewDto>> cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<IEnumerable<ReviewDto>> Handle(GetReviewsByCustomerIdQuery request, CancellationToken cancellationToken)
        {
            var key = $"reviews:customer:{request.CustomerId}";

            return await _cache.GetOrCreateAsync(
                key: key,
                factory: async () =>
                {
                    var reviews = await _repository.GetByCustomerIdAsync(request.CustomerId);
                    return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
                },
                l1Ttl: TimeSpan.FromSeconds(30),
                l2Ttl: TimeSpan.FromMinutes(5)
            ) ?? Enumerable.Empty<ReviewDto>();
        }
    }

}
