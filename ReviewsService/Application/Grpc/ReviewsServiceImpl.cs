using Application.Queries;
using Grpc.Core;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Grpc
{
    public class ReviewServiceImpl : ReviewService.ReviewServiceBase
    {
        private readonly IMediator _mediator;

        public ReviewServiceImpl(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override async Task GetReviewsByOrderId(ReviewRequest request, IServerStreamWriter<ReviewResponse> responseStream, ServerCallContext context)
        {
            var reviews = await _mediator.Send(new GetReviewsByOrderIdQuery { OrderId = request.OrderId });
            foreach (var r in reviews)
            {
                await responseStream.WriteAsync(new ReviewResponse
                {
                    Id = r.Id,
                    CustomerId = r.CustomerId,
                    OrderId = r.OrderId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt.ToString("O")
                });
            }
        }
    }
}
