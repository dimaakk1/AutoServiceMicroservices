using Application.Commands;
using Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Cache;
using Application.DTO;

namespace Application.Handlers
{
    public class UpdateReviewCommandHandler : IRequestHandler<UpdateReviewCommand, bool>
    {
        private readonly IReviewRepository _repository;
        private readonly TwoLevelCacheService<IEnumerable<ReviewDto>> _reviewListCache;
        private readonly TwoLevelCacheService<ReviewDto> _orderCache;

        public UpdateReviewCommandHandler(
            IReviewRepository repository,
            TwoLevelCacheService<IEnumerable<ReviewDto>> reviewListCache,
            TwoLevelCacheService<ReviewDto> orderCache)
        {
            _repository = repository;
            _reviewListCache = reviewListCache;
            _orderCache = orderCache;
        }

        public async Task<bool> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
        {
            var review = await _repository.GetByIdAsync(request.Id);
            if (review == null)
                return false;

            review.Update(request.Rating, request.Comment);
            await _repository.UpdateAsync(review);

            // 🔥 Інвалідація ВСЬОГО, що повʼязано з цим відгуком
            await _reviewListCache.InvalidateAsync("reviews:all");
            await _reviewListCache.InvalidateAsync($"reviews:customer:{review.CustomerId}");
            await _reviewListCache.InvalidateAsync($"reviews:order:{review.OrderId}");
            await _orderCache.InvalidateAsync($"orderwithreview:{review.OrderId}");

            return true;
        }
    }
}
