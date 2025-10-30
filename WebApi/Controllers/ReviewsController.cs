using Application.Commands;
using Application.DTO;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReviewsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reviews = await _mediator.Send(new GetAllReviewsQuery());
            return Ok(reviews);
        }

        [HttpGet("customer/{customerId:int}")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetByCustomerId(int customerId)
        {
            var query = new GetReviewsByCustomerIdQuery { CustomerId = customerId };
            var reviews = await _mediator.Send(query);
            return Ok(reviews);
        }

        [HttpGet("order/{orderId:int}")]
        public async Task<IActionResult> GetByOrder(int orderId)
        {
            var reviews = await _mediator.Send(new GetReviewsByOrderIdQuery { OrderId = orderId});
            return Ok(reviews);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReviewCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAll), new { id = result }, result);
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _mediator.Send(new DeleteReviewCommand(id));
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateReviewCommand command)
        {
            if (id != command.Id)
                return BadRequest("ID mismatch.");

            var result = await _mediator.Send(command);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
