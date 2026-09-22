using Application.DTO;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries
{
    public class GetReviewsByOrderIdQuery : IRequest<IEnumerable<ReviewDto>>
    {
        public int OrderId { get; set; }
    }
}
