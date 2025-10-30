using Application.Commands;
using Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers
{
    public class UpdateReviewCommandHandler : IRequestHandler<UpdateReviewCommand, bool>
    {
        private readonly IReviewRepository _repository;

        public UpdateReviewCommandHandler(IReviewRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
        {
            var review = await _repository.GetByIdAsync(request.Id);
            if (review == null)
                return false;

            review.Update(request.Rating, request.Comment);
            await _repository.UpdateAsync(review);
            return true;
        }
    }
}
