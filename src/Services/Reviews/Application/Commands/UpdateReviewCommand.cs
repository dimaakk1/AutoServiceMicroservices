using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands
{
    public class UpdateReviewCommand : IRequest<bool>
    {
        public string Id { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}
